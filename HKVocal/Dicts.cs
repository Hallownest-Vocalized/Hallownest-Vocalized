using SFCore.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HKVocals
{
    public partial class HKVocals
    {
        private static readonly Dictionary<string, string[]> EnemyVariants = new Dictionary<string, string[]>()
        {
            { "", new string[] { "" } }
        };
        private readonly Dictionary<(string, string, string), Action<PlayMakerFSM>> SceneFSMEdits = new Dictionary<(string, string, string), Action<PlayMakerFSM>>()
        {
            { ("GG_Radiance", "Boss Control", "Control"), fsm => { if (BossSequenceController.IsInSequence) fsm.AddAction("Flash Down", new DreamDialogueAction("RADIANCE_1", "Enemy Dreams") { waitTime = 5 }); } },
            { ("Dream_Final_Boss", "Boss Control", "Control"), fsm => { if (BossSequenceController.IsInSequence) fsm.AddAction("Flash Down", new DreamDialogueAction("RADIANCE_1", "Enemy Dreams") { waitTime = 5 }); } }
        };
        private readonly Dictionary<(string, string), Action<PlayMakerFSM>> GoFSMEdits = new Dictionary<(string, string), Action<PlayMakerFSM>>()
        {
            { ("DialogueManager", "Box Open Dream"), BoxOpenDream },
            { ("Absolute Radiance", "Control"), RadianceControl },
            { ("Absolute Radiance", "Phase Control"), fsm => { if (BossSequenceController.IsInSequence) fsm.AddAction("Set Phase 2", new DreamDialogueAction("RADIANCE_2", "Enemy Dreams")); } },
            { ("Radiance", "Control"), RadianceControl },
            { ("Radiance", "Phase Control"), fsm => fsm.AddAction("Set Phase 2", new DreamDialogueAction("RADIANCE_2", "Enemy Dreams")) },
            { ("Hornet Boss 1", "Control"), HornetControl },
            { ("Hornet Boss 2", "Control"), HornetControl },
            { ("Oro", "nailmaster"), NailmasterControl },
            { ("Mato", "nailmaster"), fsm => fsm.AddMethod("Death Start", () => { if (instance.audioSource.clip.name.Contains("MATO")) instance.audioSource.Stop(); }) }
        };
        private readonly Dictionary<string, Action<PlayMakerFSM>> FSMEdits = new Dictionary<string, Action<PlayMakerFSM>>()
        {
            { "Conversation Control", ConversationControl },
            { "FalseyControl", FalseyControl },
            { "LurkerControl", LurkerControl }
        };
    }
}
