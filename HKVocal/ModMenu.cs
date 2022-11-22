using GlobalEnums;
using UnityEngine.EventSystems;
using UMenuButton = UnityEngine.UI.MenuButton;
using MenuButton = Satchel.BetterMenus.MenuButton;
using static HKVocals.AudioOptionsMenu;

namespace HKVocals;
public static class ModMenu
{ 
    private static Menu MenuRef;
    public static MenuScreen CreateModMenuScreen(MenuScreen modListMenu)
    {
        MenuRef ??= new Menu("Hallownest Vocalized", new Element[]
        {
            //new TextPanel("To change volume, please use Audio Menu"),
            new MenuButton("Change Volume", "Change volume of voice actors", _ =>
            {
                UIManager.instance.UILeaveDynamicMenu(AudioMenuScreen, MainMenuState.AUDIO_MENU);
            }, proceed:true),

            Blueprints.HorizontalBoolOption("Scroll Lock", 
                "Should first time dialogues be scroll locked until audio has finished?",
                (i) => HKVocals._globalSettings.scrollLock = i,
                () => HKVocals._globalSettings.scrollLock),
            
            Blueprints.HorizontalBoolOption("Auto Scroll", 
                "Should dialogue autoscroll after the audio finishes",
                (i) => 
                    {
                        HKVocals._globalSettings.autoScroll = i;
                        MenuRef.Find("Auto Scroll Speed").isVisible = i;
                        MenuRef.Update();
                    }
                ,
                () => HKVocals._globalSettings.autoScroll),
            
            new HorizontalOption("Auto Scroll Speed", 
                "How fast should it autoscroll on audio finish",
                Enum.GetNames(typeof(MajorFeatures.AutoScroll.ScrollSpeed)).ToArray(),
                (i) => HKVocals._globalSettings.ScrollSpeed = (MajorFeatures.AutoScroll.ScrollSpeed) i,
                () => (int) HKVocals._globalSettings.ScrollSpeed,
                Id: "Auto Scroll Speed")
                {
                    isVisible = HKVocals._globalSettings.autoScroll
                },
            
            Blueprints.HorizontalBoolOption("Dream Nail Dialogues", 
                "Should dream nail dialogues be voiced?",
                (i) =>
                {
                    HKVocals._globalSettings.dnDialogue = i;
                    MenuRef.Find("Automatic Boss Dialogue").isVisible = !i;
                    MenuRef.Update();
                },
                () => HKVocals._globalSettings.dnDialogue),
            
            new HorizontalOption("Automatic Boss Shouts",
                "Should some bosses automatically do shouts",
                new []{"On", "Off"},
                i => HKVocals._globalSettings.automaticBossDialogue = i == 0,
                () => HKVocals._globalSettings.automaticBossDialogue ? 0 : 1,
                Id: "Automatic Boss Dialogue")
            {
                isVisible = !HKVocals._globalSettings.dnDialogue
            },
            
            Blueprints.HorizontalBoolOption("Dampen Audio",
                "Should audio be damped when audio is played",
                i => HKVocals._globalSettings.dampenAudio = i,
                () => HKVocals._globalSettings.dampenAudio),
        });
        return MenuRef.GetMenuScreen(modListMenu);
    }

    public static void AddAudioSlider()
    {
        //make clone
        GameObject HKVocalsVolume = new GameObject("HKVocals Volume");
        HKVocalsVolume.transform.SetParent(MusicSlider.transform.parent.parent);
        HKVocalsVolume.transform.localScale = Vector3.one;
        HKVocalsVolume.transform.localPosition = MusicVolume.transform.localPosition + Vector3.down * 120;
        GameObject VolumeSlider = Object.Instantiate(MusicSlider, HKVocalsVolume.transform);
            
        MenuAudioSlider VolumeSlider_MenuAudioSlider = VolumeSlider.GetComponent<MenuAudioSlider>();
        Slider VolumeSlider_Slider = VolumeSlider.GetComponent<Slider>();

        VolumeSlider.transform.localPosition = Vector3.right * 303f;
        VolumeSlider.name = "HK Vocals Slider";

        Action<float> StoreValue = f =>
        {
            VolumeSlider_MenuAudioSlider.UpdateTextValue(f);
            HKVocals._globalSettings.Volume = (int)f;
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
        VolumeSlider_MenuAudioSlider.UpdateTextValue(HKVocals._globalSettings.Volume);
        VolumeSlider_Slider.value = HKVocals._globalSettings.Volume;
        
        GameObject HKVocalsSettings = Object.Instantiate(DefaultButton, DefaultButton.transform.parent);
        HKVocalsSettings.name = "HK Vocals Settings";
        HKVocalsSettings.transform.localPosition = Vector3.up * 335f;
        HKVocalsSettings.RemoveComponent<EventTrigger>();
        HKVocalsSettings.Find("Text").RemoveComponent<AutoLocalizeTextUI>();
        HKVocalsSettings.Find("Text").GetComponent<Text>().text = "Go to Hallownest Vocalized Settings";
        var mb = HKVocalsSettings.GetComponent<UMenuButton>();
        mb.proceed = true;
        mb.buttonType = UMenuButton.MenuButtonType.CustomSubmit;
        mb.submitAction = _ =>
        {
            // we cant garuntee our modmenu has been created yet so our MenuRef.menuScreen might be null. hence we manually find modist menu and use that
            UIManager.instance.UIGoToDynamicMenu(MenuRef.menuScreen != null 
                ? MenuRef.menuScreen 
                : MenuRef.GetMenuScreen(UIManager.instance.UICanvas.Find("ModListMenu").GetComponent<MenuScreen>()));
        };
    }
}
