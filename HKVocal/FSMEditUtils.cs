using System.Collections;
using UnityEngine;

namespace HKVocals
{
    public static class FSMEditUtils
    {
    public static void AddHPDialogue(HealthManager hm, DreamDialogueAction action, int hp)
    {
        action.Owner = hm.gameObject;
        HKVocals.HpListeners.Add(self =>
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
            var x = new DreamDialogueAction(convNames, sheetNames) {convoMode = DreamDialogueAction.ConvoMode.Random};
            x.OnEnter();
        }

        yield break;
    }
    }
}