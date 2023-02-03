using JetBrains.Annotations;
using UnityEngine.EventSystems;
using UMenuButton = UnityEngine.UI.MenuButton;
using Satchel;

namespace HKVocals.UI;
public static class SettingsPrompt
{
    [CanBeNull] private static GameObject PromptButton;
    public static void CreatePrompt()
    {
        if (HKVocals._globalSettings.settingsOpened) return;
        
        var SavesProfileScreen = UIManager.instance.UICanvas.Find("SaveProfileScreen");
        var backButton = SavesProfileScreen.Find("Controls").Find("BackButton");

        PromptButton = Object.Instantiate(backButton, SavesProfileScreen.transform);
        PromptButton.name = "HKV Settings Prompt";

        PromptButton.transform.localPosition = Vector3.down * 700;

        PromptButton.RemoveComponent<EventTrigger>();
        
        PromptButton.Find("Text").RemoveComponent<AutoLocalizeTextUI>();
        PromptButton.Find("Text").GetComponent<Text>().text = "We recommend reviewing your audio settings before playing Hallownest Vocalized";
        var mb = PromptButton.GetComponent<UMenuButton>();
        mb.proceed = true;
        mb.buttonType = UMenuButton.MenuButtonType.CustomSubmit;
        mb.submitAction = _ =>
        {
            UIManager.instance.StartCoroutine(GoToModMenu());
        };
    }

    private static IEnumerator GoToModMenu()
    {
        yield return UIManager.instance.HideSaveProfileMenu();
        ModMenu.GoToModMenu();
    }

    public static void HookRemoveButton()
    {
        if (HKVocals._globalSettings.settingsOpened) return;
        
        OnUIManager.BeforeOrig.ShowMenu += RemoveButton;
    }

    private static void RemoveButton(OnUIManager.Delegates.Params_ShowMenu args)
    {
        if (ModMenu.MenuRef.menuScreen != null && ModMenu.MenuRef.menuScreen == args.menu)
        {
            HKVocals._globalSettings.settingsOpened = true;

            if (PromptButton != null)
            {
                Object.Destroy(PromptButton);
            }
            
            OnUIManager.BeforeOrig.ShowMenu -= RemoveButton; //hook no longer needed
        }
    }
}