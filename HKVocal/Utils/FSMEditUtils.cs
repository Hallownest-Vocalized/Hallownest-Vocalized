namespace HKVocals;

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
        HKVocals.instance.Log("audio from fsm string is looking for: " + fsm.FsmVariables.GetFsmString(audiokey).Value + "_0");
        AudioUtils.TryPlayAudioFor(fsm.FsmVariables.GetFsmString(audiokey).Value + "_0");
    }

    private static bool OpenedMenu = false;

    public static void OpenShopMenu(PlayMakerFSM fsm)
    {
        //Checks when you open the shop keeper menu
        fsm.AddMethod("Open Window", () => { OpenedMenu = true; });
    }

    public static void PlayUIText(this PlayMakerFSM fsm, string audiokey)
    {
        //when the UI updates and new text has to be played, no other text can be selected so it makes sense to stop all audio
        HKVocals.instance.audioSource.Stop();

        if (UITextRoutine != null)
        {
            HKVocals.CoroutineHolder.StopCoroutine(UITextRoutine);
        }

        UITextRoutine = HKVocals.CoroutineHolder.StartCoroutine(PlayAudioAfter1SecondDelay());
            
        IEnumerator PlayAudioAfter1SecondDelay()
        {
            if (OpenedMenu == true)
            {
                yield return new WaitForSeconds(1f);
                OpenedMenu = false;

            } else
            {
                yield return new WaitForSeconds(0.3f);

            }

            fsm.PlayAudioFromFsmString(audiokey);
        }
    }
    
    private static Coroutine UITextRoutine;
}