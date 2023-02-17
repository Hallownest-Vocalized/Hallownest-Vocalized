using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FrogCore;

namespace HKVocals.EasterEggs;

public static class GhostRelics
{
    private static int[] relicAmts = { 8, 7, 5, 4 };
    private static string[] relicNames = { "JOURNAL", "SEAL", "IDOL", "EGG" };
    private static AudioClip denyClip;
    private static AudioClip acceptClip;
    private static GameObject audioPlayer;
    public static void Hook()
    {
        On.ShopMenuStock.Start += ShopMenuStock_Start;
        ModHooks.GetPlayerBoolHook += ModHooks_GetPlayerBoolHook;
        ModHooks.GetPlayerIntHook += ModHooks_GetPlayerIntHook;
        ModHooks.LanguageGetHook += ModHooks_LanguageGetHook;
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged += PlaceBox;
        FSMEditData.AddGameObjectFsmEdit("UI List", "Confirm Control", AddSpecialType);
        FSMEditData.AddGameObjectFsmEdit("Relic Dealer", "Conversation Control", EditConvo);
        FSMEditData.AddGameObjectFsmEdit("Shop Menu", "shop_control", DontCloseShopGetAudio);
        FSMEditData.AddGameObjectFsmEdit("Shop Region", "Shop Region", DontCloseShop);
    }

    private static void ShopMenuStock_Start(On.ShopMenuStock.orig_Start orig, ShopMenuStock self)
    {
        HKVocals.instance.Log("Start: " + self);
        //if (self.name == "Relic Dealer")
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Ruins1_05b" && self.stock.Length == 4)
        {
            List<GameObject> stock = new List<GameObject>(self.stock);
            for (int i = 0; i < 4; i++)
            {
                GameObject go = self.stock[i];

                GameObject clone = GameObject.Instantiate(go);
                ShopItemStats stats = clone.GetComponent<ShopItemStats>();

                clone.transform.Find("Item Sprite").GetComponent<SpriteRenderer>().color = new Color(0.85f, 0.85f, 0.85f, 0.6f);

                stats.activeColour = new Color(0.85f, 0.85f, 0.85f, 0.6f);
                stats.relic = true;
                stats.relicNumber *= -1;
                stats.cost = 0;
                stats.priceConvo = "";
                stats.descConvo = "SHOP_DESC_PHANTOM";
                stats.relicPDInt = $"ghostRelic_{i + 1}";
                stats.requiredPlayerDataBool = $"hasGhostRelic_{i + 1}";
                stats.removalPlayerDataBool = "";
                stats.playerDataBoolName = "false";

                stock.Add(clone);
            }

            self.stock = stock.ToArray();
        }
        orig(self);
    }

    private static bool ModHooks_GetPlayerBoolHook(string name, bool orig)
    {
        if (name == "false")
            return false;
        try
        {
            if (name.StartsWith("hasGhostRelic"))
            {
                int relic = int.Parse(name.Split('_')[1]);

                return HKVocals._saveSettings.GhostRelics[relic - 1] > 0;
            }
        }
        catch { }
        return orig;
    }

    private static int ModHooks_GetPlayerIntHook(string name, int orig)
    {
        try
        {
            if (name.StartsWith("ghostRelic"))
            {
                int relic = int.Parse(name.Split('_')[1]);

                return HKVocals._saveSettings.GhostRelics[relic - 1];
            }
        }
        catch { }
        return orig;
    }

    private static string ModHooks_LanguageGetHook(string key, string sheetTitle, string orig)
    {
        switch (key)
        {
            case "RELICDEALER_SHOP_INTRO":
                return orig + "<page>" +
                    "Though, you adventurous types are prone to rushing things, aren’t you? That chest over there, at the end of the room, it’s my deposit box for relics.<page>" +
                    "If you really have no inclination to learn about the history of the relics that you bring me, and you wish to go swinging that blasted nail around everywhere, simply put everything you find in that box.<page>" +
                    "Don’t worry, if you bring me anything worthwhile, I’ll pay you in full.<page>" +
                    "I’m busy enough organizing my collection behind the counter, so whenever you return with some spare time, hand me the relics you deposited and I’ll put them on display where they belong.";
            case "SHOP_DESC_PHANTOM":
                return "I see you left some relics in my deposit box. If you’d be so kind and bring them here, I’ll organize them on my shelves for display.";
            case "SHOP_CHEST_DEPOSIT":
                return "DEPOSIT";
            case "SHOP_CHEST_NPC_SUPER":
            case "SHOP_CHEST_NPC_MAIN":
            case "SHOP_CHEST_NPC_SUB":
                return "";
            default:
                return orig;
        }
    }

    private static void PlaceBox(Scene arg0, Scene arg1)
    {
        if (arg1.name == "Ruins1_05b")
        {
            DialogueNPC box = DialogueNPC.CreateInstance();
            PlayMakerFSM npc_control = box.gameObject.LocateMyFSM("npc_control");
            npc_control.GetBoolVariable("Hero Always Right").Value = false;
            npc_control.GetFloatVariable("Move To Offset").Value = 0f;
            box.transform.position = new Vector3(58f, 23.4f, box.transform.position.z);
            box.DialogueSelector = BoxDialogue;
            GameObject.Destroy(box.transform.Find("Dream Dialogue"));
            box.SetTitle("SHOP_CHEST_NPC");
            box.SetUp();
        }
    }

    private static void PlayAudioOneShot(AudioClip clip)
    {
        HKVocals.instance.Log("Audio player: " + audioPlayer);
        AudioSource source = audioPlayer.Spawn(HeroController.instance.transform.position).GetComponent<AudioSource>();
        source.volume = 0.7f;
        source.pitch = 1.1f;
        source.PlayOneShot(clip);
    }

    private static DialogueOptions BoxDialogue(DialogueCallbackOptions options)
    {
        if (!options.Continue)
            return new() { Key = "SHOP_CHEST_DEPOSIT", Sheet = "", Cost = 0, Type = DialogueType.YesNo, Continue = true };
        if (options.Response == DialogueResponse.Yes)
        {
            if (PlayerDataAccess.trinket1 > 0 || PlayerDataAccess.trinket2 > 0 || PlayerDataAccess.trinket3 > 0 || PlayerDataAccess.trinket4 > 0)
            {
                GameManager.instance.AwardAchievement("ImpatientLemm");
                PlayAudioOneShot(acceptClip);
                HKVocals._saveSettings.UsedRelicBox = true;
                HeroController.instance.AddGeo(
                    (PlayerDataAccess.trinket1 * 200) + (PlayerDataAccess.trinket2 * 800) +
                    (PlayerDataAccess.trinket3 * 450) + (PlayerDataAccess.trinket4 * 1200));
                HKVocals._saveSettings.GhostRelics[0] += PlayerDataAccess.trinket1;
                HKVocals._saveSettings.GhostRelics[1] += PlayerDataAccess.trinket2;
                HKVocals._saveSettings.GhostRelics[2] += PlayerDataAccess.trinket3;
                HKVocals._saveSettings.GhostRelics[3] += PlayerDataAccess.trinket4;
                PlayerDataAccess.trinket1 = 0;
                PlayerDataAccess.trinket2 = 0;
                PlayerDataAccess.trinket3 = 0;
                PlayerDataAccess.trinket4 = 0;
            }
            else
                PlayAudioOneShot(denyClip);
        }
        return new() { Continue = false };
    }

    public static void AddSpecialType(PlayMakerFSM fsm)
    {
        FsmInt relicNum = fsm.GetIntVariable("Relic Number");
        fsm.InsertMethod("Sell Item", () => { if (relicNum.Value < 0) { relicNum.Value *= -1; HKVocals._saveSettings.GhostRelics[relicNum.Value - 1]--; fsm.SendEvent("FINISHED"); } }, 0);
    }

    public static void EditConvo(PlayMakerFSM fsm)
    {
        FsmState state = fsm.CopyState("Shop Intro", "Box Discussion");
        state.RemoveAction(1);
        state.AddMethod(() => HKVocals._saveSettings.RelicBoxConvo = true);
        state.GetAction<CallMethodProper>(0).parameters[0].stringValue = "SHOP_DESC_PHANTOM";

        FsmState choice = fsm.GetState("Convo Choice");
        choice.AddTransition("BOX", state.Name);
        choice.InsertMethod(() => { if (HKVocals._saveSettings.UsedRelicBox && !HKVocals._saveSettings.RelicBoxConvo) fsm.SendEvent("BOX"); }, 4);
    }

    public static void DontCloseShopGetAudio(PlayMakerFSM fsm)
    {
        fsm.InsertMethod("Check Relics", () => { HKVocals.instance.Log(HKVocals._saveSettings.GhostRelics.Sum()); if (HKVocals._saveSettings.GhostRelics.Sum() > 0) fsm.SendEvent("HAS RELIC"); }, 15);
        fsm = fsm.transform.Find("Item List").gameObject.LocateMyFSM("Item List Control");
        denyClip = fsm.GetAction<AudioPlayerOneShotSingle>("Can't Buy", 0).audioClip.Value as AudioClip;
        acceptClip = fsm.GetAction<AudioPlayerOneShotSingle>("Menu Down", 4).audioClip.Value as AudioClip;
        audioPlayer = fsm.GetAction<AudioPlayerOneShotSingle>("Menu Down", 4).audioPlayer.Value;
    }

    public static void DontCloseShop(PlayMakerFSM fsm)
    {
        fsm.InsertMethod("Check Relics", () => { HKVocals.instance.Log(HKVocals._saveSettings.GhostRelics.Sum()); if (HKVocals._saveSettings.GhostRelics.Sum() > 0) fsm.SendEvent("HAS RELIC"); }, 10);
    }
}
