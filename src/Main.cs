using MelonLoader;
using System.Reflection;

[assembly: AssemblyVersion(ModSettings.VERSION)]
[assembly: AssemblyFileVersion(ModSettings.VERSION)]
[assembly: MelonGame("Harmonix Music Systems, Inc.", "Audica")]
[assembly: MelonInfo(typeof(ModSettings), "Mod Settings", ModSettings.VERSION, "octo", "https://github.com/octoberU/ModSettings")]

public class ModSettings : MelonMod
{
    public const string VERSION = "0.1.4";
}


