using MelonLoader;


[assembly: MelonGame("Harmonix Music Systems, Inc.", "Audica")]
[assembly: MelonInfo(typeof(ModSettings), "Mod Settings", "0.1.0", "octo", "https://github.com/octoberU/ModSettings")]

public class ModSettings : MelonMod
{
    public override void OnLevelWasInitialized(int level)
    {
        if (level != 2) return;
        var prefs = MelonPrefs.GetPreferences();

        foreach (var category in prefs)
        {
            MelonLogger.Log("Found mod: " + category.Key);
            foreach (var pref in category.Value)
            {
                MelonLogger.Log("Found pref:" + pref.Key);
            }
        }
    }

}


