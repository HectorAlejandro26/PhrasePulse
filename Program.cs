using System.Text;
using PhrasePulse.Logic;
using PhrasePulse.Logic.Configuration;

namespace PhrasePulse;
internal class Program
{
    static void Main(string[] args)
    {
        var config = AppConfig.Load();

        Pulse pulse = new(
            "Un dia vi una vaca (sin cola (vestida) de (uniforme))",
            PulseOptions.CaseInsensitive | PulseOptions.UseRegex,
            Encoding.UTF8,
            config
        );
        pulse.Start(@"\(.*\).+\(.*\)");

        Console.Write(pulse.IndexesToString());
        Console.WriteLine(pulse.PhraseColored);
    }
}
