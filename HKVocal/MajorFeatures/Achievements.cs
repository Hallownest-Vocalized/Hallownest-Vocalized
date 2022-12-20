using System.Diagnostics;
using SFCore;

namespace HKVocals.MajorFeatures;

public static class Achievements
{
    public static void Hook() 
    {
        AchievementHelper.AddAchievement("CompendiumVocalization",Sprite.Create(Texture2D.blackTexture, Rect.zero, Vector2.zero),"Compendium Vocalization","Listen to every line of vocalized dialogue in Hallownest.",false);
        AchievementHelper.AddAchievement("ImpatientLemm",MiscUtils.LoadSprite("Resources/Impatient.png",Rect.zero,Vector2.zero, 1,1),"Impatient","Leave a relic in Lemm’s deposit box.",false);
        AchievementHelper.AddAchievement("DisdainGrub",MiscUtils.LoadSprite("Resources/Disdain.png",Rect.zero,Vector2.zero, 1,1),"Disdain","Read the dreams of a particularly ungrateful Grub.",false);
        AchievementHelper.AddAchievement("KindnessPaleLurker",Sprite.Create(Texture2D.blackTexture, Rect.zero, Vector2.zero),"Kindness","Show the Pale Lurker a new perspective on life.",false);
        AchievementHelper.AddAchievement("LastLaughOrdeal",Sprite.Create(Texture2D.blackTexture, Rect.zero, Vector2.zero),"Last Laugh","Hit the lever below The Eternal Ordeal’s Zote statue.",false);
        AchievementHelper.AddAchievement("AlubafarDreamnail",Sprite.Create(Texture2D.blackTexture, Rect.zero, Vector2.zero),"Alubafar","Listen to what an Aluba has to say",true);

        ModHooks.LanguageGetHook += AchLang;
        On.EnemyDreamnailReaction.RecieveDreamImpact += Aluba;
    }

    private static string AchLang(string key, string sheettitle, string orig)
    {
       return key switch
        {
            "Compendium Vocalization" => "Compendium Vocalization",
            "Listen to every line of vocalized dialogue in Hallownest." => "Listen to every line of vocalized dialogue in Hallownest.",
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
            _ => orig
        };
    }

    private static void CompendiumVocalization()
    {
        
    } 
    private static void Aluba(On.EnemyDreamnailReaction.orig_RecieveDreamImpact orig, EnemyDreamnailReaction self)
    {
        if(self.gameObject.name == "Aluba")
            GameManager.instance.AwardAchievement("AlubafarDreamnail");
        orig(self);
    }
}