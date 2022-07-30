namespace HKVocals;

public class HKVocals: Mod, IGlobalSettings<GlobalSettings>, ILocalSettings<SaveSettings>, ICustomMenuMod
{
    public static GlobalSettings _globalSettings { get; set; } = new GlobalSettings();
    public void OnLoadGlobal(GlobalSettings s) => _globalSettings = s;
    public GlobalSettings OnSaveGlobal() => _globalSettings;
    public SaveSettings _saveSettings { get; set; } = new SaveSettings();
    public void OnLoadLocal(SaveSettings s) => _saveSettings = s;
    public SaveSettings OnSaveLocal() => _saveSettings;
        
    public const bool RemoveOrigNPCSounds = true;
    public AssetBundle audioBundle;
    public AudioSource audioSource;
    public Coroutine autoTextRoutine;
    internal static HKVocals instance;
    public bool ToggleButtonInsideMenu => false;
    public bool IsGrubRoom = false;
    public string GrubRoom = "Crossroads_48";
    public static NonBouncer CoroutineHolder;
    public static bool PlayDNInFSM = true;
    private string lastDreamnailedEnemy;

    public HKVocals() : base("Hollow Knight Vocalized")
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
        On.DialogueBox.ShowPage += ShowPage;
        On.DialogueBox.ShowNextPage += StopConvo_NextPage;
        On.DialogueBox.HideText += StopConvo_HideText;
        On.PlayMakerFSM.Awake += FSMAwake;
        On.HutongGames.PlayMaker.Actions.AudioPlayerOneShot.DoPlayRandomClip += PlayRandomClip;
        On.PlayMakerFSM.OnEnable += OnGrubFsm;
        On.EnemyDreamnailReaction.Start += EDNRStart;
        On.EnemyDreamnailReaction.ShowConvo += ShowConvo;
        On.HealthManager.TakeDamage += TakeDamage;
        UIManager.EditMenus +=  ModMenu.AddAudioSlider;

        ModHooks.LanguageGetHook += LanguageGet;
        ModHooks.LanguageGetHook += GrubKeys;
        
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged += EternalOrdeal.DeleteZoteAudioPlayersOnSceneChange;
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged += ZoteLever.SetZoteLever;
        On.BossStatueLever.OnTriggerEnter2D += ZoteLever.UseZoteLever;

        LoadAssetBundle();
        CreateAudioSource();
    }

    private void StopConvo_HideText(On.DialogueBox.orig_HideText orig, DialogueBox self)
    {
        audioSource.Stop();
        orig.Invoke(self);
    }

    private void StopConvo_NextPage(On.DialogueBox.orig_ShowNextPage orig, DialogueBox self)
    {
        string key = self.currentConversation + "%%" + self.currentPage;
        if (_globalSettings.scrollLock && !_saveSettings.FinishedConvos.Contains(key))
        {
            return;
        }
        //Log(key);
        _saveSettings.FinishedConvos.Add(key);
        audioSource.Stop();
        orig.Invoke(self);
    }

    public void CreateAudioSource()
    {
        Log("creating asrc");
        GameObject audioGO = new GameObject("HK Vocals Audio");
        audioSource = audioGO.AddComponent<AudioSource>();
        Object.DontDestroyOnLoad(audioGO);
    }

    private void TakeDamage(On.HealthManager.orig_TakeDamage orig, HealthManager self, HitInstance hitInstance)
    {
        orig(self, hitInstance);
        for (int i = 0; i < Dictionaries.HpListeners.Count; i++)
        {
            if (Dictionaries.HpListeners[i](self))
            {
                Dictionaries.HpListeners.RemoveAt(i);
                i--;
            }
        }
    }
    
    private string LanguageGet(string key, string sheetTitle, string orig) {
        // Make sure this is dreamnail text
        if (lastDreamnailedEnemy == null) return orig;

        // Format name to match the audio file format
        var split = key.Split('_');
        string title = split[0];
        string index = split[1];

        var listField = typeof(DNAudios).GetField(title);

        if (listField == null) {
            LogError($"Enemy {lastDreamnailedEnemy} ({title}) wasn't in list!");
            return orig;
        }

        List<string> list = (List<string>) listField.GetValue(null);

        // Select last dreamnailed enemy
        string name = list.FirstOrDefault(s => lastDreamnailedEnemy.StartsWith(s));
        if (name == null) {
            LogError($"{lastDreamnailedEnemy} isn't in the {list} list!");
            return orig;
        }

        string audioId = $"${name}$_{title}_{index}".ToUpper();

        if (listField.Name is "GH" or "GB1" or "GB2") {
            // Enemy name isn't used here, so just use the generic name
            audioId = $"${name}$_{title}_{index}".ToUpper();
        }
 
        Dictionaries.audioNames.FindAll(s => { Log(s); return false; });

        List<string> availableClips = Dictionaries.audioNames.FindAll(s => s.StartsWith(audioId));
        if (availableClips == null || availableClips.Count == 0) {
            LogError($"{audioId} is not in AudioBundle");
            return orig;
        }

        int voiceActor = Random.Range(1, availableClips.Count);
        AudioUtils.TryPlayAudioFor($"{audioId}_0_{voiceActor}");

        // Prevent it from running again incorrectly
        lastDreamnailedEnemy = null;

        return orig;
    }

    private void ShowConvo(On.EnemyDreamnailReaction.orig_ShowConvo orig, EnemyDreamnailReaction self) {
        lastDreamnailedEnemy = self.gameObject.name;
        orig(self);
    }

    private void EDNRStart(On.EnemyDreamnailReaction.orig_Start orig, EnemyDreamnailReaction self)
    {
        orig(self);
        //if (self.GetComponent<EnemyDeathEffects>() != null)
        self.gameObject.AddComponent<ExDNailReaction>();
    }

    private void PlayRandomClip(On.HutongGames.PlayMaker.Actions.AudioPlayerOneShot.orig_DoPlayRandomClip orig, AudioPlayerOneShot self)
    {
        orig(self);
        if (!RemoveOrigNPCSounds /*&& _globalSettings.testSetting == 0*/ && self.Fsm.Name == "Conversation Control")
        {
            HKVocals.CoroutineHolder.StartCoroutine(FadeOutClip(ReflectionHelper.GetField<AudioPlayerOneShot, AudioSource>(self, "audio")));
        }
    }

    private void FSMAwake(On.PlayMakerFSM.orig_Awake orig, PlayMakerFSM self)
    {
        orig(self);
        /*if (self.FsmGlobalTransitions.Any(t => t.EventName.ToLower().Contains("dream")))
        {
            self.MakeLog();
            foreach (FsmTransition t in self.FsmGlobalTransitions)
                Log(t.EventName);
        }*/
        if (Dictionaries.SceneFSMEdits.TryGetValue((UnityEngine.SceneManagement.SceneManager.GetActiveScene().name, self.gameObject.name, self.FsmName), out var sceneAction))
            sceneAction(self);
        if (Dictionaries.GoFSMEdits.TryGetValue((self.gameObject.name, self.FsmName), out var goAction))
            goAction(self);
        if (Dictionaries.FSMChanges.TryGetValue(self.FsmName, out var action))
            action(self);
        /*if (self.gameObject.name.ToLower().Contains("elderbug"))
        {
            foreach (FsmVar v in self.FsmStates.SelectMany(s => s.Actions.Where(a => a is CallMethodProper call && call.behaviour.Value.ToLower() == "dialoguebox").Cast<CallMethodProper>().SelectMany(c => c.parameters)))
                Log(v.variableName + "  " + v.Type + "  " + v.GetValue());
        }*/
    }

    private void ShowPage(On.DialogueBox.orig_ShowPage orig, DialogueBox self, int pageNum)
    {
        orig(self, pageNum);
        var convo = self.currentConversation + "_" + (self.currentPage - 1);
        LogDebug($"Showing page in {convo}");
        if (self.currentPage - 1 == 0)
        {
            AudioUtils.TryPlayAudioFor(convo,37f/60f);
        }
        else
        {
            AudioUtils.TryPlayAudioFor(convo, 3f/4f);
        }
        if (audioSource.isPlaying)
        {
            if (autoTextRoutine != null)
            {
                HKVocals.CoroutineHolder.StopCoroutine(autoTextRoutine);
            }
            autoTextRoutine = HKVocals.CoroutineHolder.StartCoroutine(AutoChangePage(self));
        }
    }

    private void SetConversation(On.DialogueBox.orig_SetConversation orig, DialogueBox self, string convName,
        string sheetName)
    {
        orig(self, convName, sheetName);
        Log("Started Conversation " + convName + " " + sheetName);
        //if (_globalSettings.testSetting == 0)
        AudioUtils.TryPlayAudioFor(convName);
    }

    public void CreateDreamDialogue(string convName, string sheetName, string enemyType = "", string enemyVariant = "", GameObject enemy = null)
    {
        PlayMakerFSM fsm = FsmVariables.GlobalVariables.GetFsmGameObject("Enemy Dream Msg").Value.LocateMyFSM("Display");
        fsm.Fsm.GetFsmString("Convo Title").Value = convName;
        fsm.Fsm.GetFsmString("Sheet").Value = sheetName;
        fsm.SendEvent("DISPLAY DREAM MSG");
    }

    public IEnumerator AutoChangePage(DialogueBox dialogueBox)
    {
        int newPageNum = dialogueBox.currentPage + 1;
        string oldConvoName = dialogueBox.currentConversation;
        yield return new WaitWhile(() => AudioUtils.IsPlaying() && dialogueBox && dialogueBox.currentPage < newPageNum && dialogueBox.currentConversation == oldConvoName);
        yield return new WaitForSeconds(1f/6f);//wait additional 1/6th second
        if (_globalSettings.autoScroll &&
            dialogueBox != null &&
            dialogueBox.currentPage < newPageNum &&
            dialogueBox.currentConversation == oldConvoName)
        {
            dialogueBox.ShowNextPage();
        }
    }

    private IEnumerator FadeOutClip(AudioSource source)
    {
        float volumeChange = source.volume / 100f;
        yield return new WaitForSeconds(1f);
        for (int i = 0; i < 100; i++)
            source.volume -= volumeChange;
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

    public void OnGrubFsm(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
    {
        orig(self);

        if (self.gameObject.scene.name == "Abyss_19" && self.gameObject.name == "Dream Dialogue" && self.FsmName == "npc_dream_dialogue")
        {
            if (_saveSettings.GrubConvo < 9)
            {
                _saveSettings.GrubConvo += 1;
                self.GetFsmStringVariable("Convo Name").Value = $"GRUB_BOTTLE_DREAM_S_REPEAT_0";
                self.GetFsmStringVariable("Sheet Name").Value = "Elderbug";
                AudioUtils.TryPlayAudioFor($"GRUB_BOTTLE_DREAM_S_REPEAT_0");
            }
            else
            {
                self.GetFsmStringVariable("Convo Name").Value = $"GRUB_BOTTLE_DREAM_S_{_saveSettings.GrubConvo}";
                self.GetFsmStringVariable("Sheet Name").Value = "Elderbug";
                AudioUtils.TryPlayAudioFor($"GRUB_BOTTLE_DREAM_S_{_saveSettings.GrubConvo}");
            }
        }
    }
    public string GrubKeys(string key, string sheettitle, string orig)
    {
        switch (key)
        {
            case "GRUB_BOTTLE_DREAM_S_0":
                return " ...Home...";
            case "GRUB_BOTTLE_DREAM_S_1":
                return "Why does it stare at me so?";
            case "GRUB_BOTTLE_DREAM_S_2":
                return "Has it not come to release me? To save me from this cruel fate?";
            case "GRUB_BOTTLE_DREAM_S_3 ":
                return "Repetitively now it’s fist draws back, as if readying to shatter this invisible prison. But it only swipes the air.";
            case "GRUB_BOTTLE_DREAM_S_4 ":
                return "It wishes not to destroy my confines, but my pride.";
            case "GRUB_BOTTLE_DREAM_S_5 ":
                return "Does it really intend to mock and shame a helpless grub as I? What evil bug it must be, to knowingly prolong this torture, torn from my kin.";
            case "GRUB_BOTTLE_DREAM_S_6":
                return "From my… Grubfather.";
            case "GRUB_BOTTLE_DREAM_S_7 ":
                return "I stare back with hopeful joy, *scoff* it must think me ignorant. If only it knew of my hatred";
            case "GRUB_BOTTLE_DREAM_S_8":
                return "When the time is right and this bug least expects it...";
            case "GRUB_BOTTLE_DREAM_S_9 ":
                return "I will gladly return the favor.";
            case "GRUB_BOTTLE_DREAM_S_REPEAT_0":
                return "…";
            default:
                return orig;
        }
    }
}
