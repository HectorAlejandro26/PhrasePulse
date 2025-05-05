using System.Text.Json;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Diagnostics.CodeAnalysis;

namespace PhrasePulse.Logic.Configuration;


public struct ColorPair
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

internal class AppConfig(ConsoleColors colors)
{
    private const string FileName = "config.json";
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        IncludeFields = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ConsoleColors Colors = colors;
    public static AppConfig Default => new(new(
            Console.ForegroundColor == ConsoleColor.White ? ConsoleColor.Gray : ConsoleColor.White,
            Console.BackgroundColor,
            Console.ForegroundColor == ConsoleColor.Cyan ? ConsoleColor.Yellow : ConsoleColor.Cyan,
            Console.BackgroundColor,
            Console.ForegroundColor == ConsoleColor.Red ? ConsoleColor.Green : ConsoleColor.Red,
            Console.BackgroundColor));

    public static void Save(AppConfig config)
    {
        string json = JsonSerializer.Serialize(config, JsonOptions);
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
        return JsonSerializer.Deserialize<AppConfig>(json, JsonOptions) ?? Default;
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
}
