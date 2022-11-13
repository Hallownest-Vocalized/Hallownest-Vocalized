using MonoMod.RuntimeDetour;

namespace HKVocals.MajorFeatures;

public static class NPCDialogue
{
    public static bool DidPlayAudioOnDialogueBox = false;

    //key: clip name, value: The mute audio source data
    private static Dictionary<string, MuteAudioSourceData> ToMuteAudioSources = new ();

    public static void Hook()
    {
        OnDialogueBox.AfterOrig.ShowPage += PlayNPCDialogue;
        OnDialogueBox.BeforeOrig.HideText += StopAudioOnDialogueBoxClose;
        
        FSMEditData.AddAnyFsmEdit("Conversation Control", RemoveOriginalNPCSounds);

        FSMEditData.AddGameObjectFsmEdit("Iselda", "Shop Anim", ReduceIseldaOpenShopClipVolume);
        
        _ = new Hook(typeof(AudioSource).GetMethod(nameof(AudioSource.Play), new Type[]{}), ExcludeSomeAudios_Play);
        _ = new Hook(typeof(AudioSource).GetMethod(nameof(AudioSource.Play), new Type[] { typeof(ulong) }), ExcludeSomeAudios_Play_delay);
        _ = new Hook(typeof(AudioSource).GetMethod(nameof(AudioSource.PlayDelayed)), ExcludeSomeAudios_PlayDelayed);
        _ = new Hook(typeof(AudioSource).GetMethod(nameof(AudioSource.PlayScheduled)), ExcludeSomeAudios_PlayScheduled);
        _ = new Hook(typeof(AudioSource).GetMethod(nameof(AudioSource.PlayOneShot), new Type[] { typeof(AudioClip), typeof(float)}), ExcludeSomeAudios_PlayOneShot);
    }

    private static void PlayNPCDialogue(OnDialogueBox.Delegates.Params_ShowPage args)
    {
        //dialoguebox is a component of DialogueManager/Text
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

        if (DidPlayAudioOnDialogueBox) //removed for testing
        {
            ToMuteAudioSources.RemoveValues(data => data.originalAudioSource == null || data.originalAudioSource.clip == null);

            ToMuteAudioSources.Values.ForEach(data =>
            {
                //we wanna make sure that audiosource is not playing something else
                if (ClipsToMute.Contains(data.originalAudioSource.clip.name))
                {
                    HKVocals.instance.LogError($"Muting {data.originalAudioSource.clip.name}");
                    data.Mute();
                }
            });
        }
    }
    
    private static void StopAudioOnDialogueBoxClose(OnDialogueBox.Delegates.Params_HideText args)
    {
        AudioUtils.StopPlaying();

        ToMuteAudioSources.Values.ForEach(data =>
        {
            if (data.muted)
            {
                data.UnMute();
            }
        });
    }
    
    public static void RemoveOriginalNPCSounds(PlayMakerFSM fsm)
    {
        foreach (FsmState state in fsm.FsmStates)
        {
            state.DisableActionsThatAre(action => action.IsAudioAction());
        }
    }

    private static void ReduceIseldaOpenShopClipVolume(PlayMakerFSM fsm)
    {
       fsm.GetAction<AudioPlayerOneShotSingle>("Shop Start", 1).volume = 0.1f;
    }

    private static void StoreAudioSource(AudioSource source) => StoreAudioSource(source, source.clip);
    private static void StoreAudioSource(AudioSource source, AudioClip clip)
    {
        if (clip != null && ClipsToMute.Contains(clip.name))
        {
            ToMuteAudioSources[clip.name] = new(source);
        }
    }
    
    private static void ExcludeSomeAudios_Play(Action<AudioSource> orig, AudioSource self)
    {
        StoreAudioSource(self);
        orig(self);
    }
    private static void ExcludeSomeAudios_Play_delay(Action<AudioSource, ulong> orig, AudioSource self, ulong delay)
    {
        StoreAudioSource(self);  
        orig(self, delay);
    }
    private static void ExcludeSomeAudios_PlayDelayed(Action<AudioSource, float> orig, AudioSource self, float delay)
    {
        StoreAudioSource(self);  
        orig(self, delay);
    }
    private static void ExcludeSomeAudios_PlayScheduled(Action<AudioSource, double> orig, AudioSource self, double time)
    {
        StoreAudioSource(self); 
        orig(self, time);
    }

    private static void ExcludeSomeAudios_PlayOneShot(Action<AudioSource, AudioClip, float> orig, AudioSource self, AudioClip clip, float volumeScale)
    {
        StoreAudioSource(self, clip);   
        orig(self, clip, volumeScale);
    }

    // we need to stop these from playing when voice actor (VA) is speaking
    private static readonly List<string> ClipsToMute = new ()
    {
        "Salubra_Laugh_Loop",
        "Sly_talk_02",
        "Banker_talk_01",
        "Stag_ambient_loop",
        "Mr_Mush_Talk_Loop",
        "junk_fluke_long_loop",
        "Mask_Maker_masked_loop",
        "Mask_Maker_unmasked_loop",
        "Moss_Cultist_Loop",
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
            originalAudioSource.mute = true;
            muted = true;
        }
        
        public void UnMute()
        {
            originalAudioSource.mute = false;
            muted = false;
        }
        
    }
}