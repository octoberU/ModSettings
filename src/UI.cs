using Harmony;
using MelonLoader;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using Il2CppGeneric = Il2CppSystem.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

public static class UI
{
    static GameObject modMenu;
    static ShellPage modMenuSP;
    static OptionsMenu modMenuOM;
    static GunButton backButton;
    static ShellScrollable scroller;
    static GunButton modSettingsButton;
    static TextMeshPro modSettingsButtonLabel;
    static DisplayState displayState = DisplayState.Categories;

    public static void Initialize()
    {
        if (modMenu == null)
        {
            modMenu = GameObject.Instantiate(GameObject.Find("menu/ShellPage_Settings"));
            modMenu.hideFlags |= HideFlags.DontUnloadUnusedAsset;
            modMenuSP = modMenu.GetComponent<ShellPage>();
            modMenuOM = modMenu.transform.Find("page/ShellPanel_Center/Settings/Options").GetComponent<OptionsMenu>();
            scroller = modMenuOM.gameObject.GetComponent<ShellScrollable>();
            backButton = modMenu.transform.Find("page/backParent/back/Button").GetComponent<GunButton>();
            backButton.onHitEvent = new UnityEvent();
            backButton.onHitEvent.AddListener(new Action(() => 
            {
                if (displayState == DisplayState.Prefs) PreparePage();
                else HideModSettingsMenu(); 
            }));
        }
    }

    private static void AddModSettingsButton()
    {
        modSettingsButton = GameObject.Find("menu/ShellPage_Main/page/ShellPanel_Center/Party/Button").GetComponent<GunButton>();
        modSettingsButtonLabel = GameObject.Find("menu/ShellPage_Main/page/ShellPanel_Center/Party/Label").GetComponent<TextMeshPro>();
        GameObject.Destroy(modSettingsButtonLabel.gameObject.GetComponent<Localizer>());
        modSettingsButtonLabel.text = "Mod Settings";
        modSettingsButton.onHitEvent = new UnityEvent();
        modSettingsButton.onHitEvent.AddListener(new Action(() => 
        {
            OpenModSettingsMenu(); 
        }));
    }

    private static void OpenModSettingsMenu()
    {
        MenuState.I.mainPage.SetPageActive(false, false);
        modMenuSP.SetPageActive(false, true);
        MelonCoroutines.Start(WaitForPageOpen(PreparePage));
    }

    private static void PreparePage()
    {
        modMenuOM.ShowPage(OptionsMenu.Page.Customization);
        WipeScroller();
        modMenuOM.screenTitle.text = "Mod Settings";
        AddCategories();
        displayState = DisplayState.Categories;
    }

    private static void AddCategories()
    {
        var prefs = MelonPrefs.GetPreferences();

        int buttonIndex = 0;
        Il2CppGeneric.List<GameObject> row = new Il2CppGeneric.List<GameObject>();
        foreach (var category in prefs)
        {
            var categoryButton = modMenuOM.AddButton(buttonIndex % 2,
                AddWhitespace(category.Key),
                new Action(() => { CreateCategoryPage(category); }),
                null,
                "");
            buttonIndex++;
            row.Add(categoryButton.gameObject);
            if (row.Count == 2)
            {
                //This is the dumbest code I've ever wrote.
                Il2CppGeneric.List<GameObject> tempRow = new Il2CppGeneric.List<GameObject>();
                tempRow.Add(row[0]);
                tempRow.Add(row[1]);
                modMenuOM.scrollable.AddRow(tempRow);
                row.Clear();
            }
        }
        if (row.Count == 1) //If the last row is missing a pair, add a row with a single object.
        {
            modMenuOM.scrollable.AddRow(row[0]);
            row.Clear();
        }
    }

    private static void CreateCategoryPage(KeyValuePair<string, Dictionary<string, MelonPrefs.MelonPreference>> category)
    {
        WipeScroller();
        displayState = DisplayState.Prefs;
        var categoryHeader = modMenuOM.AddHeader(0, category.Key);
        modMenuOM.scrollable.AddRow(categoryHeader);

        int buttonIndex = 0;
        Il2CppGeneric.List<GameObject> row = new Il2CppGeneric.List<GameObject>();
        foreach (var pref in category.Value)
        {
            switch (pref.Value.Type)
            {
                case MelonPrefs.MelonPreferenceType.INT:
                    MinMaxStepDefaultInt rangesInt = ParseMinMaxStepInt(pref.Value.DisplayText);
                    if (rangesInt.Equals(default(MinMaxStepDefaultInt))) break;
                    var intSlider = modMenuOM.AddSlider(buttonIndex % 2,
                        AddWhitespace(pref.Key),
                        null,
                        new Action<float>((amount) =>
                        {
                            int currentVal = MelonPrefs.GetInt(category.Key, pref.Key);
                            int increment = (int)amount * rangesInt.step;
                            int newVal = currentVal + increment;
                            if (newVal > rangesInt.max) MelonPrefs.SetInt(category.Key, pref.Key, rangesInt.max);
                            else if (newVal < rangesInt.min) MelonPrefs.SetInt(category.Key, pref.Key, rangesInt.min);
                            else
                            {
                                MelonPrefs.SetInt(category.Key, pref.Key, newVal);
                            }
                        }),
                        new Func<float>(() => { return (float)MelonPrefs.GetInt(category.Key, pref.Key); }),
                        new Action(() => { MelonPrefs.SetFloat(category.Key, pref.Key, rangesInt.prefDefault); }),
                        RemoveTags(pref.Value.DisplayText),
                        new Func<float, string>((amount) => { return amount.ToString(); }));
                    buttonIndex++;
                    row.Add(intSlider.gameObject);
                    break;

                case MelonPrefs.MelonPreferenceType.BOOL:
                    var checkbox = modMenuOM.AddButton(buttonIndex % 2,
                        AddWhitespace(pref.Key),
                        new Action(() =>
                        {
                            bool currentVal = MelonPrefs.GetBool(category.Key, pref.Key);
                            MelonPrefs.SetBool(category.Key, pref.Key, !currentVal);
                        }),
                        new Func<bool>(() => { return MelonPrefs.GetBool(category.Key, pref.Key); }),
                        pref.Value.DisplayText);

                    row.Add(checkbox.gameObject);
                    buttonIndex++;
                    break;

                case MelonPrefs.MelonPreferenceType.FLOAT:
                    MinMaxStepDefault rangesFloat = ParseMinMaxStep(pref.Value.DisplayText);
                    if (rangesFloat.Equals(default(MinMaxStepDefault))) break;
                    var customSpecifier = GetFormatSpecifier(pref.Value.DisplayText);
                    if (customSpecifier == "") customSpecifier = "N2"; //Default to N2 if specifier is missing
                    var floatSlider = modMenuOM.AddSlider(buttonIndex % 2,
                        AddWhitespace(pref.Key),
                        "N2",
                        new Action<float>((amount) =>
                        {
                            float currentVal = MelonPrefs.GetFloat(category.Key, pref.Key);
                            float increment = rangesFloat.step * amount; //(amount * Mathf.Floor(currentVal * 10f));
                            float newVal = currentVal + increment;
                            if (newVal > rangesFloat.max) MelonPrefs.SetFloat(category.Key, pref.Key, rangesFloat.max);
                            else if (newVal < rangesFloat.min) MelonPrefs.SetFloat(category.Key, pref.Key, rangesFloat.min);
                            else
                            {
                                MelonPrefs.SetFloat(category.Key, pref.Key, currentVal + increment);
                            }
                        }),
                        new Func<float>(() => { return MelonPrefs.GetFloat(category.Key, pref.Key); }),
                        new Action(() => { MelonPrefs.SetFloat(category.Key, pref.Key, rangesFloat.prefDefault); }),
                        RemoveTags(pref.Value.DisplayText),
                        new Func<float, string>((amount) => { return amount.ToString(customSpecifier); }));
                    row.Add(floatSlider.gameObject);
                    buttonIndex++;
                    break;

                case MelonPrefs.MelonPreferenceType.STRING:
                    
                    if (pref.Value.DisplayText.ToLower().Contains("[header]"))
                    {
                        if(row.Count == 1)
                        {
                            modMenuOM.scrollable.AddRow(row[0]);
                            row.Clear();
                        }
                        var header = modMenuOM.AddHeader(0, RemoveTags(pref.Value.DisplayText));
                        modMenuOM.scrollable.AddRow(header);
                        buttonIndex = 0;
                    }
                    break;

                default:
                    break;
            }
            if (row.Count == 2)
            {
                //This is the dumbest code I've ever wrote.
                Il2CppGeneric.List<GameObject> tempRow = new Il2CppGeneric.List<GameObject>();
                tempRow.Add(row[0]);
                tempRow.Add(row[1]);
                modMenuOM.scrollable.AddRow(tempRow);
                row.Clear();
            }
            else if (buttonIndex == category.Value.Count && buttonIndex % 2 == 1) // This might be obsolete
            {
                modMenuOM.scrollable.AddRow(row[0]);
                row.Clear();
            }
        }
    }

    private static void HideModSettingsMenu()
    {
        MenuState.I.mainPage.SetPageActive(true, false);
        modMenuSP.SetPageActive(false, false);
        MelonPrefs.SaveConfig();
    }

    public static void WipeScroller()
    {
        Transform optionsTransform = modMenuOM.transform;
        for (int i = 0; i < optionsTransform.childCount; i++)
        {
            Transform child = optionsTransform.GetChild(i);
            if (child.gameObject.name.Contains("(Clone)"))
            {
                GameObject.Destroy(child.gameObject);
            }
        }
        modMenuOM.mRows.Clear();
        modMenuOM.scrollable.ClearRows();
        modMenuOM.scrollable.mRows.Clear();
        modMenuOM.scrollable.mIndex = 0;
        modMenuOM.scrollable.destroyChildren = true;
    }

    [HarmonyPatch(typeof(MenuState), "SetState", new Type[] { typeof(MenuState.State) })]
    private static class ScoreUpdater
    {
        private static void Postfix(MenuState __instance, MenuState.State state)
        {
            if (state == MenuState.State.MainPage)
            {
                AddModSettingsButton();
                Initialize();
            }
        }
    }

    static IEnumerator WaitForPageOpen(Action callback)
    {
		while (modMenuOM.transform.childCount < 5) {
			yield return null;
	    }
	    callback.Invoke();
    }

    public static MinMaxStepDefault ParseMinMaxStep(string input)
    {
        if (input.Contains("[") && input.Contains("]"))
        {
            float min;
            float max;
            float step;
            float prefDefault;
            string[] split = input.Split(new char[] { '[', ',', ']' });
            float.TryParse(split[1], NumberStyles.Any, CultureInfo.InvariantCulture, out min);
            float.TryParse(split[2], NumberStyles.Any, CultureInfo.InvariantCulture, out max);
            float.TryParse(split[3], NumberStyles.Any, CultureInfo.InvariantCulture, out step);
            float.TryParse(split[4], NumberStyles.Any, CultureInfo.InvariantCulture, out prefDefault);
            return new MinMaxStepDefault(min, max, step, prefDefault);
        }
        else return default;
    }

    public static MinMaxStepDefaultInt ParseMinMaxStepInt(string input)
    {
        if (input.Contains("[") && input.Contains("]"))
        {
            int min;
            int max;
            int step;
            int prefDefault;
            string[] split = input.Split(new char[] { '[', ',', ']' });
            int.TryParse(split[1], NumberStyles.Any, CultureInfo.InvariantCulture, out min);
            int.TryParse(split[2], NumberStyles.Any, CultureInfo.InvariantCulture, out max);
            int.TryParse(split[3], NumberStyles.Any, CultureInfo.InvariantCulture, out step);
            int.TryParse(split[4], NumberStyles.Any, CultureInfo.InvariantCulture, out prefDefault);
            return new MinMaxStepDefaultInt(min, max, step, prefDefault);
        }
        else return default;
    }

    public struct MinMaxStepDefault
    {
        public float min;
        public float max;
        public float step;
        public float prefDefault;
        public MinMaxStepDefault(float min, float max, float step, float prefDefault)
        {
            this.min = min;
            this.max = max;
            this.step = step;
            this.prefDefault = prefDefault;
        }
    }

    public struct MinMaxStepDefaultInt
    {
        public int min;
        public int max;
        public int step;
        public int prefDefault;
        public MinMaxStepDefaultInt(int min, int max, int step, int prefDefault)
        {
            this.min = min;
            this.max = max;
            this.step = step;
            this.prefDefault = prefDefault;
        }
    }

    public static string RemoveTags(string input)
    {
        Regex rx = new Regex(@"[\[{][^\[]*[\]}]");
        return rx.Replace(input, "");
    }

    public static string AddWhitespace(string input)
    {
        return string.Join(" ", Regex.Split(input, @"(?<!^)(?=[A-Z](?![A-Z]|$))"));
    }

    public static string GetFormatSpecifier(string input)
    {
        if (input.Contains("{") && input.Contains("}"))
        {
            return input.Split(new char[] { '{', '}' })[1];
        }
        else return "";
    }

    enum DisplayState
    {
        Categories,
        Prefs,
    }
}
