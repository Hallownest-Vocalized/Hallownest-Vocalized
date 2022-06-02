namespace HKVocals;
public class FSMEdits
{
    public static void BoxOpenDream(PlayMakerFSM BoxOpenDream)
    {
        PlayMakerFSM msgFSM = BoxOpenDream.transform.Find("Dream Msg").gameObject.LocateMyFSM("Display");

        //the only enabled action here is the convo title empty check
        //when we play special audio, this state will be skipped as mod will setstate to "Cancel Existing" (the next one)
        msgFSM.InsertMethod("Check Convo", () =>
        {
            HKVocals.PlayDNInFSM = true;
        },0);
        
        msgFSM.AddMethod("Display Text", () =>
        {
            if (HKVocals._globalSettings.dnDialogue)
            {
                if (HKVocals.PlayDNInFSM)
                {
                    if (AudioUtils.HasAudioFor(msgFSM.FsmVariables.FindFsmString("Convo Title").Value + "_0"))
                    {
                        AudioUtils.TryPlayAudioFor(msgFSM.FsmVariables.FindFsmString("Convo Title").Value + "_0");
                    }
                    else
                    {
                        HKVocals.instance.LogWarn(
                            $"Audio not found for: {msgFSM.FsmVariables.FindFsmString("Convo Title")}");
                    }
                }
            }

            HKVocals.PlayDNInFSM = true;
        });

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
                yield return new WaitForSeconds(AudioUtils.GetAudioFor("MATO_1_0").length + 0.5f);
                HKVocals.instance.CreateDreamDialogue("ORO_2", "Enemy Dreams");
                yield return new WaitForSeconds(AudioUtils.GetAudioFor("ORO_2_0").length + 0.5f);
                HKVocals.instance.CreateDreamDialogue("MATO_2", "Enemy Dreams");
            }
            fsm.AddMethod("Reactivate", () =>HKVocals.CoroutineHolder.StartCoroutine(DreamDialogue()));
        }
    }

    public static void JarCollectorControl(PlayMakerFSM fsm)
    {
            
        HKVocals.instance.Log("Collector");
        fsm.InsertAction("Slam", new DreamDialogueAction(new string[]{"JAR_COLLECTOR_1","JAR_COLLECTOR_2","JAR_COLLECTOR_3" }, "Enemy Dreams") {convoMode = DreamDialogueAction.ConvoMode.Random, chance = 0.4f}, 0);
    }

    public static void DreamMageLord(PlayMakerFSM fsm)
    {
            
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
         * HKVocals.CoroutineHolder.StartCoroutine(FSMEditUtils.AddLoopDialogue(20,new string[]{"GREY_PRINCE_1", "GREY_PRINCE_2"},"",fsm.gameObject));
        */
        string[] GPZDialogues = {"GREY_PRINCE_1", "GREY_PRINCE_2", "GREY_PRINCE_3", "GREY_PRINCE_4", "GREY_PRINCE_5",};
        fsm.InsertAction("Jump", new DreamDialogueAction(GPZDialogues, "Enemy Dreams"){convoMode = DreamDialogueAction.ConvoMode.Random},0);
        fsm.InsertAction("Spit Dir", new DreamDialogueAction(GPZDialogues, "Enemy Dreams"){convoMode = DreamDialogueAction.ConvoMode.Random},0);

    }

    public static void CharmText(PlayMakerFSM fsm)
    {
        fsm.AddMethod("Change Text", () => { fsm.PlayUIText("Convo Desc"); });
    }

    public static void InventoryText(PlayMakerFSM fsm)
    {
        fsm.AddMethod("Change Text", () => { fsm.PlayUIText("Convo Desc"); });
    }

    public static void JournalText(PlayMakerFSM fsm)
    {
        fsm.AddMethod("Get Details", () => { fsm.PlayUIText("Item Desc Convo"); });
        fsm.AddMethod("Get Details", () => { fsm.PlayUIText("Item Notes Convo"); });
    }

    public static void ShopText(PlayMakerFSM fsm)
    {
        fsm.AddMethod("Get Details Init", () => { fsm.PlayUIText("Item Desc Convo"); });
        fsm.AddMethod("Get Details", () => { fsm.PlayUIText("Item Desc Convo"); });
    }
}