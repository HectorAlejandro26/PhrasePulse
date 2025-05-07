using System.CommandLine;
using System.Text.RegularExpressions;

namespace PhrasePulse.Logic.Configuration;

internal static class CommandOptions
{
    public static readonly Argument<string> SearchPatternArgument = new(
        name: "pattern",
        description: "The search pattern to look for in the input",
        parse: result =>
        {
            var value = result.Tokens[0].Value;
            if (string.IsNullOrWhiteSpace(value))
            {
                result.ErrorMessage = "Search pattern cannot be empty";
                return null;
            }
            return value;
        })
    {
        Arity = ArgumentArity.ExactlyOne
    };

    public static readonly Option<string?> InputTextOption = new(
        aliases: ["--text", "-t"],
        description: "The input text to search in")
    {
        ArgumentHelpName = "STRING",
    };

    public static readonly Option<FileInfo?> InputFileOption = new(
        aliases: ["--file", "-f"],
        description: "The input file to search in")
    {
        ArgumentHelpName = "FILE",
    };

    public static readonly Option<PulseOptions> SearchOptionsOption = new(
        aliases: ["--options", "-o"],
        description: "Search options as flags combination",
        getDefaultValue: () => PulseOptions.Default);

    public static readonly Option<RegexOptions> RegexOptionsOption = new(
        aliases: ["--regex-options", "-r"],
        description: "Additional regex options when using regex search",
        getDefaultValue: () => RegexOptions.None);

    public static readonly Option<int?> MatchTimeoutOption = new(
        aliases: ["--timeout", "-to"],
        description: "Regex match timeout in seconds (default: 3)");

    public static readonly Option<string?> EncodingOption = new(
        aliases: ["--encoding", "-e"],
        description: "Text encoding to use (default: utf-8)");
}