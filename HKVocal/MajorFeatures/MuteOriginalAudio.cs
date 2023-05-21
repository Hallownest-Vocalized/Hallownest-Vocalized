using MonoMod.RuntimeDetour;
using Satchel;

namespace HKVocals.MajorFeatures;

/// <summary>
/// Mutes some original ingame audio that collides with vas
/// </summary>
public static class MuteOriginalAudio
{
    //key: clip name, value: The mute audio source data
    private static readonly Dictionary<string, MuteAudioSourceData> ToMuteAudioSources = new ();
    
    public static void Hook()
    {
        FSMEditData.AddAnyFsmEdit("Conversation Control", RemoveOriginalNPCSounds);
        
        //we need a way to get the AudioSources that play the to mute audios that not as expensive as Resources.FindObjectsOfTypeAll<AudioSource>() 
        HookAudioSourcePlays();
        
        //mute/unmute those audios we collected in the audiosource play hooks
        NPCDialogue.OnPlayNPCDialogue += MuteCollectedAudios;
        OnDialogueBox.BeforeOrig.HideText += UnMuteCollectedAudios;

        //remove audio special s=cases 
        
        //make iselda not play her shop open audio the first time and subsequently play it at lower volume
        FSMEditData.AddGameObjectFsmEdit("Iselda", "Conversation Control", DontSetMetIseldaPDBool);
        Hooks.HookStateEntered(new ("Iselda", "Shop Anim", "Shop Start"), ChangeIseldaShopAudio);
        
        //remove the audio mask masker loops on but also 1) replay it by rerouting the fsm and 2) make sure its replayed by manually calling play on audiosource
        Hooks.HookStateEntered(new ("Maskmaker NPC", "Conversation Control", "Mask Choice"), RemoveMaskMakerAudioOnSpeak);
        Hooks.HookStateEntered(new ("Maskmaker NPC", "Conversation Control", "Play Audio"), MakeSureMaskMakerAudioIsReplayed);
        FSMEditData.AddGameObjectFsmEdit("Maskmaker NPC", "Conversation Control", ReplayMaskMakerAudioOnFinish);
        
        //remove audio of sly and lemm thats found in shop reigon fsm
        FSMEditData.AddGameObjectFsmEdit("Shop Region", "Shop Region", RemoveLemmAndSlyShopAudio);
        
        //remove audio after relic sold cuz conflict with va
        FSMEditData.AddGameObjectFsmEdit("Relic Dealer","Relic Discussions", RemoveRelicDiscussionsAudio);
        
        //manually store fluke hermits AudioSource cuz the hooks arent picking it up
        Hooks.HookStateEntered(new ("Fluke Hermit", "Conversation Control", "Box Up"), RemoveFlukeHermitAudio);
    }

    private static void RemoveOriginalNPCSounds(PlayMakerFSM fsm)
    {
        foreach (FsmState state in fsm.FsmStates)
        {
            state.DisableFsmActionsThatAre(action => action.IsAudioAction() && !action.GetClipNames().Any(clip => ClipsToInclude.Contains(clip)));
            foreach (var action in state.Actions)
            {
                if (action.IsAudioAction())
                {
                    action.GetClipNames().Where(x => ClipsToInclude.Contains(x)).ForEach(HKVocals.instance.Log);
                }
            }
        }
    }

    private static void MuteCollectedAudios()
    {
        ToMuteAudioSources.RemoveValues(data => data.IsNull());
        ToMuteAudioSources.Values.ForEach(data =>
        {
            //we wanna make sure that audiosource is not playing something else
            if (ClipsToMute.Contains(data.originalAudioSource.clip.name))
            {
                data.Mute();
            }
        });
    }
    
    private static void UnMuteCollectedAudios(OnDialogueBox.Delegates.Params_HideText args)
    {
        ToMuteAudioSources.RemoveValues(data => data.IsNull());
        ToMuteAudioSources.Values.ForEach(data =>
        {
            data.UnMute();
                
            CoroutineHelper.WaitForFramesBeforeInvoke(20, data.ReplayAudio);
        });
    }
    

    #region Remove Audio Special Cases
    private static void DontSetMetIseldaPDBool(PlayMakerFSM fsm)
    {
        //disable the action that sets the pdBool cuz we dont want to play audio the first time
        fsm.DisableFsmAction("Meet", 2);
    }
    private static void ChangeIseldaShopAudio(PlayMakerFSM fsm)
    {
        //lower the audio
        var iseldaOpenAudio = fsm.GetFsmAction<AudioPlayerOneShotSingle>("Shop Start", 1);
        iseldaOpenAudio.volume = 0.4f;
        
        //only play the audio every time after the first. we need to wait a bit to set the pd bool cuz this fsm runs first
        if (!PlayerDataAccess.metIselda)
        {
            iseldaOpenAudio.Enabled = false;
            CoroutineHelper.WaitForSecondsBeforeInvoke(2f, () => PlayerDataAccess.metIselda = true);
        }
        else
        {
            iseldaOpenAudio.Enabled = true;
        }
    }
    
    private static void ReplayMaskMakerAudioOnFinish(PlayMakerFSM fsm)
    {
        //we need restart audio after make masked dialogue 
        foreach(string stateToReroute in new[] { "Mask1", "Mask 2", "Mask 3", "Mask 4" })
        {
            fsm.ChangeFsmTransition(stateToReroute, "CONVO_FINISH", "Play Audio");
        }
    }
    private static void RemoveMaskMakerAudioOnSpeak(PlayMakerFSM fsm)
    {
        fsm.gameObject.Find("Voice Loop").GetComponent<AudioSource>().Stop();
    }
    private static void MakeSureMaskMakerAudioIsReplayed(PlayMakerFSM fsm)
    {
        var source = fsm.gameObject.Find("Voice Loop").GetComponent<AudioSource>();
        source.volume = 1f;
        source.pitch = 1f;
        source.Play();
    }
    
    private static void RemoveLemmAndSlyShopAudio(PlayMakerFSM fsm)
    {
        fsm.ChangeFsmTransition("Voice", "LEMM", "Convo");
        fsm.ChangeFsmTransition("Voice", "SLY", "Convo");
        fsm.GetFsmState("Convo Relic").DisableFsmActionsThatAre(a => a.IsAudioAction());
    }
    
    private static void RemoveRelicDiscussionsAudio(PlayMakerFSM fsm)
    {
        //we cant add this to mute list because we do want the same audios to play at other times
        fsm.GetFsmState("Convo").DisableFsmActionsThatAre(a => a.IsAudioAction());
    }
    
    private static void RemoveFlukeHermitAudio(PlayMakerFSM fsm)
    {
        //for some reason our hooks dont capture it so we'll do it manually
        StoreAudioSourceToMute(fsm.gameObject.GetComponent<AudioSource>());
    }
    
    #endregion

    #region Store AudioSources
    private static void HookAudioSourcePlays()
    {
        _ = new Hook(typeof(AudioSource).GetMethod(nameof(AudioSource.Play), Array.Empty<Type>()), ExcludeSomeAudios_Play);
        _ = new Hook(typeof(AudioSource).GetMethod(nameof(AudioSource.Play), new [] { typeof(ulong) }), ExcludeSomeAudios_Play_delay);
        _ = new Hook(typeof(AudioSource).GetMethod(nameof(AudioSource.PlayDelayed)), ExcludeSomeAudios_PlayDelayed);
        _ = new Hook(typeof(AudioSource).GetMethod(nameof(AudioSource.PlayScheduled)), ExcludeSomeAudios_PlayScheduled);
        _ = new Hook(typeof(AudioSource).GetMethod(nameof(AudioSource.PlayOneShot), new [] { typeof(AudioClip), typeof(float)}), ExcludeSomeAudios_PlayOneShot);
    }
    private static bool StoreAudioSourceToMute(AudioSource source) => StoreAudioSourceToMute(source, source.clip);
    private static bool StoreAudioSourceToMute(AudioSource source, AudioClip clip)
    {
        if (clip != null && ClipsToMute.Contains(clip.name))
        {
            ToMuteAudioSources[clip.name] = new(source);
            HKVocals.instance.LogDebug($"Storing audio for mute {clip.name}");
            return true;
        }

        return false;
    }
    
    private static void ExcludeSomeAudios_Play(Action<AudioSource> orig, AudioSource self)
    {
        if (!StoreAudioSourceToMute(self)) orig(self);
    }
    private static void ExcludeSomeAudios_Play_delay(Action<AudioSource, ulong> orig, AudioSource self, ulong delay)
    {
        if (!StoreAudioSourceToMute(self)) orig(self, delay);
    }
    private static void ExcludeSomeAudios_PlayDelayed(Action<AudioSource, float> orig, AudioSource self, float delay)
    {
        if (!StoreAudioSourceToMute(self)) orig(self, delay);
    }
    private static void ExcludeSomeAudios_PlayScheduled(Action<AudioSource, double> orig, AudioSource self, double time)
    {
        if (!StoreAudioSourceToMute(self)) orig(self, time);
    }

    private static void ExcludeSomeAudios_PlayOneShot(Action<AudioSource, AudioClip, float> orig, AudioSource self, AudioClip clip, float volumeScale)
    {
        if (!StoreAudioSourceToMute(self, clip)) orig(self, clip, volumeScale);
    }
    
    #endregion

    // we need to stop these from playing when voice actor (VA) is speaking
    private static readonly List<string> ClipsToMute = new ()
    {
        "Salubra_Laugh_Loop",
        "Sly_talk",
        "Sly_talk_02",
        "Sly_talk_03",
        "Sly_talk_04",
        "Sly_talk_05",
        "Banker_talk_01",
        "Stag_ambient_loop",
        "junk_fluke_long_loop",
        "junk_fluke_long_loop_nervous",
        "Moss_Cultist_Loop",
        "Grimm_talk_02",
        "Grimm_talk_03",
        "Grimm_talk_05",
        "Grimm_talk_06", 
        "Nailmsith_greet",
        "Nailmsith_talk_02",
        "Nailmsith_talk_03",
        "Nailmsith_talk_04",
        "Nailmsith_talk_05",
        "Hornet_Dialogue_Generic_02",
        "Hornet_Dialogue_Generic_03",
        "Hornet_Dialogue_Generic_04",
        "Hornet_Dialogue_Generic_05",
        "Bow_Repeat",
        "WD_outro",
        "Stag_02",
        "Hornet_Greenpath_01",
        "Salubra_Talk",
        "Mr_Mush_talk_03",
        "GS_standard_01",
        "GS_standard_02",
        "GS_standard_05",
        "GS_standard_06",
        "GS_standard_07",
        "GS_engine_room",
        "GS_engine_room_03",
        "GS_engine_room_04",
        "Hunter_journal_02",
        "Relic_Dealer_04",
    };

    private static readonly List<string> ClipsToInclude = new()
    {
        "switch_gate_gate",
        "dream_enter",
        "dream_ghost_appear",
        "Grey_Mourner_wake",
        "stalactite_break",
        "dream_ghost_appear",
        "dream_plant_hit",
        "dream_orb_pickup",
        "dream_enter_pt_2",
        "shaman_charging_spell",
        "shaman_spell",
        "shiny_item_pickup",
        "misc_rumble_loop",
        "misc_rumble_impact",
        "mage_lord_summon_projectiles",
        "dream_plant_hit",
        "mawlek_spit",
        "HK_Cloth_Battle_Cry",
        "Nailmaster_roar_01",
        "Nailmaster_roar_02",
        "Nailmaster_Bama_01",
        "Nailmaster_Bama_02",
        "new_heartpiece_puzzle_bit",
        "manhole_key_turn",
        "coccoon_burst_open",
        "manhole_key_turn",
        "misc_rumble_loop",
        "misc_rumble_impact",
        "coccoon_god_body_fall_out",
        "godseeker_groan_01",
        "godseeker_ground_hit",
        "bathhouse_door_open",
        "manhole_key_turn",
        "jiji_door_open_pt_1",
        "jiji_door_open_pt_2",
        "focus_health_charging",
        "GS_engine_hum_loop",
        "Sly_dazed_loop",
        "Bretta_scared_loop",
        "HK_Cloth_Burst01",
        "hornet_wall_land",
        "hornet_small_jump",
        "hornet_jump",
        "Mr_Mush_Talk_Loop",
        "Nailmaster_Bama_01",
        "Nailmaster_Bama_02",
        "Jiji_Summon",
        "dark_spell_absorb",
        "dark_spell_get",
        "Jiji_retreat_01",
        "hornet_ground_land",
        "boss_explode_clean",
        "ghost_dialogue_death_initial_white",
        "ghost_dialogue_death_explode",
        "ghost_absorb",
        "ghost_absorb_final_impact",
        "hollow_knight_rumble_pre_chain_break",
        "final_boss_atrium_white_4",
        "S83-14 Accordion",
        "shiny_item_loop",
        "manhole_open",
        "grimm_teleport_in",
        "accordion_sting_03",
        "Grimm_click",
        "nightmare_lantern_powering_up",
        "nightmare_lantern_flame_burst",
        "grimm_teleport_out",
        "Brumm_flame_give_short",
        "flamebearer_fire_spiral_prepare",
        "flamebearer_fire_spiral_cast",
        "Brumm_grunt_01",
        "Brumm_grunt_double",
        "nightmare_bg_eye_break_1",
        "nightmare_bg_eye_break_2",
        "spell_information_screen",
        "mawlek_scream",
        "ruin_sentry_sword_1",
        "Divine_eat_01",
        "hatcher_give_birth",
        "Divine_post_eat_sigh",
        "Divine_poop_long",
        "spider_zombie_shake",
        "Divine_poop_end",
        "Leg_Eater_angry_01",
        "GS_engine_room_02",
        "Iselda_Shop_Open",
    };

    private class MuteAudioSourceData
    {
        public AudioSource originalAudioSource;
        public bool muted;
        
        public MuteAudioSourceData(AudioSource _originalAudioSource)
        {
            originalAudioSource = _originalAudioSource;
            muted = false;
        }

        public void Mute()
        {
            HKVocals.instance.LogError($"Muting {originalAudioSource.clip.name}");
            originalAudioSource.mute = true;
            muted = true;
        }
        
        public void UnMute()
        {
            HKVocals.instance.LogError($"UnMuting {originalAudioSource.clip.name}");
            originalAudioSource.mute = false;
            muted = false;
        }

        public bool IsNull()
        {
            return originalAudioSource == null || originalAudioSource.clip == null;
        }

        //for when loop audio sometimes doesnt restart
        public void ReplayAudio()
        {
            if (!IsNull())
            {
                if (!originalAudioSource.isPlaying && originalAudioSource.loop)
                {
                    HKVocals.instance.LogDebug($"Re-playing {originalAudioSource.clip}");
                    originalAudioSource.Play();
                }
            }
        }
    }
}