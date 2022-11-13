namespace HKVocals;
public class CancelDreamDialogueOnDeath : MonoBehaviour
{
    private void OnDestroy()
    {
        FsmGameObject enemyDreamMsg = FsmVariables.GlobalVariables.GetFsmGameObject("Enemy Dream Msg");
        if (enemyDreamMsg != null && enemyDreamMsg.Value != null)
        {
            PlayMakerFSM fsm = enemyDreamMsg.Value.LocateMyFSM("Display");

            if (fsm != null)
            {
                FsmGameObject lastEnemy = fsm.FsmVariables.GetFsmGameObject("Last Enemy");
                if (lastEnemy != null && lastEnemy.Value != null && lastEnemy.Value == gameObject)
                {
                    fsm.SendEvent("CANCEL ENEMY DREAM");
                }
            }
        }
    }
}