namespace HKVocals.EasterEggs;
public static class SpecialGrub
{
    public static string SpeicalGrubSceneName = "Abyss_19";
    public static void Hook()
    {
        ModHooks.LanguageGetHook += GetSpecialGrubDialogue;
        On.PlayMakerFSM.OnEnable += EditSpecialGrub;
    }
    
    public static void EditSpecialGrub(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
    {
        orig(self);

        if (self.gameObject.scene.name == SpeicalGrubSceneName && self.gameObject.name == "Dream Dialogue" && self.FsmName == "npc_dream_dialogue")
        {
            if (HKVocals._saveSettings.GrubConvo < 9) 
            {
                HKVocals._saveSettings.GrubConvo += 1;
                self.GetFsmStringVariable("Convo Name").Value = $"GRUB_BOTTLE_DREAM_S_{HKVocals._saveSettings.GrubConvo}";
                self.GetFsmStringVariable("Sheet Name").Value = "Elderbug";
                AudioUtils.TryPlayAudioFor($"GRUB_BOTTLE_DREAM_S_{HKVocals._saveSettings.GrubConvo}");
            }
            else
            {
                self.GetFsmStringVariable("Convo Name").Value = $"GRUB_BOTTLE_DREAM_S_REPEAT_0";
                self.GetFsmStringVariable("Sheet Name").Value = "Elderbug";
                AudioUtils.TryPlayAudioFor($"GRUB_BOTTLE_DREAM_S_REPEAT_0");
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
            "GRUB_BOTTLE_DREAM_S_3 " => "Repetitively now it’s fist draws back, as if readying to shatter this invisible prison. But it only swipes the air.",
            "GRUB_BOTTLE_DREAM_S_4 " => "It wishes not to destroy my confines, but my pride.",
            "GRUB_BOTTLE_DREAM_S_5 " => "Does it really intend to mock and shame a helpless grub as I? What evil bug it must be, to knowingly prolong this torture, torn from my kin.",
            "GRUB_BOTTLE_DREAM_S_6" => "From my… Grubfather.",
            "GRUB_BOTTLE_DREAM_S_7 " => "I stare back with hopeful joy, *scoff* it must think me ignorant. If only it knew of my hatred",
            "GRUB_BOTTLE_DREAM_S_8" => "When the time is right and this bug least expects it...",
            "GRUB_BOTTLE_DREAM_S_9 " => "I will gladly return the favor.",
            "GRUB_BOTTLE_DREAM_S_REPEAT_0" => "…",
            _ => orig
        };
    }
}
