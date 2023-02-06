namespace HKVocals.EasterEggs;
public static class SpecialGrub
{
    public static string SpeicalGrubSceneName = "Abyss_19";
    public static void Hook()
    {
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged += EditSpecialGrub;
        ModHooks.LanguageGetHook += GetSpecialGrubDialogue;
    }

    public static void EditSpecialGrub(Scene From, Scene To)
    {
        if (To.name == "Abyss_19")
        {
            GameObject.Find("Grub Bottle").transform.GetChild(0).GetChild(0).gameObject.AddComponent<OnDreamNail>();
        }
    }

    private class OnDreamNail : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D col)
        {
            if(col.tag == "Dream Attack")
            {
                MixerLoader.SetSnapshot(Snapshots.Dream);
                if (HKVocals._saveSettings.GrubConvo < 8) 
                {
                    HKVocals._saveSettings.GrubConvo += 1;
                    GameObject.Find("Grub Bottle").transform.GetChild(0).GetChild(0).GetComponent<PlayMakerFSM>().GetFsmStringVariable("Sheet Name").Value = "Elderbug";
                    GameObject.Find("Grub Bottle").transform.GetChild(0).GetChild(0).GetComponent<PlayMakerFSM>().GetFsmStringVariable("Convo Name").Value = $"GRUB_BOTTLE_DREAM_S_{HKVocals._saveSettings.GrubConvo}";
                    AudioPlayer.TryPlayAudioFor($"GRUB_BOTTLE_DREAM_S_{HKVocals._saveSettings.GrubConvo}");
                }
                else
                {
                    GameManager.instance.AwardAchievement("DisdainGrub");
                    GameObject.Find("Grub Bottle").transform.GetChild(0).GetChild(0).GetComponent<PlayMakerFSM>().GetFsmStringVariable("Convo Name").Value = $"GRUB_BOTTLE_DREAM_S_REPEAT_0";
                    GameObject.Find("Grub Bottle").transform.GetChild(0).GetChild(0).GetComponent<PlayMakerFSM>().GetFsmStringVariable("Sheet Name").Value = "Elderbug";
                }
            }
        }
    }
    public static string GetSpecialGrubDialogue(string key, string sheettitle, string orig)
    {
        return key switch
        {
            "GRUB_BOTTLE_DREAM_S_0" => " ...Home...",
            "GRUB_BOTTLE_DREAM_S_1" => "Why does it stare at me so?",
            "GRUB_BOTTLE_DREAM_S_2" => "Has it not come to release me? To save me from this cruel fate?",
            "GRUB_BOTTLE_DREAM_S_3" => "Repetitively now its fist draws back, as if readying to shatter this invisible prison. But it only swipes the air.",
            "GRUB_BOTTLE_DREAM_S_4" => "It wishes not to destroy my confines, but my pride.",
            "GRUB_BOTTLE_DREAM_S_5" => "Does it really intend to mock and shame a helpless grub as I? What evil bug it must be, to knowingly prolong this torture, torn from my kin.",
            "GRUB_BOTTLE_DREAM_S_6" => "From my... Grubfather.",
            "GRUB_BOTTLE_DREAM_S_7" => "When the time is right and this bug least expects it...",
            "GRUB_BOTTLE_DREAM_S_8" => "I will gladly return the favor.",
            "GRUB_BOTTLE_DREAM_S_REPEAT_0" => "...",
            _ => orig
        };
    }
}
