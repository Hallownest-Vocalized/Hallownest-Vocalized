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
