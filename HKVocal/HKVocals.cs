using System.Text.RegularExpressions;
using HKMirror.Hooks.OnHooks;
using HKMirror.Reflection;
using HKMirror.Reflection.InstanceClasses;

namespace HKVocals;

public class HKVocals: Mod, IGlobalSettings<GlobalSettings>, ILocalSettings<SaveSettings>, ICustomMenuMod
{
    public static GlobalSettings _globalSettings { get; set; } = new GlobalSettings();
    public void OnLoadGlobal(GlobalSettings s) => _globalSettings = s;
    public GlobalSettings OnSaveGlobal() => _globalSettings;
    public static SaveSettings _saveSettings { get; set; } = new SaveSettings();
    public void OnLoadLocal(SaveSettings s) => _saveSettings = s;
    public SaveSettings OnSaveLocal() => _saveSettings;
        
    public const bool RemoveOrigNPCSounds = true;
    public AssetBundle audioBundle;
    public AudioSource audioSource;
    internal static HKVocals instance;
    public bool ToggleButtonInsideMenu => false;
    public static NonBouncer CoroutineHolder;

    public HKVocals() : base("Hallownest Vocalized")
    {
        var go = new GameObject("HK Vocals Coroutine Holder");
        CoroutineHolder = go.AddComponent<NonBouncer>();
        Object.DontDestroyOnLoad(CoroutineHolder);
    }
    public override string GetVersion() => "0.0.0.1";

    public MenuScreen GetMenuScreen(MenuScreen modListMenu, ModToggleDelegates? toggleDelegates) => ModMenu.CreateModMenuScreen(modListMenu);
        
    public override void Initialize()
    {
        instance = this;

        OnHealthManager.AfterOrig.TakeDamage += RemoveHpListeners;
        
        UIManager.EditMenus +=  ModMenu.AddAudioSlider;

        MajorFeatures.SpecialAudio.Hook();
        MajorFeatures.NPCDialogue.Hook();
        MajorFeatures.DreamNailDialogue.Hook();
        MajorFeatures.AutoScroll.Hook();
        MajorFeatures.ScrollLock.Hook();
        
        EasterEggs.EternalOrdeal.Hook();
        EasterEggs.SpecialGrub.Hook();
        
        On.PlayMakerFSM.Awake += AddFSMEdits;

        LoadAssetBundle();
        CreateAudioSource();
    }

    public void CreateAudioSource()
    {
        LogDebug("creating new asrc");
        GameObject audioGO = new GameObject("HK Vocals Audio");
        audioSource = audioGO.AddComponent<AudioSource>();
        Object.DontDestroyOnLoad(audioGO);
    }

    private void RemoveHpListeners(OnHealthManager.Delegates.Params_TakeDamage args)
    {
        for (int i = 0; i < Dictionaries.HpListeners.Count; i++)
        {
            bool sucess = Dictionaries.HpListeners[i](args.self);
            if (sucess)
            {
                Dictionaries.HpListeners.RemoveAt(i);
                i--;
            }
        }
    }

    private void AddFSMEdits(On.PlayMakerFSM.orig_Awake orig, PlayMakerFSM self)
    {
        orig(self);

        string sceneName = MiscUtils.GetCurrentSceneName();
        string gameObjectName = self.gameObject.name;
        string fsmName = self.FsmName;

        foreach (var fsmEdit in FSMEditData.SceneFsmEdits.FindAll(x => x.DoesMatch(sceneName, gameObjectName, fsmName)))
        {
            fsmEdit.Invoke(self);
        }
        
        foreach (var fsmEdit in FSMEditData.GameObjectFsmEdits.FindAll(x => x.DoesMatch(gameObjectName, fsmName)))
        {
            fsmEdit.Invoke(self);
        }
        
        foreach (var fsmEdit in FSMEditData.AnyFsmEdits.FindAll(x => x.DoesMatch(fsmName)))
        {
            fsmEdit.Invoke(self);
        }
    }

   
    
    public void CreateDreamDialogue(string convName, string sheetName, string enemyType = "", string enemyVariant = "", GameObject enemy = null)
    {
        PlayMakerFSM fsm = FsmVariables.GlobalVariables.GetFsmGameObject("Enemy Dream Msg").Value.LocateMyFSM("Display");
        fsm.Fsm.GetFsmString("Convo Title").Value = convName;
        fsm.Fsm.GetFsmString("Sheet").Value = sheetName;
        fsm.SendEvent("DISPLAY DREAM MSG");
    }

    private void LoadAssetBundle()
    {
        Assembly asm = Assembly.GetExecutingAssembly();
        //audioBundle = AssetBundle.LoadFromStream(asm.GetManifestResourceStream("HKVocals.audiobundle"));
        audioBundle = AssetBundle.LoadFromStream(File.OpenRead(Path.GetDirectoryName(asm.Location) + "/audiobundle"));
        string[] allAssetNames = audioBundle.GetAllAssetNames();
        for (int i = 0; i < allAssetNames.Length; i++)
        {
            if (Dictionaries.audioExtentions.Any(ext => allAssetNames[i].EndsWith(ext)))
            {
                Dictionaries.audioNames.Add(Path.GetFileNameWithoutExtension(allAssetNames[i]).ToUpper());
            }
            LogDebug($"Object in audiobundle: {allAssetNames[i]} {Path.GetFileNameWithoutExtension(allAssetNames[i])?.ToUpper().Replace("KNGHT", "KNIGHT")}");
        }
    }
}
