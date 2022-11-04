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
    public static bool PlayDNInFSM = true;
    private GameObject lastDreamnailedEnemy;
    public static bool DidPlayAudioOnDialogueBox = false;

    private Regex enemyTrimRegex;

    public HKVocals() : base("Hallownest Vocalized")
    {
        var go = new GameObject("HK Vocals Coroutine Holder");
        CoroutineHolder = go.AddComponent<NonBouncer>();
        Object.DontDestroyOnLoad(CoroutineHolder);

        enemyTrimRegex = new Regex("([^0-9\\(\\)]+)", RegexOptions.Compiled);
    }
    public override string GetVersion() => "0.0.0.1";

    public MenuScreen GetMenuScreen(MenuScreen modListMenu, ModToggleDelegates? toggleDelegates) => ModMenu.CreateModMenuScreen(modListMenu);
        
    public override void Initialize()
    {
        instance = this;
        
        On.PlayMakerFSM.Awake += AddFSMEdits;

        OnDialogueBox.AfterOrig.ShowPage += PlayNPCDialogue;
        OnDialogueBox.BeforeOrig.HideText += StopAudioOnDialogueBoxClose;
        
        OnEnemyDreamnailReaction.AfterOrig.Start += AddCancelDreamDialogueOnDeath;
        OnEnemyDreamnailReaction.BeforeOrig.ShowConvo += SetLastDreamNailedEnemy;
        OnHealthManager.AfterOrig.TakeDamage += RemoveHpListeners;
        
        UIManager.EditMenus +=  ModMenu.AddAudioSlider;

        ModHooks.LanguageGetHook += PlayDreamNailDialogue;
        ModHooks.LanguageGetHook += AddSpecialElderbugAudioKey;

        OnAnimatorSequence.AfterOrig.Begin += PlayMonomonIntroPoem;
        OnAnimatorSequence.WithOrig.Skip += LockSkippingMonomonIntro;
        OnChainSequence.WithOrig.Update += WaitForAudioBeforeNextCutscene;
        
        ModHooks.LanguageGetHook += EasterEggs.SpecialGrub.GetSpecialGrubDialogue;
        On.PlayMakerFSM.OnEnable += EasterEggs.SpecialGrub.EditSpecialGrub;
        OnBossStatueLever.WithOrig.OnTriggerEnter2D += EasterEggs.ZoteLever.UseZoteLever;
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged += EasterEggs.EternalOrdeal.DeleteZoteAudioPlayersOnSceneChange;
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged += EasterEggs.ZoteLever.SetZoteLever;

        LoadAssetBundle();
        CreateAudioSource();
    }

    private string AddSpecialElderbugAudioKey(string key, string sheettitle, string orig)
    {
        if (key == "ELDERBUG_INTRO_MAIN_ALT" && sheettitle == "Elderbug")
        {
            orig = Language.Language.Get("ELDERBUG_INTRO_MAIN", sheettitle);
        }

        return orig;
    }

    private void LockSkippingMonomonIntro(On.AnimatorSequence.orig_Skip orig, AnimatorSequence self)
    {
        if (!_globalSettings.scrollLock)
        {
            orig(self);
            audioSource.Stop();
        }
    }
    private void WaitForAudioBeforeNextCutscene(On.ChainSequence.orig_Update orig, ChainSequence self)
    {
        ChainSequenceR selfr = new(self);
        if (selfr.CurrentSequence != null && !selfr.CurrentSequence.IsPlaying && !selfr.isSkipped && AudioUtils.IsPlaying())
        {
            selfr.Next();
        }
    }
    private void PlayMonomonIntroPoem(OnAnimatorSequence.Delegates.Params_Begin args)
    {
        MiscUtils.WaitForFramesBeforeInvoke(2, () => AudioUtils.TryPlayAudioFor("RANDOM_POEM_STUFF_0"));
    }

    private void StopAudioOnDialogueBoxClose(OnDialogueBox.Delegates.Params_HideText args)
    {
        audioSource.Stop();
    }

    private void PlayNPCDialogue(OnDialogueBox.Delegates.Params_ShowPage args)
    {
        var convo = args.self.currentConversation + "_" + (args.self.currentPage - 1);

        float removeTime = args.self.currentPage - 1 == 0 ? 37f / 60f : 3f / 4f;

        //this controls scroll lock and autoscroll
        DidPlayAudioOnDialogueBox = AudioUtils.TryPlayAudioFor(convo, removeTime);
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

    public static string GetUniqueId(Transform transform, string path = "") {
        if (transform.parent == null)
        {
            return $"{UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}:" + path + transform.name;
        }
        else
        {
            return GetUniqueId(transform.parent, path + $"{transform.name}/");
        }
    }

    private string PlayDreamNailDialogue(string key, string sheetTitle, string orig) 
    {
        // Make sure this is dreamnail text
        if (lastDreamnailedEnemy == null)
        {
            return orig;
        }

        // Grab the ID and name now
        string id = GetUniqueId(lastDreamnailedEnemy.transform);
        string name = enemyTrimRegex.Match(lastDreamnailedEnemy.name).Value.Trim();

        // Prevent it from running again incorrectly
        lastDreamnailedEnemy = null;

        // For the special case of grouped (generic) enemies
        if (DNAudios.DNGroups.ContainsKey(name)) name = DNAudios.DNGroups[name];

        List<string> availableClips = Dictionaries.audioNames.FindAll(s => s.Contains($"${name}$_{key}".ToUpper()));
        if (availableClips == null || availableClips.Count == 0) 
        {
            LogError($"No clips for ${name}$_{key}");
            return orig;
        }

        // Either use the already registered VA or make one and save it
        int voiceActor;

        if (_saveSettings.PersistentVoiceActors.ContainsKey(id))
        {
            voiceActor = _saveSettings.PersistentVoiceActors[id];
        }
        else 
        {
            voiceActor = Random.Range(1, availableClips.Count);
            _saveSettings.PersistentVoiceActors[id] = voiceActor;
        }

        AudioUtils.TryPlayAudioFor($"${name}$_{key}_0_{voiceActor}".ToUpper());
        
        return orig;
    }

    private void SetLastDreamNailedEnemy(OnEnemyDreamnailReaction.Delegates.Params_ShowConvo args) 
    {
        lastDreamnailedEnemy = args.self.gameObject;
    }

    private void AddCancelDreamDialogueOnDeath(OnEnemyDreamnailReaction.Delegates.Params_Start args)
    {
        args.self.gameObject.AddComponent<CancelDreamDialogueOnDeath>();
    }

    private void AddFSMEdits(On.PlayMakerFSM.orig_Awake orig, PlayMakerFSM self)
    {
        orig(self);
        
        
        if (Dictionaries.SceneFSMEdits.TryGetValue((UnityEngine.SceneManagement.SceneManager.GetActiveScene().name, self.gameObject.name, self.FsmName), out var sceneAction))
            sceneAction(self);
        if (Dictionaries.GoFSMEdits.TryGetValue((self.gameObject.name, self.FsmName), out var goAction))
            goAction(self);
        if (Dictionaries.FSMChanges.TryGetValue(self.FsmName, out var action))
            action(self);
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
