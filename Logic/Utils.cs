using System.CommandLine.Invocation;

namespace PhrasePulse.Logic;

internal static class Utils
{
    public static bool TerminalSupportsColor(InvocationContext context)
    {
        if (context.Console.IsOutputRedirected) return false;

        // Para .NET 5+ en Windows
        if (OperatingSystem.IsWindows()) return Environment.OSVersion.Version.Major >= 10;

        // Para Unix/Linux
        var term = Environment.GetEnvironmentVariable("TERM");
        return !string.IsNullOrEmpty(term) && term != "dumb";
    }
    public static readonly string DefaultForeColor = "\x1b[39m";
    public static readonly string DefaultBackColor = "\x1b[49m";
    public static readonly string DefaultColor = "\x1b[0m";

    public static string ToForeColor(this ConsoleColor color)
    {
        return color switch
        {
            ConsoleColor.Black => "\x1b[30m",
            ConsoleColor.DarkBlue => "\x1b[34m",
            ConsoleColor.DarkGreen => "\x1b[32m",
            ConsoleColor.DarkCyan => "\x1b[36m",
            ConsoleColor.DarkRed => "\x1b[31m",
            ConsoleColor.DarkMagenta => "\x1b[35m",
            ConsoleColor.DarkYellow => "\x1b[33m",
            ConsoleColor.Gray => "\x1b[37m",
            ConsoleColor.DarkGray => "\x1b[90m",
            ConsoleColor.Blue => "\x1b[94m",
            ConsoleColor.Green => "\x1b[92m",
            ConsoleColor.Cyan => "\x1b[96m",
            ConsoleColor.Red => "\x1b[91m",
            ConsoleColor.Magenta => "\x1b[95m",
            ConsoleColor.Yellow => "\x1b[93m",
            ConsoleColor.White => "\x1b[97m",
            _ => DefaultForeColor
        };
    }
    
    public static string ToBackColor(this ConsoleColor color)
    {
        return color switch
        {
            ConsoleColor.Black => "\x1b[40m",
            ConsoleColor.DarkBlue => "\x1b[44m",
            ConsoleColor.DarkGreen => "\x1b[42m",
            ConsoleColor.DarkCyan => "\x1b[46m",
            ConsoleColor.DarkRed => "\x1b[41m",
            ConsoleColor.DarkMagenta => "\x1b[45m",
            ConsoleColor.DarkYellow => "\x1b[43m",
            ConsoleColor.Gray => "\x1b[47m",
            ConsoleColor.DarkGray => "\x1b[100m",
            ConsoleColor.Blue => "\x1b[104m",
            ConsoleColor.Green => "\x1b[102m",
            ConsoleColor.Cyan => "\x1b[106m",
            ConsoleColor.Red => "\x1b[101m",
            ConsoleColor.Magenta => "\x1b[105m",
            ConsoleColor.Yellow => "\x1b[103m",
            ConsoleColor.White => "\x1b[107m",
            _ => DefaultBackColor
        };
    }

}
