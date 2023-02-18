using SFCore;
using Satchel;

namespace HKVocals.MajorFeatures;

public static class Achievements
{
    public static void Hook() 
    {
        AchievementHelper.AddAchievement("ImpatientLemm",AssemblyUtils.GetSpriteFromResources("HKVocals.Resources.Impateint.png",66f),"Impatient","Leave a relic in Lemm’s deposit box.",true);
        AchievementHelper.AddAchievement("DisdainGrub",AssemblyUtils.GetSpriteFromResources("HKVocals.Resources.Distain.png",66f),"Disdain","Read the dreams of a particularly ungrateful Grub.",true);
        AchievementHelper.AddAchievement("KindnessPaleLurker",AssemblyUtils.GetSpriteFromResources("HKVocals.Resources.Kindness.png",66f),"Kindness","Show the Pale Lurker a new perspective on life.",true);
        AchievementHelper.AddAchievement("LastLaughOrdeal",AssemblyUtils.GetSpriteFromResources("HKVocals.Resources.Last_Laugh.png",66f),"Last Laugh","Hit the lever below The Eternal Ordeal’s Zote statue.",true);
        AchievementHelper.AddAchievement("AlubafarDreamnail",AssemblyUtils.GetSpriteFromResources("HKVocals.Resources.Alubafar.png",66f),"Alubafar","Listen to what an Aluba has to say",true);

        AchievementHelper.AddAchievement("CompendiumVocalization",AssemblyUtils.GetSpriteFromResources("HKVocals.Resources.Full_compendium.png",66f),"Compendium Vocalization","Listen to every line of vocalized dialogue in Hallownest.",false);
        AchievementHelper.AddAchievement("Consideration",AssemblyUtils.GetSpriteFromResources("HKVocals.Resources.All_Dialogue.png",66f),"Consideration","Listen to every word of Hallownest’s living inhabitants.",false);
        AchievementHelper.AddAchievement("Ambition",AssemblyUtils.GetSpriteFromResources("HKVocals.Resources.All_Dreams.png",66f),"Ambition","Uncover every dream, of bug and spirit alike.",false);
        AchievementHelper.AddAchievement("Chronology",AssemblyUtils.GetSpriteFromResources("HKVocals.Resources.All_Lore_Tablets.png",66f),"Chronology","Find every lore tablet buried under this dead Kingdom.",false);
        AchievementHelper.AddAchievement("Acquisition",AssemblyUtils.GetSpriteFromResources("HKVocals.Resources.All_Items_and_Journal.png",66f),"Acquisition","Review every item and journal entry there is to acquire.",false);
        
        ModHooks.LanguageGetHook += AchLang;
        OnEnemyDreamnailReaction.BeforeOrig.RecieveDreamImpact += Aluba;
    }

    private static string AchLang(string key, string sheettitle, string orig)
    {
        return key switch
        {
                "Impatient" => "Impatient",
                "Leave a relic in Lemm’s deposit box." => "Leave a relic in Lemm’s deposit box.",
                "Disdain" => "Disdain",
                "Read the dreams of a particularly ungrateful Grub." => "Read the dreams of a particularly ungrateful Grub.",
                "Kindness" => "Kindness",
                "Show the Pale Lurker a new perspective on life." => "Show the Pale Lurker a new perspective on life.",
                "Last Laugh" => "Last Laugh",
                "Hit the lever below The Eternal Ordeal’s Zote statue." => "Hit the lever below The Eternal Ordeal’s Zote statue.",
                "Alubafar" => "Alubafar",
                "Listen to what an Aluba has to say" => "Listen to what an Aluba has to say",
                "Compendium Vocalization" => "Compendium Vocalization",
                "Listen to every line of vocalized dialogue in Hallownest." => "Listen to every line of vocalized dialogue in Hallownest.",
                "Consideration" => "Consideration",
                "Listen to every word of Hallownest’s living inhabitants." => "Listen to every word of Hallownest’s living inhabitants.",
                "Ambition" => "Ambition",
                "Uncover every dream, of bug and spirit alike." => "Uncover every dream, of bug and spirit alike.",
                "Chronology" => "Chronology",
                "Find every lore tablet buried under this dead Kingdom." => "Find every lore tablet buried under this dead Kingdom.",
                "Acquisition" => "Acquisition",
                "Review every item and journal entry there is to acquire." => "Review every item and journal entry there is to acquire.",
                _ => orig
        };
    }

    private static void Aluba(OnEnemyDreamnailReaction.Delegates.Params_RecieveDreamImpact args)
    {
        if (args.self.gameObject.name == "Aluba")
        {
            GameManager.instance.AwardAchievement("AlubafarDreamnail");
        }
    }
}
