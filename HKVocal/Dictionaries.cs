using SFCore.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HKVocals
{
    public class Dictionaries
    {
        public static readonly Dictionary<string, string[]> EnemyVariants = new Dictionary<string, string[]>()
        {
            { "", new string[] { "" } }
        };
        public static readonly Dictionary<(string, string, string), Action<PlayMakerFSM>> SceneFSMEdits = new Dictionary<(string, string, string), Action<PlayMakerFSM>>()
        {
            { ("GG_Radiance", "Boss Control", "Control"), fsm => { if (BossSequenceController.IsInSequence) fsm.AddAction("Flash Down", new DreamDialogueAction("RADIANCE_1", "Enemy Dreams") { waitTime = 5 }); } },
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
            { ("Jar Collector", "Control"), FSMEdits.JarCollectorControl},
            { ("Dream Mage Lord Phase2" , "Mage Lord 2"), FSMEdits.DreamMageLordPhase2 },
            { ("Dream Mage Lord" , "Mage Lord"), FSMEdits.DreamMageLord },
            { ("Grey Prince" , "Control"), FSMEdits.GreyPrinceControl },
            { ("Enemy List" , "Item List Control"), FSMEdits.JournalText },
            { ("Inv" , "Update Text"), FSMEdits.InventoryText },
            {("Charms", "Update Text"), FSMEdits.CharmText},
            {("Item List", "Item List Control"), FSMEdits.ShopText},
            
            {("Zote Boss", "Control"), FSMEdits.EternalOrdeal_Normal},
            {("Zote Crew Normal (1)", "Control"), FSMEdits.EternalOrdeal_Normal},
            {("Zote Crew Normal (2)", "Control"), FSMEdits.EternalOrdeal_Normal},
            {("Zote Crew Normal (3)", "Control"), FSMEdits.EternalOrdeal_Normal},
            {("Zote Crew Normal (4)", "Control"), FSMEdits.EternalOrdeal_Normal},
            {("Zote Crew Fat (1)", "Control"), FSMEdits.EternalOrdeal_Normal},
            {("Zote Crew Fat (2)", "Control"), FSMEdits.EternalOrdeal_Normal},
            {("Zote Crew Fat (3)", "Control"), FSMEdits.EternalOrdeal_Normal},
            {("Zote Crew Tall (1)", "Control"), FSMEdits.EternalOrdeal_Normal},
            {("Zote Crew Tall", "Control"), FSMEdits.EternalOrdeal_Normal},
            {("Zote Balloon (1)", "Control"), FSMEdits.EternalOrdeal_Balloon},
            {("Zote Balloon Ordeal", "Control"), FSMEdits.EternalOrdeal_Balloon},
            {("Ordeal Zoteling", "Control"), FSMEdits.EternalOrdeal_Zoteling},
            {("Ordeal Zoteling (1)", "Control"), FSMEdits.EternalOrdeal_Zoteling},
            {("Zote Fluke", "Control"), FSMEdits.EternalOrdeal_Other},
            {("Zote Salubra", "Control"), FSMEdits.EternalOrdeal_Other},
            {("Zote Turret", "Control"), FSMEdits.EternalOrdeal_Other},
            {("Zote Thwomp", "Control"), FSMEdits.EternalOrdeal_Thwomp},
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
    }
}
