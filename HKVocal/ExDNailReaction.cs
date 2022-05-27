namespace HKVocals;
public class ExDNailReaction : MonoBehaviour
{
    internal string Variation = "";
    internal string PDName = "";
    private EnemyDeathEffects ede;
    private void Awake()
    {
        ede = GetComponent<EnemyDeathEffects>();
        if (ede)
        {
            PDName = ede.GetAttr<EnemyDeathEffects, string>("playerDataName");
            Variation = Dictionaries.EnemyVariants.ContainsKey(PDName) ? Dictionaries.EnemyVariants[PDName][UnityEngine.Random.Range(0, Dictionaries.EnemyVariants[PDName].Length)] : "";
        }
    }
    private void OnDestroy()
    {
        PlayMakerFSM fsm = FsmVariables.GlobalVariables.GetFsmGameObject("Enemy Dream Msg").Value.LocateMyFSM("Display");
        if (fsm.FsmVariables.GetFsmGameObject("Last Enemy").Value == gameObject)
            fsm.SendEvent("CANCEL ENEMY DREAM");
    }
}