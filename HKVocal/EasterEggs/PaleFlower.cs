using Satchel;

namespace HKVocals.EasterEggs;
public static class PaleFlower
{
    public static void Hook()
    {
        /*On.HealthManager.TakeDamage += TakeDamage;*/
        On.HealthManager.Die += PaleLurkerDie;
        On.PlayMakerFSM.OnEnable += PaleLurkerFSM;
    }


    private static void PaleLurkerDie(On.HealthManager.orig_Die orig, HealthManager self, float? attackDirection, AttackTypes attackType, bool ignoreEvasion)
    {
        if (self.gameObject.name != "Pale Lurker")
        {
            orig(self, attackDirection, attackType, ignoreEvasion);
        }
        else if (self.gameObject.name == "Pale Lurker")
        {
            self.gameObject.AddComponent<PLMono>();
            self.gameObject.GetComponent<tk2dSpriteAnimator>().Play("Idle");
            self.gameObject.AddComponent<OnTriggerEnter>();
            self.gameObject.layer = 13;

            /*     self.gameObject.RemoveComponent<DamageHero>();
            self.gameObject.RemoveComponent<HealthManager>();*/
        }

    }

    private class PLMono : MonoBehaviour
    {
            
        private void Start()
        {
            gameObject.RemoveComponent<DamageHero>();
            gameObject.RemoveComponent<HealthManager>();
        }

        private void Update()
        {
            if (gameObject.GetComponent<Rigidbody2D>().velocity.y == 0f && gameObject.GetComponent<Rigidbody2D>().velocity.x == 0f)
            {
                gameObject.layer = 13;
                gameObject.GetComponent<Rigidbody2D>().isKinematic = false;
                gameObject.GetComponent<Rigidbody2D>().gravityScale = 0f;
                gameObject.GetComponent<Collider2D>().isTrigger = true;
            }
        }

    }

    /*        private static void TakeDamage(On.HealthManager.orig_TakeDamage orig, HealthManager self, HitInstance hitInstance)
            {
                if (self.gameObject.name == "Pale Lurker")
                {
                    if (self.hp == 1)
                    {
                        return;
                    }
                    else if (self.hp <= hitInstance.DamageDealt)
                    {
                        self.hp = 1;
                        *//*PlayMakerFSM.BroadcastEvent("Pale Lurker 0 HP");*//*
                            return;
                        }
                    }
                    orig(self, hitInstance);
                }*/


    public static void PaleLurkerFSM(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
    {
        orig(self);


        if (self.gameObject.scene.name == "GG_Lurker" && self.FsmName == "Lurker Control" && self.gameObject.name == "Pale Lurker")
        {
            GameManager.instance.transform.Find("_GameManager/AudioManager/Music/Action").gameObject.SetActive(false);

                

/*                self.AddState("Dead");
                *//*self.AddGlobalTransition("Pale Lurker 0 HP", "Dead");*//*
                self.gameObject.LocateMyFSM("Lurker Control").SetState("Dead");

                self.AddAction("Dead", new Tk2dPlayAnimation()
                {
                    gameObject = new FsmOwnerDefault()
                    {
                        OwnerOption = OwnerDefaultOption.UseOwner
                    },
                    animLibName = "",
                    clipName = "Idle"
                }); */
        }
    }

    private class OnTriggerEnter : MonoBehaviour
    {

    }
}