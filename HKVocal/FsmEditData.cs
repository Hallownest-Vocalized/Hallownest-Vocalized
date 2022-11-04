namespace HKVocals;

public static class FSMEditData
{
    public static List<GameObjectFsmEditData> GameObjectFsmEdits = new List<GameObjectFsmEditData>();
    public static List<AnyFsmEditData> AnyFsmEdits = new List<AnyFsmEditData>();
    public static List<SceneFsmEditData> SceneFsmEdits = new List<SceneFsmEditData>();

    public static void Add(SceneFsmEditData sceneFsmEdit)
    {
        SceneFsmEdits.Add(sceneFsmEdit);
    }
    public static void Add(GameObjectFsmEditData gameObjectFsmEdit)
    {
        GameObjectFsmEdits.Add(gameObjectFsmEdit);
    }
    public static void Add(AnyFsmEditData anyFsmEdit)
    {
        AnyFsmEdits.Add(anyFsmEdit);
    }
    
    public static void AddSceneFsmEdit(string sceneName, string gameObjectName, string fsmName, Action<PlayMakerFSM> fsmEdits)
    {
        SceneFsmEdits.Add(new SceneFsmEditData(sceneName, gameObjectName, fsmName, fsmEdits));
    }
    public static void AddGameObjectFsmEdit(string gameObjectName, string fsmName, Action<PlayMakerFSM> fsmEdits)
    {
        GameObjectFsmEdits.Add(new GameObjectFsmEditData(gameObjectName, fsmName, fsmEdits));
    }
    public static void AddAnyFsmEdit(string fsmName, Action<PlayMakerFSM> fsmEdits)
    {
        AnyFsmEdits.Add(new AnyFsmEditData(fsmName, fsmEdits));
    }
    
    public static void AddRange(IEnumerable<SceneFsmEditData> sceneFsmEdits)
    {
        SceneFsmEdits.AddRange(sceneFsmEdits);
    }
    public static void AddRange(IEnumerable<GameObjectFsmEditData> gameObjectFsmEdits)
    {
        GameObjectFsmEdits.AddRange(gameObjectFsmEdits);
    }
    public static void AddRange(IEnumerable<AnyFsmEditData> anyFsmEdits)
    {
        AnyFsmEdits.AddRange(anyFsmEdits);
    }
    
    public static void AddRangeOfSceneFsmEdit(IEnumerable<(string, string, string, Action<PlayMakerFSM>)> sceneEditData)
    {
        SceneFsmEdits.AddRange( sceneEditData.Select(x => 
            new SceneFsmEditData(x.Item1, x.Item2, x.Item3, x.Item4)));
    }
    
    public static void AddRangeOfGameObjectFsmEdit(IEnumerable<(string, string, Action<PlayMakerFSM>)> gameObectFsmEditData)
    {
        GameObjectFsmEdits.AddRange( gameObectFsmEditData.Select(x => 
            new GameObjectFsmEditData(x.Item1, x.Item2, x.Item3)));
    }
    
    public static void AddRangeOfAnyFsmEdit(IEnumerable<(string, Action<PlayMakerFSM>)> sceneEditData)
    {
        AnyFsmEdits.AddRange(sceneEditData.Select(x => 
            new AnyFsmEditData(x.Item1, x.Item2)));
    }
}


/// <summary>
/// A struct to hold data required to edit an fsm on a gameobject
/// </summary>
public struct GameObjectFsmEditData
{
    public string GameObjectName;
    public string FsmName;
    public Action<PlayMakerFSM> FsmEdits;

    public GameObjectFsmEditData(string gameObjectName, string fsmName, Action<PlayMakerFSM> fsmEdits)
    {
        GameObjectName = gameObjectName;
        FsmName = fsmName;
        FsmEdits = fsmEdits;
    }

    public bool DoesMatch(string gameObjectName, string fsmName)
    {
        return GameObjectName == gameObjectName && FsmName == fsmName;
    }
    
    public void Invoke(PlayMakerFSM fsm)
    {
        FsmEdits(fsm);
    }
}

/// <summary>
/// a struct to hold data required to edit any fsm with that name
/// </summary>
public struct AnyFsmEditData
{
    public string FsmName;
    public Action<PlayMakerFSM> FsmEdits;

    public AnyFsmEditData(string fsmName, Action<PlayMakerFSM> fsmEdits)
    {
        FsmName = fsmName;
        FsmEdits = fsmEdits;
    }
    
    public bool DoesMatch(string fsmName)
    {
        return FsmName == fsmName;
    }
    
    public void Invoke(PlayMakerFSM fsm)
    {
        FsmEdits(fsm);
    }
}

/// <summary>
/// a struct to hold data required to edit an fsm on a gameobject in a specific scene
/// </summary>
public struct SceneFsmEditData
{
    public string SceneName;
    public string GameObjectName;
    public string FsmName;
    public Action<PlayMakerFSM> FsmEdits;

    public SceneFsmEditData(string sceneName, string gameObjectName, string fsmName, Action<PlayMakerFSM> fsmEdits)
    {
        SceneName = sceneName;
        GameObjectName = gameObjectName;
        FsmName = fsmName;
        FsmEdits = fsmEdits;
    }
    
    public bool DoesMatch(string sceneName, string gameObjectName, string fsmName)
    {
        return SceneName == sceneName && GameObjectName == gameObjectName && FsmName == fsmName;
    }
    
    public void Invoke(PlayMakerFSM fsm)
    {
        FsmEdits(fsm);
    }
}