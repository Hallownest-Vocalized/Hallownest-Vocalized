namespace HKVocals;

/// <summary>
/// A class that stores all the fsm edits that need to happen in PlayMakerFSM.Awake
/// </summary>
public static class FSMEditData
{
    public static Dictionary<HKVocalsFsmData, Action<PlayMakerFSM>> FsmEdits = new();

    public static void Add(HKVocalsFsmData fsmData, Action<PlayMakerFSM> action)
    {
        if (!FsmEdits.ContainsKey(fsmData))
        {
            FsmEdits.Add(fsmData, action);
        }
        else
        {
            FsmEdits[fsmData] += action;
        }
    }

    public static void AddSceneFsmEdit(string sceneName, string gameObjectName, string fsmName, Action<PlayMakerFSM> fsmEdits)
    {
        Add(new HKVocalsFsmData(sceneName, gameObjectName, fsmName), fsmEdits);
    }
    public static void AddGameObjectFsmEdit(string gameObjectName, string fsmName, Action<PlayMakerFSM> fsmEdits)
    {
        Add(new HKVocalsFsmData(gameObjectName, fsmName), fsmEdits);
    }
    public static void AddAnyFsmEdit(string fsmName, Action<PlayMakerFSM> fsmEdits)
    {
        Add(new HKVocalsFsmData(fsmName), fsmEdits);
    }
    
    public static void AddRange(Dictionary<HKVocalsFsmData, Action<PlayMakerFSM>> fsmEditsList)
    {
        foreach (var (data, fsmEdit) in fsmEditsList)
        {
            Add(data, fsmEdit);
        }
    }
}

/// <summary>
/// Provides data necessary to find the FSM you want to edit. Use this overload when you want to edit and fsm on a gameObject only in a specific scene
/// </summary>
/// <param name="SceneName">The scene name where you want the edit to happen</param>
/// <param name="GameObjectName">The name of the gameObject that the fsm resides in</param>
/// <param name="FsmName">The name of the FSM you want to edit</param>
public record HKVocalsFsmData(string SceneName, string GameObjectName, string FsmName)
{
    /// <summary>
    /// Provides data necessary to find the FSM you want to edit. Use this overload when you want to edit and fsm on a gameObject in all scenes
    /// </summary>
    /// <param name="GameObjectName">The name of the gameobject that the fsm resides in</param>
    /// <param name="FsmName">The name of the FSM you want to edit</param>
    public HKVocalsFsmData(string GameObjectName, string FsmName) : this(null, GameObjectName, FsmName) { }
    /// <summary>
    /// Provides data necessary to find the FSM you want to edit. Use this overload when you want to edit and an fsm on all gameObjects and all scenes
    /// </summary>
    /// <param name="FsmName">The name of the FSM you want to edit</param>
    public HKVocalsFsmData(string FsmName) : this(null, null, FsmName) { }
}