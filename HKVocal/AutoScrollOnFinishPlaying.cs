namespace HKVocals;

public class AutoScrollOnFinishPlaying : FsmStateAction
{
    private FsmBool DidAudioPlay;
    /// <summary>
    /// its for when it goes to convo end state but technically theres still lines left. happens when theres a choice to be made in the dialogue
    /// </summary>
    private FsmBool IsConvoEnding;
    public bool ShouldConsiderConvoEnding;
    private float timer;
    private float WaitTime = 1f / 6f;

    public AutoScrollOnFinishPlaying(bool shouldConsiderConvoEnding, FsmBool didAudioPlay, FsmBool isConvoEnding)
    {
        DidAudioPlay = didAudioPlay;
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

        if (DidAudioPlay.Value)
        {
            if (!ShouldConsiderConvoEnding ||
                ShouldConsiderConvoEnding && !IsConvoEnding.Value)
            {
                timer += Time.deltaTime;
                if (timer >= WaitTime)
                {
                    timer = 0f;
                    Fsm.Event("NEXT");
                }
            }
        }
    }
}
