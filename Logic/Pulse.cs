using PhrasePulse.Logic.Configuration;
using System.Text;
using System.Text.RegularExpressions;

namespace PhrasePulse.Logic;

internal class Pulse
{
    public string Phrase { get; }
    private readonly PulseSearchConfiguration _config;
    private readonly AppConfig _appConfig;
    public List<(int Start, int End)> Indexes { get; private set; } = [];

    // Configuración de colores
    private string BorderColor => _config.NoColor 
        ? string.Empty
        : _appConfig.Colors.BorderColors.ToString();
    private string FoundColor => _config.NoColor
        ? string.Empty
        : _appConfig.Colors.FoundColors.ToString();
    private string TextColor => _config.NoColor
        ? string.Empty
        : _appConfig.Colors.TextColors.ToString();
    private string DefaultColor => _config.NoColor
        ? string.Empty
        : ConsoleColors.Reset;


    public string HighlightedPhrase
    {
        get
        {
            StringBuilder result = new(Phrase);
            var sortedIndexes = Indexes.OrderBy(t => t.Start).ToList();
            int offset = 0;

            for (int i = 0; i < sortedIndexes.Count; i++)
            {
                var (_start, _end) = sortedIndexes[i];
                int start = _start + offset;
                int end = _end + offset;

                // Determinar si el cierre está dentro de otro rango
                bool isInside = false;
                for (int j = i + 1; j < sortedIndexes.Count; j++)
                {
                    if (_end > sortedIndexes[j].Start)
                    {
                        isInside = true;
                        break;
                    }
                }

                // Insertar marcador de fin primero
                string atEnd = isInside
                    ? $"{BorderColor}»{FoundColor}"
                    : $"{BorderColor}»{TextColor}";

                result.Insert(end, atEnd);

                // Insertar marcador de inicio
                string atBeginning = $"{BorderColor}«{FoundColor}";
                result.Insert(start, atBeginning);

                // Actualizar offset
                offset += atBeginning.Length + atEnd.Length;
            }

            return StandardColoredOutput(result.ToString());
        }
    }
    public string HighlightedIndexes
    {
        get
        {
            StringBuilder sb = new();
            if (Indexes.Count == 0)
            {
                sb.AppendLine("No matches found.");
                return sb.ToString();
            }
            sb.AppendLine(Environment.NewLine + "Matches:");
            for (int i = 0; i < Indexes.Count; i++)
            {
                sb.Append($"{BorderColor}[");
                sb.Append($"{FoundColor}{i + 1}");
                sb.Append($"{BorderColor}]");
                sb.Append($"{TextColor} From ");
                sb.Append($"{FoundColor}{Indexes[i].Start}");
                sb.Append($"{TextColor} To ");
                sb.Append($"{FoundColor}{Indexes[i].End}");
            }

            return StandardColoredOutput(sb.ToString());
        }
    }

    public Pulse(PulseSearchConfiguration config, AppConfig? appConfig = null)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _appConfig = appConfig ?? AppConfig.Default;

        if (config.InputText is not null)
        {
            Phrase = config.InputText;
        }
        else if (config.InputFile is not null)
        {
            if (!config.InputFile.Exists)
                throw new FileNotFoundException("Input file not found", config.InputFile.FullName);

            Phrase = File.ReadAllText(config.InputFile.FullName, config.Encoding);
        }
        else throw new ArgumentException("Input text or file must be provided");
    }

    public bool FindMatches()
    {
        if (_config.UseRegex) FindMatchesRegex();
        else FindMatchesSimple();

        return Indexes.Count > 0;
    }
    private void FindMatchesSimple()
    {
        List<(int, int)> indexes = [];
        if (_config.AllowOverlapping)
        {
            for (int i = 0; i <= Phrase.Length - _config.SearchPattern.Length; i++)
            {
                if (Phrase.Substring(i, _config.SearchPattern.Length).Equals(_config.SearchPattern, _config.IgnoreCase ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture))
                {
                    indexes.Add(new(i, i + _config.SearchPattern.Length));
                }
            }
        }
        else
        {
            var matches = Regex.Matches(Phrase, Regex.Escape(_config.SearchPattern), _config.IgnoreCase ? RegexOptions.IgnoreCase : RegexOptions.None);
            foreach (Match match in matches)
            {
                indexes.Add(new(match.Index, match.Index + _config.SearchPattern.Length));
            }
        }

        Indexes = indexes;
    }
    private void FindMatchesRegex()
    {
        // Implementación con regex
        try
        {
            var regex = new Regex(
                _config.SearchPattern,
                _config.RegexOptions,
                TimeSpan.FromSeconds(_config.MatchTimeoutSeconds)
            );

            Indexes = [.. regex.Matches(Phrase)
                                   .Select(m => (m.Index, m.Index + m.Length))];
        }
        catch (RegexMatchTimeoutException)
        {
            throw new PhrasePulseException($"Regex matching timed out after {_config.MatchTimeoutSeconds} seconds");
        }
        catch (ArgumentException ex)
        {
            throw new PhrasePulseException($"Invalid regex pattern: {ex.Message}");
        }
    }
    private string StandardColoredOutput(string str)
    {
        return $"{TextColor}{str}{DefaultColor}";
    }
}
