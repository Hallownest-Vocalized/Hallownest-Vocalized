using Satchel;

namespace HKVocals.MajorFeatures;

/// <summary>
/// Plays dialogue when NPC speaks
/// </summary>
public static class NPCDialogue
{
    public static bool DidPlayAudioOnDialogueBox;

    public delegate void OnPlayNPCDialogueHandler();

    public static event OnPlayNPCDialogueHandler OnPlayNPCDialogue;

    public static List<int> CollectorVAs = new List<int>() { 28, 30, 31 };

    public static Dictionary<string, int> GrubVAs = new()
    {
        {"Crossroads_05", 1},
        {"Crossroads_48", 2},
        {"Crossroads_03", 3},
        {"Crossroads_31", 4},
        {"Crossroads_35", 5},
        {"Fungus1_06", 6},
        {"Fungus1_07", 7},
        {"Fungus1_13", 8},
        {"Fungus1_21", 9},
        {"Mines_03", 10},
        {"Mines_35", 11},
        {"Mines_19", 12},
        {"Mines_04", 13},
        {"Mines_31", 14},
        {"Mines_16", 15},
        {"Mines_24", 16},
        {"Fungus2_18", 17},
        {"Fungus2_20", 18},
        {"Fungus3_47", 19},
        {"Waterways_04", 20},
        {"Waterways_13", 21},
        {"Waterways_14", 22},
        {"Ruins1_05", 23},
        {"Ruins1_32", 24},
        {"Ruins2_03", 25},
        {"Ruins_House_01", 26},
        {"Ruins2_07", 27},
        //{"Ruins2_11", 28},
        //{"Ruins2_11", 30},
        //{"Ruins2_11", 31},
        {"Deepnest_East_11", 29},
        {"Deepnest_East_14", 32},
        {"Hive_04", 33},
        {"Hive_03", 34},
        {"Fungus3_10", 35},
        {"Fungus3_22", 36},
        {"Fungus3_48", 37},
        {"Deepnest_Spider_Town", 38},
        {"Deepnest_31", 39},
        {"Deepnest_03", 40},
        {"Deepnest_36", 41},
        {"Deepnest_39", 42},
        {"Abyss_17", 43},
        {"RestingGrounds_10", 44},
        {"Fungus1_28", 45},
    };

    public static List<string> DWInspects = new()
    {
        "ALADAR_INSPECT",
        "GALIEN_INSPECT",
        "HU_INSPECT",
        "MARKOTH_INSPECT",
        "MUMCAT_INSPECT",
        "NOEYES_INSPECT",
        "XERO_INSPECT",
    };

    public static void Hook()
    {
        ModHooks.LanguageGetHook += LanguageGet;
        OnDialogueBox.AfterOrig.ShowPage += PlayAudioForNPCDialogue;
        OnDialogueBox.BeforeOrig.HideText += _ => AudioPlayer.StopPlaying();
    }

    private static string LanguageGet(string key, string sheetTitle, string orig)
    {
        if (key == "JINN_OFFER" && sheetTitle != "Prompts")
        {
            AudioPlayer.TryPlayAudioFor("JINN_OFFER_TALK_0", 3 / 5f);
        }
        return orig;
    }

    private static void PlayAudioForNPCDialogue(OnDialogueBox.Delegates.Params_ShowPage args)
    {
        //DialogueBox is a component of DialogueManager/Text
        var dialogueManager = args.self.gameObject.transform.parent.gameObject;

        bool isDreamBoxOpen = dialogueManager.Find("Box Dream").GetComponent<MeshRenderer>().enabled;
        bool isDialogueBoxOpen = dialogueManager.Find("DialogueBox").Find("backboard").GetComponent<SpriteRenderer>().enabled;

        if (isDialogueBoxOpen)
        {
            MixerLoader.SetSnapshot(MiscUtils.GetCurrentSceneName());
        }
        else if (isDreamBoxOpen)
        {
            // we dont wanna play dn dialogue when toggled off
            if (!HKVocals._globalSettings.dnDialogue)
            {
                return;
            }
            else
            {
                MixerLoader.SetSnapshot(Snapshots.Dream);
            }
        }
        
        if (args.self.currentConversation is "TUT_TAB_01" or "TUT_TAB_02" or "TUT_TAB_02_1" or "TUT_TAB_03")
        {
            MixerLoader.SetSnapshot(Snapshots.Cave);
        }
        if (args.self.currentConversation is "CLIFF_TAB_02" or "CLIFF_TAB_02_1")
        {
            MixerLoader.SetSnapshot(Snapshots.Room);
        }
        if (args.self.currentConversation is "ABYSS_TUT_TAB_01" or "ABYSS_TUT_TAB_01_1")
        {
            MixerLoader.SetSnapshot(Snapshots.Cave);
        }
        if (args.self.currentConversation == "SENSE_TAB_01")
        {
            MixerLoader.SetSnapshot(Snapshots.Arena);
        }

        //convos start at _0 but page numbers start from 1
        int convoNumber = args.self.currentPage - 1;
        string convo = args.self.currentConversation + "_" + convoNumber;

        float removeTime = convoNumber == 0 ? 3 / 5f : 3 / 4f;

        if (DWInspects.Contains(args.self.currentConversation))
        {
            convo = PlayerData.instance.hasDreamNail == false ? args.self.currentConversation + "_ALT_0" : args.self.currentConversation + "_0";
        }
        
        if (args.self.currentConversation == "DREAM")
        {
            if (MiscUtils.GetCurrentSceneName() == "Room_Ouiji")
            {
                convo = "DREAM_JIJI" + $"_{args.self.currentPage - 1}";
            }
        }
        
        HKVocals.instance.Log(args.self.currentConversation);

        if (args.self.currentConversation == "WHITE_DEFENDER_OUTRO_1a" || args.self.currentConversation == "WHITE_DEFENDER_OUTRO_1b")
        {
            convo = args.self.currentConversation == "WHITE_DEFENDER_OUTRO_1a" ? "WHITE_DEFENDER_OUTRO_1A_0" : "WHITE_DEFENDER_OUTRO_1B_0";
        }
        
        if (args.self.currentConversation == "JINN_OFFER")
        {
            return;
        }
        
        if (args.self.currentConversation == "GRUB_BOTTLE_DREAM") 
        {
                int VaNo;
                foreach (KeyValuePair<string, int> scene in GrubVAs)
                {
                    if (MiscUtils.GetCurrentSceneName() == "Ruins2_11")
                    {
                        VaNo = CollectorVAs[Random.Range(0, 3)];
                        var DidPlayAudio = AudioPlayer.TryPlayAudioFor(convo + $"_{VaNo}", removeTime);
                        break;
                    }
                    else if (scene.Key == MiscUtils.GetCurrentSceneName())
                    {
                        VaNo = scene.Value;
                        var DidPlayAudio = AudioPlayer.TryPlayAudioFor(convo + $"_{VaNo}", removeTime);
                    }
                }
        }

        //this controls scroll lock and autoscroll
        DidPlayAudioOnDialogueBox = AudioPlayer.TryPlayAudioFor(convo, removeTime);

        if (DidPlayAudioOnDialogueBox)
        {
            OnPlayNPCDialogue?.Invoke();
        }
    }
}