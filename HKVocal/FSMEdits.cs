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
using Object = UnityEngine.Object;
using USceneManager = UnityEngine.SceneManagement.SceneManager;

namespace HKVocals
{
    public class FSMEdits
    { 
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
           // fsm.MakeLog();
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
                FSMEditUtils.AddHPDialogue(hm, new DreamDialogueAction("HORNET_" + namePart + "_1", "Enemy Dreams"), (3 * hm.hp) / 4);
                FSMEditUtils.AddHPDialogue(hm, new DreamDialogueAction("HORNET_" + namePart + "_2", "Enemy Dreams"), hm.hp / 2);
                FSMEditUtils.AddHPDialogue(hm, new DreamDialogueAction("HORNET_" + namePart + "_3", "Enemy Dreams"), hm.hp / 4);
            }
        }
        public static void NailmasterControl(PlayMakerFSM fsm)
        {
            if (!BossSequenceController.IsInSequence)
            {
                HKVocals.instance.Log("Oro Control Activated");
                FSMEditUtils.AddHPDialogue(fsm.GetComponent<HealthManager>(), new DreamDialogueAction("ORO_1", "Enemy Dreams"), 150);
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
                fsm.AddMethod("Reactivate", () => GameManager.instance.StartCoroutine(DreamDialogue()));
                //fsm.MakeLog();
            }
        }

        public static void JarCollectorControl(PlayMakerFSM fsm)
        {
            //fsm.MakeLog();
            HKVocals.instance.Log("Collector");
            fsm.InsertAction("Slam", new DreamDialogueAction(new string[]{"JAR_COLLECTOR_1","JAR_COLLECTOR_2","JAR_COLLECTOR_3" }, "Enemy Dreams") {convoMode = DreamDialogueAction.ConvoMode.Random, chance = 0.4f}, 0);
        }

        public static void DreamMageLord(PlayMakerFSM fsm)
        {
            //fsm.MakeLog();
            HealthManager hm = fsm.GetComponent<HealthManager>();
            FSMEditUtils.AddHPDialogue(hm, new DreamDialogueAction("MAGELORD_D_2", "Enemy Dreams"), (int)(hm.hp * 2f/3f));
            FSMEditUtils.AddHPDialogue(hm, new DreamDialogueAction("MAGELORD_D_3", "Enemy Dreams"), (int)(hm.hp * 1f/3f));
        }

        public static void DreamMageLordPhase2(PlayMakerFSM fsm)
        {
            //i chose music cuz its after the wait and its just when tyrant dives
            fsm.InsertAction("Music", new DreamDialogueAction("MAGELORD_D_1","Enemy Dreams"), 0);
        }

        public static void GreyPrinceControl(PlayMakerFSM fsm)
        {
            /*
             * say dialogue every 20 seconds
             * GameManager.instance.StartCoroutine(FSMEditUtils.AddLoopDialogue(20,new string[]{"GREY_PRINCE_1", "GREY_PRINCE_2"},"",fsm.gameObject));
            */
            string[] GPZDialogues = {"GREY_PRINCE_1", "GREY_PRINCE_2", "GREY_PRINCE_3", "GREY_PRINCE_4", "GREY_PRINCE_5",};
            fsm.InsertAction("Jump", new DreamDialogueAction(GPZDialogues, "Enemy Dreams"){convoMode = DreamDialogueAction.ConvoMode.Random},0);
            fsm.InsertAction("Spit Dir", new DreamDialogueAction(GPZDialogues, "Enemy Dreams"){convoMode = DreamDialogueAction.ConvoMode.Random},0);

        }

        public static void CharmText(PlayMakerFSM fsm)
        {
            fsm.InsertMethod("Change Text", () => { fsm.PlayAudioFromFsmString("Convo Desc"); }, 4);
        }

        public static void InventoryText(PlayMakerFSM fsm)
        {
            fsm.InsertMethod("Change Text", () => { fsm.PlayAudioFromFsmString("Convo Desc"); }, 5);
        }

        public static void JournalText(PlayMakerFSM fsm)
        {
            fsm.InsertMethod("Get Details", () => { fsm.PlayAudioFromFsmString("Item Desc Convo"); }, 9);
            fsm.InsertMethod("Get Details", () => { fsm.PlayAudioFromFsmString("Item Notes Convo"); }, 6);
        }

        public static void ShopText(PlayMakerFSM fsm)
        {
            fsm.InsertMethod("Get Details Init", () => { fsm.PlayAudioFromFsmString("Item Desc Convo"); }, 10);
            fsm.InsertMethod("Get Details", () => { fsm.PlayAudioFromFsmString("Item Desc Convo"); }, 7);
        }

        private static string[] ZoteDialogues = new[] {"ZOTE_1", "ZOTE_2", "ZOTE_3",};
        public static void EternalOrdeal_Normal(PlayMakerFSM fsm)
        {
            if (GameManager.instance.nextSceneName.Contains("GG_Mighty_Zote") || GameManager.instance.sceneName.Contains("GG_Mighty_Zote"))
            {
                fsm.InsertMethod("Tumble Out", () =>
                {
                    if (UnityEngine.Random.value <= 0.4f)
                    {
                        int loc = UnityEngine.Random.Range(1, 4);
                        HKVocals.instance.PlayAudioFor(ZoteDialogues[loc], GetZoteAudioPlayer());
                    }
                }, 0);
            }
            
        }

        public static void EternalOrdeal_Balloon(PlayMakerFSM fsm)
        {
            if (GameManager.instance.nextSceneName.Contains("GG_Mighty_Zote") || GameManager.instance.sceneName.Contains("GG_Mighty_Zote"))
            {
                fsm.InsertMethod("Appear", () =>
                {
                    if (UnityEngine.Random.value <= 0.4f)
                    {
                        int loc = UnityEngine.Random.Range(1, 4);
                        HKVocals.instance.PlayAudioFor(ZoteDialogues[loc], GetZoteAudioPlayer());
                    }
                },0);
            }
            
        }
        
        public static void EternalOrdeal_Zoteling(PlayMakerFSM fsm)
        {
            if (GameManager.instance.nextSceneName.Contains("GG_Mighty_Zote") || GameManager.instance.sceneName.Contains("GG_Mighty_Zote"))
            {
                fsm.InsertMethod("Ball", () =>
                {
                    if (UnityEngine.Random.value <= 0.4f)
                    {
                        int loc = UnityEngine.Random.Range(1, 4);
                        HKVocals.instance.PlayAudioFor(ZoteDialogues[loc], GetZoteAudioPlayer());
                    }
                },0);
            }
            
        }
        public static void EternalOrdeal_Other(PlayMakerFSM fsm)
        {
            if (GameManager.instance.nextSceneName.Contains("GG_Mighty_Zote") || GameManager.instance.sceneName.Contains("GG_Mighty_Zote"))
            {
                fsm.InsertMethod("Antic", () =>
                    {
                        if (UnityEngine.Random.value <= 0.4f)
                        {
                            int loc = UnityEngine.Random.Range(1, 4);
                            HKVocals.instance.PlayAudioFor(ZoteDialogues[loc], GetZoteAudioPlayer());
                        }
                    }, 0);
            }
            
        }

        public static void EternalOrdeal_Thwomp(PlayMakerFSM fsm)
        {
            if (GameManager.instance.nextSceneName.Contains("GG_Mighty_Zote") || GameManager.instance.sceneName.Contains("GG_Mighty_Zote"))
            {
                fsm.InsertMethod("Antic Shake", () =>
                {
                    if (UnityEngine.Random.value <= 0.4f)
                    {
                        int loc = UnityEngine.Random.Range(1, 4);
                        HKVocals.instance.PlayAudioFor(ZoteDialogues[loc], GetZoteAudioPlayer());
                    }
                }, 0);
            }

        }

        //for grub do normal edit and set bool in uscene manager and the check for dnails
        
        
        public static List<AudioSource> ZoteAudioPlayers = new List<AudioSource>();
        public static string ZoteAudioPlayer = "HKVocal ZoteAudioPlayer";

        public static AudioSource GetZoteAudioPlayer()
        {
            AudioSource asrc =  ZoteAudioPlayers.FirstOrDefault(p => !p.isPlaying);

            if (asrc == null)
            {
                AddZoteAudioPlayer();
                asrc =  ZoteAudioPlayers.FirstOrDefault(p => !p.isPlaying);
            }

            return asrc;
        }

        public static void DeleteZoteAudioPlayers()
        {
            if (ZoteAudioPlayers.Count == 0) return;
            
            foreach (AudioSource asrc in ZoteAudioPlayers)
            {
                Object.Destroy(asrc);
            }
            ZoteAudioPlayers.Clear();
        }

        public static void AddZoteAudioPlayer()
        {
            GameObject go = new GameObject(ZoteAudioPlayer);
            AudioSource asrc = go.AddComponent<AudioSource>();
            ZoteAudioPlayers.Add(asrc);
            Object.DontDestroyOnLoad(go);
        }

    }
}
