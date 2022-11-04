namespace HKVocals.MajorFeatures;

public static class AutoScroll
{
    public static void Hook()
    {
        FSMEditData.AddGameObjectFsmEdit("Text", "Dialogue Page Control", ImplementAutoScroll);
    }

    public static void ImplementAutoScroll(PlayMakerFSM fsm)
    {
        var isConvoEnding = fsm.AddFsmBoolVariable("Is Convo Ending");
        isConvoEnding.Value = false;
        ImplementAutoScroll_OnPageEnd(fsm, isConvoEnding);
        ImplementAutoScroll_OnHalfConvoEnd(fsm, isConvoEnding);
    }
    
    private static void ImplementAutoScroll_OnPageEnd(PlayMakerFSM fsm, FsmBool isConvoEnding)
    {
        fsm.AddAction("Page End", new AutoScrollOnFinishPlaying(isConvoEnding));
        
        //we dont wanna play next page sound on autoscroll
        RemoveAudioOnAutoScroll(fsm,"Show Next Page", "Page End"); 
    }

    private static void ImplementAutoScroll_OnHalfConvoEnd(PlayMakerFSM fsm, FsmBool isConvoEnding)
    {
        fsm.AddMethod("Arrow", () => isConvoEnding.Value = false);
        fsm.AddMethod("Stop", () => isConvoEnding.Value = true);

        fsm.AddAction("Conversation End", new AutoScrollOnFinishPlaying(isConvoEnding, shouldConsiderConvoEnding: true));

        //we dont wanna play next page sound on autoscroll
        RemoveAudioOnAutoScroll(fsm, "SFX", "End Conversation");
    }
    
    private static void RemoveAudioOnAutoScroll(PlayMakerFSM fsm, string audioState, string previousState)
    {
        //create a new state with same actions as the normal state except the one that plays audio
        //we will go to this state via the next o audio event we invoke in AutoScrollOnFinishPlaying    
        var SFX = fsm.GetState(audioState);
        var SFX_NoAudio = fsm.CopyState(SFX.Name, $"{audioState} No Audio");
        SFX_NoAudio.Actions = SFX.Actions;
        SFX_NoAudio.RemoveAction(SFX_NoAudio.Actions.GetIndexOf(a => a is AudioPlayerOneShotSingle));

        fsm.AddFsmTransition(previousState, "NEXT_NOAUDIO", SFX_NoAudio.Name);
    }
}