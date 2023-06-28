using System.Text.RegularExpressions;

namespace HKVocals.MajorFeatures;

public static class DreamNailDialogue
{
    private static GameObject lastDreamnailedEnemy;
    private static Regex enemyTrimRegex = new Regex("([^0-9\\(\\)]+)", RegexOptions.Compiled);
    
    public delegate void OnPlayDreamDialogueHandler();
    
    public static event OnPlayDreamDialogueHandler OnPlayDreamDialogue;

    private static string[] AutomaticKeys = new string[] {
        "KING_ABYSS_",
        "GRIMM_REMINDER"
    };
    
    public static void Hook()
    {
        ModHooks.LanguageGetHook += PlayDreamNailDialogue;
        OnEnemyDreamnailReaction.AfterOrig.Start += AddCancelDreamDialogueOnDeath;
        OnEnemyDreamnailReaction.BeforeOrig.ShowConvo += SetLastDreamNailedEnemy;
    }
    
     public static string GetUniqueId(Transform transform, string path = "")
     {
        if (transform.parent == null)
        {
            return $"{MiscUtils.GetCurrentSceneName()}:" + path + transform.name;
        }
        else
        {
            return GetUniqueId(transform.parent, path + $"{transform.name}/");
        }
    }

    public static void InvokeAutomaticBossDialogue(GameObject boss, string key) {
        lastDreamnailedEnemy = boss;
        if (HKVocals._globalSettings.automaticBossDialogue || HKVocals._globalSettings.dnDialogue)
        {
            FSMEditUtils.CreateDreamDialogue(AutomaticBossDialogue.ABDKeyPrefix + key, "Enemy Dreams");
        }
    }

    private static string PlayDreamNailDialogue(string key, string sheetTitle, string orig) {
        // Automatic keys (abyss climb, grimm)
        foreach (string auto in AutomaticKeys) {
            if (key.StartsWith(auto)) {
                MixerLoader.SetSnapshot(key.Contains("ABYSS") ? Snapshots.Cave : Snapshots.Dream);
                AudioPlayer.TryPlayAudioFor($"{key}_0");
                return orig;
            }
        }

        //check for if the passed in key is passed in by AutomaticBossDialogue
        bool isAutomaticBossDialogue = false;

        HKVocals.instance.Log($"Language get attempted with key {key}");
        
        if (key.StartsWith(AutomaticBossDialogue.ABDKeyPrefix)) {
            // get the original key by removing the prefix
            key = key.Remove(0, AutomaticBossDialogue.ABDKeyPrefix.Length);
            orig = Language.Language.GetInternal(key, sheetTitle);
            isAutomaticBossDialogue = true;
        }

        if (key == "FK_MACE_1")
        {
            AudioPlayer.TryPlayAudioFor($"$Mace Head Bug$_FK_MACE_{Random.Range(1, 5)}_0_1".ToUpper());
        }
        
        // Make sure this is dreamnail text or ABD text
        if (lastDreamnailedEnemy == null && !isAutomaticBossDialogue) {
            return orig;
        }

        //dont play audio if the dn dialogue toggle is off
        if (!HKVocals._globalSettings.dnDialogue) {
            //but also do play it if its an AutomaticBossDialogue and that setting is turned on
            if (!(HKVocals._globalSettings.automaticBossDialogue && isAutomaticBossDialogue)) {
                return orig;
            }
        }

        // Grab the ID and name now
        string id = GetUniqueId(lastDreamnailedEnemy.transform);
        string name = enemyTrimRegex.Match(lastDreamnailedEnemy.name).Value.Trim();

        // Prevent it from running again incorrectly
        lastDreamnailedEnemy = null;

        // For the special case of grouped (generic) enemies
        if (DNGroups.ContainsKey(name)) name = DNGroups[name];

        List<string> availableClips = HallownestVocalizedAudioLoaderMod.AudioNames.FindAll(s => s.Contains($"${name}$_{key}".ToUpper()));
        if (availableClips == null || availableClips.Count == 0) 
        {
            HKVocals.instance.LogError($"No clips for ${name}$_{key}");
            return orig;
        }

        // Either use the already registered VA or make one and save it
        int voiceActor;

        if (HKVocals._saveSettings.PersistentVoiceActors.ContainsKey(id)) {
            voiceActor = HKVocals._saveSettings.PersistentVoiceActors[id];
        } else {
            voiceActor = Random.Range(1, availableClips.Count + 1);
            HKVocals._saveSettings.PersistentVoiceActors[id] = voiceActor;
        }
        
        MixerLoader.SetSnapshot(Snapshots.Dream);

        bool didPlay = AudioPlayer.TryPlayAudioFor($"${name}$_{key}_0_{voiceActor}".ToUpper());

        if (didPlay) {
            OnPlayDreamDialogue?.Invoke();
        }
        
        // Check for Oro's alt delivery
        if (key == "ORO_ALT_2") return Language.Language.Get("ORO_2", "Enemy Dreams");
        return orig;
    }

    private static void SetLastDreamNailedEnemy(OnEnemyDreamnailReaction.Delegates.Params_ShowConvo args) 
    {
        lastDreamnailedEnemy = args.self.gameObject;
    }

    private static void AddCancelDreamDialogueOnDeath(OnEnemyDreamnailReaction.Delegates.Params_Start args)
    {
        args.self.gameObject.AddComponent<CancelDreamDialogueOnDeath>();
    }
    
    
    
    public static Dictionary<string, string> DNGroups = new Dictionary<string, string>() {
        { "Crawler", "GB1" }, 
        { "Climber", "GB1" }, 
        { "Buzzer", "GB1" }, 
        { "Spitter", "GB1" }, 
        { "Hatcher", "GB1" }, 
        { "Roller", "GB1" }, 
        { "Mosquito", "GB1" }, 
        { "Fat Fly", "GB1" }, 
        { "Acid Walker", "GB1" }, 
        { "Acid Flyer", "GB1" }, 
        { "Ceiling Dropper", "GB1" }, 
        { "Flip Hopper", "GB1" }, 
        { "Inflater", "GB1" }, 
        { "Mines Crawler", "GB1" }, 
        { "Crystal Flyer", "GB1" }, 
        { "Baby Centipede", "GB1" }, 
        { "Centipede Hatcher", "GB1" }, 
        { "Tiny Spider", "GB1" }, 
        { "Spider Mini", "GB1" }, 
        { "Super Spitter", "GB1" }, 
        { "Hopper", "GB1" }, 
        { "Grass Hopper", "GB1" }, 
        { "Colosseum_Armoured_Roller", "GB1" }, 
        { "Blobble", "GB1" }, 
        { "Colosseum Grass Hopper", "GB1" }, 
        { "Colosseum_Armoured_Mosquito", "GB1" }, 
        { "Spawn Roller v", "GB1" }, 
        { "Spitter R", "GB1" }, 
        { "Buzzer R", "GB1" }, 
        { "Roller R", "GB1" }, 
        { "Super Spitter Col", "GB1" }, 
        { "Buzzer Col", "GB1" }, 
        { "Ceiling Dropper Col", "GB1" }, 
        { "Baby Centipede Spawner", "GB1" },
        { "Super Spitter R", "GB1" },
        { "Colosseum_Armoured_Roller R", "GB1" },
        { "Colosseum_Armoured_Mosquito R", "GB1" },

        { "Worm", "GB2" }, 
        { "Big Centipede", "GB2" }, 
        { "Crystal Crawler", "GB2" }, 
        { "Blow Fly", "GB2" }, 
        { "Giant Hopper", "GB2" }, 
        { "Big Centipede Col", "GB2" }, 

        { "Zombie Runner", "GH" }, 
        { "Zombie Hornhead", "GH" }, 
        { "Zombie Leaper", "GH" }, 
        { "Zombie Barger", "GH" }, 
        { "Zombie Shield", "GH" }, 
        { "Zombie Guard", "GH" }, 
        { "Garden Zombie", "GH" },
        { "Zombie Fungus A", "GH" },
        { "Zombie Fungus B", "GH" },
        { "Zombie Runner Sp", "GH" },
        { "Zombie Hornhead Sp", "GH" }
    };
}