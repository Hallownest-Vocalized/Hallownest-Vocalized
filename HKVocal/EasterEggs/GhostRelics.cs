using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HKVocals.Utils;

namespace HKVocals.EasterEggs;

public static class GhostRelics
{
    private static int[] relicAmts = { 8, 7, 5, 4 };
    private static string[] relicNames = { "JOURNAL", "SEAL", "IDOL", "EGG" };
    private static int[] relicPrices = { 0, 0, 0, 0 };
    private static int customRelicID = -1;
    private static int customTypeID = -1;
    private static string selectedConvo;
    private static AudioClip denyClip;
    private static AudioClip acceptClip;
    private static GameObject audioPlayer;
    public static void Hook()
    {
        On.ShopMenuStock.BuildFromMasterList += ShopMenuStock_BuildItemList;
        ModHooks.GetPlayerBoolHook += ModHooks_GetPlayerBoolHook;
        ModHooks.LanguageGetHook += ModHooks_LanguageGetHook;
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged += PlaceBox;
        FSMEditData.AddGameObjectFsmEdit("UI List", "Confirm Control", AddSpecialType);
        FSMEditData.AddGameObjectFsmEdit("Relic Dealer", "Relic Discussions", AddRelicDiscussion);
        FSMEditData.AddGameObjectFsmEdit("Relic Dealer", "Conversation Control", EditConvo);
        FSMEditData.AddGameObjectFsmEdit("Item List", "Item List Control", GetRelicData);
    }

    private static void ShopMenuStock_BuildItemList(On.ShopMenuStock.orig_BuildFromMasterList orig, ShopMenuStock self)
    {
        orig(self);
        if (self.name == "Relic Dealer")
        {
            List<GameObject> stock = new List<GameObject>(self.stockInv);
            for (int i = 0; i < 4; i++)
            {
                GameObject go = self.stockInv[i];
                relicPrices[i] = go.GetComponent<ShopItemStats>().cost;
                for (int j = 1; j < relicAmts[i] + 1; j++)
                {
                    GameObject clone = GameObject.Instantiate(go);
                    ShopItemStats stats = clone.GetComponent<ShopItemStats>();

                    stats.activeColour = new Color(0.8f, 0.8f, 0.8f, 0.8f);
                    stats.relic = false;
                    stats.relicNumber = customRelicID;
                    stats.specialType = customTypeID;
                    stats.cost = 0;
                    stats.descConvo = $"RELICDEALER_{relicNames[i]}_{j}";
                    stats.requiredPlayerDataBool = $"soldRelics{i}_{j}";
                    stats.playerDataBoolName = "false";

                    stock.Add(clone);
                }
            }

            self.stockInv = stock.ToArray();
        }
    }

    private static bool ModHooks_GetPlayerBoolHook(string name, bool orig)
    {
        try
        {
            if (name.StartsWith("soldRelics"))
            {
                string[] split = name.Split('_');
                int relic = int.Parse(split[0].Substring(10, 1));
                int amt = int.Parse(split[1]);

                return PlayerData.instance.GetInt("soldTrinket" + relic) >= amt;
            }
        }
        catch { }
        return orig;
    }

    private static string ModHooks_LanguageGetHook(string key, string sheetTitle, string orig)
    {
        switch (key)
        {
            case "RELICDEALER_SHOP_INTRO_1":
                return Language.Language.Get("RELICDEALER_SHOP_INTRO");
            case "RELICDEALER_SHOP_INTRO_2":
                return "";
            case "RELICDEALER_SHOP_INTRO_3":
                return "";
            case "RELICDEALER_SHOP_INTRO_4":
                return "";
            case "SHOP_DESC_PHANTOM_0":
                return "";
            case "SHOP_CHEST_DEPOSIT":
                return "DEPOSIT";
            default:
                return orig;
        }
    }

    private static void PlaceBox(Scene arg0, Scene arg1)
    {
        if (arg1.name == "Ruins1_05b")
        {
            DialogueNPC box = DialogueNPC.CreateInstance();
            box.DialogueSelector = BoxDialogue;
        }
    }

    private static DialogueOptions BoxDialogue(DialogueCallbackOptions options)
    {
        if (!options.Continue)
        {
            if (options.Key == "SHOP_CHEST_DEPOSIT" && options.Response == DialogueResponse.Yes)
            {
                if (PlayerDataAccess.trinket1 > 0 || PlayerDataAccess.trinket2 > 0 || PlayerDataAccess.trinket3 > 0 || PlayerDataAccess.trinket4 > 0)
                {
                    HKVocals._saveSettings.UsedRelicBox = true;
                    audioPlayer.Spawn(HeroController.instance.transform.position).GetComponent<AudioSource>().PlayOneShot(acceptClip);
                    HeroController.instance.AddGeo(
                        (PlayerDataAccess.trinket1 * relicPrices[0]) + (PlayerDataAccess.trinket2 * relicPrices[1]) + 
                        (PlayerDataAccess.trinket3 * relicPrices[2]) + (PlayerDataAccess.trinket4 * relicPrices[3]));
                    PlayerDataAccess.soldTrinket1 += PlayerDataAccess.trinket1;
                    PlayerDataAccess.soldTrinket2 += PlayerDataAccess.trinket2;
                    PlayerDataAccess.soldTrinket3 += PlayerDataAccess.trinket3;
                    PlayerDataAccess.soldTrinket4 += PlayerDataAccess.trinket4;
                    PlayerDataAccess.trinket1 = 0;
                    PlayerDataAccess.trinket2 = 0;
                    PlayerDataAccess.trinket3 = 0;
                    PlayerDataAccess.trinket4 = 0;
                }
                else
                {
                    audioPlayer.Spawn(HeroController.instance.transform.position).GetComponent<AudioSource>().PlayOneShot(denyClip);
                }
            }
            return new() { Key = "SHOP_CHEST_DEPOSIT", Sheet = "", Cost = 0, Type = DialogueType.YesNo, Continue = true };
        }
        return new() { Continue = false };
    }

    public static void AddSpecialType(PlayMakerFSM fsm)
    {
        FsmState resell = fsm.CopyState("Trink 1", "Resell Trink");
        FsmState special = fsm.GetState("Special Type?");
        FsmEvent @event = new FsmEvent("TRINK-1");

        resell.GetAction<SendEventByName>(0).sendEvent = "SOLD TRINKET -1";
        special.AddTransition(@event.Name, resell.Name);
        IntSwitch action = special.GetAction<IntSwitch>(1);
        action.compareTo = action.compareTo.Append(customTypeID).ToArray();
        action.sendEvent = action.sendEvent.Append(@event).ToArray();
    }

    public static void AddRelicDiscussion(PlayMakerFSM fsm)
    {
        FsmString convo = fsm.GetStringVariable("Convo Prefix");
        FsmState prefix = fsm.CopyState("Set Prefix 1", "Set Prefix -1");
        fsm.AddTransition("Idle", "SOLD TRINKET -1", prefix.Name);
        prefix.RemoveAction(0);
        prefix.GetAction<SetStringValue>(0).stringValue = "soldTrinket-1";
        prefix.AddFsmMethod(() => convo.Value = selectedConvo);
    }

    public static void EditConvo(PlayMakerFSM fsm)
    {
        FsmState statePrev = fsm.GetState("Shop Intro");
        FsmState state;
        for (int i = 2; i < 5; i++)
        {
            state = fsm.CopyState("Shop Intro", "Shop Intro " + i);
            statePrev.ChangeTransition("CONVO_FINISH", state.Name);

            state.GetAction<CallMethodProper>(0).parameters[0].stringValue = "RELICDEALER_SHOP_INTRO_" + i;
        }
        state = fsm.CopyState("Shop Intro", "Shop Intro ");
    }

    public static void GetRelicData(PlayMakerFSM fsm)
    {
        FsmString desc = fsm.GetStringVariable("Item Desc Convo");
        fsm.AddMethod("Activate UI", () => selectedConvo = desc.Value);

        denyClip = fsm.GetAction<AudioPlayerOneShotSingle>("Can't Buy", 0).audioClip.Value as AudioClip;
        acceptClip = fsm.GetAction<AudioPlayerOneShotSingle>("Menu Down", 4).audioClip.Value as AudioClip;
        audioPlayer = fsm.GetAction<AudioPlayerOneShotSingle>("Menu Down", 4).audioPlayer.Value;
    }
}
