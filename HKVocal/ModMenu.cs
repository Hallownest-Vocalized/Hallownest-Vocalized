namespace HKVocals;
public static class ModMenu
{
    public static Menu MenuRef;
    public static MenuScreen CreateModMenuScreen(MenuScreen modListMenu)
    {
        MenuRef ??= new Menu("HK Vocals Menu", new Element[]
        {
            new TextPanel("To change volume, please use Audio Menu"),
            new HorizontalOption("Auto Scroll?", 
                "Should dialogue autoscroll after the audio finishes",
                new []{"Yes", "No"},
                (i) => HKVocals._globalSettings.autoScroll = i == 0,
                () => HKVocals._globalSettings.autoScroll ? 1 : 0),
            new HorizontalOption("Dream Nail Dialogue?", 
                "Should dream nail dialogue be voiced?",
                new []{"Yes", "No"},
                (i) => HKVocals._globalSettings.dnDialogue = i == 0,
                () => HKVocals._globalSettings.dnDialogue ? 1 : 0),
            new HorizontalOption("Scroll Lock?", 
                "Should first time dialogues be scroll locked until audio has finished?",
                new []{"Yes", "No"},
                (i) => HKVocals._globalSettings.scrollLock = i == 0,
                () => HKVocals._globalSettings.scrollLock ? 1 : 0),
        });
            

        return MenuRef.GetMenuScreen(modListMenu);
    }

    public static void AddAudioSlider()
    {
        //get go of a current slider
        GameObject MusicSlider = UIManager.instance.gameObject.transform.Find("UICanvas/AudioMenuScreen/Content/MusicVolume/MusicSlider").gameObject;
            
        //make clone
        GameObject VolumeSlider = GameObject.Instantiate(MusicSlider, MusicSlider.transform.parent);
            
        MenuAudioSlider VolumeSlider_MenuAudioSlider = VolumeSlider.GetComponent<MenuAudioSlider>();
        Slider VolumeSlider_Slider = VolumeSlider.GetComponent<Slider>();
            
        //all the other sliders are 0.6 down from each other
        VolumeSlider.transform.position -= new Vector3(0, 0.9f, 0f); 
        VolumeSlider.name = "HkVocalsSlider";

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
        UnityEngine.Object.Destroy(VolumeSlider.transform.Find("Label").GetComponent<AutoLocalizeTextUI>());
        VolumeSlider.transform.Find("Label").GetComponent<Text>().text = "HK Vocals Volume: ";
        VolumeSlider.SetActive(true);
            
        //to make sure when go is cloned, it gets the value of the previous session not the value of the music slider
        VolumeSlider_MenuAudioSlider.UpdateTextValue(HKVocals._globalSettings.Volume);
        VolumeSlider_Slider.value = HKVocals._globalSettings.Volume;
    }
}