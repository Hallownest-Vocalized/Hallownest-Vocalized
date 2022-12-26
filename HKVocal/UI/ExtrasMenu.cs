using UnityEngine.EventSystems;
using UMenuButton = UnityEngine.UI.MenuButton;
using Satchel;

namespace HKVocals.UI;
public static class ExtrasMenu
{
    public static void AddCreditsButton()
    {
        GameObject HKVocalsCreditsHolder = Object.Instantiate(RefVanillaMenu.CreditButtonHolder, RefVanillaMenu.CreditButtonHolder.transform.parent);
        HKVocalsCreditsHolder.name = "HK Vocal Credits";
        HKVocalsCreditsHolder.transform.SetSiblingIndex(2);

        var HKVocalsCreditsButton = HKVocalsCreditsHolder.Find("CreditsButton").gameObject;
        HKVocalsCreditsButton.RemoveComponent<EventTrigger>();
        HKVocalsCreditsButton.RemoveComponent<ContentSizeFitter>();
        HKVocalsCreditsButton.RemoveComponent<AutoLocalizeTextUI>();
        HKVocalsCreditsButton.Find("Text").RemoveComponent<AutoLocalizeTextUI>();
        HKVocalsCreditsButton.Find("Text").GetComponent<Text>().text = "Hallownest Vocalized Credits";
        var mb = HKVocalsCreditsButton.GetComponent<UMenuButton>();
        mb.proceed = true;
        mb.buttonType = UMenuButton.MenuButtonType.CustomSubmit;
        mb.submitAction = _ => HKVocals.CoroutineHolder.StartCoroutine(MajorFeatures.RollCredits.LoadCreditsFromMenu());
    }
}
