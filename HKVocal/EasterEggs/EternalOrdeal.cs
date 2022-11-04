using HKMirror.Hooks.OnHooks;
using HKMirror.Reflection;

namespace HKVocals.EasterEggs;

public static class EternalOrdeal
{
    private static GameObject ZoteLeverGo;
    private static string[] ZoteDialogues = new[] {"ZOTE_1", "ZOTE_2", "ZOTE_3",};
    private static string ZoteFsmName = "Control";
        
    public static List<AudioSource> ZoteAudioPlayers = new List<AudioSource>();

    public static void Hook()
    {
        OnBossStatueLever.WithOrig.OnTriggerEnter2D += UseZoteLever;
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged += DeleteZoteAudioPlayersOnSceneChange;
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged += PlaceZoteLever;

        FSMEditData.AddRange(
            Zote_Normal.Select(goName => new GameObjectFsmEditData(goName, ZoteFsmName,
                        fsm => EditFsmToPlayRandomDialogue(fsm, "Tumble Out")))
                .Concat(Zote_Balloon.Select(goName => new GameObjectFsmEditData(goName, ZoteFsmName,
                            fsm => EditFsmToPlayRandomDialogue(fsm, "Appear"))))
                .Concat(Zote_Zoteling.Select(goName => new GameObjectFsmEditData(goName, ZoteFsmName,
                            fsm => EditFsmToPlayRandomDialogue(fsm, "Ball"))))
                .Concat(Zote_Other.Select(goName => new GameObjectFsmEditData(goName, ZoteFsmName,
                            fsm => EditFsmToPlayRandomDialogue(fsm, "Antic"))))
                .Concat(Zote_Thowmp.Select(goName => new GameObjectFsmEditData(goName, ZoteFsmName,
                            fsm => EditFsmToPlayRandomDialogue(fsm, "Antic Shake")))));
    }
    
    public static void PlaceZoteLever(Scene From, Scene To)
    {
        if (To.name == "GG_Workshop")
        {
            ZoteLeverGo = Object.Instantiate(GameObject.Find("GG_Statue_MantisLords/alt_lever/GG_statue_switch_lever"), new Vector3(196.8f, 63.5f, 1), Quaternion.identity);
            ZoteLeverGo.name = "ZoteLever";
            ZoteLeverGo.GetComponent<BossStatueLever>().SetOwner(null);
        }
    }
    public static void UseZoteLever(On.BossStatueLever.orig_OnTriggerEnter2D orig, BossStatueLever self, Collider2D collision)
    {
        if (self.gameObject.name != "ZoteLever")
        {
            orig(self, collision);
        }
        else
        {
            if (ZoteLeverGo == null || !ZoteLeverGo.activeInHierarchy || (!collision.CompareTag("Nail Attack"))) return;

            var ZoteLeverComponent = ZoteLeverGo.GetComponent<BossStatueLever>().Reflect();

            if (!ZoteLeverComponent.canToggle) return;
            
            ZoteLeverComponent.canToggle = false;
            
            ZoteLeverComponent.switchSound.SpawnAndPlayOneShot(GetZoteAudioPlayer(), HeroController.instance.transform.position);
            
            GameManager.instance.FreezeMoment(1);
            GameCameras.instance.cameraShakeFSM.SendEvent("EnemyKillShake");
            

            if (ZoteLeverComponent.strikeNailPrefab && ZoteLeverComponent.hitOrigin)
            {
                ZoteLeverComponent.strikeNailPrefab.Spawn(ZoteLeverComponent.hitOrigin.transform.position);
            }

            if (!ZoteLeverComponent.leverAnimator) return;
            
            ZoteLeverComponent.leverAnimator.Play("Hit");

            HKVocals._globalSettings.OrdealZoteSpeak = !HKVocals._globalSettings.OrdealZoteSpeak;

            MiscUtils.WaitForSecondsBeforeInvoke(1f, () =>
            {
                ZoteLeverComponent.canToggle = true;
                ZoteLeverComponent.leverAnimator.Play("Shine");
            });
        }
    }

    public static void DeleteZoteAudioPlayersOnSceneChange(Scene from, Scene to)
    {
        if (from.name == "GG_Mighty_Zote")
        {
            DeleteZoteAudioPlayers();
        }
    }

    public static AudioSource GetZoteAudioPlayer()
    {
        AudioSource asrc =  ZoteAudioPlayers.FirstOrDefault(p => !p.isPlaying);

        if (asrc == null)
        {
            AddZoteAudioPlayer();
            asrc =  ZoteAudioPlayers.FirstOrDefault(p => !p.isPlaying);
        }

        return asrc;
    }

    public static void DeleteZoteAudioPlayers()
    {
        if (ZoteAudioPlayers.Count == 0) return;
            
        foreach (AudioSource asrc in ZoteAudioPlayers)
        {
            Object.Destroy(asrc);
        }
        ZoteAudioPlayers.Clear();
    }

    public static void AddZoteAudioPlayer()
    {
        AudioSource asrc = new GameObject("HKVocal ZoteAudioPlayer").AddComponent<AudioSource>();
        ZoteAudioPlayers.Add(asrc);
        Object.DontDestroyOnLoad(asrc.gameObject);
    }

    public static List<string> Zote_Normal = new List<string>()
    {
        "Zote Boss",
        "Zote Crew Normal (1)",
        "Zote Crew Normal (2)",
        "Zote Crew Normal (3)",
        "Zote Crew Normal (4)",
        "Zote Crew Fat (1)",
        "Zote Crew Fat (2)",
        "Zote Crew Fat (3)",
        "Zote Crew Tall (1)",
        "Zote Crew Tall",
    };
    
    public static List<string> Zote_Balloon = new List<string>()
    {
        "Zote Balloon (1)",
        "Zote Balloon Ordeal",
    };
    
    public static List<string> Zote_Zoteling = new List<string>()
    {
        "Ordeal Zoteling", 
        "Ordeal Zoteling (1)", 
    };
    
    public static List<string> Zote_Other = new List<string>()
    {
        "Zote Fluke",
        "Zote Salubra",
        "Zote Turret",
    };
    
    public static List<string> Zote_Thowmp = new List<string>()
    {
        "Zote Thwomp"
    };

    public static void EditFsmToPlayRandomDialogue(PlayMakerFSM fsm, string stateName)
    {
        fsm.InsertMethod(stateName, () =>
        {
            if (MiscUtils.GetCurrentSceneName() == "GG_Mighty_Zote")
            {
                if (HKVocals._globalSettings.OrdealZoteSpeak && Random.value <= 0.4f)
                {
                    AudioUtils.PlayAudioFor(ZoteDialogues[Random.Range(1, 4)], GetZoteAudioPlayer());
                }
            }
        }, 0);
    }
}