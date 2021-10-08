using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFCore.Utils;
using UnityEngine;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using System.Collections;
using USceneManager = UnityEngine.SceneManagement.SceneManager;

namespace HKVocals
{
    public class FSMEdits
    {
        public static void AddHPDialogue(HealthManager hm, DreamDialogueAction action, int hp)
        {
            action.Owner = hm.gameObject;
            HKVocals.HpListeners.Add(self =>
            {
                if (self == hm && self.hp >= hp)
                {
                    action.OnEnter(); 
                    return true;
                } 
                return false;
            });
        }
        public static void BoxOpenDream(PlayMakerFSM fsm)
        {
            //fsm.transform.Log();
            PlayMakerFSM msgFSM = fsm.transform.Find("Dream Msg").gameObject.LocateMyFSM("Display");
            msgFSM.AddGameObjectVariable("Last Enemy");
            msgFSM.AddStringVariable("Enemy Type");
            msgFSM.AddStringVariable("Enemy Variant");
            msgFSM.AddBoolVariable("Is Enemy");
            msgFSM.AddMethod("Display Text", () =>
            {
                FsmString[] ConvoStrings = new FsmString[] { 
                    msgFSM.FsmVariables.FindFsmString("Convo Title"),
                    msgFSM.FsmVariables.FindFsmString("Enemy Type"),
                    msgFSM.FsmVariables.FindFsmString("Enemy Variant")
                };
                if (HKVocals.instance.HasAudioFor(ConvoStrings[0].Value + "_" + ConvoStrings[1].Value + "_" + ConvoStrings[2].Value + "_0"))
                {
                    HKVocals.instance.PlayAudioFor(ConvoStrings[0].Value + "_" + ConvoStrings[1].Value + "_" + ConvoStrings[2].Value + "_0");
                }
                else if (HKVocals.instance.HasAudioFor(ConvoStrings[0].Value + "_" + ConvoStrings[1].Value + "_0"))
                {
                    HKVocals.instance.PlayAudioFor(ConvoStrings[0].Value + "_" + ConvoStrings[1].Value + "_0");
                }
                else if (HKVocals.instance.HasAudioFor(ConvoStrings[0].Value + "_0"))
                {
                    HKVocals.instance.PlayAudioFor(ConvoStrings[0].Value + "_0");
                }
                ConvoStrings[1].Value = "";
                ConvoStrings[2].Value = "";
                msgFSM.FsmVariables.FindFsmBool("Is Enemy").Value = false;
            });
            msgFSM.CopyState("Display Text", "Display Text Enemy");
            msgFSM.InsertAction("Display Text", new BoolTest()
            { 
                boolVariable = fsm.FsmVariables.FindFsmBool("Is Enemy"), 
                isTrue = new FsmEvent("IS ENEMY") 
            }, 0);
            msgFSM.AddTransition("Display Text", "IS ENEMY", "Display Text Enemy");
            msgFSM.AddAction("Display Text Enemy", new GameObjectIsNull()
            { 
                everyFrame = true, 
                gameObject = fsm.FsmVariables.FindFsmGameObject("Last Enemy"), 
                isNull = new FsmEvent("ENEMY DEAD") 
            });
            msgFSM.AddTransition("Display Text Enemy", "ENEMY DEAD", "Text Down");
            msgFSM.AddMethod("Text Down", HKVocals.instance.audioSource.Stop);
        }
        public static void ConversationControl(PlayMakerFSM fsm)
        {
            if (HKVocals.RemoveOrigNPCSounds /*&& _globalSettings.testSetting == 0*/)
            {
                foreach (FsmState state in fsm.FsmStates)
                {
                    if (state.Actions.Any(action => action is AudioPlayerOneShot || action is AudioPlayerOneShotSingle))
                        state.Actions = state.Actions.Where(action => !(action is AudioPlayerOneShot || action is AudioPlayerOneShotSingle)).ToArray();
                }
            }
        }
        public static void FalseyControl(PlayMakerFSM fsm)
        {
            HKVocals.instance.Log("Falsey Control Activated");
            fsm.InsertAction("Start Fall", new DreamDialogueAction("FALSE_KNIGHT_1", "Enemy Dreams") { waitTime = 10 }, 0);
            fsm.InsertAction("Recover", new DreamDialogueAction(new string[] { "FALSE_KNIGHT_2", "FALSE_KNIGHT_3" }, "Enemy Dreams") { waitTime = 6, convoOccurances = new int[] { 0, 0, -1 } }, 0);
            fsm.MakeLog();
        }
        public static void LurkerControl(PlayMakerFSM fsm)
        {
            DreamDialogueAction action = new DreamDialogueAction(new string[] { "LURKER_1", "LURKER_2", "LURKER_3" }, "Enemy Dreams") { waitTime = 3f, convoMode = DreamDialogueAction.ConvoMode.Random, convoOccurances = new int[] { -1, 0 } };
            fsm.InsertMethod("Aleart Anim", () => action.convoOccurances[0] = 0, 0);
            fsm.InsertAction("Hop Antic", action, 0);
        }
        public static void RadianceControl(PlayMakerFSM fsm)
        {
            if (BossSequenceController.IsInSequence)
            {
                fsm.InsertAction("Rage1 Start", new DreamDialogueAction("RADIANCE_3", "Enemy Dreams"), 0);
                fsm.InsertAction("Tendrils1", new DreamDialogueAction("RADIANCE_4", "Enemy Dreams") { waitTime = 1f }, 0);
                fsm.InsertAction("Arena 2 Start", new DreamDialogueAction("RADIANCE_5", "Enemy Dreams") { waitTime = 2f }, 0);
                fsm.InsertAction("Ascend Tele", new DreamDialogueAction("RADIANCE_6", "Enemy Dreams") { waitTime = 5f }, 0);
            }
        }
        public static void HornetControl(PlayMakerFSM fsm)
        {
            if ((!USceneManager.GetActiveScene().name.Contains("GG") && fsm.gameObject.name.Contains("1")) || (BossSequenceController.IsInSequence && fsm.gameObject.name.Contains("2")))
            {
                string namePart = BossSequenceController.IsInSequence ? "GG" : "GREENPATH";
                HealthManager hm = fsm.GetComponent<HealthManager>();
                AddHPDialogue(hm, new DreamDialogueAction("HORNET_" + namePart + "_1", "Enemy Dreams"), (3 * hm.hp) / 4);
                AddHPDialogue(hm, new DreamDialogueAction("HORNET_" + namePart + "_2", "Enemy Dreams"), hm.hp / 2);
                AddHPDialogue(hm, new DreamDialogueAction("HORNET_" + namePart + "_3", "Enemy Dreams"), hm.hp / 4);
            }
        }
        public static void NailmasterControl(PlayMakerFSM fsm)
        {
            if (!BossSequenceController.IsInSequence)
            {
                HKVocals.instance.Log("Oro Control Activated");
                AddHPDialogue(fsm.GetComponent<HealthManager>(), new DreamDialogueAction("ORO_1", "Enemy Dreams"), 150);
                fsm.AddMethod("Death Start", () => { if ((HKVocals.instance.audioSource.clip?.name.Contains("ORO")).GetValueOrDefault()) HKVocals.instance.audioSource.Stop(); });
                IEnumerator DreamDialogue()
                {
                    yield return new WaitForSeconds(1f);
                    HKVocals.instance.CreateDreamDialogue("MATO_1", "Enemy Dreams");
                    yield return new WaitForSeconds(HKVocals.instance.GetAudioFor("MATO_1_0").length + 0.5f);
                    HKVocals.instance.CreateDreamDialogue("ORO_2", "Enemy Dreams");
                    yield return new WaitForSeconds(HKVocals.instance.GetAudioFor("ORO_2_0").length + 0.5f);
                    HKVocals.instance.CreateDreamDialogue("MATO_2", "Enemy Dreams");
                }
                fsm.AddMethod("Reactivate", () => HKVocals.instance.coroutineSlave.StartCoroutine(DreamDialogue()));
                fsm.MakeLog();
            }
        }
    }
}
