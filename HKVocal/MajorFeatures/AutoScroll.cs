namespace HKVocals.MajorFeatures;

public static class AutoScroll
{
    public enum ScrollSpeed
    {
        Instant, Normal, Slow
    }
    public static float InstantScrollSpeed = 0f;
    public static float NormalScrollSpeed = 1f/6f;
    public static float SlowScrollSpeed = 1f/2f;
    
    public static string CustomNextEvent = "NEXT_NOAUDIO"; //our custom transition

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

        fsm.AddFsmTransition(previousState, CustomNextEvent, SFX_NoAudio.Name);
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
        WaitTime = HKVocals._globalSettings.ScrollSpeed switch
        {
            AutoScroll.ScrollSpeed.Instant => AutoScroll.InstantScrollSpeed,
            AutoScroll.ScrollSpeed.Normal => AutoScroll.NormalScrollSpeed,
            AutoScroll.ScrollSpeed.Slow => AutoScroll.SlowScrollSpeed,
            _ => throw new NotImplementedException(),
        };
        CheckForFinish();
    }

    public override void Reset()
    {
        timer = 0f;
    }

    public override void OnUpdate() => CheckForFinish();

    public void CheckForFinish()
    {
        if (AudioUtils.IsPlaying())
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