using PhrasePulse.Logic;
using PhrasePulse.Logic.Configuration;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;

namespace PhrasePulse;

internal class Program
{
    public static int Main(string[] args)
    {
        // Configuración del comando raíz
        RootCommand rootCommand = new("PhrasePulse text search tool");

        rootCommand.AddArgument(CommandOptions.SearchPatternArgument);
        rootCommand.AddOption(CommandOptions.InputTextOption);
        rootCommand.AddOption(CommandOptions.InputFileOption);
        rootCommand.AddOption(CommandOptions.SearchOptionsOption);
        rootCommand.AddOption(CommandOptions.RegexOptionsOption);
        rootCommand.AddOption(CommandOptions.MatchTimeoutOption);
        rootCommand.AddOption(CommandOptions.EncodingOption);

        // Configuración del manejador del comando
        rootCommand.SetHandler((InvocationContext context) =>
        {
            try
            {
                var config = PulseSearchConfiguration.Bind(context.ParseResult);
                if (!Utils.TerminalSupportsColor(context)) config.NoColor = true;

                var appConfig = AppConfig.Load();

                Pulse pulse = new(config);

                int found = pulse.FindMatches() ? 0 : 1;

                if (!config.HidePhrase)
                { 
                    Console.WriteLine(pulse.HighlightedPhrase);
                }
                if (!config.HideIndexes)
                {
                    Console.WriteLine(pulse.HighlightedIndexes);
                }
                context.ExitCode = found;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
                context.ExitCode = -1;
            }
            finally
            {
                Console.ResetColor();
            }
        });

        return rootCommand.Invoke(args);
    }
}

/* Ejemplo de código (Falla)
rootCommand.SetHandler((reque, file, phrase, ignoreCase, useRegex, allowOverlapping,
                              matchTimeout, encodingName, hidePhrase, hideIndexes, context) =>
        {
            try
            {
                // Validaciones básicas
                if (reque == null)
                {
                    Console.WriteLine("Error: The search pattern (reque) is required.");
                    context.ExitCode = 1;
                    return;
                }

                if (phrase == null && file == null)
                {
                    Console.WriteLine("Error: You must provide either --phrase or a file argument.");
                    context.ExitCode = 1;
                    return;
                }

                // Configurar encoding
                Encoding encoding;
                try
                {
                    encoding = encodingName == null
                        ? Encoding.UTF8
                        : Encoding.GetEncoding(encodingName);
                }
                catch (ArgumentException)
                {
                    Console.WriteLine($"Error: Unsupported encoding '{encodingName}'.");
                    context.ExitCode = 1;
                    return;
                }

                // Construir opciones de Pulse
                PulseOptions options = PulseOptions.Default;
                if (ignoreCase) options |= PulseOptions.CaseInsensitive;
                if (useRegex) options |= PulseOptions.UseRegex;
                if (allowOverlapping) options |= PulseOptions.AllowOverlapping;
                if (hidePhrase) options |= PulseOptions.HidePhrasePhrase;
                if (hideIndexes) options |= PulseOptions.HideIndexes;

                // Crear instancia de Pulse
                Pulse pulse;
                if (file != null)
                {
                    pulse = Pulse.FromFile(file, options, encoding);
                }
                else
                {
                    pulse = new Pulse(phrase!, options, encoding);
                }

                // Ejecutar búsqueda
                bool found = pulse.Start(reque);

                // Mostrar resultados
                if (!pulse.Options.HidePhrase && pulse.PhraseColored != null)
                {
                    Console.WriteLine(pulse.PhraseColored);
                }

                if (!pulse.Options.HideIndexes)
                {
                    Console.WriteLine(pulse.IndexesToString());
                }

                context.ExitCode = found ? 0 : 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                context.ExitCode = 1;
            }
        },
        requeArgument, fileArgument, phraseOption, ignoreCaseOption, useRegexOption,
        allowOverlappingOption, matchTimeoutOption, encodingOption,
        hidePhraseOption, hideIndexesOption);
*/