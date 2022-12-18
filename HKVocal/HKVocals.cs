using UnityEngine.Audio;

namespace HKVocals;

public sealed class HKVocals: Mod, IGlobalSettings<GlobalSettings>, ILocalSettings<SaveSettings>, ICustomMenuMod
{
    public static GlobalSettings _globalSettings { get; private set; } = new ();
    public void OnLoadGlobal(GlobalSettings s) => _globalSettings = s;
    public GlobalSettings OnSaveGlobal() => _globalSettings;
    public static SaveSettings _saveSettings { get; private set; } = new ();
    public void OnLoadLocal(SaveSettings s) => _saveSettings = s;
    public SaveSettings OnSaveLocal() => _saveSettings;
        
    public AudioSource audioSource;
    internal static HKVocals instance;
    public static NonBouncer CoroutineHolder;
    
    public static readonly string BundleLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/audiobundle";
    public static bool BundleExists => File.Exists(BundleLocation);

    public HKVocals() : base("Hallownest Vocalized")
    {
        MajorFeatures.Achievements.Hook();
    }

    public static string Version = "0.0.0.1";

    public override string GetVersion()
    {
        if (BundleExists)
        {
            return $"{Version} ({MiscUtils.GetFileHash(BundleLocation)})";
        }
        else
        {
            return $"{Version} Did not load. Missing audios";
        }
    }

    public override void Initialize()
    {
        instance = this;

        if (BundleExists)
        {
            AudioLoader.LoadAssetBundle();
            MixerLoader.LoadAssetBundle();

            MajorFeatures.SpecialAudio.Hook();
            MajorFeatures.NPCDialogue.Hook();
            MajorFeatures.MuteOriginalAudio.Hook();
            MajorFeatures.DampenAudio.Hook();
            MajorFeatures.DreamNailDialogue.Hook();
            MajorFeatures.AutoScroll.Hook();
            MajorFeatures.ScrollLock.Hook();
            MajorFeatures.AutomaticBossDialogue.Hook();
            MajorFeatures.UITextAudio.Hook();

            EasterEggs.EternalOrdeal.Hook();
            EasterEggs.SpecialGrub.Hook();
            EasterEggs.PaleFlower.Hook();

            UIManager.EditMenus += ModMenu.AddAudioSlider;
            Hooks.PmFsmBeforeStartHook += AddFSMEdits;

            CoroutineHolder = new GameObject("HK Vocals Coroutine Holder").AddComponent<NonBouncer>();
            Object.DontDestroyOnLoad(CoroutineHolder);

            CreateAudioSource();
            
            Log("HKVocals initialized");
        }
        else
        {
            LogError("HKVocals Did not load because there was no audio bundle");
        }
    }

    public void CreateAudioSource()
    { 
        LogDebug("creating new asrc");
        GameObject audioGO = new GameObject("HK Vocals Audio");
        audioSource = audioGO.AddComponent<AudioSource>();
        
        audioSource.SetMixerGroup();
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
        if (FSMEditData.FsmEdits.TryGetValue(new HKVocalsFsmData(fsmName), out var action_3))
        {
            action_3.TryInvokeActions(fsm);
        }
    }

    public static void DoLogDebug(object s) => instance.LogDebug(s);
    public static void DoLog(object s) => instance.Log(s);

    public MenuScreen GetMenuScreen(MenuScreen modListMenu, ModToggleDelegates? toggleDelegates) => ModMenu.CreateModMenuScreen(modListMenu);
    public bool ToggleButtonInsideMenu => false;
}
