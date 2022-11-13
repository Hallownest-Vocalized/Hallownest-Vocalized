namespace HKVocals.MajorFeatures;

public class LemmBox
{
    private static readonly int[] RELIC_COST = { 200, 450, 800, 1200 };
    private static string LemmScene = "Ruins1_05b";
    
    public static void Hook()
    {
        ModHooks.LanguageGetHook += LemmLang;
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged += CheckScene;
    }

    private static void CheckScene(Scene lastScene, Scene currentScene)
    {
        if (currentScene.name == LemmScene)
        {
            HKVocals.instance.Log("Is Lemm Scene");
            InitBox();
            LemmDialogue();
        }
           
    }

    private static string LemmLang(string key, string sheettitle, string orig)
    {
        return orig = key switch
        {
            "RELICDEALER_SHOP_INTRO_2 " => "Though, you adventurous types are prone to rushing things, aren’t you? That chest over there, at the end of the room, it’s my deposit box for relics.",
            "RELICDEALER_SHOP_INTRO_3" => "If you really have no inclination to learn about the history of the relics that you bring me, and you wish to go swinging that blasted nail around everywhere, simply put everything you find in that box. Don’t worry, if you bring me anything worthwhile, I’ll pay you in full.",
            "RELICDEALER_SHOP_INTRO_4" => "I’m busy enough organizing my collection behind the counter, so whenever you return with some spare time, hand me the relics you deposited and I’ll put them on display where they belong.",
            "SHOP_DESC_PHANTOM_0" => " I see you left some relics in my deposit box. If you’d be so kind and bring them here, I’ll organize them on my shelves for display.",
            "BOX_DEPOSIT" => "Deposit?",
            _ => orig
        };
    }

    private static void InitBox()
    {
         //Todo: load box and set position to it's correct place
         HKVocals.instance.Log("setting up Box...");
         var box = GameObject.Instantiate(HKVocals.preloadedGO["Bell"],new Vector3(2000,300),Quaternion.identity);
         box.SetActive(true);
         GameObject.Destroy(box.LocateMyFSM("Bell-Bell COntrol"));
         PlayMakerFSM self = box.LocateMyFSM("Station Bell-Stag Bell");
         self.RemoveFsmAction("Init",6);
         self.RemoveTransition("Init","Opened");
         self.RemoveState("Opened");
         self.RemoveState("Bell Up");
         self.RemoveState("Bell Up Pause");
         self.RemoveState("Open Grate");
         self.RemoveState("Box Disappear Anim");
         var OnYes = self.GetState("Yes");
         OnYes.RemoveFsmAction(1);
         OnYes.RemoveFsmAction(2);
         OnYes.RemoveFsmAction(3);
         OnYes.InsertMethod(Deposit,1);
         self.GetState("Pause Before Box Drop").AddTransition("FINISHED", "Out Of Range");
    }

    private static void LemmDialogue()
    {
        //Todo: change dialogue
        HKVocals.instance.Log("setting up Lemm Dialogue...");
    }

    private static void Deposit()
    {
        var pd = PlayerData.instance;
        
        for (int i = 1; i <= 4; i++)
        {
            int amount = pd.GetInt($"trinket{i}");

            if (amount == 0) 
                continue;

            int price = amount * RELIC_COST[i - 1];

            pd.SetInt($"soldTrinket{i}", pd.GetInt($"soldTrinket{i}") + amount);
            pd.SetInt($"trinket{i}", 0);
                
            HeroController.instance.AddGeo(price);
        }
    }
}