using HKVocals.MajorFeatures;

namespace HKVocals;

public static class FSMEditUtils
{
    public static void CreateDreamDialogue(string convName, string sheetName)
    {
        PlayMakerFSM fsm = FsmVariables.GlobalVariables.GetFsmGameObject("Enemy Dream Msg").Value.LocateMyFSM("Display");
        fsm.Fsm.GetFsmString("Convo Title").Value = convName;
        fsm.Fsm.GetFsmString("Sheet").Value = sheetName;
        fsm.SendEvent("DISPLAY DREAM MSG");
    }

    public static void PlayAudioFromFsmString(this PlayMakerFSM fsm, string audiokey)
    {
        HKVocals.instance.LogDebug("audio from fsm string is looking for: " + fsm.FsmVariables.GetFsmString(audiokey).Value + "_0");
        AudioPlayer.TryPlayAudioFor(fsm.FsmVariables.GetFsmString(audiokey).Value + "_0");
    }

    public static void PlayUIText(this PlayMakerFSM fsm, string audiokey)
    {
        //when the UI updates and new text has to be played, no other text can be selected so it makes sense to stop all audio
        AudioPlayer.StopPlaying();

        if (UITextRoutine != null)
        {
            HKVocals.CoroutineHolder.StopCoroutine(UITextRoutine);
        }

        UITextRoutine = HKVocals.CoroutineHolder.StartCoroutine(PlayAudioAfterDelay());
            
        IEnumerator PlayAudioAfterDelay()
        {
            if (UITextAudio.OpenedShopMenu)
            {
                yield return new WaitForSeconds(1f);
                UITextAudio.OpenInvMenu = false;
            }
            else if (UITextAudio.OpenInvMenu)
            {
                yield return new WaitForSeconds(1f);
                UITextAudio.OpenInvMenu = false;
            } 
            else
            {
                yield return new WaitForSeconds(0.3f);
            }
            fsm.PlayAudioFromFsmString(audiokey);
        }
    }
    
    private static Coroutine UITextRoutine;
    
    public static FsmState CreateEmptyState(this PlayMakerFSM fsm, string name)
    {
        var empty = fsm.CopyFsmState(fsm.FsmStates[0].Name, name);
        empty.Actions = Array.Empty<FsmStateAction>();
        empty.Transitions = Array.Empty<FsmTransition>();
        return empty;
    }
    public static void DisableFsmAction(this FsmState state, int index)
    {
        state.GetAction(index).Enabled = false;
    }

    public static void DisableFsmActionInRange(this FsmState state, params int[] indexes)
    {
        for (int i = 0; i < state.Actions.Length; i++)
        {
            if (indexes.Contains(i))
            {
                state.Actions[i].Enabled = false;
            }
        }
    }

    public static void DisableFsmActionsThatAre(this FsmState state, Func<FsmStateAction, bool> predicate)
    {
        if (state.Actions.Any(predicate))
        {
            foreach (var action in state.Actions.Where(predicate))
            {
                action.Enabled = false;
            }
        }
    }
    
    public static void EnableActionsThatAre(this FsmState state, Func<FsmStateAction, bool> predicate)
    {
        if (state.Actions.Any(predicate))
        {
            foreach (var action in state.Actions.Where(predicate))
            {
                action.Enabled = true;
            }
        }
    }

    public static bool IsAudioAction(this FsmStateAction action)
    {
        return action is AudioPlayerOneShot or AudioPlayerOneShotSingle or AudioPlaySimple;
    }

    public static string[] GetClipNames(this FsmStateAction action)
    {
        List<string> ret = new List<string>();
        if (action is AudioPlayerOneShot audioPlayerOneShot)
        {
            if (audioPlayerOneShot.audioClips is { Length: > 0 })
            {
                audioPlayerOneShot.audioClips.ForEach(c =>
                {
                    if (c != null)
                    {
                        ret.Add(c.name);
                    }
                });
            }
        }
        else if (action is AudioPlayerOneShotSingle audioPlayerOneShotSingle)
        {
            if (audioPlayerOneShotSingle.audioClip.Value as AudioClip != null)
            {
                ret.Add(audioPlayerOneShotSingle.audioClip.Value.name);
            }
        }
        else if (action is AudioPlaySimple audioPlaySimple)
        {
            var owner = audioPlaySimple.Fsm.GetOwnerDefaultTarget(audioPlaySimple.gameObject);
            if (owner != null)
            {
                var source = owner.GetComponent<AudioSource>();
                if (source != null)
                {
                    if (audioPlaySimple.oneShotClip.Value as AudioClip != null)
                    {
                        ret.Add(audioPlaySimple.oneShotClip.Value.name);
                    }
                    else
                    {
                        ret.Add(source.clip.name);
                    } 
                }
            }
        }

        return ret.ToArray();
    }
}