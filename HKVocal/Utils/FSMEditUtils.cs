namespace HKVocals;

public static class FSMEditUtils
{
    public static void AddHPDialogue(HealthManager hm, DreamDialogueAction action, int hpBenchmark)
    {
        action.Owner = hm.gameObject;
        Dictionaries.HpListeners.Add(hmInstance =>
        {
            if (hmInstance == hm && hmInstance.hp < hpBenchmark)
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

    private static bool OpenedShopMenu = false;
    public static bool OpenInvMenu = false;
    public static bool InvMenuClosed = true;

    public static void ShopMenuOpenClose(PlayMakerFSM fsm)
    {
        //Checks when you open the shop keeper menu
        fsm.AddMethod("Open Window", () => { OpenedShopMenu = true; });
        //Checks when you close a shop keeper menu
        fsm.AddMethod("Down", () => { AudioUtils.StopPlaying(); });
    }

    public static void InventoryOpenClose(PlayMakerFSM fsm)
    {
        fsm.AddMethod("Open", () => { OpenInvMenu = true; InvMenuClosed = false; });
        fsm.AddMethod("Close", () => { AudioUtils.StopPlaying(); InvMenuClosed = true; });
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
            if (OpenedShopMenu)
            {
                yield return new WaitForSeconds(1f);
                OpenInvMenu = false;
            }
            else if (OpenInvMenu)
            {
                yield return new WaitForSeconds(1f);
                OpenInvMenu = false;
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
}