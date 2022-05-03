using System.Collections;
using System.Collections.Generic;
using On.HutongGames.PlayMaker;
using UnityEngine;

namespace HKVocals
{
    public static class FSMEditUtils
    {
        public static void AddHPDialogue(HealthManager hm, DreamDialogueAction action, int hp)
        {
            action.Owner = hm.gameObject;
            Dictionaries.HpListeners.Add(self =>
            {
                if (self == hm && self.hp >= hp)
                {
                    action.OnEnter();
                    return true;
                }

                return false;
            });
        }

        public static IEnumerator AddLoopDialogue(float time, string[] convNames, string sheetNames, GameObject go)
        {
            while (go.activeInHierarchy)
            {
                yield return new WaitForSeconds(time);
                var x = new DreamDialogueAction(convNames, sheetNames)
                    {convoMode = DreamDialogueAction.ConvoMode.Random};
                x.OnEnter();
            }
        }

        public static void PlayAudioFromFsmString(this PlayMakerFSM fsm, string audiokey)
        {
            HKVocals.instance.TryPlayAudioFor(fsm.FsmVariables.GetFsmString(audiokey).Value);
        }

        public static void PlayUIText(this PlayMakerFSM fsm, string audiokey)
        {
            //when the UI updates and new text has to be played, no other text can be selected so it makes sense to stop all audio
            HKVocals.instance.audioSource.Stop();

            if (UITextRoutine != null)
            {
                GameManager.instance.StopCoroutine(UITextRoutine);
            }

            UITextRoutine = GameManager.instance.StartCoroutine(PlayAudioAfter1SecondDelay());
            
            IEnumerator PlayAudioAfter1SecondDelay()
            {
                yield return new WaitForSeconds(1);
                fsm.PlayAudioFromFsmString(audiokey);
            }
        }

        private static Coroutine UITextRoutine;
    }
}