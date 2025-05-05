using System.Diagnostics;
using System.Text.RegularExpressions;

namespace PhrasePulse.Logic.Configuration;

public enum PulseOptions
{
    Default = 0b00000000,
    CaseInsensitive = 0b00000001,
    UseRegex = 0b00000010,
    NoColor = 0b00000100,
    AllowOverlapping = 0b00001000,
    ShowPhraseColored = 0b00010000
}

[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
internal readonly struct PulseOptionsManager
{
    public readonly bool UsingDefault;
    public readonly bool IgnoreCase;
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
        UseRegex = (options & PulseOptions.UseRegex) == PulseOptions.UseRegex;
        NoColor = (options & PulseOptions.NoColor) == PulseOptions.NoColor;
        AllowOverlapping = (options & PulseOptions.AllowOverlapping) == PulseOptions.AllowOverlapping;

        if (matchTimeout < 0)
        {
            throw new PhrasePulseException(
                "If null, the default value of 3 seconds will be used.",
                new RegexMatchTimeoutException(
                    "Match timeout cannot be negative. Use null to set the default value."
                )
            );
        }
        MatchTimeout = matchTimeout ?? 3;
    }

    public static PulseOptionsManager Default => new(PulseOptions.Default);

    public override string ToString()
    {
        if (UsingDefault) return "Default";

        List<string> options = [];
        if (IgnoreCase) options.Add("CaseInsensitive");
        if (UseRegex) options.Add("UseRegex");
        if (NoColor) options.Add("NoColor");
        if (AllowOverlapping) options.Add("AllowOverlapping");

        return string.Join(" | ", options);
    }

    private string GetDebuggerDisplay() => ToString();
}
