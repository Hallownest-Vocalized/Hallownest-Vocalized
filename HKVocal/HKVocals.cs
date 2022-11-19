using UnityEngine.Audio;

namespace HKVocals;

public class HKVocals: Mod, IGlobalSettings<GlobalSettings>, ILocalSettings<SaveSettings>, ICustomMenuMod
{
    public static GlobalSettings _globalSettings { get; set; } = new GlobalSettings();
    public void OnLoadGlobal(GlobalSettings s) => _globalSettings = s;
    public GlobalSettings OnSaveGlobal() => _globalSettings;
    public static SaveSettings _saveSettings { get; set; } = new SaveSettings();
    public void OnLoadLocal(SaveSettings s) => _saveSettings = s;
    public SaveSettings OnSaveLocal() => _saveSettings;
        
    public AssetBundle audioBundle;
    public AudioSource audioSource;
    internal static HKVocals instance;
    public static NonBouncer CoroutineHolder;
    
    public static List<string> audioExtentions = new List<string>() { ".mp3", ".wav" };
    public static List<string> audioNames = new List<string>();

    public HKVocals() : base("Hallownest Vocalized")
    {
        var go = new GameObject("HK Vocals Coroutine Holder");
        CoroutineHolder = go.AddComponent<NonBouncer>();
        Object.DontDestroyOnLoad(CoroutineHolder);
    }
    
    //todo: add hash of audiobundle here, if not present warn users in get version
    public override string GetVersion() => "0.0.0.1";

    public override void Initialize()
    {
        instance = this;

        UIManager.EditMenus +=  ModMenu.AddAudioSlider;

        MajorFeatures.SpecialAudio.Hook();
        MajorFeatures.NPCDialogue.Hook();
        MajorFeatures.DreamNailDialogue.Hook();
        MajorFeatures.AutoScroll.Hook();
        MajorFeatures.ScrollLock.Hook();
        MajorFeatures.AutomaticBossDialogue.Hook();
        MajorFeatures.UITextAudio.Hook();
        
        EasterEggs.EternalOrdeal.Hook();
        EasterEggs.SpecialGrub.Hook();
        EasterEggs.PaleFlower.Hook();

        Hooks.PmFsmBeforeStartHook += AddFSMEdits;

        LoadAssetBundle();
        CreateAudioSource();
    }

    public void CreateAudioSource()
    {
        LogDebug("creating new asrc");
        GameObject audioGO = new GameObject("HK Vocals Audio");
        audioSource = audioGO.AddComponent<AudioSource>();
        //make our audio not be affected by any slider other than master
        audioSource.outputAudioMixerGroup = Resources.FindObjectsOfTypeAll<AudioMixerGroup>().First(x => x.name == "Master" && x.audioMixer != null && x.audioMixer.name == "Master");
        Object.DontDestroyOnLoad(audioGO);
    }

    private void AddFSMEdits(PlayMakerFSM fsm)
    {
        string sceneName = MiscUtils.GetCurrentSceneName();
        string gameObjectName = fsm.gameObject.name;
        string fsmName = fsm.FsmName;

        if (FSMEditData.FsmEdits.TryGetValue(new HKVocalsFsmData(sceneName, gameObjectName, fsmName), out var action_1))
        {
            action_1.TryInvokeActions(fsm);
        }
        if (FSMEditData.FsmEdits.TryGetValue(new HKVocalsFsmData(gameObjectName, fsmName), out var action_2))
        {
            action_2.TryInvokeActions(fsm);
        }
        if (FSMEditData.FsmEdits.TryGetValue(new HKVocalsFsmData(gameObjectName, fsmName), out var action_3))
        {
            action_3.TryInvokeActions(fsm);
        }
    }

    private void LoadAssetBundle()
    {
        Assembly asm = Assembly.GetExecutingAssembly();
        audioBundle = AssetBundle.LoadFromStream(File.OpenRead(Path.GetDirectoryName(asm.Location) + "/audiobundle"));
        string[] allAssetNames = audioBundle.GetAllAssetNames();
        for (int i = 0; i < allAssetNames.Length; i++)
        {
            if (audioExtentions.Any(ext => allAssetNames[i].EndsWith(ext)))
            {
                audioNames.Add(Path.GetFileNameWithoutExtension(allAssetNames[i]).ToUpper());
            }
            LogDebug($"Object in audiobundle: {allAssetNames[i]} {Path.GetFileNameWithoutExtension(allAssetNames[i])?.ToUpper().Replace("KNGHT", "KNIGHT")}");
        }
    }
    
    
    public MenuScreen GetMenuScreen(MenuScreen modListMenu, ModToggleDelegates? toggleDelegates) => ModMenu.CreateModMenuScreen(modListMenu);
    public bool ToggleButtonInsideMenu => false;
}
