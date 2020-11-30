# Audica Mod Settings

This mod lets players change mod settings from within the game.

![Banner](https://i.imgur.com/JVbzWPv.png "Banner")

## To modders
For a quick start you can use [this config](https://github.com/octoberU/ScoreOverlay/blob/master/Config.cs) as an example.

 
### **Min Max Step Default**  
MelonPrefs don't have a way to set step values or default values. You'll have to use the display string to set them.

Format:  
`[0,5,0.2,1.4]`  
`[Min,Max,Step,Default]`  

Example:   
```cs
MelonPrefs.RegisterFloat(Category, nameof(OverlayScale), 1.4f, "Changes the scale of the overlay [0,5,0.2,1.4]");
```

### **Format Specifier (Optional)**  
If you want the display to use a specific format, you can add [format specifiers](https://docs.microsoft.com/en-us/dotnet/standard/base-types/standard-numeric-format-strings) to your display string. The default format specifier is N3.


Format:  
`{N3}`  
`{FormatSpecifier}`  

Example:   
```cs
MelonPrefs.RegisterFloat(Category, nameof(OverlayScale), 1.4f, "Changes the scale of the overlay [0,5,0.2,1.4] {N3}");
```

### **Headers (Optional)**  
You can create headers by using string prefs. If the string pref display string contains`[Header]`, it will display ingame as a divider.


Format:  
`[Header]`

Example:   
```cs
MelonPrefs.RegisterString(Category, nameof(SpeedSection), "", "[Header]Speed options");
```
  
**Important**  
These values will be hidden ingame. `int` and `float` prefs will be ignored unless they have `MinMaxStepDefault` values.

### Updating values in your mod
`OnModSettingsApplied` will get called everytime a value is changed. You might have to update objects within your mod, make sure to do it in the `OnModSettingsApplied` method.
