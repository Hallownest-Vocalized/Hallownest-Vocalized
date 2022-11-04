﻿using HKVocals.MajorFeatures;

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
        AudioUtils.TryPlayAudioFor(fsm.FsmVariables.GetFsmString(audiokey).Value + "_0");
    }

    public static void PlayUIText(this PlayMakerFSM fsm, string audiokey)
    {
        //when the UI updates and new text has to be played, no other text can be selected so it makes sense to stop all audio
        AudioUtils.StopPlaying();

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
        var empty = fsm.CopyState(fsm.FsmStates[0].Name, name);
        empty.Actions = Array.Empty<FsmStateAction>();
        empty.Transitions = Array.Empty<FsmTransition>();
        return empty;
    }
    public static void DisableAction(this FsmState state, int index)
    {
        Satchel.FsmUtil.GetAction(state, index).Enabled = false;
    }

    public static void DisableActionInRange(this FsmState state, params int[] indexes)
    {
        for (int i = 0; i < state.Actions.Length; i++)
        {
            if (indexes.Contains(i))
            {
                state.Actions[i].Enabled = false;
            }
        }
    }

    public static void DisableActionsThatAre(this FsmState state, Func<FsmStateAction, bool> predicate)
    {
        if (state.Actions.Any(action => action.IsAudioAction()))
        {
            foreach (var action in state.Actions.Where(predicate))
            {
                action.Enabled = false;
            }
        }
    }
    
    public static void EnableActionsThatAre(this FsmState state, Func<FsmStateAction, bool> predicate)
    {
        if (state.Actions.Any(action => action.IsAudioAction()))
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
}