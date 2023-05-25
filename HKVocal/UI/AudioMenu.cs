using UnityEngine.EventSystems;
using UMenuButton = UnityEngine.UI.MenuButton;
using Satchel;

namespace HKVocals.UI;
public static class AudioMenu
{
    public static void AddAudioSliderAndSettingsButton()
    {
        //make clone
        GameObject HKVocalsVolume = new GameObject("HKVocals Volume");
        HKVocalsVolume.transform.SetParent(RefVanillaMenu.MusicVolumeSlider.transform.parent.parent);
        HKVocalsVolume.transform.localScale = Vector3.one;
        HKVocalsVolume.transform.localPosition = RefVanillaMenu.MusicVolumeSliderHolder.transform.localPosition + Vector3.down * 120;
        GameObject VolumeSlider = Object.Instantiate(RefVanillaMenu.MusicVolumeSlider, HKVocalsVolume.transform);
            
        MenuAudioSlider VolumeSlider_MenuAudioSlider = VolumeSlider.GetComponent<MenuAudioSlider>();
        Slider VolumeSlider_Slider = VolumeSlider.GetComponent<Slider>();

        VolumeSlider.transform.localPosition = Vector3.right * 303f;
        VolumeSlider.name = "HK Vocals Slider";

        Action<float> StoreValue = f =>
        {
            VolumeSlider_MenuAudioSlider.UpdateTextValue(f);
            HKVocals._globalSettings.volume = (int)f;
            MixerLoader.SetMixerVolume();
        };

        // stuff to happen whenever slider is moved
        var SliderEvent = new Slider.SliderEvent();
        SliderEvent.AddListener(StoreValue.Invoke);

        VolumeSlider_Slider.onValueChanged = SliderEvent ;

        //change the key of the text so it can be changed
        VolumeSlider.Find("Label").RemoveComponent<AutoLocalizeTextUI>();
        VolumeSlider.Find("Label").GetComponent<Text>().text = "HK Vocals Volume: ";
        VolumeSlider.SetActive(true);
            
        //to make sure when go is cloned, it gets the value of the previous session not the value of the music slider
        VolumeSlider_MenuAudioSlider.UpdateTextValue(HKVocals._globalSettings.volume);
        VolumeSlider_Slider.value = HKVocals._globalSettings.volume;

        GameObject HKVocalsSettings = Object.Instantiate(RefVanillaMenu.DefaultAudioSettingsButton, RefVanillaMenu.DefaultAudioSettingsButton.transform.parent);
        HKVocalsSettings.name = "HK Vocals Settings";
        HKVocalsSettings.transform.localPosition = Vector3.up * 335f;
        HKVocalsSettings.RemoveComponent<EventTrigger>();
        HKVocalsSettings.Find("Text").RemoveComponent<AutoLocalizeTextUI>();
        HKVocalsSettings.Find("Text").GetComponent<Text>().text = "Go to Hallownest Vocalized Settings";
        var mb = HKVocalsSettings.GetComponent<UMenuButton>();
        mb.proceed = true;
        mb.buttonType = UMenuButton.MenuButtonType.CustomSubmit;
        mb.submitAction = _ => ModMenu.GoToModMenu();
        
        //fix navigation
        var musicSelectable = RefVanillaMenu.MusicVolumeSlider.GetComponent<Selectable>();
        var hkvVolumeSelectable = VolumeSlider.GetComponent<Selectable>();
        var hkvSettingsSelectable = HKVocalsSettings.GetComponent<Selectable>();
        var defaultAudioSettingsSelectable = RefVanillaMenu.DefaultAudioSettingsButton.GetComponent<Selectable>();
        
        
        musicSelectable.navigation = musicSelectable.navigation with { selectOnDown = hkvVolumeSelectable };

        hkvVolumeSelectable.navigation = new Navigation()
        {
            selectOnUp = musicSelectable,
            selectOnDown = hkvSettingsSelectable,
            mode = Navigation.Mode.Explicit
        };
        
        hkvSettingsSelectable.navigation = new Navigation()
        {
            selectOnUp = hkvVolumeSelectable,
            selectOnDown = defaultAudioSettingsSelectable,
            mode = Navigation.Mode.Explicit
        };
        
        defaultAudioSettingsSelectable.navigation = defaultAudioSettingsSelectable.navigation with
        {
            selectOnUp = hkvSettingsSelectable,
        };
    }
}
