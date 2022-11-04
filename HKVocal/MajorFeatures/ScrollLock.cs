namespace HKVocals.MajorFeatures;

public static class ScrollLock
{
    public static void Hook()
    {
        FSMEditData.AddGameObjectFsmEdit("Text", "Dialogue Page Control", ImplementLockScroll);
    }
    
    public static void ImplementLockScroll(PlayMakerFSM fsm)
    {
        //there are 2 cases, page ends and conversation ends
        ImplementLock(fsm, "Page End", "PAGE_END");
        ImplementLock(fsm, "Stop Pause", "CONVERSATION_END");
    }
    
    private static void ImplementLock(PlayMakerFSM fsm, string landingStateName, string eventName)
    {
        //what we are doing is changing the event transition to my state where we will wait for lock to end
        //before we let fsm continue. if lock == false, the LockScrollOnFinishPlaying will see that and not wait and just continue
        
        var newPageEnd = fsm.CreateEmptyState($"New {landingStateName}");
        newPageEnd.AddAction(new LockScrollOnFinishPlaying());
        newPageEnd.AddTransition("FINISHED", landingStateName);

        var pageEndTransition =  fsm.FsmGlobalTransitions.First(s => s.EventName == eventName);
        pageEndTransition.ToState = newPageEnd.Name;
        pageEndTransition.ToFsmState = newPageEnd;
    }
}