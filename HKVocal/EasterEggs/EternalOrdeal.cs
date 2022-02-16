using System.Collections.Generic;
using System.Linq;
using SFCore.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HKVocals
{
    public static class EternalOrdeal
    {
        public static void DeleteZoteAudioPlayersOnSceneChange(Scene from, Scene to)
        {
            if (from.name == "GG_Mighty_Zote")
            {
                DeleteZoteAudioPlayers();
            }
        }
        
        public static List<AudioSource> ZoteAudioPlayers = new List<AudioSource>();
        public static string ZoteAudioPlayer = "HKVocal ZoteAudioPlayer";

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
            GameObject go = new GameObject(ZoteAudioPlayer);
            AudioSource asrc = go.AddComponent<AudioSource>();
            ZoteAudioPlayers.Add(asrc);
            Object.DontDestroyOnLoad(go);
        }
        
        private static string[] ZoteDialogues = new[] {"ZOTE_1", "ZOTE_2", "ZOTE_3",};
        public static void EternalOrdeal_Normal(PlayMakerFSM fsm)
        {
            if (GameManager.instance.nextSceneName.Contains("GG_Mighty_Zote") || GameManager.instance.sceneName.Contains("GG_Mighty_Zote"))
            {
                fsm.InsertMethod("Tumble Out", () =>
                {
                    if (UnityEngine.Random.value <= 0.4f)
                    {
                        int loc = UnityEngine.Random.Range(1, 4);
                        HKVocals.instance.PlayAudioFor(ZoteDialogues[loc], GetZoteAudioPlayer());
                    }
                }, 0);
            }
            
        }

        public static void EternalOrdeal_Balloon(PlayMakerFSM fsm)
        {
            if (GameManager.instance.nextSceneName.Contains("GG_Mighty_Zote") || GameManager.instance.sceneName.Contains("GG_Mighty_Zote"))
            {
                fsm.InsertMethod("Appear", () =>
                {
                    if (UnityEngine.Random.value <= 0.4f)
                    {
                        int loc = UnityEngine.Random.Range(1, 4);
                        HKVocals.instance.PlayAudioFor(ZoteDialogues[loc], GetZoteAudioPlayer());
                    }
                },0);
            }
            
        }
        
        public static void EternalOrdeal_Zoteling(PlayMakerFSM fsm)
        {
            if (GameManager.instance.nextSceneName.Contains("GG_Mighty_Zote") || GameManager.instance.sceneName.Contains("GG_Mighty_Zote"))
            {
                fsm.InsertMethod("Ball", () =>
                {
                    if (UnityEngine.Random.value <= 0.4f)
                    {
                        int loc = UnityEngine.Random.Range(1, 4);
                        HKVocals.instance.PlayAudioFor(ZoteDialogues[loc], GetZoteAudioPlayer());
                    }
                },0);
            }
            
        }
        public static void EternalOrdeal_Other(PlayMakerFSM fsm)
        {
            if (GameManager.instance.nextSceneName.Contains("GG_Mighty_Zote") || GameManager.instance.sceneName.Contains("GG_Mighty_Zote"))
            {
                fsm.InsertMethod("Antic", () =>
                    {
                        if (UnityEngine.Random.value <= 0.4f)
                        {
                            int loc = UnityEngine.Random.Range(1, 4);
                            HKVocals.instance.PlayAudioFor(ZoteDialogues[loc], GetZoteAudioPlayer());
                        }
                    }, 0);
            }
            
        }

        public static void EternalOrdeal_Thwomp(PlayMakerFSM fsm)
        {
            if (GameManager.instance.nextSceneName.Contains("GG_Mighty_Zote") || GameManager.instance.sceneName.Contains("GG_Mighty_Zote"))
            {
                fsm.InsertMethod("Antic Shake", () =>
                {
                    if (UnityEngine.Random.value <= 0.4f)
                    {
                        int loc = UnityEngine.Random.Range(1, 4);
                        HKVocals.instance.PlayAudioFor(ZoteDialogues[loc], GetZoteAudioPlayer());
                    }
                }, 0);
            }

        }    }
}