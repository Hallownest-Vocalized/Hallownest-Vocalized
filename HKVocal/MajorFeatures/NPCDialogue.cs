namespace HKVocals.MajorFeatures;

public static class NPCDialogue
{
    public static bool DidPlayAudioOnDialogueBox = false;
    
    public static void Hook()
    {
        OnDialogueBox.AfterOrig.ShowPage += PlayNPCDialogue;
        OnDialogueBox.BeforeOrig.HideText += StopAudioOnDialogueBoxClose;
        
        FSMEditData.AddAnyFsmEdit("Conversation Control", RemovOriginalNPCSounds);
        FSMEditData.AddGameObjectFsmEdit("Iselda", "Shop Anim", RemoveIsdelaShopAudio);
        FSMEditData.AddGameObjectFsmEdit("Mr Mushroom NPC", "Control", MrMushroomAudio);
    }
    
    private static void StopAudioOnDialogueBoxClose(OnDialogueBox.Delegates.Params_HideText args)
    {
        AudioUtils.StopPlaying();
    }

    private static void PlayNPCDialogue(OnDialogueBox.Delegates.Params_ShowPage args)
    {
        var convo = args.self.currentConversation + "_" + (args.self.currentPage - 1);

        float removeTime = args.self.currentPage - 1 == 0 ? 37f / 60f : 3f / 4f;

        //this controls scroll lock and autoscroll
        DidPlayAudioOnDialogueBox = AudioUtils.TryPlayAudioFor(convo, removeTime);
    }
    
    public static void RemovOriginalNPCSounds(PlayMakerFSM fsm)
    {
        foreach (FsmState state in fsm.FsmStates)
        {
            state.DisableActionsThatAre(action => action.IsAudioAction());
        }
    }
    
    //todo: re-implement. instead of enabling/disabling, just disable what is not required
    public static void RemoveIsdelaShopAudio(PlayMakerFSM fsm)
    {
        foreach (FsmState state in fsm.FsmStates)
        {
            state.DisableActionsThatAre(action => action.IsAudioAction());
        }

        fsm.AddMethod("Shop Start", () =>
        {
            foreach (FsmState state in fsm.FsmStates)
            {
                state.EnableActionsThatAre(action => action.IsAudioAction());
            }
        });
    }

    //todo: re-implement. instead of enabling/disabling, just disable what is not required
    public static void MrMushroomAudio(PlayMakerFSM fsm)
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
    
}