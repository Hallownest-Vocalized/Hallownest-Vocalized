using System.Collections;
using Modding;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HKVocals
{
    public static class ZoteLever
    {
        private static GameObject ZoteLeverGo;
    
        public static void UseZoteLever(On.BossStatueLever.orig_OnTriggerEnter2D orig, BossStatueLever self, Collider2D collision)
        {
            if (self.gameObject.name != "ZoteLever")
            {
                orig(self, collision);
            }
            else
            {
                if (ZoteLeverGo == null) return;

                if (!ZoteLeverGo.activeInHierarchy || (!collision.CompareTag("Nail Attack"))) return;

                BossStatueLever ZoteLeverComponent = ZoteLeverGo.GetComponent<BossStatueLever>();
                
                
                if(ReflectionHelper.GetField<BossStatueLever, bool>(ZoteLeverComponent, "canToggle") == false) return;
                
                ReflectionHelper.SetField(ZoteLeverComponent, "canToggle", false);
                
                ReflectionHelper.GetField<BossStatueLever, AudioEvent>(ZoteLeverComponent, "switchSound")
                    .SpawnAndPlayOneShot(EternalOrdeal.GetZoteAudioPlayer(), HeroController.instance.transform.position);
                
                GameManager.instance.FreezeMoment(1);
                GameCameras.instance.cameraShakeFSM.SendEvent("EnemyKillShake");

                GameObject strikeNailPrefab =
                    ReflectionHelper.GetField<BossStatueLever, GameObject>(ZoteLeverComponent, "strikeNailPrefab");
                Transform hitOrigin =
                    ReflectionHelper.GetField<BossStatueLever, Transform>(ZoteLeverComponent, "hitOrigin");
                

                if (strikeNailPrefab && hitOrigin)
                {
                    strikeNailPrefab.Spawn(hitOrigin.transform.position);
                }

                if (!ZoteLeverComponent.leverAnimator) return;
                
                ZoteLeverComponent.leverAnimator.Play("Hit");

                HKVocals._globalSettings.OrdealZoteSpeak = !HKVocals._globalSettings.OrdealZoteSpeak;

                GameManager.instance.StartCoroutine(EnableToggle());

                IEnumerator EnableToggle()
                {
                    yield return new WaitForSeconds(1f);
                    ReflectionHelper.SetField(ZoteLeverComponent, "canToggle", true);
                    ZoteLeverComponent.leverAnimator.Play("Shine");
                }
            }
        }

        public static void SetZoteLever(Scene From, Scene To)
        {
            if (To.name == "GG_Workshop")
            {
                ZoteLeverGo = GameObject.Instantiate(
                    GameObject.Find("GG_Statue_MantisLords/alt_lever/GG_statue_switch_lever"));

                ZoteLeverGo.transform.position = new Vector3(196.8f, 63.5f, 1);
                ZoteLeverGo.name = "ZoteLever";
                ZoteLeverGo.GetComponent<BossStatueLever>().SetOwner(null);
            }
        }
    }
}