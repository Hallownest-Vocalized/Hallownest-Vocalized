using Language;
using Satchel;
using UnityEngine.Audio;
using Newtonsoft.Json;

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
    public static bool AudioLoaderAssemblyExists => AppDomain.CurrentDomain.GetAssemblies().Any(a => a.GetName().Name == "Hallownest-Vocalized-AudioLoader");

    // At least one entry is required to have the main menu be reloaded for the custom menu theme, here it's just a dummy entry
    public override List<ValueTuple<string, string>> GetPreloadNames()
    {
        return new List<(string, string)>()
        {
            ("Room_shop", "_SceneManager")
        };
    }

    public HKVocals() : base("Hallownest Vocalized")
    {
        OnMenuStyleTitle.AfterOrig.SetTitle += AddCustomBanner;
        On.UIManager.Start += AddIcon;
        if (AudioLoaderAssemblyExists)
        {
            SFCore.ItemHelper.unusedInit();
            SFCore.MenuStyleHelper.AddMenuStyleHook += MajorFeatures.MenuTheme.AddTheme;
            MajorFeatures.Achievements.Hook();
        }
    }
    
    private static string Version = "1.0.0.0";
    public override string GetVersion() => $"{Version}" + (AudioLoaderAssemblyExists ? "" : $"ERROR: Missing Hallownest Vocalized AudioLoader");

    public override void Initialize()
    {
        instance = this;
        
        if (AudioLoaderAssemblyExists)
        {
            MixerLoader.LoadAssetBundle();
            CreditsLoader.LoadAssetBundle();
            StyleLoader.LoadAssetBundle();

            MajorFeatures.SpecialAudio.Hook();
            MajorFeatures.NPCDialogue.Hook();
            MajorFeatures.MuteOriginalAudio.Hook();
            MajorFeatures.DampenAudio.Hook();
            MajorFeatures.DreamNailDialogue.Hook();
            MajorFeatures.AutoScroll.Hook();
            MajorFeatures.ScrollLock.Hook();
            MajorFeatures.AutomaticBossDialogue.Hook();
            MajorFeatures.UITextAudio.Hook();
            MajorFeatures.RollCredits.Hook();
            MajorFeatures.Patches.Hook();

            EasterEggs.EternalOrdeal.Hook();
            EasterEggs.SpecialGrub.Hook();
            EasterEggs.PaleFlower.Hook();
            EasterEggs.GhostRelics.Hook();

            UIManager.EditMenus += UI.AudioMenu.AddAudioSliderAndSettingsButton;
            UIManager.EditMenus += UI.ExtrasMenu.AddCreditsButton;
            UIManager.EditMenus += UI.SettingsPrompt.CreatePrompt;

            UI.SettingsPrompt.HookRemoveButton();
            
            Hooks.PmFsmBeforeStartHook += AddFSMEdits;

            CoroutineHolder = new GameObject("HK Vocals Coroutine Holder").AddComponent<NonBouncer>();
            Object.DontDestroyOnLoad(CoroutineHolder);
            CreateAudioSource();

            // Set the menu style to the custom one
            var tmpStyle = MenuStyles.Instance.styles.First(x => x.styleObject.name.Contains("HKVStyle"));
            MenuStyles.Instance.SetStyle(MenuStyles.Instance.styles.ToList().IndexOf(tmpStyle), false);

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
    
    private void AddCustomBanner(OnMenuStyleTitle.Delegates.Params_SetTitle args)
    {
        //only change for english language. i doubt people on other languages want it
        if (Language.Language.CurrentLanguage() == LanguageCode.EN)
        {
            if (AudioLoaderAssemblyExists)
            {
                args.self.Title.sprite = AssemblyUtils.GetSpriteFromResources(Random.Range(1,1000) == 1 && _globalSettings.settingsOpened 
                    ? "Resources.Title_alt.png" 
                    : "Resources.Title.png");
            }
            else
            {
                args.self.Title.sprite = AssemblyUtils.GetSpriteFromResources("Resources.Title_missingDeps.png");
            }
        }
        
    }
    private void InitAchievements()
    {
        string json = @"[ 'smth']";
        if(!_globalSettings.GotHJAchievement)
        {
            _globalSettings.FinishedHJDialoge = JsonConvert.DeserializeObject<List<string>>(json);
        }
        if(!_globalSettings.GotUIAchievement)
        {
           // _globalSettings.FinishedUIDialoge = JsonConvert.DeserializeObject<List<string>>(json);
        }
        if(!_globalSettings.GotNPCAchievement)
        {
           // _globalSettings.FinishedNPCDialoge = JsonConvert.DeserializeObject<List<string>>(json);
        }
        if(!_globalSettings.GotDNailAchievement)
        {
           // _globalSettings.FinishedDNailDialoge = JsonConvert.DeserializeObject<List<string>>(json);
        }
        if(!_globalSettings.GotLoreTabletAchievement)
        {
           // _globalSettings.FinishedLoreTabletDialoge = JsonConvert.DeserializeObject<List<string>>(json);
        }
    }
    private static Sprite icon;
    private void AddIcon(On.UIManager.orig_Start orig, UIManager self)
    {
        orig(self);

        var dlc = self.transform.Find("UICanvas/MainMenuScreen/TeamCherryLogo/Hidden_Dreams_Logo").gameObject;

        var clone = Object.Instantiate(dlc, dlc.transform.parent);
        clone.SetActive(true);

        var pos = clone.transform.position;
        clone.transform.position = pos + new Vector3(3.2f, -0.111f, 0);
        clone.transform.SetScaleX(233f);
        clone.transform.SetScaleY(233f);

        icon = Satchel.AssemblyUtils.GetSpriteFromResources("Resources.icon.png");
        clone.GetComponent<SpriteRenderer>().sprite = icon;
    }
    public static void DoLogDebug(object s) => instance.LogDebug(s);
    public static void DoLog(object s) => instance.Log(s);
    public MenuScreen GetMenuScreen(MenuScreen modListMenu, ModToggleDelegates? toggleDelegates) => UI.ModMenu.CreateModMenuScreen(modListMenu);
    public bool ToggleButtonInsideMenu => false;
}
