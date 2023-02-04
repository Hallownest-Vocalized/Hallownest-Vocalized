using Satchel;

namespace HKVocals.EasterEggs;
public static class EternalOrdeal
{
    private static GameObject ZoteLeverGo;
    private static string[] ZoteDialogues = new[] {"ZOTE_1", "ZOTE_2", "ZOTE_3",};
    private static string[] ZoteHit = new[] { "ZOTE_EO_HIT_A1_0", "ZOTE_EO_HIT_A2_0", "ZOTE_EO_HIT_A3_0", };
    private static string[] ZoteOw = new[] {"ZOTE_EO_OW_1_0", "ZOTE_EO_OW_2_0", "ZOTE_EO_OW_3_0", "ZOTE_EO_OW_4_0", "ZOTE_EO_OW_5_0", "ZOTE_EO_OW_6_0", "ZOTE_EO_OW_7_0", "ZOTE_EO_OW_8_0", };
    private static bool FirstFail = false;
    private static bool InStatueRoom = false;
    private static string ZoteFsmName = "Control";
        
    public static List<AudioSource> ZoteAudioPlayers = new List<AudioSource>();

    public static void Hook()
    {
        /*OnBossStatueLever.WithOrig.OnTriggerEnter2D += UseZoteLever;
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged += DeleteZoteAudioPlayersOnSceneChange;
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged += PlaceZoteLever;
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged += ZoteFail;*/
        ModHooks.HeroUpdateHook += HeroUpdate;

        foreach (var zote in Zote_Normal)
        {
            Hooks.HookStateEntered(new FSMData(zote, ZoteFsmName, "Tumble Out"), PlayRandomDialogue);
        }
        foreach (var zote in Zote_Balloon)
        {
            Hooks.HookStateEntered(new FSMData(zote, ZoteFsmName, "Appear"), PlayRandomDialogue);
        }
        foreach (var zote in Zote_Zoteling)
        {
            Hooks.HookStateEntered(new FSMData(zote, ZoteFsmName, "Ball"), PlayRandomDialogue);
        }
        foreach (var zote in Zote_Other)
        {
            Hooks.HookStateEntered(new FSMData(zote, ZoteFsmName, "Antic"), PlayRandomDialogue);
        }
        foreach (var zote in Zote_Thowmp)
        {
            Hooks.HookStateEntered(new FSMData(zote, ZoteFsmName, "Antic Shake"), PlayRandomDialogue);
        }
    }

    private static void ZoteFail(Scene From, Scene To)
    {
                    if (From.name == "GG_Mighty_Zote" && To.name == "GG_Workshop")
                    {
                        if (GameObject.Find("Battle Control").LocateMyFSM("Kill Counter").FindIntVariable("Kills").Value < 57)
                        {
                            if (HKVocals._saveSettings.OrdealFails == 0)
                            {
                                ZoteLeverGo.SetActive(true);
                                HKVocals._saveSettings.OrdealFails = 1;
                            } 
                            else if (HKVocals._saveSettings.OrdealFails > 1)
                            {
                                HKVocals._saveSettings.OrdealFails += 1;
                            }
                            if (HKVocals._saveSettings.OrdealFails == 1)
                            {
                                if (!HKVocals._saveSettings.FinshedOrdealLines.Contains("ZOTE_EO_F_FIRST_0"))
                                {
                                    FirstFail = true;
                                    HKVocals._globalSettings.ordealZoteSpeak = true;
                                    AudioPlayer.TryPlayAudioFor("ZOTE_EO_F_FIRST_0");
                                    HKVocals.CoroutineHolder.StartCoroutine(FirstFailZoteAudioChecks());

                                }
                            }
                            else if (HKVocals._saveSettings.OrdealFails > 1)
                            {
                                if (!HKVocals._saveSettings.FinshedOrdealLines.Contains("ZOTE_EO_F_FIRST_0") || !HKVocals._saveSettings.FinshedOrdealLines.Contains("ZOTE_EO_HIT_FIRST_0"))
                                {
                                    AudioPlayer.TryPlayAudioFor("ZOTE_EO_HIT_FIRST_0");
                                    HKVocals._saveSettings.FinshedOrdealLines.Add("ZOTE_EO_HIT_FIRST_0");
                                }
                                else
                                {
                                    AudioPlayer.TryPlayAudioFor("ZOTE_EO_F_GENERIC_0");
                                }
                            }
                        }
                        else if (GameObject.Find("Battle Control").LocateMyFSM("Kill Counter").FindIntVariable("Kills").Value >= 57)
                        {
                        }
                    }

    }

    static IEnumerator FirstFailZoteAudioChecks()
    {
        yield return new WaitUntil(AudioPlayer.IsPlaying);
        HKVocals.instance.Log("firstfailcheck");
        if (AudioPlayer.IsPlaying() && HKVocals._globalSettings.ordealZoteSpeak == false)
        {
            AudioPlayer.StopPlaying();
            ZoteRandomOw();
            FirstFail = false;
            HKVocals.CoroutineHolder.StartCoroutine(HitLeverAfter()); 
            yield break;
        }
        else if (AudioPlayer.IsPlaying() == false && HKVocals._globalSettings.ordealZoteSpeak)
        {
            HKVocals._saveSettings.FinshedOrdealLines.Add("ZOTE_EO_F_FIRST_0");
            HKVocals.CoroutineHolder.StartCoroutine(ZoteListen());  
            FirstFail = false;
            yield break;
        }
        HKVocals.CoroutineHolder.StartCoroutine(FirstFailZoteAudioChecks()); 
    }

    static IEnumerator HitLeverAfter()
    {
        if (InStatueRoom && HKVocals._globalSettings.ordealZoteSpeak == true)
        {
            AudioPlayer.TryPlayAudioFor("ZOTE_EO_HIT_FIRST_0");
            HKVocals.CoroutineHolder.StartCoroutine(HitLeverAfterChecks());
            yield break;
        }
        else if (InStatueRoom == false)
        {
            yield break;
        }
        HKVocals.CoroutineHolder.StartCoroutine(HitLeverAfter());
    }
    
    static IEnumerator HitLeverAfterChecks()
    {
        yield return new WaitUntil(AudioPlayer.IsPlaying);
        if (AudioPlayer.IsPlaying() && HKVocals._globalSettings.ordealZoteSpeak == false )
        {
            AudioPlayer.StopPlaying();
            yield break;
        }
        else if (AudioPlayer.IsPlaying() == false && HKVocals._globalSettings.ordealZoteSpeak)
        {
            HKVocals._saveSettings.FinshedOrdealLines.Add("ZOTE_EO_HIT_FIRST_0");
            yield break;
        }
        HKVocals.CoroutineHolder.StartCoroutine(HitLeverAfterChecks()); 
    }
    private static void HeroUpdate()
    {
        /*if (Input.GetKeyDown(KeyCode.F))
        {
/*            HKVocals.instance.Log(ZoteOw[Random.Range(0, 8)]);
            ZoteRandomOw();#1#
            var join = string.Join("-", HKVocals._saveSettings.FinshedOrdealLines);

            HKVocals.instance.Log(join);
            HKVocals.instance.Log(HKVocals._saveSettings.OrdealFails);
            

        }*/
        if (Input.GetKey(KeyCode.Alpha5) && Input.GetKey(KeyCode.Alpha7))
        {
            HKVocals._saveSettings.Precepts = 1;
            HKVocals.CoroutineHolder.StartCoroutine(ZotePrecepts());
        }
        
        /*if (Input.GetKeyDown(KeyCode.G))
        {
            HKVocals._saveSettings.OrdealFails = 0;
            HKVocals._saveSettings.FinshedOrdealLines.Clear();
        }*/

        /*if ((HeroController.instance.transform.position.x >= 187 && HeroController.instance.transform.position.y >= 53) && (HeroController.instance.transform.position.x < 210 && HeroController.instance.transform.position.y < 76))
        {
            InStatueRoom = true;
        }
        else if (HeroController.instance.transform.position.x < 187 || HeroController.instance.transform.position.y < 53 || HeroController.instance.transform.position.x >= 210 || HeroController.instance.transform.position.y >= 76)
        {
            InStatueRoom = false;
        }
        */

    }

    public static void PlaceZoteLever(Scene From, Scene To)
    {
        if (To.name == "GG_Workshop")
        {
            ZoteLeverGo = Object.Instantiate(GameObject.Find("GG_Statue_MantisLords/alt_lever/GG_statue_switch_lever"), new Vector3(196.8f, 63.5f, 1), Quaternion.identity);
            ZoteLeverGo.name = "ZoteLever";
            ZoteLeverGo.GetComponent<BossStatueLever>().SetOwner(new BossStatue());
            if (HKVocals._saveSettings.OrdealFails >= 1)
            {
                ZoteLeverGo.SetActive(true); 
            }
            else
            {
                ZoteLeverGo.SetActive(false); 
            }
        }
    }

    static IEnumerator HitLever()
    {
        if(FirstFail == false) yield break;
        GameManager.instance.AwardAchievement("LastLaughOrdeal");
        AudioPlayer.TryPlayAudioFor(ZoteHit[Random.Range(0, 3)]);
        yield return new WaitWhile(AudioPlayer.IsPlaying);
        if (!HKVocals._saveSettings.FinshedOrdealLines.Contains("ZOTE_EO_PATIENCE_0"))   
        {                                                                                
            AudioPlayer.TryPlayAudioFor("ZOTE_EO_HIT_B2_0");                             
        }                                                                                
        else if (HKVocals._saveSettings.Precepts < 57)                                   
        {                                                                                
            AudioPlayer.TryPlayAudioFor("ZOTE_EO_HIT_B3_0");                             
            HKVocals._saveSettings.Precepts = 1;                                         
            HKVocals.CoroutineHolder.StartCoroutine(ZotePrecepts());                     
        }                                                                                
        else                                                                             
        {                                                                                
            AudioPlayer.TryPlayAudioFor("ZOTE_EO_HIT_B1_0");                             
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

            HKVocals._globalSettings.ordealZoteSpeak = !HKVocals._globalSettings.ordealZoteSpeak;

            CoroutineHelper.WaitForSecondsBeforeInvoke(1f, () =>
            {
                ZoteLeverComponent.canToggle = true;
                ZoteLeverComponent.leverAnimator.Play("Shine");
            });
        }

        if (HKVocals._globalSettings.ordealZoteSpeak == false)
        {
            AudioPlayer.StopPlaying();
            ZoteRandomOw();
        } 
        else if (HKVocals._globalSettings.ordealZoteSpeak == true)
        {
            HKVocals.CoroutineHolder.StartCoroutine(HitLever());
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
        asrc.outputAudioMixerGroup = MixerLoader.HKVAudioGroup;
        ZoteAudioPlayers.Add(asrc);
        Object.DontDestroyOnLoad(asrc.gameObject);
    }

    private static readonly List<string> Zote_Normal = new List<string>()
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
    
    private static readonly List<string> Zote_Balloon = new List<string>()
    {
        "Zote Balloon (1)",
        "Zote Balloon Ordeal",
    };
    
    private static readonly List<string> Zote_Zoteling = new List<string>()
    {
        "Ordeal Zoteling", 
        "Ordeal Zoteling (1)", 
    };
    
    private static readonly List<string> Zote_Other = new List<string>()
    {
        "Zote Fluke",
        "Zote Salubra",
        "Zote Turret",
    };
    
    private static readonly List<string> Zote_Thowmp = new List<string>()
    {
        "Zote Thwomp"
    };
    public static void PlayRandomDialogue(PlayMakerFSM fsm)
    {
        if (MiscUtils.GetCurrentSceneName() == "GG_Mighty_Zote")
        {
            if (HKVocals._globalSettings.ordealZoteSpeak && Random.value <= 0.4f)
            {
                AudioPlayer.TryPlayAudioFor(ZoteDialogues[Random.Range(0, 3)]);
            }
        }
    }
    public static void ZoteRandomOw()
    {
        AudioPlayer.TryPlayAudioFor(ZoteOw[Random.Range(0, 8)]);
    }

    static IEnumerator ZoteListen()
    {
        yield return new WaitWhile(AudioPlayer.IsPlaying);
        if (HKVocals._globalSettings.ordealZoteSpeak == true && (HKVocals._saveSettings.FinshedOrdealLines.Contains("ZOTE_EO_F_FIRST_0") || HKVocals._saveSettings.FinshedOrdealLines.Contains("ZOTE_EO_F_GENERIC_0")))
        {
            AudioPlayer.TryPlayAudioFor("");
            yield return new WaitWhile(AudioPlayer.IsPlaying);
            AudioPlayer.TryPlayAudioFor("ZOTE_EO_PATIENCE_0");
            yield return new WaitWhile(AudioPlayer.IsPlaying);
            //if lever is not hit and zote is not interrupted
            HKVocals.CoroutineHolder.StartCoroutine(ZotePrecepts());
        }
        /*HKVocals.CoroutineHolder.StartCoroutine(ZoteListenChecks());*/
    }
    

    static IEnumerator ZotePrecepts()
    {
        yield return new WaitWhile(AudioPlayer.IsPlaying);
        if (HKVocals._saveSettings.Precepts == 1)
        {
            AudioPlayer.TryPlayAudioFor("PRECEPT_1_R_0");  
        }
        else if (HKVocals._saveSettings.Precepts == 2)
        {
            AudioPlayer.TryPlayAudioFor("PRECEPT_2_0");
            yield return new WaitWhile(AudioPlayer.IsPlaying);
            AudioPlayer.TryPlayAudioFor("PRECEPT_2_1");
        } 
        else if(HKVocals._saveSettings.Precepts == 55)
        {
            AudioPlayer.TryPlayAudioFor("PRECEPT_55_0");
            yield return new WaitWhile(AudioPlayer.IsPlaying);
            AudioPlayer.TryPlayAudioFor("PRECEPT_55_1");
        } 
        else if (HKVocals._saveSettings.Precepts == 57)
        {
            AudioPlayer.TryPlayAudioFor("PRECEPT_57_0");
            yield return new WaitWhile(AudioPlayer.IsPlaying);
            AudioPlayer.TryPlayAudioFor("PRECEPT_57_1");
            HKVocals.instance.Log("Stopped");
            yield break;
        }
        else
        {
            AudioPlayer.TryPlayAudioFor("PRECEPT_" + HKVocals._saveSettings.Precepts + "_0");
        }
        HKVocals.instance.Log("Audio Played");
        HKVocals._saveSettings.Precepts += 1;
        HKVocals.instance.Log(HKVocals._saveSettings.Precepts);
        HKVocals.CoroutineHolder.StartCoroutine(ZotePrecepts());
    }
}