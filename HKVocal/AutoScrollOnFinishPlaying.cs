namespace HKVocals;

public class AutoScrollOnFinishPlaying : FsmStateAction
{
    /// <summary>
    /// its for when it goes to convo end state but technically theres still lines left. happens when theres a choice to be made in the dialogue
    /// </summary>
    private FsmBool IsConvoEnding;
    private bool ShouldConsiderConvoEnding;
    private float timer;
    private float WaitTime = 1f / 6f;
    public string NextEventName = "NEXT_NOAUDIO"; //our custom transition

    public AutoScrollOnFinishPlaying(FsmBool isConvoEnding, bool shouldConsiderConvoEnding = false)
    {
        IsConvoEnding = isConvoEnding;
        ShouldConsiderConvoEnding = shouldConsiderConvoEnding;
    }
    
    public override void OnEnter()
    {
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

        if (HKVocals.DidPlayAudioOnDialogueBox && HKVocals._globalSettings.autoScroll)
        {
            if (!ShouldConsiderConvoEnding ||
                ShouldConsiderConvoEnding && !IsConvoEnding.Value)
            {
                timer += Time.deltaTime;
                if (timer >= WaitTime)
                {
                    timer = 0f;
                    Fsm.Event(NextEventName);
                }
            }
        }
    }
}
