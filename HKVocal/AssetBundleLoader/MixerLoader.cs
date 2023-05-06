using UnityEngine.Audio;
using Satchel;

namespace HKVocals;

public static class MixerLoader
{
    public static AssetBundle mixerBundle;
    public static AudioMixerGroup HKVAudioGroup;
    public static AudioMixerGroup DampenExcludeMusic;
    public static AudioMixerGroup DampenExcludeSFX;
    private static AudioMixer HKVMixer;
    private static readonly Dictionary<string, AudioMixerSnapshot> AudioMixerSnapshots = new();

    public static void LoadAssetBundle()
    {
        mixerBundle = AssetBundle.LoadFromMemory(AssemblyUtils.GetBytesFromResources("Resources.mixerbundle"));

        mixerBundle.LoadAllAssets(typeof(AudioMixer)).ForEach(x =>
        {
            HKVMixer = x as AudioMixer;
            Object.DontDestroyOnLoad(HKVMixer);
        });
        
        mixerBundle.LoadAllAssets(typeof(AudioMixerGroup)).ForEach(x =>
        {
            if (x.name == "HKV AudioMixerGroup")
            {
                HKVAudioGroup = x as AudioMixerGroup;
                Object.DontDestroyOnLoad(HKVAudioGroup);
            }
            else if (x.name == "Dampen Exclude Music")
            {
                DampenExcludeMusic = x as AudioMixerGroup;
                Object.DontDestroyOnLoad(DampenExcludeMusic);
            }
            else if (x.name == "Dampen Exclude SFX")
            {
                DampenExcludeSFX = x as AudioMixerGroup;
                Object.DontDestroyOnLoad(DampenExcludeSFX);
            }
        });

        foreach (var snaphotName in Enum.GetNames(typeof(Snapshots)))
        {
            AudioMixerSnapshots[snaphotName] = HKVMixer.FindSnapshot(snaphotName);
        }
        
        HKVMixer.SetFloat("Dampen Exclude Music Volume", MiscUtils.GetDecibelVolume(GameManager.instance.gameSettings.musicVolume));
        HKVMixer.SetFloat("Dampen Exclude SFX Volume", MiscUtils.GetDecibelVolume(GameManager.instance.gameSettings.soundVolume));

        OnMenuAudioSlider.AfterOrig.SetMusicLevel += SetDampenExcludeMusicVolume;
        OnMenuAudioSlider.AfterOrig.SetSoundLevel += SetDampenExcludeSFXVolume;
    }

    /// <summary>
    /// Sets the audio mixer to the specified snapshot. used to add different sfx to the audio
    /// </summary>
    /// <param name="snapshot"></param>
    public static void SetSnapshot(Snapshots snapshot)
    {
        AudioMixerSnapshots[snapshot.ToString()].TransitionTo(0.1f);
    }
    
    /// <summary>
    /// Sets the audio mixer to the one specified for that scene. used to add different sfx to the audio
    /// </summary>
    /// <param name="sceneName"></param>
    public static void SetSnapshot(string sceneName)
    {
        if (SnapshotsByScene.TryGetValue(sceneName, out var snapshot))
        {
            SetSnapshot(snapshot);
        }
        else
        {
            SetSnapshot(Snapshots.No_Effect);
        }
    }

    /// <summary>
    /// Sets the volume level of the mixer to the value in gs.
    /// </summary>
    public static void SetMixerVolume()
    {
        HKVMixer.SetFloat("VA Volume", MiscUtils.GetDecibelVolume(HKVocals._globalSettings.volume));
    }
    /// <summary>
    /// Sets the output mixer group of the source to ours
    /// </summary>
    public static void SetMixerGroup(this AudioSource source)
    {
        if (source.outputAudioMixerGroup == null || source.outputAudioMixerGroup != HKVAudioGroup)
        {
            source.outputAudioMixerGroup = HKVAudioGroup;
        }
    }
    
    private static void SetDampenExcludeMusicVolume(OnMenuAudioSlider.Delegates.Params_SetMusicLevel args)
    {
        HKVMixer.SetFloat("Dampen Exclude Music Volume", MiscUtils.GetDecibelVolume(args.musicLevel));
    }
    private static void SetDampenExcludeSFXVolume(OnMenuAudioSlider.Delegates.Params_SetSoundLevel args)
    {
        HKVMixer.SetFloat("Dampen Exclude SFX Volume", MiscUtils.GetDecibelVolume(args.soundLevel));
    }

    private static readonly Dictionary<string, Snapshots> SnapshotsByScene = new ()
    {
        { "Opening_Sequence", Snapshots.Cave },
        { "Tutorial_01", Snapshots.Cave },
        { "Town", Snapshots.Cliffs },
        { "Room_Town_Stag_Station", Snapshots.Cave },
        { "Room_Charm_Shop", Snapshots.Room },
        { "Room_Mender_House", Snapshots.Room },
        { "Room_mapper", Snapshots.Room },
        { "Room_nailmaster", Snapshots.Room },
        { "Room_nailmaster_02", Snapshots.Room },
        { "Room_nailmaster_03", Snapshots.Cave },
        { "Room_nailsmith", Snapshots.Room },
        { "Room_shop", Snapshots.Room },
        { "Room_Sly_Storeroom", Snapshots.Room },
        { "Room_temple", Snapshots.Arena },
        { "Room_ruinhouse", Snapshots.Room },
        { "Room_Mask_Maker", Snapshots.Cave },
        { "Room_Mansion", Snapshots.Room },
        { "Room_Bretta", Snapshots.Room },
        { "Room_Bretta_Basement", Snapshots.Room },
        { "Room_Ouiji", Snapshots.Cave },
        { "Room_Jinn", Snapshots.Cave },
        { "Room_Colosseum_01", Snapshots.Cave },
        { "Room_Colosseum_02", Snapshots.Cave },
        { "Room_Slug_Shrine", Snapshots.Room },
        { "Crossroads_10", Snapshots.Cave },
        { "Crossroads_11_alt", Snapshots.Cave },
        { "Crossroads_33", Snapshots.Cave },
        { "Crossroads_45", Snapshots.Cave },
        { "Crossroads_ShamanTemple", Snapshots.Cave },
        { "Crossroads_47", Snapshots.Cave },
        { "Crossroads_50", Snapshots.Cave },
        { "Crossroads_52", Snapshots.Cave },
        { "Ruins_House_03", Snapshots.Room },
        { "Ruins_Elevator", Snapshots.Room },
        { "Ruins_Bathhouse", Snapshots.Room },//todo: fix
        //{ "Ruins_Bathhouse", Snapshots.Room TO MARISSA },
        //{ "Ruins_Bathhouse", Snapshots.Spa TO MILLIBELLE },
        { "Ruins1_02", Snapshots.Cave },
        { "Ruins1_04", Snapshots.Cave },
        { "Ruins1_05b", Snapshots.Cave },
        { "Ruins1_06", Snapshots.Cave },
        { "Ruins1_23", Snapshots.Cave },
        { "Ruins1_24", Snapshots.Cave },
        { "Ruins1_27", Snapshots.Cliffs },
        { "Ruins1_29", Snapshots.Cave },
        { "Ruins1_31", Snapshots.Cave },
        { "Ruins1_32", Snapshots.Cave },
        { "Ruins2_04", Snapshots.Cliffs },
        { "Ruins2_08", Snapshots.Cave },
        { "Ruins2_11", Snapshots.Arena },
        { "Ruins2_Watcher_Room", Snapshots.Arena },
        { "Fungus1_04", Snapshots.Cave },
        { "Fungus1_06", Snapshots.Cave },
        { "Fungus1_08", Snapshots.Cave },
        { "Fungus1_13", Snapshots.Cave },
        { "Fungus1_16_alt", Snapshots.Cave },
        { "Fungus1_17", Snapshots.Cave },
        { "Fungus1_19", Snapshots.Cave },
        { "Fungus1_20_v02", Snapshots.Cave },
        { "Fungus1_21", Snapshots.Cave },
        { "Fungus1_24", Snapshots.Cave },
        { "Fungus1_30", Snapshots.Cave },
        { "Fungus1_32", Snapshots.Cave },
        { "Fungus1_35", Snapshots.Arena },
        { "Fungus2_01", Snapshots.Cave },
        { "Fungus2_02", Snapshots.Cave },
        { "Fungus2_04", Snapshots.Cave },
        { "Fungus2_07", Snapshots.Cave },
        { "Fungus2_09", Snapshots.Cave },
        { "Fungus2_12", Snapshots.Cave },
        { "Fungus2_14", Snapshots.Cave },
        { "Fungus2_18", Snapshots.Cave },
        { "Fungus2_20", Snapshots.Cave },
        { "Fungus2_21", Snapshots.Cave },
        { "Fungus2_23", Snapshots.Cave },
        { "Fungus2_25", Snapshots.Cave },
        { "Fungus2_26", Snapshots.Cave },
        { "Fungus2_30", Snapshots.Cave },
        { "Fungus2_32", Snapshots.Cave },
        { "Fungus2_34", Snapshots.Cave },
        { "Fungus3_23", Snapshots.Cave },
        { "Fungus3_25", Snapshots.Fog_Canyon },
        { "Fungus3_34", Snapshots.Cave },
        { "Fungus3_35", Snapshots.Cave },
        { "Fungus3_39", Snapshots.Cave },
        { "Fungus3_40", Snapshots.Cave },
        { "Fungus3_44", Snapshots.Cave },
        { "Fungus3_47", Snapshots.Fog_Canyon },
        { "Fungus3_49", Snapshots.Cave },
        { "Fungus3_archive_02", Snapshots.Cave },
        { "Cliffs_01", Snapshots.Room },
        { "Cliffs_02", Snapshots.Cliffs },
        { "Cliffs_03", Snapshots.Cave },
        { "Cliffs_05", Snapshots.Cave },
        { "Cliffs_06", Snapshots.Cave },
        { "RestingGrounds_02", Snapshots.Cave },
        { "RestingGrounds_04", Snapshots.Arena },
        { "RestingGrounds_07", Snapshots.Cave },
        { "RestingGrounds_08", Snapshots.Cave },
        { "RestingGrounds_09", Snapshots.Cave },
        { "Mines_13", Snapshots.Cave },
        { "Mines_30", Snapshots.Cave },
        { "Deepnest_09", Snapshots.Cave },
        { "Deepnest_14", Snapshots.Cave },
        { "Deepnest_30", Snapshots.Spa },
        { "Deepnest_33", Snapshots.Cave },
        { "Deepnest_40", Snapshots.Cave },
        { "Deepnest_41", Snapshots.Cave },
        { "Deepnest_44", Snapshots.Cave },
        { "Deepnest_Spider_Town", Snapshots.Cave }, 
        { "Room_spider_small", Snapshots.Cave }, 
        { "Deepnest_01b", Snapshots.Cave },
        { "Deepnest_East_01", Snapshots.Cave },
        { "Deepnest_East_03", Snapshots.Arena },
        { "Deepnest_East_04", Snapshots.Cave }, 
        { "Deepnest_East_10", Snapshots.Cave }, 
        { "Deepnest_East_17", Snapshots.Cave },
        { "Deepnest_East_Hornet", Snapshots.Arena },
        { "Abyss_04", Snapshots.Cave },
        { "Abyss_05", Snapshots.Arena },
        { "Abyss_06_Core", Snapshots.Cave }, //todo: fix
        //{ "Abyss_06_Core", Snapshots.Cave FOR LORE TABLET },
        //{ "Abyss_06_Core", Snapshots.Arena FOR HORNET },
        { "Abyss_09", Snapshots.Arena },
        { "Abyss_15", Snapshots.Arena },
        { "Abyss_17", Snapshots.Cave },
        { "Abyss_21", Snapshots.Cave },
        { "Abyss_22", Snapshots.Cave },
        { "Room_Queen", Snapshots.Cave },
        { "Waterways_03", Snapshots.Sewerpipe },
        { "Waterways_05", Snapshots.Cave },
        { "Waterways_07", Snapshots.Sewerpipe },
        { "Waterways_09", Snapshots.Cave },
        { "Waterways_15", Snapshots.Cave },
        { "White_Palace_08", Snapshots.Cave },
        { "White_Palace_09", Snapshots.Arena },
        { "White_Palace_13", Snapshots.Arena },
        { "White_Palace_18", Snapshots.Arena },
        { "Hive_05", Snapshots.Cave },
        { "Grimm_Divine", Snapshots.Room },
        { "Grimm_Main_Tent", Snapshots.Cave },
        { "Dream_Nailcollection", Snapshots.Dream },
        { "Dream_01_False_Knight", Snapshots.Cave },
        { "Dream_02_Mage_Lord", Snapshots.Cave },
        { "Dream_04_White_Defender", Snapshots.Cave },
        { "Dream_Mighty_Zote", Snapshots.Room },
        { "Dream_Backer_Shrine", Snapshots.Dream },
        { "Dream_Room_Believer_Shrine", Snapshots.Dream },
        { "Dream_Abyss", Snapshots.Arena },
        { "Dream_Final_Boss", Snapshots.Arena },
        { "Room_Final_Boss_Atrium", Snapshots.Arena },
        { "GG_Waterways", Snapshots.Arena },
        { "GG_Brooding_Mawlek", Snapshots.Cave },
        { "GG_Collector", Snapshots.Cave },
        { "GG_Crystal_Guardian", Snapshots.Cave },
        { "GG_Crystal_Guardian_2", Snapshots.Cave },
        { "GG_Dung_Defender", Snapshots.Cave },
        { "GG_Failed_Champion", Snapshots.Arena },
        { "GG_False_Knight", Snapshots.Cave },
        { "GG_Flukemarm", Snapshots.Sewerpipe },
        { "GG_God_Tamer", Snapshots.Cave },
        { "GG_Grey_Prince_Zote", Snapshots.Cave },
        { "GG_Grimm", Snapshots.Cave },
        { "GG_Grimm_Nightmare", Snapshots.Arena },
        { "GG_Gruz_Mother", Snapshots.Cave },
        { "GG_Hive_Knight", Snapshots.Cave },
        { "GG_Hornet_1", Snapshots.Cave },
        { "GG_Hornet_2", Snapshots.Cave },
        { "GG_Lurker", Snapshots.Cave },
        { "GG_Mantis_Lords", Snapshots.Cave },
        { "GG_Mega_Moss_Charger", Snapshots.Cave },
        { "GG_Nailmasters", Snapshots.Cave },
        { "GG_Oblobbles", Snapshots.Cave },
        { "GG_Painter", Snapshots.Cave },
        { "GG_Radiance", Snapshots.Arena },
        { "GG_Sly", Snapshots.Arena },
        { "GG_Soul_Master", Snapshots.Cave },
        { "GG_Soul_Tyrant", Snapshots.Cave },
        { "GG_Traitor_Lord", Snapshots.Cave },
        { "GG_Uumuu", Snapshots.Cave },
        { "GG_Vengefly", Snapshots.Arena },
        { "GG_Watcher_Knights", Snapshots.Arena },
        { "GG_White_Defender", Snapshots.Cave },
        { "GG_Workshop", Snapshots.Cave },
        { "Room_GG_Shortcut", Snapshots.Arena },
        { "GG_Engine", Snapshots.Cave },
        { "GG_Engine_Prime", Snapshots.Arena },
        { "GG_Engine_Root", Snapshots.Arena },
        { "GG_Mage_Knight", Snapshots.Cave },
        { "GG_Vengefly_V", Snapshots.Arena },
        { "GG_Entrance_Cutscene", Snapshots.Arena },
        { "GG_Boss_Door_Entrance", Snapshots.Arena },
        { "GG_Mighty_Zote", Snapshots.Cave },
        { "GG_Gruz_Mother_V", Snapshots.Cave },
        { "GG_Brooding_Mawlek_V", Snapshots.Cave },
        { "GG_Mantis_Lords_V", Snapshots.Arena },
        { "GG_Uumuu_V", Snapshots.Cave },
        { "GG_Mage_Knight_V", Snapshots.Cave },
        { "GG_Collector_V", Snapshots.Cave },
        { "GG_Wyrm", Snapshots.Arena },
        { "GG_Unn", Snapshots.Arena },
        { "GG_Unlock_Wastes", Snapshots.Cliffs },
    };
}

public enum Snapshots
{
    No_Effect,
    Dream,
    Cave,
    Spa,
    Cliffs,
    Room,
    Arena,
    Sewerpipe,
    Fog_Canyon,
}