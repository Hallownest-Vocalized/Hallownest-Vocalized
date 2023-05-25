using UnityEngine.EventSystems;
using UMenuButton = UnityEngine.UI.MenuButton;
using Satchel;

namespace HKVocals.UI;
public static class ExtrasMenu
{

    private static GameObject HKVocalsCreditsButton;
    public static void AddCreditsButton()
    {
        GameObject HKVocalsCreditsHolder = Object.Instantiate(RefVanillaMenu.CreditButtonHolder, RefVanillaMenu.CreditButtonHolder.transform.parent);
        HKVocalsCreditsHolder.name = "HK Vocal Credits";
        HKVocalsCreditsHolder.transform.SetSiblingIndex(2);

        HKVocalsCreditsButton = HKVocalsCreditsHolder.Find("CreditsButton").gameObject;
        HKVocalsCreditsButton.RemoveComponent<EventTrigger>();
        HKVocalsCreditsButton.RemoveComponent<ContentSizeFitter>();
        HKVocalsCreditsButton.RemoveComponent<AutoLocalizeTextUI>();
        HKVocalsCreditsButton.Find("Text").RemoveComponent<AutoLocalizeTextUI>();
        HKVocalsCreditsButton.Find("Text").GetComponent<Text>().text = "Hallownest Vocalized Credits";
        var mb = HKVocalsCreditsButton.GetComponent<UMenuButton>();
        mb.proceed = true;
        mb.buttonType = UMenuButton.MenuButtonType.CustomSubmit;
        mb.submitAction = _ => HKVocals.CoroutineHolder.StartCoroutine(MajorFeatures.RollCredits.LoadCreditsFromMenu());
        
        //fix navigation
        On.UIManager.ShowMenu -= ShowMenu;
        On.UIManager.ShowMenu += ShowMenu;
    }

    private static IEnumerator ShowMenu(On.UIManager.orig_ShowMenu orig, UIManager self, MenuScreen menu)
    {
        yield return orig(self, menu);
        if (menu.name == "ExtrasMenuScreen")
        {
            var hkvCreditsButtonSelectable = HKVocalsCreditsButton.GetComponent<Selectable>();
            var creditsButtonSelectable =
                RefVanillaMenu.CreditButtonHolder.Find("CreditsButton").GetComponent<Selectable>();
            var pack1Selectable = RefVanillaMenu.PackDetailButtons[0].Find("DetailsButton").GetComponent<Selectable>();

            creditsButtonSelectable.navigation = creditsButtonSelectable.navigation with
            {
                selectOnDown = hkvCreditsButtonSelectable
            };

            hkvCreditsButtonSelectable.navigation = new Navigation()
            {
                selectOnDown = pack1Selectable,
                selectOnUp = creditsButtonSelectable,
                mode = Navigation.Mode.Explicit
            };

            pack1Selectable.navigation = pack1Selectable.navigation with
            {
                selectOnUp = hkvCreditsButtonSelectable
            };
        }
    }
}
