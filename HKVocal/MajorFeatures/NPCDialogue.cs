namespace HKVocals.MajorFeatures;

public static class NPCDialogue
{
    public static bool DidPlayAudioOnDialogueBox = false;
    
    public static void Hook()
    {
        OnDialogueBox.AfterOrig.ShowPage += PlayNPCDialogue;
        OnDialogueBox.BeforeOrig.HideText += StopAudioOnDialogueBoxClose;
        
        FSMEditData.AddAnyFsmEdit("Conversation Control", RemoveOriginalNPCSounds);
        FSMEditData.AddGameObjectFsmEdit("Shop Menu", "shop_control", MuteShopOpenAudio);
        FSMEditData.AddGameObjectFsmEdit("Iselda", "Shop Anim", RemoveIseldaAndSalubraShopAudio);
        FSMEditData.AddGameObjectFsmEdit("Charm Slug", "Extra Anim", RemoveIseldaAndSalubraShopAudio);
        FSMEditData.AddGameObjectFsmEdit("Mr Mushroom NPC", "Control", MuteMrMushroomAudio);
        FSMEditData.AddGameObjectFsmEdit("Maskmaker NPC", "Conversation Control", MuteMaskMakerAudio);
        FSMEditData.AddGameObjectFsmEdit("Fluke Hermit", "Conversation Control", MuteFlukeHermitAudio);
    }
    
    private static void StopAudioOnDialogueBoxClose(OnDialogueBox.Delegates.Params_HideText args)
    {
        AudioUtils.StopPlaying();
    }

    private static void PlayNPCDialogue(OnDialogueBox.Delegates.Params_ShowPage args)
    {
        //dialoguebox is a component of DialogueManager/Text
        var dialogueManager = args.self.gameObject.transform.parent.gameObject;

        bool isDreamBoxOpen = dialogueManager.Find("Box Dream").GetComponent<MeshRenderer>().enabled;
        bool isDialogueBoxOpen = dialogueManager.Find("DialogueBox").Find("backboard").GetComponent<SpriteRenderer>().enabled;

        // we dont wanna play dn dialogue when toggled off
        if (!HKVocals._globalSettings.dnDialogue && isDreamBoxOpen)
        {
            return;
        }

        //convos start at _0 but page numbers start from 1
        int convoNumber = args.self.currentPage - 1;
        string convo = args.self.currentConversation + "_" + convoNumber;

        float removeTime = convoNumber == 0 ? 3/5f : 3/4f;

        //this controls scroll lock and autoscroll
        DidPlayAudioOnDialogueBox = AudioUtils.TryPlayAudioFor(convo, removeTime);
    }
    
    public static void RemoveOriginalNPCSounds(PlayMakerFSM fsm)
    {
        foreach (FsmState state in fsm.FsmStates)
        {
            state.DisableActionsThatAre(action => action.IsAudioAction());
        }
    }
    
    public static void RemoveIseldaAndSalubraShopAudio(PlayMakerFSM fsm)
    {
        fsm.GetState("Shop Start").DisableActionsThatAre(action => action.IsAudioAction());
    }

    //todo: re-implement. instead of enabling/disabling, just disable what is not required
    public static void MuteMrMushroomAudio(PlayMakerFSM fsm)
    {
        foreach (FsmState state in fsm.FsmStates)
        {
            state.DisableActionsThatAre(action => action.IsAudioAction());
        }

        fsm.AddMethod("Bounce On", () =>
        {
            foreach (FsmState state in fsm.FsmStates)
            {
                state.EnableActionsThatAre(action => action.IsAudioAction());
            }
        });
    }
    
    public static void MuteShopOpenAudio(PlayMakerFSM fsm)
    {
        fsm.GetState("Slug").DisableActionsThatAre(action => action.IsAudioAction());
        fsm.GetState("Iselda").DisableActionsThatAre(action => action.IsAudioAction());
        fsm.GetState("Sly").DisableActionsThatAre(action => action.IsAudioAction());
        fsm.GetState("Sly 2").DisableActionsThatAre(action => action.IsAudioAction());
        fsm.GetState("Leg Eater").DisableActionsThatAre(action => action.IsAudioAction());
    }
    
    public static void MuteMaskMakerAudio(PlayMakerFSM fsm)
    {
        fsm.GetState("Mask Choice").InsertMethod(() =>
        {
            fsm.GetGameObjectVariable("Voice Loop").Value.gameObject.GetComponent<AudioSource>().Stop();
        },0);
    }
    public static void MuteFlukeHermitAudio(PlayMakerFSM fsm)
    {
        fsm.GetState("Reset").DisableAction(1);
    }
    
}