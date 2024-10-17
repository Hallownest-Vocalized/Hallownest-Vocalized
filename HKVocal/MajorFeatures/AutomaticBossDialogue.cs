namespace HKVocals.MajorFeatures;
using Newtonsoft.Json;

public static class AutomaticBossDialogue {

    // DN Dialogue prefix to allow for distinguishing of automatic and player-activated dialogue
    public const string ABDKeyPrefix = "HKVocals_ABD_";
    private const string ANY_GO = "*";

    struct FsmLocation(string go, string fsm, string scene = null) {
        public string scene = scene;
        public string go = go;
        public string fsm = fsm;
    }

    struct GenericBoss(string go, string key, bool requiresDreamNail = false) {
        public string go = go;
        public string key = key;
        public bool requiresDreamNail = requiresDreamNail;
    }

    struct ABDLine(string[] lines, float chance = 1f, float wait = 0) {
        public string[] lines = lines;
        public float chance = chance;
        public float wait = wait;
    }

    struct ABDStates(Dictionary<string, ABDLine> dialogueStates, Dictionary<string, Func<GameObject, IEnumerator>> coroutineStates = null, Action<GameObject> init = null) {
        public Dictionary<string, ABDLine> dialogueStates = dialogueStates;
        public Dictionary<string, Func<GameObject, IEnumerator>> coroutineStates = coroutineStates;
        public Action<GameObject> init = init;
    }

    public enum BossClassification {
        Normal,
        RequiresDreamNail
    }

    private static readonly Dictionary<FsmLocation, ABDStates> BossDialogueGoFsm = new Dictionary<FsmLocation, ABDStates> {
        { new FsmLocation("False Knight New", "FalseyControl"), new ABDStates(new Dictionary<string, ABDLine> {
            { "Start Fall", new ABDLine(["FALSE_KNIGHT_1"], 1f, 5f )},
            { "Recover", new ABDLine(["FALSE_KNIGHT_2", "FALSE_KNIGHT_3"], 1f, 1f )},
        })},
        
        { new FsmLocation("False Knight Dream", "FalseyControl"), new ABDStates(new Dictionary<string, ABDLine> {
            { "Start Fall", new ABDLine(["FALSE_KNIGHT_D_1"], 1f, 5f )},
            { "Recover", new ABDLine(["FALSE_KNIGHT_D_2", "FALSE_KNIGHT_D_3"], 1f, 1f )},
        })},

        { new FsmLocation("Oro", "nailmaster"), new ABDStates(new Dictionary<string, ABDLine>(), new Dictionary<string, Func<GameObject, IEnumerator>> {
            { "Death Start", OroDialogue }
        }) },

        { new FsmLocation("Jar Collector", "Control"), new ABDStates(new Dictionary<string, ABDLine> {
            { "Slam", new ABDLine(["JAR_COLLECTOR_1", "JAR_COLLECTOR_2", "JAR_COLLECTOR_3"], 0.4f ) }
        })},

        { new FsmLocation("Dream Mage Lord Phase2", "Mage Lord 2"), new ABDStates(new Dictionary<string, ABDLine> {
            { "Music", new ABDLine(["MAGELORD_D_1"] ) }
        })},

        { new FsmLocation("Grey Prince", "Control"), new ABDStates(new Dictionary<string, ABDLine> {
            { "Jump", new ABDLine(["GREY_PRINCE_1", "GREY_PRINCE_2", "GREY_PRINCE_3", "GREY_PRINCE_4", "GREY_PRINCE_5"], 0.2f )},
            { "Spit Dir", new ABDLine(["GREY_PRINCE_1", "GREY_PRINCE_2", "GREY_PRINCE_3", "GREY_PRINCE_4", "GREY_PRINCE_5"], 0.2f )}
        })},

        { new FsmLocation("Dung Defender", "Dung Defender"), new ABDStates(new Dictionary<string, ABDLine>(), new Dictionary<string, Func<GameObject, IEnumerator>> {
            { "Wake", DungDefenderDialogue }
        }) },

        // Absolute Radiance

        { new FsmLocation("Absolute Radiance", "Control"), new ABDStates(new Dictionary<string, ABDLine> {
            { "Set Arena 1", new ABDLine(["RADIANCE_1"], 1f, 5f )},
            { "Rage1 Tele", new ABDLine(["RADIANCE_3"] )},
            { "Tendrils1", new ABDLine(["RADIANCE_4"] )},
            { "Arena 2 Start", new ABDLine(["RADIANCE_5"], 1f, 2f )},
            { "Scream", new ABDLine(["RADIANCE_6"], 1f, 5f )}
        })},

        { new FsmLocation("Absolute Radiance", "Phase Control"), new ABDStates(new Dictionary<string, ABDLine> {
            { "Set Phase 2", new ABDLine(["RADIANCE_2"] )}
        })},

        // Radiance

        { new FsmLocation("Radiance", "Control"), new ABDStates(new Dictionary<string, ABDLine> {
            { "Set Arena 1", new ABDLine(["RADIANCE_1"], 1f, 5f )},
            { "Rage1 Tele", new ABDLine(["RADIANCE_3"] )},
            { "Tendrils1", new ABDLine(["RADIANCE_4"] )},
            { "Arena 2 Start", new ABDLine(["RADIANCE_5"], 1f, 2f )},
            { "Ascend Tele", new ABDLine(["RADIANCE_6"], 1f, 5f )}
        })},

        { new FsmLocation("Radiance", "Phase Control"), new ABDStates(new Dictionary<string, ABDLine> {
            { "Set Phase 2", new ABDLine(["RADIANCE_2"] )}
        })}


    };

    private static readonly Dictionary<FsmLocation, Dictionary<float, ABDLine>> HealthTriggers = new() {
        { new FsmLocation("Dream Mage Lord", "Dream Mage Lord"), new Dictionary<float, ABDLine> {
            { 2f / 3f, new ABDLine(["MAGELORD_D_2"]) }, 
            { 1f / 3f, new ABDLine(["MAGELORD_D_3"]) }
        }},
        { new FsmLocation("Hornet Boss 1", "Hornet Boss 1", "Fungus1_04_boss"), new Dictionary<float, ABDLine> {
            { 3f / 4f, new ABDLine(["HORNET_GREENPATH_1"]) }, 
            { 2f / 4f, new ABDLine(["HORNET_GREENPATH_2"]) },
            { 1f / 4f, new ABDLine(["HORNET_GREENPATH_3"]) }
        }},
        { new FsmLocation("Hornet Boss 2", "Hornet Boss 2", "GG_Hornet_2"), new Dictionary<float, ABDLine> {
            { 3f / 4f, new ABDLine(["HORNET_GG_1"]) }, 
            { 2f / 4f, new ABDLine(["HORNET_GG_2"]) },
            { 1f / 4f, new ABDLine(["HORNET_GG_3"]) }
        }},
        { new FsmLocation("Oro", "Oro"), new Dictionary<float, ABDLine> {
            { 0.3f, new ABDLine(["ORO_1"]) }
        }}
    };

    // Generic bosses (Play lines at 75%, 50%, 25% HP)
    // From the GameObject name only
    private static readonly List<GenericBoss> GenericBosses = [
        new GenericBoss("Hive Knight", "HIVE_KNIGHT", true),
        new GenericBoss("Mawlek Body", "MAWLEK", true),
        new GenericBoss("Fluke Mother", "FLUKEMOTHER", true),
        new GenericBoss("Lobster", "GENERIC", true),
        new GenericBoss("Giant Fly", "GRUZMOTHER", true),
        new GenericBoss("Mega Moss Charger", "GENERIC", true),
        new GenericBoss("Mage Knight", "MAGE_KNIGHT", true),
        new GenericBoss("Mega Jellyfish", "MEGAJELLYFISH", true),

        new GenericBoss("Mega Zombie Beam Miner", "ZOMBIE_MEGA_MINER"),
        new GenericBoss("Lancer", "ZOMBIE"),
        new GenericBoss("Sly Boss", "SLY"),
        new GenericBoss("Grimm Boss", "GRIMM"),
        new GenericBoss("Nightmare Grimm Boss", "NIGHTMARE_GRIMM"),
        new GenericBoss("Sheo Boss", "SHEO"),
        new GenericBoss("Mantis Traitor Lord", "MANTIS_TRAITOR"),
        new GenericBoss("Zote Boss", "ZOTE")
    ];

    private static readonly Dictionary<FsmLocation, float> LastHealthValues = [];
    private static readonly Dictionary<FsmLocation, float> MaxHealthValues = [];

    public static void Hook() {     
        // Add all the generic bosses to HealthTriggers
        foreach (var boss in GenericBosses) {
            // Get all dialogue
            var keys = AudioAPI.AudioNames.FindAll(s => s.StartsWith(boss.key.ToUpper()));
            int lineCount = Math.Min(keys.Count, 3);

            var lines = new Dictionary<float, ABDLine>();

            for (int i = 0; i < lineCount; i ++) {
                var keyIndex = Random.Range(0, keys.Count);

                float percentage = (i + 1f) / (lineCount + 1f);
                lines[percentage] = new ABDLine([ keys[keyIndex] ]);

                keys.RemoveAt(keyIndex);
            }
            
            HealthTriggers.Add(
                new FsmLocation(boss.go, ""), // Empty FSM name for generic bosses
                lines
            );
        }

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

            // @TODO: This might not work for every location
            pair.Value.init?.Invoke(GameObject.Find(pair.Key.go));
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
            // Generic boss check (empty FSM)
            if (entry.Key.fsm == "" && go.name == entry.Key.go) return entry.Key;

            if (entry.Key.fsm == fsm.name && entry.Key.go == go.name && (entry.Key.scene == null || entry.Key.scene == go.scene.name)) {
                return entry.Key;
            }
        }

        HKVocals.instance.LogDebug($"Found an HM/FSM with no matching trigger. GameObject: {go.name} FSM: {fsm.name} Scene: ${go.scene.name}");

        return null;
    }

    private static void InitHpListeners(OnHealthManager.Delegates.Params_Start args) {        
        // Otherwise, use the manual health trigger registration
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
            if (lastHpPercent > entry.Key && currentHpPercent <= entry.Key) {
                // Health has gone below a threshold, so play the line
                // However, one edge case: if it's a generic boss with
                // requiresDreamNail set to true and the player doesn't
                // have the DN, don't play

                // We should probably have a lookup table for boss gos
                var matchingGenericBosses = GenericBosses.Where(boss => boss.go == trigger.go);
                if (matchingGenericBosses.Count() == 0) {
                    // Not a generic boss, play
                    PlayABDLine(entry.Value, args.self.gameObject);
                } else {
                    var genericBoss = matchingGenericBosses.First();

                    if (
                        (genericBoss.requiresDreamNail && PlayerData.instance.hasDreamNail) ||
                        (!genericBoss.requiresDreamNail)
                    ) {
                        PlayABDLine(entry.Value, args.self.gameObject);
                    }
                }
            }
        }

        LastHealthValues[trigger] = args.self.hp;
    }

    private static IEnumerator OroDialogue(GameObject oro) {
        GameObject mato = GameObject.Find("Mato");
        yield return new WaitForSeconds(10f);
        DreamNailDialogue.InvokeAutomaticBossDialogue(mato, "MATO_1");
        yield return new WaitForSeconds(AudioPlayer.GetAudioFor("$Mato$_MATO_1_0_1").length + 0.5f); // probably should automatically format keys like this
        DreamNailDialogue.InvokeAutomaticBossDialogue(oro, "ORO_ALT_2");
        yield return new WaitForSeconds(AudioPlayer.GetAudioFor("$Oro$_ORO_ALT_2_0_1").length + 0.5f);
        DreamNailDialogue.InvokeAutomaticBossDialogue(mato, "MATO_2");
    }

    private static IEnumerator DungDefenderDialogue(GameObject boss) {
        yield return new WaitForSeconds(5f);
        DreamNailDialogue.InvokeAutomaticBossDialogue(boss, "DUNG_DEF_1");
        yield return new WaitForSeconds(AudioPlayer.GetAudioFor("$Dung Defender$_DUNG_DEF_1_0_1").length + 4f);
        DreamNailDialogue.InvokeAutomaticBossDialogue(boss, "DUNG_DEF_2");
        yield return new WaitForSeconds(AudioPlayer.GetAudioFor("$Dung Defender$_DUNG_DEF_2_0_1").length + 4f);
        DreamNailDialogue.InvokeAutomaticBossDialogue(boss, "DUNG_DEF_3");
    }

}