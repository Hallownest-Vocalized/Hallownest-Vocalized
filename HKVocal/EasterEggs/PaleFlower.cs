
namespace HKVocals.EasterEggs
{
    public static class PaleFlower
    {
        public static void Hook()
        {
            On.HealthManager.TakeDamage += TakeDamage;
            On.PlayMakerFSM.OnEnable += PaleLurkerFSM;
        }

        private static void TakeDamage(On.HealthManager.orig_TakeDamage orig, HealthManager self, HitInstance hitInstance)
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
                    PlayMakerFSM.BroadcastEvent("Pale Lurker 0 HP");
                    return;
                }
            }
            orig(self, hitInstance);
        }


        private static void PaleLurkerFSM(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
        {
            orig(self);


            if (self.gameObject.scene.name == "GG_Lurker" && self.FsmName == "Lurker Control" && self.gameObject.name == "Pale Lurker")
            {
                GameManager.instance.transform.Find("_GameManager/AudioManager/Music/Action").gameObject.SetActive(false);
                self.AddFsmState("Dead");
                self.AddGlobalTransition("Pale Lurker 0 HP", "Dead");

                self.AddFsmAction("Dead", new Tk2dPlayAnimation()
                {
                    gameObject = new FsmOwnerDefault()
                    {
                        OwnerOption = OwnerDefaultOption.UseOwner
                    },
                    animLibName = "",
                    clipName = "Idle"
                });
            }
        }

    }
}
