using System.Diagnostics;

namespace PhrasePulse.Logic.Configuration;

public enum PulseOptions
{
    Default = 0b0000,
    CaseInsensitive = 0b0001,
    CompleteWords = 0b0010,
    UseRegex = 0b0100,
    NoColor = 0b1000,
    AllowOverlapping = 0b10000
}

[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
internal readonly struct PulseOptionsManager
{
    public readonly bool UsingDefault;
    public readonly bool IgnoreCase;
    public readonly bool FullWords;
    public readonly bool UseRegex;
    public readonly bool NoColor;
    public readonly bool AllowOverlapping;
    /// <summary>
    /// Timeout in seconds for regex matches.
    /// Default is 3 seconds.
    /// </summary>
    public readonly int MatchTimeout;

    public PulseOptionsManager(PulseOptions options, int? matchTimeout = null)
    {
        UsingDefault = options == PulseOptions.Default;

        IgnoreCase = (options & PulseOptions.CaseInsensitive) == PulseOptions.CaseInsensitive;
        FullWords = (options & PulseOptions.CompleteWords) == PulseOptions.CompleteWords;
        UseRegex = (options & PulseOptions.UseRegex) == PulseOptions.UseRegex;
        NoColor = (options & PulseOptions.NoColor) == PulseOptions.NoColor;
        AllowOverlapping = (options & PulseOptions.AllowOverlapping) == PulseOptions.AllowOverlapping;
        MatchTimeout = matchTimeout ?? 3;

        if (UseRegex && FullWords)
        {
            throw new PhrasePulseException(
                "Please use only one of these options at a time.",
                new ArgumentException("UseRegex cannot be used with CaseInsensitive options.")
            );
        }
    }

    public static PulseOptionsManager Default => new(PulseOptions.Default);

    public override string ToString()
    {
        if (UsingDefault) return "Default";

        List<string> options = [];
        if (IgnoreCase) options.Add("CaseInsensitive");
        if (FullWords) options.Add("CompleteWords");
        if (UseRegex) options.Add("UseRegex");
        if (NoColor) options.Add("NoColor");
        if (AllowOverlapping) options.Add("AllowOverlapping");

        return string.Join(" | ", options);
    }

    private string GetDebuggerDisplay() => ToString();
}
