using PhrasePulse.Logic.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;

namespace PhrasePulse.Logic;

internal class Pulse
{
    public string? PhraseColored { get; private set; } = null;

    public readonly string Phrase;
    public readonly Encoding Encoding;
    public readonly PulseOptionsManager Options;
    public readonly RegexOptions RegexOptions;
    public readonly AppConfig Config;
    private List<(int, int)> Indexes = [];

    public Pulse(
        string phrase,
        PulseOptions options = PulseOptions.Default,
        Encoding? encoding = null,
        AppConfig? config = null,
        RegexOptions regexOptions = RegexOptions.None)
    {
        ArgumentException.ThrowIfNullOrEmpty(phrase, nameof(phrase));

        Phrase = phrase;
        Encoding = encoding ?? Encoding.UTF8;
        Config = config ?? AppConfig.Default;
        Options = options == PulseOptions.Default
            ? PulseOptionsManager.Default
            : new(options);
        RegexOptions = (Options.IgnoreCase ? RegexOptions.IgnoreCase : RegexOptions.None) | regexOptions;
    }
    public static Pulse FromFile(
        string path,
        PulseOptions options = PulseOptions.Default,
        Encoding? encoding = null,
        AppConfig? config = null,
        RegexOptions regexOptions = RegexOptions.None)
    {
        try
        {
            var phrase = File.ReadAllText(path, encoding ?? Encoding.UTF8);
            return new(phrase, options, encoding ?? Encoding.UTF8, config, regexOptions);
        }
        catch (Exception ex)
        {
            throw new PhrasePulseException(
                "An error occurred while trying to read the file: " + ex.Message,
                ex.InnerException ?? ex
            );
        }
    }

    public bool Start([StringSyntax("Regex", "Options")] string reque)
    {
        ColorPair colorDef = (Console.ForegroundColor, Console.BackgroundColor);

        if (Options.UseRegex)
        {
            // Implementación con regex
            try
            {
                var regex = new Regex(
                    reque,
                    RegexOptions,
                    TimeSpan.FromSeconds(Options.MatchTimeout)
                );

                Indexes = [.. regex.Matches(Phrase)
                                   .Select(m => (m.Index, m.Index + m.Length))];
            }
            catch (RegexMatchTimeoutException)
            {
                throw new PhrasePulseException($"Regex matching timed out after {Options.MatchTimeout} seconds");
            }
            catch (ArgumentException ex)
            {
                throw new PhrasePulseException($"Invalid regex pattern: {ex.Message}");
            }
        }
        else Indexes = FindIndexes(reque);

        PhraseColored = $"{Config.Colors.TextColors}{SetColors()}{colorDef}";
        return Indexes.Count > 0;
    }

    private List<(int, int)> FindIndexes(string reque)
    {
        List<(int, int)> indexes = [];
        if (Options.AllowOverlapping)
        {
            for (int i = 0; i <= Phrase.Length - reque.Length; i++)
            {
                if (Phrase.Substring(i, reque.Length).Equals(reque, Options.IgnoreCase ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture))
                {
                    indexes.Add(new(i, i + reque.Length));
                }
            }
        }
        else
        {
            var matches = Regex.Matches(Phrase, Regex.Escape(reque), Options.IgnoreCase ? RegexOptions.IgnoreCase : RegexOptions.None);
            foreach (Match match in matches)
            {
                indexes.Add(new(match.Index, match.Index + reque.Length));
            }
        }

        return indexes;
    }

    private string SetColors()
    {
        StringBuilder result = new(Phrase);
        var sortedIndexes = Indexes.OrderBy(t => t.Item1).ToList();
        int offset = 0;

        for (int i = 0; i < sortedIndexes.Count; i++)
        {
            var current = sortedIndexes[i];
            int start = current.Item1 + offset;
            int end = current.Item2 + offset;

            // Determinar si el cierre está dentro de otro rango
            bool isInsideAnotherRange = false;
            for (int j = i + 1; j < sortedIndexes.Count; j++)
            {
                if (current.Item2 > sortedIndexes[j].Item1)
                {
                    isInsideAnotherRange = true;
                    break;
                }
            }

            // Insertar marcador de fin primero
            string atEnd = isInsideAnotherRange
                ? $"{Config.Colors.BorderColors}»{Config.Colors.FoundColors}"
                : $"{Config.Colors.BorderColors}»{Config.Colors.TextColors}";

            result.Insert(end, atEnd);

            // Insertar marcador de inicio
            string atBeginning = $"{Config.Colors.BorderColors}«{Config.Colors.FoundColors}";
            result.Insert(start, atBeginning);

            // Actualizar offset
            offset += atBeginning.Length + atEnd.Length;
        }

        return result.ToString();
    }

    public string IndexesToString()
    {
        StringBuilder sb = new();
        if (Indexes.Count == 0)
        {
            sb.AppendLine("No coincidences found.");
            return sb.ToString();
        }
        sb.AppendLine(Environment.NewLine + "Coincidences:");
        for (int i = 0; i < Indexes.Count; i++)
        {
            sb.AppendLine($"[{i+1}] from {Indexes[i].Item1} to {Indexes[i].Item2}");
        }

        return sb.ToString();
    }
}
