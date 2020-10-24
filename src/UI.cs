using Harmony;
using MelonLoader;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public static class UI
{
    static GameObject modMenu;
    static ShellPage modMenuSP;
    static OptionsMenu modMenuOM;
    static GunButton backButton;
    static ShellScrollable scroller;

    static GunButton modSettingsButton;
    static TextMeshPro modSettingsButtonLabel;

    public static void Initialize()
    {
        if(modMenu == null)
        {
            modMenu = GameObject.Instantiate(GameObject.Find("menu/ShellPage_Settings"));
            modMenu.hideFlags |= HideFlags.DontUnloadUnusedAsset;
            modMenuSP = modMenu.GetComponent<ShellPage>();
            modMenuOM = modMenu.transform.Find("page/ShellPanel_Center/Settings/Options").GetComponent<OptionsMenu>();
            scroller = modMenuOM.gameObject.GetComponent<ShellScrollable>();
            backButton = modMenu.transform.Find("page/backParent/back/Button").GetComponent<GunButton>();
            backButton.onHitEvent = new UnityEvent();
            backButton.onHitEvent.AddListener(new Action(() => { HideModSettingsMenu(); }));
        }
    }

    private static void AddModSettingsButton()
    {
        modSettingsButton = GameObject.Find("menu/ShellPage_Main/page/ShellPanel_Center/Party/Button").GetComponent<GunButton>();
        modSettingsButtonLabel = GameObject.Find("menu/ShellPage_Main/page/ShellPanel_Center/Party/Label").GetComponent<TextMeshPro>();
        GameObject.Destroy(modSettingsButtonLabel.gameObject.GetComponent<Localizer>());
        modSettingsButtonLabel.text = "Mod Settings";
        modSettingsButton.onHitEvent = new UnityEvent();
        modSettingsButton.onHitEvent.AddListener(new Action(() => { OpenModSettingsMenu(); }));
    }

    private static void OpenModSettingsMenu()
    {
        MenuState.I.mainPage.SetPageActive(false, false);
        modMenuSP.SetPageActive(true, false);
        modMenuOM.ShowPage(OptionsMenu.Page.Customization);
        WipeScroller();
        modMenuOM.screenTitle.text = "Mod Settings";
    }

    private static void HideModSettingsMenu()
    {
        MenuState.I.mainPage.SetPageActive(true, false);
        modMenuSP.SetPageActive(false, false);
    }

    public static void WipeScroller()
    {
        for (int i = 0; i < modMenuOM.transform.childCount; i++)
        {
            Transform child = modMenuOM.transform.GetChild(i);
            if(child.name.Contains("(Clone)")) GameObject.Destroy(child);

        }
        scroller.ClearRows();
    }

    [HarmonyPatch(typeof(MenuState), "SetState", new Type[] { typeof(MenuState.State)})]
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

}
