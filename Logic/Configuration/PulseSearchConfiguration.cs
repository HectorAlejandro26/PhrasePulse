using System.CommandLine.Parsing;
using System.Text;
using System.Text.RegularExpressions;

namespace PhrasePulse.Logic.Configuration;

[Flags]
public enum PulseOptions
{
    None = 0,
    CaseInsensitive = 1 << 0,
    UseRegex = 1 << 1,
    AllowOverlapping = 1 << 3,
    HidePhrase = 1 << 4,
    HideIndexes = 1 << 5,
    NoColor = 1 << 6,

    // Combinaciones comunes
    Default = None,
    BasicSearch = CaseInsensitive | AllowOverlapping,
    AdvancedSearch = CaseInsensitive | UseRegex
}

internal class PulseSearchConfiguration
{
    public bool IgnoreCase { get; internal set; }
    public bool UseRegex { get; internal set; }
    public bool AllowOverlapping { get; internal set; }
    public bool HidePhrase { get; internal set; }
    public bool HideIndexes { get; internal set; }
    public bool NoColor { get; internal set; }
    public int MatchTimeoutSeconds { get; internal set; } = 3;
    public RegexOptions RegexOptions { get; internal set; }

    public string SearchPattern { get; }
    public string? InputText { get; }
    public FileInfo? InputFile { get; }
    public Encoding Encoding { get; } = Encoding.UTF8;

    public PulseSearchConfiguration(
        string searchPattern,
        string? inputText = null,
        FileInfo? inputFile = null,
        PulseOptions options = PulseOptions.Default,
        RegexOptions regexOptions = RegexOptions.None,
        int? matchTimeoutSeconds = null,
        string? encoding = null)
    {
        SearchPattern = searchPattern ?? throw new ArgumentNullException(nameof(searchPattern));
        InputText = inputText;
        InputFile = inputFile;

        // Validación básica
        if (inputText == null && inputFile == null)
            throw new ArgumentException("Either input text or input file must be provided");

        // Configurar opciones
        IgnoreCase = options.HasFlag(PulseOptions.CaseInsensitive);
        UseRegex = options.HasFlag(PulseOptions.UseRegex);
        AllowOverlapping = options.HasFlag(PulseOptions.AllowOverlapping);
        HidePhrase = options.HasFlag(PulseOptions.HidePhrase);
        HideIndexes = options.HasFlag(PulseOptions.HideIndexes);
        NoColor = options.HasFlag(PulseOptions.NoColor);

        // Configurar timeout
        MatchTimeoutSeconds = matchTimeoutSeconds ?? 3;
        if (MatchTimeoutSeconds <= 0)
            throw new ArgumentOutOfRangeException(nameof(matchTimeoutSeconds), "Timeout must be positive");

        // Configurar encoding
        if (encoding != null)
        {
            try
            {
                Encoding = Encoding.GetEncoding(encoding);
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentException("Invalid encoding specified", nameof(encoding), ex);
            }
        }

        // Configurar RegexOptions
        RegexOptions = regexOptions | (IgnoreCase ? RegexOptions.IgnoreCase : RegexOptions.None);
    }

    public static PulseSearchConfiguration Bind(ParseResult parseResult)
    {
        var searchPattern = parseResult.GetValueForArgument(CommandOptions.SearchPatternArgument);
        var inputText = parseResult.GetValueForOption(CommandOptions.InputTextOption);
        var inputFile = parseResult.GetValueForOption(CommandOptions.InputFileOption);
        var options = parseResult.GetValueForOption(CommandOptions.SearchOptionsOption);
        var regexOptions = parseResult.GetValueForOption(CommandOptions.RegexOptionsOption);
        var timeout = parseResult.GetValueForOption(CommandOptions.MatchTimeoutOption);
        var encoding = parseResult.GetValueForOption(CommandOptions.EncodingOption);

        return new(
            searchPattern,
            inputText,
            inputFile,
            options,
            regexOptions,
            timeout,
            encoding
        );
    }
}
