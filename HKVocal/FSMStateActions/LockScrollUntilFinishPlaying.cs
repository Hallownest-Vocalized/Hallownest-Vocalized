using HKMirror.Reflection;
using HKVocals.MajorFeatures;

namespace HKVocals;

public class LockScrollOnFinishPlaying : FsmStateAction
{
    public override void OnEnter()
    {
        var db = Fsm.GameObject.gameObject.GetComponent<DialogueBox>().Reflect();
            
        string key = $"{db.currentConversation}_{db.currentPage - 1}";
        if (!HKVocals._saveSettings.FinishedConvos.Contains(key))
        {
            //adds key regardless of scroll lock being on
            HKVocals._saveSettings.FinishedConvos.Add(key);
            
            if (HKVocals._globalSettings.scrollLock)
            {
                CheckForFinishState();
                return;
            }
        }
        //comes here if scroll lock false or key already heard
        Finish();
    }

    public override void OnUpdate() => CheckForFinishState();

    public void CheckForFinishState()
    {
        //safegaurd for when an audio doesnt exist
        if (!NPCDialogue.DidPlayAudioOnDialogueBox)
        {
            Finish();
            return;
        }

        if (AudioUtils.IsPlaying())
        {
            return;
        }
        
        Finish();
    }
}
