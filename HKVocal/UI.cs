using GlobalEnums;
using JetBrains.Annotations;
using UnityEngine.EventSystems;
using UMenuButton = UnityEngine.UI.MenuButton;
using MenuButton = Satchel.BetterMenus.MenuButton;
using Satchel;
using MagicUI.Core;
using MagicUI.Elements;
using UnityEngine;

namespace HKVocals;
public static class UI
{
    private static GameObject TextCanvas;
    private static Text SettingTextGO;
    [CanBeNull] private static LayoutRoot layout;
    public static void Hook()
    {
        On.UIManager.GoToProfileMenu += ShowSettingText;
        On.UIManager.GoToMainMenu += MainMenuHideSettingText;
        On.UIManager.GoToPlayModeMenu += PlayModeHideSettingText;
        On.GameManager.LoadGame += AwakeHideSettingText;
        UIManager.EditMenus += OnUIManagerEdits;
    }
    public static Menu MenuRef;

    public static void OnUIManagerEdits()
    {
        if (layout == null)
        {
            layout = new(true, "SettingButton");
            
            Setup(layout);
        }
    }
    public static MenuScreen CreateModMenuScreen(MenuScreen modListMenu)
    {
        MenuRef ??= new Menu("Hallownest Vocalized", new Element[]
        {
            //new TextPanel("To change volume, please use Audio Menu"),
            new MenuButton("Change Volume", "Change volume of voice actors", _ =>
            {
                UIManager.instance.UILeaveDynamicMenu(RefVanillaMenu.AudioMenuScreen, MainMenuState.AUDIO_MENU);
            }, proceed:true),

            Blueprints.HorizontalBoolOption("Scroll Lock", 
                "Should first time dialogues be scroll locked until audio has finished?",
                (i) => HKVocals._globalSettings.scrollLock = i,
                () => HKVocals._globalSettings.scrollLock),
            
            Blueprints.HorizontalBoolOption("Auto Scroll", 
                "Should dialogue autoscroll after the audio finishes?",
                (i) => 
                    {
                        HKVocals._globalSettings.autoScroll = i;
                        MenuRef.Find("Auto Scroll Speed").isVisible = i;
                        MenuRef.Update();
                    }
                ,
                () => HKVocals._globalSettings.autoScroll),
            
            new HorizontalOption("Auto Scroll Speed", 
                "How fast should it autoscroll after audio finishes playing?",
                Enum.GetNames(typeof(MajorFeatures.AutoScroll.ScrollSpeed)).ToArray(),
                (i) => HKVocals._globalSettings.ScrollSpeed = (MajorFeatures.AutoScroll.ScrollSpeed) i,
                () => (int) HKVocals._globalSettings.ScrollSpeed,
                Id: "Auto Scroll Speed")
                {
                    isVisible = HKVocals._globalSettings.autoScroll
                },
            
            Blueprints.HorizontalBoolOption("Dream Nail Dialogue", 
                "Should dream nail dialogue be voiced?",
                (i) =>
                {
                    HKVocals._globalSettings.dnDialogue = i;
                    MenuRef.Find("Automatic Boss Dialogue").isVisible = !i;
                    MenuRef.Update();
                },
                () => HKVocals._globalSettings.dnDialogue),
            
            new HorizontalOption("Automatic Boss Shouts",
                "Should some bosses automatically do shouts?",
                new []{"On", "Off"},
                i => HKVocals._globalSettings.automaticBossDialogue = i == 0,
                () => HKVocals._globalSettings.automaticBossDialogue ? 0 : 1,
                Id: "Automatic Boss Dialogue")
            {
                isVisible = !HKVocals._globalSettings.dnDialogue
            },
            
            Blueprints.HorizontalBoolOption("Dampen Audio",
                "Should audio be dampened when audio is played?",
                i =>
                {
                    HKVocals._globalSettings.dampenAudio = i;
                    if (!HKVocals._globalSettings.dampenAudio)
                    {
                        MajorFeatures.DampenAudio.ForceStopDampenAudio();
                    }
                },
                () => HKVocals._globalSettings.dampenAudio),
        });
        return MenuRef.GetMenuScreen(modListMenu);
    }

    public static void AddAudioSliderandSettingsButton()
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
            HKVocals._globalSettings.Volume = (int)f;
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
        VolumeSlider_MenuAudioSlider.UpdateTextValue(HKVocals._globalSettings.Volume);
        VolumeSlider_Slider.value = HKVocals._globalSettings.Volume;
        
        GameObject HKVocalsSettings = Object.Instantiate(RefVanillaMenu.DefaultAudioSettingsButton, RefVanillaMenu.DefaultAudioSettingsButton.transform.parent);
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
    
    public static void CreateSettingsText(On.UIManager.orig_Start orig, UIManager self)
    {
        orig(self);
        
        TextCanvas = CanvasUtil.CreateCanvas(RenderMode.ScreenSpaceOverlay, new Vector2(1920, 1080));
        TextCanvas.name = "SettingText";

        CanvasGroup cg = TextCanvas.GetComponent<CanvasGroup>();
        cg.interactable = false;
        cg.blocksRaycasts = false;

        GameObject background = CanvasUtil.CreateImagePanel
        (
            TextCanvas,
            CanvasUtil.NullSprite(new byte[] {0x80, 0x00, 0x00, 0x00}),
            new CanvasUtil.RectData(Vector2.zero, Vector2.zero, Vector2.zero, Vector2.one)
        );

        var SettingText = CanvasUtil.CreateTextPanel
        (
            background,
            "Notice: We recommend reviewing your audio settings before playing Hallownest Vocalized",
            35,
            TextAnchor.MiddleCenter,
            new CanvasUtil.RectData(new Vector2(-5, -5), Vector2.zero, Vector2.zero, Vector2.one),
            MenuResources.Perpetua
        );
        SettingText.transform.position = new Vector3(960, 30, 0f);
        

        SettingTextGO = SettingText.GetComponent<Text>();
        TextCanvas.SetActive(false);
    }

    public static void Setup(LayoutRoot inputLayout)
    {
        layout = inputLayout; 
        
        MagicUI.Elements.Button SettingButton = new(layout)
        {
            Content = "Go to Hallownest Vocalized Settings",
            FontSize = 30,
            Borderless = true,
            Font = MenuResources.Perpetua,

        };
        GameObject.Find("SettingButton").transform.GetChild(0).GetChild(0).localPosition = new Vector3(755.5f, -987, 0);
        GameObject.Find("SettingButton").transform.GetChild(0).GetChild(0).gameObject.SetActive(false);
        SettingButton.Click += GoToMenu;
    }

    private static void GoToMenu(MagicUI.Elements.Button sender)
    {
        UIManager.instance.LeaveExitToMenuPrompt();
        UIManager.instance.UICanvas.transform.GetChild(3).gameObject.SetActive(false);
        UIManager.instance.UIGoToDynamicMenu(MenuRef.menuScreen != null ? MenuRef.menuScreen : MenuRef.GetMenuScreen(UIManager.instance.UICanvas.Find("ModListMenu").GetComponent<MenuScreen>()));
    }
    
    private static IEnumerator ShowSettingText(On.UIManager.orig_GoToProfileMenu orig, UIManager self)
    {
        TextCanvas.SetActive(true);
        GameObject.Find("SettingButton").transform.GetChild(0).GetChild(0).gameObject.SetActive(true);
        yield return orig(self);
    }
    private static IEnumerator MainMenuHideSettingText(On.UIManager.orig_GoToMainMenu orig, UIManager self)
    {
        TextCanvas.SetActive(false);
        GameObject.Find("SettingButton").transform.GetChild(0).GetChild(0).gameObject.SetActive(false);
        yield return orig(self);
    }
    private static IEnumerator PlayModeHideSettingText(On.UIManager.orig_GoToPlayModeMenu orig, UIManager self)
    {
        TextCanvas.SetActive(false);
        GameObject.Find("SettingButton").transform.GetChild(0).GetChild(0).gameObject.SetActive(false);
        yield return orig(self);
    }
    private static void AwakeHideSettingText(On.GameManager.orig_LoadGame orig, GameManager self, int saveSlot, Action<bool> callback)
    {
        orig(self, saveSlot, callback);
        TextCanvas.SetActive(false);
        GameObject.Find("SettingButton").transform.GetChild(0).GetChild(0).gameObject.SetActive(false);
    }
}
