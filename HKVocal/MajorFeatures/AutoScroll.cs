namespace HKVocals.MajorFeatures;

public static class AutoScroll
{
    public enum ScrollSpeed
    {
        Instant, Fast, Normal, Slow
    }

    public static Dictionary<ScrollSpeed, float> ScrollSpeeds = new Dictionary<ScrollSpeed, float>()
    {
        { ScrollSpeed.Instant, 0f },
        { ScrollSpeed.Fast, 0.25f },
        { ScrollSpeed.Normal, 0.6f },
        { ScrollSpeed.Slow, 1f }
    };
    
    public const string CustomNextEvent = "NEXT_NOAUDIO"; //our custom transition

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
        fsm.GetFsmState("Page End").AddFsmAction(new AutoScrollOnFinishPlaying(isConvoEnding));
        
        // we dont want to always show next page prompt
        RemoveNextPagePrompt(fsm, "Page End", 1);
        
        //we dont wanna play next page sound on autoscroll
        RemoveAudioOnAutoScroll(fsm,"Show Next Page", "Page End"); 
    }

    private static void ImplementAutoScroll_OnHalfConvoEnd(PlayMakerFSM fsm, FsmBool isConvoEnding)
    {
        fsm.AddFsmMethod("Arrow", () => isConvoEnding.Value = false);
        fsm.AddFsmMethod("Stop", () => isConvoEnding.Value = true);

        fsm.AddFsmAction("Conversation End", new AutoScrollOnFinishPlaying(isConvoEnding, shouldConsiderConvoEnding: true));

        
        // we dont want to always show next page prompt
        RemoveNextPagePrompt(fsm, "Arrow", 2);
        
        //we dont wanna play next page sound on autoscroll
        RemoveAudioOnAutoScroll(fsm, "SFX", "Conversation End");
    }
    
    private static void RemoveAudioOnAutoScroll(PlayMakerFSM fsm, string audioState, string previousState)
    {
        //create a new state with same actions as the normal state except the one that plays audio
        //we will go to this state via the next o audio event we invoke in AutoScrollOnFinishPlaying    
        var SFX = fsm.GetFsmState(audioState);
        var SFX_NoAudio = fsm.CopyFsmState(SFX.Name, $"{audioState} No Audio");
        SFX_NoAudio.ReplaceAllActions(SFX.Actions);
        SFX_NoAudio.RemoveFsmAction(SFX_NoAudio.Actions.GetIndexOf(a => a is AudioPlayerOneShotSingle));

        fsm.AddFsmTransition(previousState, CustomNextEvent, SFX_NoAudio.Name);
    }
    
    private static void RemoveNextPagePrompt(PlayMakerFSM fsm, string state, int arrowUpActionIndex)
    {
        fsm.GetFsmState(state).DisableFsmAction(arrowUpActionIndex);
        fsm.GetFsmState(state).InsertFsmMethod(() =>
        {
            //only do this when autoscroll and scroll lock is on
            if (!(NPCDialogue.DidPlayAudioOnDialogueBox && HKVocals._globalSettings.autoScroll &&
                HKVocals._globalSettings.scrollLock))
            {
                fsm.Fsm.GameObject.gameObject.transform.parent.Find("Arrow").gameObject.LocateMyFSM("Arrow Anim").SendEvent("UP");
            }
        }, arrowUpActionIndex);
    }
}

public class AutoScrollOnFinishPlaying : FsmStateAction
{
    /// <summary>
    /// its for when it goes to convo end state but technically theres still lines left. happens when theres a choice to be made in the dialogue
    /// </summary>
    private FsmBool IsConvoEnding;
    private bool ShouldConsiderConvoEnding;
    private float timer;
    private float WaitTime;

    public AutoScrollOnFinishPlaying(FsmBool isConvoEnding, bool shouldConsiderConvoEnding = false)
    {
        IsConvoEnding = isConvoEnding;
        ShouldConsiderConvoEnding = shouldConsiderConvoEnding;
    }
    
    public override void OnEnter()
    {
        timer = 0f;
        WaitTime = AutoScroll.ScrollSpeeds[HKVocals._globalSettings.scrollSpeed];
        CheckForFinish();
    }

    public override void Reset()
    {
        timer = 0f;
    }

    public override void OnUpdate() => CheckForFinish();

    public void CheckForFinish()
    {
        if (AudioPlayer.IsPlaying())
        {
            return;
        }

        if (NPCDialogue.DidPlayAudioOnDialogueBox && HKVocals._globalSettings.autoScroll)
        {
            if (!ShouldConsiderConvoEnding ||
                ShouldConsiderConvoEnding && !IsConvoEnding.Value)
            {
                timer += Time.deltaTime;
                if (timer >= WaitTime)
                {
                    timer = 0f;
                    Fsm.Event(AutoScroll.CustomNextEvent);
                }
            }
        }
    }
}