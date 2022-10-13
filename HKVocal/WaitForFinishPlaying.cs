namespace HKVocals;

public class WaitForFinishPlaying : FsmStateAction
{
    public FsmEventTarget eventTarget;
    public FsmEvent OnFinish;

    public override void Reset()
    {
        eventTarget = null;
        FsmBool fsmBool = new FsmBool();
        fsmBool.UseVariable = true;
    }

    public override void OnEnter()
    {
        CheckForFinish();
    }

    public override void OnUpdate() => CheckForFinish();

    public void CheckForFinish()
    {
        if (AudioUtils.IsPlaying())
        {
            return;
        }

        if (HKVocals.ShouldAutoScroll)
        {
            Fsm.Event(eventTarget, OnFinish);
        }
    }
}