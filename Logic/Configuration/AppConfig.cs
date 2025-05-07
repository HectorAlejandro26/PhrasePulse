using System.Text.Json;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Diagnostics.CodeAnalysis;

namespace PhrasePulse.Logic.Configuration;


public partial struct ColorPair
{
    public ConsoleColor Fore { get; set; }
    public ConsoleColor Back { get; set; }

    public ColorPair(ConsoleColor fore, ConsoleColor back)
    {
        Fore = fore;
        Back = back;
    }
    public ColorPair(ColorPair pair)
    {
        Fore = pair.Fore;
        Back = pair.Back;
    }
    public static implicit operator ColorPair((ConsoleColor fore, ConsoleColor back) pair) => 
        new(pair.fore, pair.back);

    public override readonly string ToString() => $"{Fore.ToForeColor()}{Back.ToBackColor()}";
    public readonly void Apply()
    {
        Console.ForegroundColor = Fore;
        Console.BackgroundColor = Back;
    }
    public readonly ColorPair Inverse() => new(Back, Fore);

    public static ColorPair GetLastColors(string str, int index)
    {
        if (index > str.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(index), "Index must be within the string bounds");
        }
        index = index < 0 ? str.Length + index : index;

        ConsoleColor lastFg = Console.ForegroundColor;
        ConsoleColor lastBg = Console.BackgroundColor;

        // Patrón regex para encontrar secuencias ANSI de color
        var pattern = "\x1b\\[([0-9;]*)m";
        var matches = Regex.Matches(str[..index], pattern);

        foreach (Match match in matches)
        {
            if (!match.Success) continue;

            var codes = match.Groups[1].Value.Split(';');

            foreach (var codeStr in codes)
            {
                if (!int.TryParse(codeStr, out int code)) continue;

                // Reset/valores por defecto
                if (code == 0)
                {
                    lastFg = ConsoleColor.Gray;
                    lastBg = ConsoleColor.Black;
                }
                // Colores de texto (30-37, 90-97)
                else if (code >= 30 && code <= 37)
                {
                    lastFg = (ConsoleColor)(code - 30);
                }
                else if (code >= 90 && code <= 97)
                {
                    lastFg = (ConsoleColor)(code - 90 + 8); // Bright colors (8-15)
                }
                // Colores de fondo (40-47, 100-107)
                else if (code >= 40 && code <= 47)
                {
                    lastBg = (ConsoleColor)(code - 40);
                }
                else if (code >= 100 && code <= 107)
                {
                    lastBg = (ConsoleColor)(code - 100 + 8); // Bright colors (8-15)
                }
                // Establecer color mediante 38/48 (no soportado directamente por ConsoleColor)
                // Estos casos los ignoramos ya que ConsoleColor no soporta RGB
            }
        }

        return new(lastFg, lastBg);
    }

    public override readonly bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is not ColorPair other) return false;
        return Fore == other.Fore && Back == other.Back;
    }
    public override readonly int GetHashCode() => HashCode.Combine(Fore, Back);

    public static bool operator ==(ColorPair left, ColorPair right) => left.Equals(right);
    public static bool operator !=(ColorPair left, ColorPair right) => !left.Equals(right);
}
public struct ConsoleColors
{
    public ColorPair TextColors { get; set; }
    public ColorPair FoundColors { get; set; }
    public ColorPair BorderColors { get; set; }
    public static readonly string ResetFore = Utils.DefaultForeColor;
    public static readonly string ResetBack = Utils.DefaultBackColor;
    public static readonly string Reset = Utils.DefaultColor;

    public ConsoleColors(
        ConsoleColor tFore,
        ConsoleColor tBack,
        ConsoleColor fFore,
        ConsoleColor fBack,
        ConsoleColor bFore,
        ConsoleColor bBack)
    {
        TextColors = new(tFore, tBack);
        FoundColors = new(fFore, fBack);
        BorderColors = new(bFore, bBack);
    }
    public ConsoleColors(ColorPair text, ColorPair found, ColorPair border)
    {
        TextColors = text;
        FoundColors = found;
        BorderColors = border;
    }
    public ConsoleColors(ConsoleColors colors)
    {
        TextColors = colors.TextColors;
        FoundColors = colors.FoundColors;
        BorderColors = colors.BorderColors;
    }
    public static implicit operator ConsoleColors((ColorPair text, ColorPair found, ColorPair border) pairs) =>
        new(pairs.text, pairs.found, pairs.border);
}

internal partial class AppConfig(ConsoleColors colors)
{
    private const string FileName = "config.json";
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        IncludeFields = true
    };
    public ConsoleColors Colors = colors;

    // [^0-15] es considerado un color por defecto
    public static AppConfig Default => new(new(
            Console.ForegroundColor == ConsoleColor.White ? ConsoleColor.Gray : ConsoleColor.White,
            (ConsoleColor) 16,
            Console.ForegroundColor == ConsoleColor.Cyan ? ConsoleColor.Yellow : ConsoleColor.Cyan,
            (ConsoleColor) 16,
            Console.ForegroundColor == ConsoleColor.Red ? ConsoleColor.Green : ConsoleColor.Red,
            (ConsoleColor) 16));

    public static void Save(AppConfig config)
    {
        string nl = Environment.NewLine;
        string colorMapComment = "    // Color map:" + nl +
        "    // 0: Black        | 8:  DarkGray" + nl +
        "    // 1: DarkBlue     | 9:  Blue" + nl +
        "    // 2: DarkGreen    | 10: Green" + nl +
        "    // 3: DarkCyan     | 11: Cyan" + nl +
        "    // 4: DarkRed      | 12: Red" + nl +
        "    // 5: DarkMagenta  | 13: Magenta" + nl +
        "    // 6: DarkYellow   | 14: Yellow" + nl +
        "    // 7: Gray         | 15: White" + nl +
        "    // Any other number is Default" + nl + nl;

        string json = JsonSerializer.Serialize(config, JsonOptions);
        json = json.Replace(
            $"\"Colors\": {{{nl}    \"TextColors\"",
            $"\"Colors\": {{{nl}{colorMapComment}    \"TextColors\""
        );
        File.WriteAllText(FileName, json);
    }
    public static AppConfig Load()
    {
        if (!File.Exists(FileName))
        {
            Save(Default);
            return Default;
        }
        string json = File.ReadAllText(FileName);
        json = StripCommentsRegex().Replace(json, "");
        json = StripBlockCommentsRegex().Replace(json, "");
        var config = JsonSerializer.Deserialize<AppConfig>(json, JsonOptions);
        if (config is null)
        {
            Save(Default);
            return Default;
        }
        return config;
    }
    public static void Open()
    {
        if (!File.Exists(FileName)) Save(Default);

        Process.Start(new ProcessStartInfo
        {
            FileName = FileName,
            UseShellExecute = true,
            CreateNoWindow = false,
            WindowStyle = ProcessWindowStyle.Normal
        });
    }

    [GeneratedRegex(@"^\s*//.*$", RegexOptions.Multiline)]
    private static partial Regex StripCommentsRegex();
    [GeneratedRegex(@"/\*.*?\*/", RegexOptions.Singleline)]
    private static partial Regex StripBlockCommentsRegex();
}
