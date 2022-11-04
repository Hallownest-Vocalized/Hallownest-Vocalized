namespace HKVocals;
public class CancelDreamDialogueOnDeath : MonoBehaviour
{
    private void OnDestroy()
    {
        PlayMakerFSM fsm = FsmVariables.GlobalVariables.GetFsmGameObject("Enemy Dream Msg").Value.LocateMyFSM("Display");
        if (fsm.FsmVariables.GetFsmGameObject("Last Enemy").Value == gameObject)
        {
            fsm.SendEvent("CANCEL ENEMY DREAM");
        }
    }
}