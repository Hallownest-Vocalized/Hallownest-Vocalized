namespace HKVocals.MajorFeatures;

public static class AutomaticBossDialogue
{
    // a string to prefix dn dialogue keys so that is can be distinguished in langauge get hook
    public const string ABDKeyPrefix = "HKVocals_ABD_"; 
    
    private static List<Func<HealthManager, bool>> HpListeners = new List<Func<HealthManager, bool>>(); 
    
    private static readonly List<AnyFsmEditData> BossToAddList_AnyFsm = new List<AnyFsmEditData>()
    {
        new("FalseyControl", AddToFalseKnightAndFailedChampion ),
        new("LurkerControl", AddToPaleLurker),
    };
    
    private static readonly List<GameObjectFsmEditData> BossToAddList_GoFsm = new List<GameObjectFsmEditData>()
    {
        new ("Absolute Radiance", "Control", AddToRadiances),
        new ("Absolute Radiance", "Phase Control", AddToRadiances_Phase2),
        new ("Radiance", "Control", AddToRadiances),
        new ("Radiance", "Phase Control", AddToRadiances_Phase2),
        new ("Hornet Boss 1", "Control", AddToHornets),
        new ("Hornet Boss 2", "Control", AddToHornets),
        new ("Oro", "nailmaster", AddToOro),
        new ("Mato", "nailmaster", AddToMato),
        new ("Jar Collector", "Control", AddToCollector),
        new ("Dream Mage Lord Phase2", "Mage Lord 2", AddToSoulTyrant_Phase2),
        new ("Dream Mage Lord", "Mage Lord", AddToSoulTyrant ),
        new ("Grey Prince", "Control", AddToGreyPrinceZote ),
    };

    private static List<SceneFsmEditData> BossToAddList_SceneFsm = new List<SceneFsmEditData>()
    {
        new("GG_Radiance", "Boss Control", "Control", AddToRadiances_Spawn),
        new("Dream_Final_Boss", "Boss Control", "Control", AddToRadiances_Spawn),
    };
    public static void Hook()
    {
        OnHealthManager.AfterOrig.TakeDamage += RemoveHpListeners;
        
        FSMEditData.AddRange(BossToAddList_GoFsm);
        FSMEditData.AddRange(BossToAddList_AnyFsm);
        FSMEditData.AddRange(BossToAddList_SceneFsm);
    }
    
    private static void RemoveHpListeners(OnHealthManager.Delegates.Params_TakeDamage args)
    {
        for (int i = 0; i < HpListeners.Count; i++)
        {
            bool sucess = HpListeners[i](args.self);
            if (sucess)
            {
                HpListeners.RemoveAt(i);
                i--;
            }
        }
    }
    
    private static void AddHPDialogue(HealthManager hm, DreamDialogueAction action, int hpBenchmark)
    {
        action.Owner = hm.gameObject;
        HpListeners.Add(hmInstance =>
        {
            if (hmInstance == hm && hmInstance.hp < hpBenchmark)
            {
                action.OnEnter();
                return true;
            }

            return false;
        });
    }

    private static void AddToFalseKnightAndFailedChampion(PlayMakerFSM fsm)
    {
        HKVocals.instance.LogDebug("Adding ABD to False Knight or Failed Champion");
        fsm.InsertAction("Start Fall", new DreamDialogueAction(ABDKeyPrefix + "FALSE_KNIGHT_1", "Enemy Dreams") { waitTime = 10 }, 0);
        fsm.InsertAction("Recover", new DreamDialogueAction(new string[] {ABDKeyPrefix + "FALSE_KNIGHT_2", ABDKeyPrefix + "FALSE_KNIGHT_3" }, "Enemy Dreams") { waitTime = 6, convoOccurances = new int[] { 0, 0, -1 } }, 0);
           
    }
    private static void AddToPaleLurker(PlayMakerFSM fsm)
    {
        HKVocals.instance.LogDebug("Adiing ADB to Pale Lurker");
        DreamDialogueAction action = new DreamDialogueAction(new string[] { ABDKeyPrefix + "LURKER_1", ABDKeyPrefix + "LURKER_2", ABDKeyPrefix +"LURKER_3" }, "Enemy Dreams") { waitTime = 3f, convoMode = DreamDialogueAction.ConvoMode.Random, convoOccurances = new int[] { -1, 0 } };
        fsm.InsertMethod("Aleart Anim", () => action.convoOccurances[0] = 0, 0);
        fsm.InsertAction("Hop Antic", action, 0);
    }
    private static void AddToRadiances(PlayMakerFSM fsm)
    {
        if (BossSequenceController.IsInSequence)
        {
            HKVocals.instance.LogDebug("Adiing ADB to Radiances");
            fsm.InsertAction("Rage1 Start", new DreamDialogueAction(ABDKeyPrefix +"RADIANCE_3", "Enemy Dreams"), 0);
            fsm.InsertAction("Tendrils1", new DreamDialogueAction(ABDKeyPrefix + "RADIANCE_4", "Enemy Dreams") { waitTime = 1f }, 0);
            fsm.InsertAction("Arena 2 Start", new DreamDialogueAction(ABDKeyPrefix + "RADIANCE_5", "Enemy Dreams") { waitTime = 2f }, 0);
            fsm.InsertAction("Ascend Tele", new DreamDialogueAction(ABDKeyPrefix + "RADIANCE_6", "Enemy Dreams") { waitTime = 5f }, 0);
        }
    }
    
    private static void AddToRadiances_Phase2(PlayMakerFSM fsm)
    {
        if (BossSequenceController.IsInSequence)
        {
            HKVocals.instance.LogDebug("Adiing ADB to Radiances_Phase2");
            if (fsm.FsmName == "Phase Control")
            {
                fsm.AddAction("Set Phase 2", new DreamDialogueAction(ABDKeyPrefix + "RADIANCE_2", "Enemy Dreams"));
            }
        }
    }
    
    private static void AddToRadiances_Spawn(PlayMakerFSM fsm)
    {
        if (BossSequenceController.IsInSequence)
        {
            HKVocals.instance.LogDebug("Adiing ADB to Radiances_Spawn");
            fsm.AddAction("Flash Down", new DreamDialogueAction(ABDKeyPrefix + "RADIANCE_1", "Enemy Dreams") { waitTime = 5 });
        }
    }
    
    //todo: check this? it doesnt make sense
    private static void AddToHornets(PlayMakerFSM fsm)
    {
        if ((!MiscUtils.GetCurrentSceneName().Contains("GG") && fsm.gameObject.name.Contains("1")) ||
            (BossSequenceController.IsInSequence && fsm.gameObject.name.Contains("2")))
        {
            HKVocals.instance.LogDebug("Adiing ADB to Hornets");
            string namePart = BossSequenceController.IsInSequence ? "GG" : "GREENPATH";
            HealthManager hm = fsm.GetComponent<HealthManager>();
            AddHPDialogue(hm, new DreamDialogueAction(ABDKeyPrefix + "HORNET_" + namePart + "_1", "Enemy Dreams"), (3 * hm.hp) / 4);
            AddHPDialogue(hm, new DreamDialogueAction(ABDKeyPrefix + "HORNET_" + namePart + "_2", "Enemy Dreams"), hm.hp / 2);
            AddHPDialogue(hm, new DreamDialogueAction(ABDKeyPrefix + "HORNET_" + namePart + "_3", "Enemy Dreams"), hm.hp / 4);
        }
    }
    private static void AddToOro(PlayMakerFSM fsm)
    {
        if (BossSequenceController.IsInSequence)
        {
            HKVocals.instance.LogDebug("Adiing ADB to Oro");
            AddHPDialogue(fsm.GetComponent<HealthManager>(), new DreamDialogueAction(ABDKeyPrefix + "ORO_1", "Enemy Dreams"), 150);
            fsm.AddMethod("Death Start", () => 
                {
                    if ((HKVocals.instance.audioSource.clip?.name.Contains("ORO")).GetValueOrDefault())
                    {
                        HKVocals.instance.audioSource.Stop();
                    } 
                });
            
            fsm.AddMethod("Reactivate", () => HKVocals.CoroutineHolder.StartCoroutine(DreamDialogue()));
            
            IEnumerator DreamDialogue()
            {
                yield return new WaitForSeconds(1f);
                FSMEditUtils.CreateDreamDialogue(ABDKeyPrefix + "MATO_1", "Enemy Dreams");
                yield return new WaitForSeconds(AudioUtils.GetAudioFor("MATO_1_0").length + 0.5f);
                FSMEditUtils.CreateDreamDialogue(ABDKeyPrefix + "ORO_2", "Enemy Dreams");
                yield return new WaitForSeconds(AudioUtils.GetAudioFor("ORO_2_0").length + 0.5f);
                FSMEditUtils.CreateDreamDialogue(ABDKeyPrefix + "MATO_2", "Enemy Dreams");
            }
        }
    }

    private static void AddToMato(PlayMakerFSM fsm)
    {
        HKVocals.instance.LogDebug("Adiing ADB to Mato");
        fsm.AddMethod("Death Start", () =>
        {
            if (HKVocals.instance.audioSource.clip.name.Contains("MATO"))
            {
                HKVocals.instance.audioSource.Stop();
            }
        });
    }

    private static void AddToCollector(PlayMakerFSM fsm)
    {
        HKVocals.instance.LogDebug("Adiing ADB to Collector");
        fsm.InsertAction("Slam", new DreamDialogueAction(new string[]{ABDKeyPrefix + "JAR_COLLECTOR_1",ABDKeyPrefix + "JAR_COLLECTOR_2",ABDKeyPrefix + "JAR_COLLECTOR_3" }, "Enemy Dreams") {convoMode = DreamDialogueAction.ConvoMode.Random, chance = 0.4f}, 0);
    }

    private static void AddToSoulTyrant(PlayMakerFSM fsm)
    {
        HKVocals.instance.LogDebug("Adiing ADB to soul tyrant");
        HealthManager hm = fsm.GetComponent<HealthManager>();
        AddHPDialogue(hm, new DreamDialogueAction(ABDKeyPrefix + "MAGELORD_D_2", "Enemy Dreams"), (int)(hm.hp * 2f/3f));
        AddHPDialogue(hm, new DreamDialogueAction(ABDKeyPrefix + "MAGELORD_D_3", "Enemy Dreams"), (int)(hm.hp * 1f/3f));
    }

    private static void AddToSoulTyrant_Phase2(PlayMakerFSM fsm)
    {
        HKVocals.instance.LogDebug("Adiing ADB to Soul Tyrant Phase 2");
        //i chose music cuz its after the wait and its just when tyrant dives
        fsm.InsertAction("Music", new DreamDialogueAction(ABDKeyPrefix + "MAGELORD_D_1","Enemy Dreams"), 0);
    }

    private static void AddToGreyPrinceZote(PlayMakerFSM fsm)
    {
        HKVocals.instance.LogDebug("Adiing ADB to Grey Prince Zote");
        string[] GPZDialogues = {ABDKeyPrefix + "GREY_PRINCE_1", ABDKeyPrefix + "GREY_PRINCE_2", ABDKeyPrefix + "GREY_PRINCE_3", ABDKeyPrefix + "GREY_PRINCE_4", ABDKeyPrefix + "GREY_PRINCE_5",};
        fsm.InsertAction("Jump", new DreamDialogueAction(GPZDialogues, "Enemy Dreams"){convoMode = DreamDialogueAction.ConvoMode.Random},0);
        fsm.InsertAction("Spit Dir", new DreamDialogueAction(GPZDialogues, "Enemy Dreams"){convoMode = DreamDialogueAction.ConvoMode.Random},0);
    }
}