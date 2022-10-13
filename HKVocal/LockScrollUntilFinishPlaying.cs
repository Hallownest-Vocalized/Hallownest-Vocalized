using HKMirror.InstanceClasses;

namespace HKVocals;

public class LockScrollOnFinishPlaying : FsmStateAction
{
    private bool shouldRun = true;
    private FsmBool DidAudioPlay;

    public LockScrollOnFinishPlaying()
    {
        DidAudioPlay = Fsm.GetFsmBool("Did Audio Play");
    }

    public override void Reset()
    {
        shouldRun = true;
    }
    
    public override void OnEnter()
    {
        var db = new DialogueBoxR(Fsm.GameObject.gameObject.GetComponent<DialogueBox>());
            
        string key = $"{db.currentConversation}_{db.currentPage - 1}";
        if (HKVocals._globalSettings.scrollLock && !HKVocals._saveSettings.FinishedConvos.Contains(key))
        {
            HKVocals._saveSettings.FinishedConvos.Add(key);
            shouldRun = true;
        }
        else
        {
            shouldRun = false;
        }

        CheckForFinish();
    }

    public override void OnUpdate() => CheckForFinish();

    public void CheckForFinish()
    {
        if (!shouldRun || !DidAudioPlay.Value)
        {
            OnFinish();
            return;
        }
        
        if (AudioUtils.IsPlaying())
        {
            return;
        }
        
        OnFinish();
    }

    private void OnFinish()
    {
        shouldRun = true;
        Finish();
    }
}
