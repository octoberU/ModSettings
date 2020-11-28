# Audica Mod Settings

This mod lets players change mod settings from within the game.

## To modders
For a quick start you can use [this config](https://github.com/octoberU/ScoreOverlay/blob/master/Config.cs) as an example.

**Min Max Step Default**  
MelonPrefs don't have a way to set step values or default values. You'll have to use the display string to set them.

Format:  
`[0,5,0.2,1.4]`  
`[Min,Max,Step,Default]`  

Example:   
```cs
MelonPrefs.RegisterFloat(Category, nameof(OverlayScale), 1.4f, "Changes the scale of the overlay [0,5,0.2,1.4]");
```
  
**Important**  
These values will be hidden ingame. `int` and `float` prefs will be ignored unless they have `MinMaxStepDefault` values.

### Updating values in your mod
`OnModSettingsApplied` will get called everytime a value is changed. You might have to update objects within your mod, make sure to do it in the `OnModSettingsApplied` method.