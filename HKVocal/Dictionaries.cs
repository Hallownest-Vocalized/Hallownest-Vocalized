namespace HKVocals;

public static class Dictionaries
{
    public static readonly Dictionary<string, string[]> EnemyVariants = new Dictionary<string, string[]>()
    {
        { "", new string[] { "" } }
    };

    public static readonly Dictionary<(string, string, string), Action<PlayMakerFSM>> SceneFSMEdits = new Dictionary<(string, string, string), Action<PlayMakerFSM>>()
    { 
        {("GG_Radiance", "Boss Control", "Control"), fsm => { if (BossSequenceController.IsInSequence) fsm.AddAction("Flash Down", new DreamDialogueAction("RADIANCE_1", "Enemy Dreams") { waitTime = 5 }); } },
        { ("Dream_Final_Boss", "Boss Control", "Control"), fsm => { if (BossSequenceController.IsInSequence) fsm.AddAction("Flash Down", new DreamDialogueAction("RADIANCE_1", "Enemy Dreams") { waitTime = 5 }); } }
    };

    public static readonly Dictionary<(string, string), Action<PlayMakerFSM>> GoFSMEdits = new Dictionary<(string, string), Action<PlayMakerFSM>>()
    {
        { ("DialogueManager", "Box Open Dream"), FSMEdits.BoxOpenDream },
        { ("Absolute Radiance", "Control"), FSMEdits.RadianceControl },
        { ("Absolute Radiance", "Phase Control"), fsm => { if (BossSequenceController.IsInSequence) fsm.AddAction("Set Phase 2", new DreamDialogueAction("RADIANCE_2", "Enemy Dreams")); } },
        { ("Radiance", "Control"), FSMEdits.RadianceControl },
        { ("Radiance", "Phase Control"), fsm => fsm.AddAction("Set Phase 2", new DreamDialogueAction("RADIANCE_2", "Enemy Dreams")) },
        { ("Hornet Boss 1", "Control"), FSMEdits.HornetControl },
        { ("Hornet Boss 2", "Control"), FSMEdits.HornetControl },
        { ("Oro", "nailmaster"), FSMEdits.NailmasterControl },
        { ("Mato", "nailmaster"), fsm => fsm.AddMethod("Death Start", () => { if (HKVocals.instance.audioSource.clip.name.Contains("MATO")) HKVocals.instance.audioSource.Stop(); }) },
        { ("Jar Collector", "Control"), FSMEdits.JarCollectorControl },
        { ("Dream Mage Lord Phase2", "Mage Lord 2"), FSMEdits.DreamMageLordPhase2 },
        { ("Dream Mage Lord", "Mage Lord"), FSMEdits.DreamMageLord },
        { ("Grey Prince", "Control"), FSMEdits.GreyPrinceControl },
        { ("Enemy List", "Item List Control"), FSMEdits.JournalText },
        { ("Inv", "Update Text"), FSMEdits.InventoryText },
        { ("Charms", "Update Text"), FSMEdits.CharmText },
        { ("Item List", "Item List Control"), FSMEdits.ShopText },
        { ("Shop Menu", "shop_control"), FSMEditUtils.ShopMenuOpenClose },
        { ("Inventory", "Inventory Control"), FSMEditUtils.InventoryOpenClose },
        { ("Text", "Dialogue Page Control"), FSMEdits.ContinueScrollOnConvoEnd_AndScrollLock },
        { ("Iselda", "Shop Anim"), FSMEdits.IseldaAudio },
        { ("Mr Mushroom NPC", "Control"), FSMEdits.MrMushroomAudio },

        { ("Zote Boss", "Control"), EternalOrdeal.EternalOrdeal_Normal },
        { ("Zote Crew Normal (1)", "Control"), EternalOrdeal.EternalOrdeal_Normal },
        { ("Zote Crew Normal (2)", "Control"), EternalOrdeal.EternalOrdeal_Normal },
        { ("Zote Crew Normal (3)", "Control"), EternalOrdeal.EternalOrdeal_Normal },
        { ("Zote Crew Normal (4)", "Control"), EternalOrdeal.EternalOrdeal_Normal },
        { ("Zote Crew Fat (1)", "Control"), EternalOrdeal.EternalOrdeal_Normal },
        { ("Zote Crew Fat (2)", "Control"), EternalOrdeal.EternalOrdeal_Normal },
        { ("Zote Crew Fat (3)", "Control"), EternalOrdeal.EternalOrdeal_Normal },
        { ("Zote Crew Tall (1)", "Control"), EternalOrdeal.EternalOrdeal_Normal },
        { ("Zote Crew Tall", "Control"), EternalOrdeal.EternalOrdeal_Normal },
        { ("Zote Balloon (1)", "Control"), EternalOrdeal.EternalOrdeal_Balloon },
        { ("Zote Balloon Ordeal", "Control"), EternalOrdeal.EternalOrdeal_Balloon },
        { ("Ordeal Zoteling", "Control"), EternalOrdeal.EternalOrdeal_Zoteling },
        { ("Ordeal Zoteling (1)", "Control"), EternalOrdeal.EternalOrdeal_Zoteling },
        { ("Zote Fluke", "Control"), EternalOrdeal.EternalOrdeal_Other },
        { ("Zote Salubra", "Control"), EternalOrdeal.EternalOrdeal_Other },
        { ("Zote Turret", "Control"), EternalOrdeal.EternalOrdeal_Other },
        { ("Zote Thwomp", "Control"), EternalOrdeal.EternalOrdeal_Thwomp },
    };

    public static readonly Dictionary<string, Action<PlayMakerFSM>> FSMChanges = new Dictionary<string, Action<PlayMakerFSM>>()
    {
        { "Conversation Control", FSMEdits.ConversationControl },
        { "FalseyControl", FSMEdits.FalseyControl },
        { "LurkerControl", FSMEdits.LurkerControl },
    };

    //probably needs to be changed. just a placeholder
    public static readonly List<string> NoAudioMixer = new List<string>()
    {
        //menderbug
    };

    public static List<Func<HealthManager, bool>> HpListeners = new List<Func<HealthManager, bool>>();
    public static Dictionary<AssetBundle, string[]> CustomAudioBundles = new Dictionary<AssetBundle, string[]>();
    public static Dictionary<AudioClip, string> CustomAudioClips = new Dictionary<AudioClip, string>();
    public static List<string> audioExtentions = new List<string>() { ".mp3", ".wav" };
    public static List<string> audioNames = new List<string>();
}