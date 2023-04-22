namespace HKVocals.MajorFeatures;
using Newtonsoft.Json;

public static class AutomaticBossDialogue {

    // DN Dialogue prefix to allow for distinguishing of automatic and player-actived dialogue
    public const string ABDKeyPrefix = "HKVocals_ABD_";
    private const string ANY_GO = "*";

    private static List<Func<HealthManager, bool>> HpListeners = new List<Func<HealthManager, bool>>();

    struct FsmLocation {
        public string scene;
        public string go;
        public string fsm;

        public FsmLocation(string go, string fsm) {
            this.go = go;
            this.fsm = fsm;
            this.scene = null;
        }
    }

    struct ABDLine {
        public string[] lines;
        public float chance;
        public float wait;

        public ABDLine(string[] lines, float chance = 1f, float wait = 0) {
            this.lines = lines;
            this.chance = chance;
            this.wait = wait;
        }
    }

    struct ABDStates {
        public Dictionary<string, ABDLine> dialogueStates;
        public Dictionary<string, Func<GameObject, IEnumerator>> coroutineStates;
        public Action<GameObject> init;

        public ABDStates(Dictionary<string, ABDLine> dialogueStates, Dictionary<string, Func<GameObject,IEnumerator>> coroutineStates = null, Action<GameObject> init = null) {
            this.dialogueStates = dialogueStates;
            this.coroutineStates = coroutineStates;
            
            this.init = init;
        }    
    }

    private static readonly Dictionary<FsmLocation, ABDStates> BossDialogueGoFsm = new Dictionary<FsmLocation, ABDStates> {
        //{ ("Absolute Radiance", "Control"), AddToRadiances },
        //{ ("Absolute Radiance", "Phase Control"), AddToRadiances_Phase2 },
        //{ ("Radiance", "Control"), AddToRadiances },
        //{ ("Radiance", "Phase Control"), AddToRadiances_Phase2 },
        //{ ("Hornet Boss 1", "Control"), AddToHornets },
        //{ ("Hornet Boss 2", "Control"), AddToHornets },
        { new FsmLocation(ANY_GO, "FalseyControl"), new ABDStates(new Dictionary<string, ABDLine> {
            { "Start Fall", new ABDLine(new string[] { "FALSE_KNIGHT_1" }, 1.0f, 10f )}
        })},

        { new FsmLocation("Oro", "nailmaster"), new ABDStates(new Dictionary<string, ABDLine>(), new Dictionary<string, Func<GameObject, IEnumerator>> {
            { "Death Start", OroDialogue }
        }) },

        { new FsmLocation("Jar Collector", "Control"), new ABDStates(new Dictionary<string, ABDLine> {
            { "Slam", new ABDLine(new string[] { "JAR_COLLECTOR_1", "JAR_COLLECTOR_2", "JAR_COLLECTOR_3" }, 0.4f ) }
        })},

        { new FsmLocation("Dream Mage Lord Phase2", "Dream Mage Lord Phase2"), new ABDStates(new Dictionary<string, ABDLine> {
            { "Music", new ABDLine(new string[] { "MAGELORD_D_1" } ) }
        })},

        { new FsmLocation("Grey Prince", "Control"), new ABDStates(new Dictionary<string, ABDLine> {
            { "Jump", new ABDLine(new string[] { "GREY_PRINCE_1", "GREY_PRINCE_2", "GREY_PRINCE_3", "GREY_PRINCE_4", "GREY_PRINCE_5" }, 0.2f )},
            { "Spit Dir", new ABDLine(new string[] { "GREY_PRINCE_1", "GREY_PRINCE_2", "GREY_PRINCE_3", "GREY_PRINCE_4", "GREY_PRINCE_5" }, 0.2f )}
        })},

        // Absolute Radiance

        { new FsmLocation("Absolute Radiance", "Control"), new ABDStates(new Dictionary<string, ABDLine> {
            { "Set Arena 1", new ABDLine(new string[] { "RADIANCE_1" }, 1f, 5f )},
            { "Rage1 Tele", new ABDLine(new string[] { "RADIANCE_3" } )},
            { "Tendrils1", new ABDLine(new string[] { "RADIANCE_4" } )},
            { "Arena 2 Start", new ABDLine(new string[] { "RADIANCE_5" }, 1f, 2f )},
            { "Scream", new ABDLine(new string[] { "RADIANCE_6" }, 1f, 5f )}
        })},

        { new FsmLocation("Absolute Radiance", "Phase Control"), new ABDStates(new Dictionary<string, ABDLine> {
            { "Set Phase 2", new ABDLine(new string[] { "RADIANCE_2" } )}
        })},

        // Radiance

        { new FsmLocation("Radiance", "Control"), new ABDStates(new Dictionary<string, ABDLine> {
            { "Set Arena 1", new ABDLine(new string[] { "RADIANCE_1" }, 1f, 5f )},
            { "Rage1 Tele", new ABDLine(new string[] { "RADIANCE_3" } )},
            { "Tendrils1", new ABDLine(new string[] { "RADIANCE_4" } )},
            { "Arena 2 Start", new ABDLine(new string[] { "RADIANCE_5" }, 1f, 2f )},
            { "Ascend Tele", new ABDLine(new string[] { "RADIANCE_6" }, 1f, 5f )}
        })},

        { new FsmLocation("Radiance", "Phase Control"), new ABDStates(new Dictionary<string, ABDLine> {
            { "Set Phase 2", new ABDLine(new string[] { "RADIANCE_2" } )}
        })}

    };

    private static readonly Dictionary<FsmLocation, Dictionary<float, ABDLine>> HealthTriggers = new Dictionary<FsmLocation, Dictionary<float, ABDLine>> {
        { new FsmLocation("Dream Mage Lord", "Dream Mage Lord"), new Dictionary<float, ABDLine> {
            { 2f / 3f, new ABDLine(new string[] { "MAGELORD_D_2" }) }, 
            { 1f / 3f, new ABDLine(new string[] { "MAGELORD_D_3" }) }
        }}
    };

    private static Dictionary<FsmLocation, float> LastHealthValues = new Dictionary<FsmLocation, float>();
    private static Dictionary<FsmLocation, float> MaxHealthValues = new Dictionary<FsmLocation, float>();

    private static Dictionary<HKVocalsFsmData, Action<PlayMakerFSM>> BossDialogueSceneFsm = new Dictionary<HKVocalsFsmData, Action<PlayMakerFSM>>
    {
        { new("GG_Radiance", "Boss Control", "Control"), AddToRadiances_Spawn },
        { new("Dream_Final_Boss", "Boss Control", "Control"), AddToRadiances_Spawn },
    };

    public static void Hook() { 
        OnHealthManager.AfterOrig.Start += InitHpListeners;
        OnHealthManager.AfterOrig.TakeDamage += CheckHpListeners;

        foreach (var pair in BossDialogueGoFsm) {
            if (pair.Value.coroutineStates != null) {
                foreach (var coroutine in pair.Value.coroutineStates) {
                    // This doesn't work in the case of ANY_GO
                    Hooks.HookStateEnteredFromTransition(new FSMData(pair.Key.go, pair.Key.fsm, coroutine.Key), (PlayMakerFSM FSM, string type) => HKVocals.CoroutineHolder.StartCoroutine(coroutine.Value(FSM.gameObject)));
                }
            }

            foreach (var dialogue in pair.Value.dialogueStates) {

                FSMData data = (pair.Key.go == ANY_GO) ?
                    new FSMData(pair.Key.fsm, dialogue.Key) :
                    new FSMData(pair.Key.go, pair.Key.fsm, dialogue.Key);

                Hooks.HookStateEnteredFromTransition(
                    data,
                    (PlayMakerFSM fsm, string type) => {
                        PlayABDLine(dialogue.Value, fsm.gameObject);
                    }
                );
            }

            if (pair.Value.init != null) {
                // @TODO: This might not work for every location
                pair.Value.init(GameObject.Find(pair.Key.go));
            }
        }
    }

    private static void PlayABDLine(ABDLine line, GameObject go) {
        string key = line.lines[Random.Range(0, line.lines.Length)];
        bool play = Random.value <= line.chance;

        if (play) HKVocals.CoroutineHolder.StartCoroutine(PlayLineAfter(go, key, line.wait));
    }

    private static IEnumerator PlayLineAfter(GameObject go, string key, float time) {
        yield return new WaitForSeconds(time);
        DreamNailDialogue.InvokeAutomaticBossDialogue(go, key);
    }

    public static void OnFsmInit() {}

    private static FsmLocation? GetHealthTrigger(HealthManager hm) {
        GameObject go = hm.gameObject;
        PlayMakerFSM fsm = go.GetComponent<PlayMakerFSM>();

        if (fsm == null) return null;

        foreach (var entry in HealthTriggers) {
            if (entry.Key.fsm == fsm.name && entry.Key.go == go.name) {
                return entry.Key;
            }
        }

        HKVocals.instance.Log($"Found an HM/FSM with no matching trigger. GameObject: {go.name} FSM: {fsm.name}");

        return null;
    }

    private static void InitHpListeners(OnHealthManager.Delegates.Params_Start args) {

        var location = GetHealthTrigger(args.self);
        if (location == null) return;

        var trigger = (FsmLocation) location;

        LastHealthValues[trigger] = args.self.hp;
        MaxHealthValues[trigger] = args.self.hp;
    }
    
    private static void CheckHpListeners(OnHealthManager.Delegates.Params_TakeDamage args) {
        var location = GetHealthTrigger(args.self);
        if (location == null) return;

        var trigger = (FsmLocation) location;

        float lastHpPercent = (float) LastHealthValues[trigger] / (float) MaxHealthValues[trigger];
        float currentHpPercent = (float) args.self.hp / (float) MaxHealthValues[trigger];

        foreach (var entry in HealthTriggers[trigger]) {
            if (lastHpPercent > entry.Key && currentHpPercent <= entry.Key) PlayABDLine(entry.Value, args.self.gameObject);
        }

        LastHealthValues[trigger] = args.self.hp;
    }
    
    private static void AddHPDialogue(HealthManager hm, string key, int hpBenchmark) {
        HpListeners.Add(hmInstance => {
            if (hmInstance == hm && hmInstance.hp < hpBenchmark) {
                DreamNailDialogue.InvokeAutomaticBossDialogue(hm.gameObject, key);
                return true;
            }

            return false;
        });
    }

    private static void AddToPaleLurker(PlayMakerFSM fsm)
    {
        HKVocals.instance.LogDebug("Adiing ADB to Pale Lurker");
        DreamDialogueAction action = new DreamDialogueAction(new string[] { ABDKeyPrefix + "LURKER_1", ABDKeyPrefix + "LURKER_2", ABDKeyPrefix +"LURKER_3" }, "Enemy Dreams") { waitTime = 3f, convoMode = DreamDialogueAction.ConvoMode.Random, convoOccurances = new int[] { -1, 0 } };
        fsm.InsertFsmMethod("Aleart Anim", () => action.convoOccurances[0] = 0, 0);
        fsm.InsertFsmAction("Hop Antic", action, 0);
    }

    private static void AddToRadiances(PlayMakerFSM fsm)
    {
        if (BossSequenceController.IsInSequence)
        {
            HKVocals.instance.LogDebug("Adiing ADB to Radiances");
            fsm.InsertFsmAction("Rage1 Start", new DreamDialogueAction(ABDKeyPrefix +"RADIANCE_3", "Enemy Dreams"), 0);
            fsm.InsertFsmAction("Tendrils1", new DreamDialogueAction(ABDKeyPrefix + "RADIANCE_4", "Enemy Dreams") { waitTime = 1f }, 0);
            fsm.InsertFsmAction("Arena 2 Start", new DreamDialogueAction(ABDKeyPrefix + "RADIANCE_5", "Enemy Dreams") { waitTime = 2f }, 0);
            fsm.InsertFsmAction("Ascend Tele", new DreamDialogueAction(ABDKeyPrefix + "RADIANCE_6", "Enemy Dreams") { waitTime = 5f }, 0);
        }
    }
    
    private static void AddToRadiances_Phase2(PlayMakerFSM fsm)
    {
        if (BossSequenceController.IsInSequence)
        {
            HKVocals.instance.LogDebug("Adiing ADB to Radiances_Phase2");
            if (fsm.FsmName == "Phase Control")
            {
                fsm.AddFsmAction("Set Phase 2", new DreamDialogueAction(ABDKeyPrefix + "RADIANCE_2", "Enemy Dreams"));
            }
        }
    }
    
    private static void AddToRadiances_Spawn(PlayMakerFSM fsm)
    {
        if (BossSequenceController.IsInSequence)
        {
            HKVocals.instance.LogDebug("Adiing ADB to Radiances_Spawn");
            fsm.AddFsmAction("Flash Down", new DreamDialogueAction(ABDKeyPrefix + "RADIANCE_1", "Enemy Dreams") { waitTime = 5 });
        }
    }
    
    //todo: check this? it doesnt make sense
    private static void AddToHornets(PlayMakerFSM fsm)
    {
        if ((!MiscUtils.GetCurrentSceneName().Contains("GG") && fsm.gameObject.name.Contains("1")) ||
            (BossSequenceController.IsInSequence && fsm.gameObject.name.Contains("2")))
        {
            HKVocals.instance.LogDebug("Adiing ADB to Hornets");
            string namePart = BossSequenceController.IsInSequence ? "GG" : "GREENPATH";
            HealthManager hm = fsm.GetComponent<HealthManager>();
            //AddHPDialogue(hm, new DreamDialogueAction(ABDKeyPrefix + "HORNET_" + namePart + "_1", "Enemy Dreams"), (3 * hm.hp) / 4);
            //AddHPDialogue(hm, new DreamDialogueAction(ABDKeyPrefix + "HORNET_" + namePart + "_2", "Enemy Dreams"), hm.hp / 2);
            //AddHPDialogue(hm, new DreamDialogueAction(ABDKeyPrefix + "HORNET_" + namePart + "_3", "Enemy Dreams"), hm.hp / 4);
        }
    }

    private static IEnumerator OroDialogue(GameObject boss) {
        yield return new WaitForSeconds(1f);
        DreamNailDialogue.InvokeAutomaticBossDialogue(boss, "ORO_1");
        yield return new WaitForSeconds(AudioPlayer.GetAudioFor("$Oro$_ORO_1_0_1").length + 0.5f); // probably should automatically format keys like this
        DreamNailDialogue.InvokeAutomaticBossDialogue(boss, "ORO_2");
        yield return new WaitForSeconds(AudioPlayer.GetAudioFor("$Oro$_ORO_2_0_1").length + 0.5f);
        DreamNailDialogue.InvokeAutomaticBossDialogue(boss, "MATO_2");
    }

    private static void AddToSoulTyrant_Phase2(PlayMakerFSM fsm)
    {
        HKVocals.instance.LogDebug("Adiing ADB to Soul Tyrant Phase 2");
        //i chose music cuz its after the wait and its just when tyrant dives
        fsm.InsertFsmAction("Music", new DreamDialogueAction(ABDKeyPrefix + "MAGELORD_D_1","Enemy Dreams"), 0);
    }
}