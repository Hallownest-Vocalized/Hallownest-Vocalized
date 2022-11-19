using MonoMod.RuntimeDetour;

namespace HKVocals.MajorFeatures;

public static class NPCDialogue
{
    public static bool DidPlayAudioOnDialogueBox;

    //key: clip name, value: The mute audio source data
    private static readonly Dictionary<string, MuteAudioSourceData> ToMuteAudioSources = new ();

    public static void Hook()
    {
        OnDialogueBox.AfterOrig.ShowPage += PlayAudioForNPCDialogue;
        OnDialogueBox.BeforeOrig.HideText += StopAudioOnDialogueBoxClose;

        FSMEditData.AddAnyFsmEdit("Conversation Control", RemoveOriginalNPCSounds);

        FSMEditData.AddGameObjectFsmEdit("DialogueManager", "Box Open", UnMuteAudiosThatWereMuted);
        
        //remove audio in special 
        FSMEditData.AddGameObjectFsmEdit("Iselda", "Conversation Control", DontSetMetIseldaPDBool);
        Hooks.HookStateEntered(new FSMData("Iselda", "Shop Anim", "Shop Start"), ChangeIseldaShopAudio);
        FSMEditData.AddGameObjectFsmEdit("Maskmaker NPC", "Conversation Control", ReplayMaskMakerAudioOnFinish);
        Hooks.HookStateEntered(new FSMData("Maskmaker NPC", "Conversation Control", "Mask Choice"), RemoveMaskMakerAudioOnSpeak);
        Hooks.HookStateEntered(new FSMData("Maskmaker NPC", "Conversation Control", "Play Audio"), MakeSureMaskMakerAudioIsReplayed);
        FSMEditData.AddGameObjectFsmEdit("Shop Region", "Shop Region", RemoveLemmAndSlyShopAudio);
        FSMEditData.AddGameObjectFsmEdit("Relic Dealer","Relic Discussions", RemoveRelicDiscussionsAudio);
        Hooks.HookStateEntered(new FSMData("Fluke Hermit", "Conversation Control", "Box Up"), RemoveFlukeHermitAudio);
        
        //we need a way to get the AudioSources that play the to mute audios that not as expensive as Resources.FindObjectsOfTypeAll<AudioSource>() 
        HookAudioSourcePlays();
    }

    private static void PlayAudioForNPCDialogue(OnDialogueBox.Delegates.Params_ShowPage args)
    {
        //DialogueBox is a component of DialogueManager/Text
        var dialogueManager = args.self.gameObject.transform.parent.gameObject;

        bool isDreamBoxOpen = dialogueManager.Find("Box Dream").GetComponent<MeshRenderer>().enabled;
        bool isDialogueBoxOpen = dialogueManager.Find("DialogueBox").Find("backboard").GetComponent<SpriteRenderer>().enabled;

        // we dont wanna play dn dialogue when toggled off
        if (!HKVocals._globalSettings.dnDialogue && isDreamBoxOpen)
        {
            return;
        }

        //convos start at _0 but page numbers start from 1
        int convoNumber = args.self.currentPage - 1;
        string convo = args.self.currentConversation + "_" + convoNumber;

        float removeTime = convoNumber == 0 ? 3/5f : 3/4f;

        //this controls scroll lock and autoscroll
        DidPlayAudioOnDialogueBox = AudioUtils.TryPlayAudioFor(convo, removeTime);

        if (DidPlayAudioOnDialogueBox || true) //todo: change after testing complete
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
    }
    
    private static void UnMuteAudiosThatWereMuted(PlayMakerFSM fsm)
    {
        Action unmute = () =>
        {
            ToMuteAudioSources.RemoveValues(data => data.IsNull());
            ToMuteAudioSources.Values.ForEach(data =>
            {
                data.UnMute();
                
                CoroutineHelper.WaitForFramesBeforeInvoke(20, data.ReplayAudio);
            });
        };

        fsm.AddFsmMethod("Box Down", unmute);
        fsm.AddFsmMethod("Box Down No HUD", unmute);
    }
    
    private static void StopAudioOnDialogueBoxClose(OnDialogueBox.Delegates.Params_HideText args)
    {
        AudioUtils.StopPlaying();
    }
    
    private static void RemoveOriginalNPCSounds(PlayMakerFSM fsm)
    {
        foreach (FsmState state in fsm.FsmStates)
        {
            state.DisableFsmActionsThatAre(action => action.IsAudioAction() && !action.GetClipNames().Any(clip => ClipsToInclude.Contains(clip)));
        }
    }

    #region Remove Audio Special Cases
    private static void DontSetMetIseldaPDBool(PlayMakerFSM fsm)
    {
        //disable the action that sets the pdBool
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
        "Mr_Mush_Talk_Loop",
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
        "new_heartpiece_puzzle_bit"
        
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