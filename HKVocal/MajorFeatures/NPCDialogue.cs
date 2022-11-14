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

        FSMEditData.AddGameObjectFsmEdit("DialogueManager", "Box Open", UnMuteAudios);
        FSMEditData.AddGameObjectFsmEdit("Iselda", "Shop Anim", ReduceIseldaOpenShopClipVolume);
        FSMEditData.AddGameObjectFsmEdit("Iselda", "Conversation Control", FindIfFirstIseldaConvo);
        FSMEditData.AddGameObjectFsmEdit("Maskmaker NPC", "Conversation Control", RemoveMaskMakerAudio);
        FSMEditData.AddGameObjectFsmEdit("Shop Region", "Shop Region", f =>
        {
            if (f.gameObject.scene.name == "Room_mapper")
            {
                f.GetAction<SetPlayerDataBool>("Box Up", 0).Enabled = false;
                
                var y =f.AddFsmBoolVariable("YO WTF");
                y.Value = false;

                f.InsertMethod("Check Intro Bool", () =>
                {
                    if (GameObject.Find("Iselda").LocateMyFSM("Shop Anim").FindFsmBoolVariable("LMAO_WTF").Value)
                    {
                        f.SetState("Title Up");
                    }
                    if (PlayerDataAccess.metIselda)
                    {
                        f.SetState("Sly Key Convo?");
                    }
                },0);
                f.GetAction<PlayerDataBoolTest>("Check Intro Bool", 1).Enabled = false;
                f.InsertMethod("Sly Key Convo?",() =>
                {
                    if (!y.Value)
                    {
                        if (GameObject.Find("Iselda").LocateMyFSM("Shop Anim").FindFsmBoolVariable("LMAO_WTF").Value)
                        {
                            f.SetState("Box Up");
                        }
                    }
                }, 0);

                f.InsertMethod("Box Down", () => y.Value = true, 0);
                
                f.MakeLog();
            }
            f.ChangeTransition("Voice", "LEMM", "Convo");
            f.ChangeTransition("Voice", "SLY", "Convo");
            f.GetState("Convo Relic").DisableActionsThatAre(a => a.IsAudioAction());
        });
        FSMEditData.AddGameObjectFsmEdit("Relic Dealer","Relic Discussions", f =>
        { 
            f.GetState("Convo").DisableActionsThatAre(a => a.IsAudioAction());
        });
        
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

        if (DidPlayAudioOnDialogueBox)
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
    }
    
    private static void RemoveOriginalNPCSounds(PlayMakerFSM fsm)
    {
        foreach (FsmState state in fsm.FsmStates)
        {
            state.DisableActionsThatAre(action => action.IsAudioAction()); // && action.GetClipNames().Any(clip => ClipsToInclude.Contains(clip)));
        }
    }

    private static void FindIfFirstIseldaConvo(PlayMakerFSM fsm)
    {
        fsm.GetAction<SetPlayerDataBool>("Meet", 2).Enabled = false;
        var b1 = fsm.AddFsmBoolVariable("ISELDA_MEET");
        b1.Value = false;
    }
    
    private static void ReduceIseldaOpenShopClipVolume(PlayMakerFSM fsm)
    {
        var action = fsm.GetAction<AudioPlayerOneShotSingle>("Shop Start", 1);
        action.Enabled = false;
        var wtf = fsm.AddFsmBoolVariable("LMAO_WTF");
        wtf.Value = false;
        fsm.AddMethod("Shop Start",() =>
        {
            if (!PlayerDataAccess.metIselda)
            {
                wtf.Value = true;
                if (fsm.gameObject.LocateMyFSM("Conversation Control").GetFsmBoolVariable("ISELDA_MEET").Value)
                {
                    HKVocals.instance.Log("I SEMI HATE ISELDA");
                    PlayerDataAccess.metIselda = true;
                }
                else
                {
                    HKVocals.instance.Log("I HATE ISELDA");
                    fsm.gameObject.LocateMyFSM("Conversation Control").GetFsmBoolVariable("ISELDA_MEET").Value = true;
                }
            }
            else
            {
                HKVocals.instance.Log("I KINDA HATE ISELDA");
                if (action.audioPlayer.Value != null)
                {
                    action.storePlayer.Value = action.audioPlayer.Value.Spawn(action.spawnPoint.Value.transform.position, Quaternion.Euler(Vector3.up));
                    var source = action.storePlayer.Value.GetComponent<AudioSource>();
                    var clip = action.audioClip.Value as AudioClip;
                    source.pitch = 1f;
                    source.volume = 0.4f;
                    if (clip != null) source.PlayOneShot(clip);
                }
            }
        });
    }
    
    private static void RemoveMaskMakerAudio(PlayMakerFSM fsm)
    {
        fsm.InsertMethod("Mask Choice", () => fsm.gameObject.transform.Find("Voice Loop").GetComponent<AudioSource>().Stop(), 0);

        fsm.ChangeTransition("Mask1", "CONVO_FINISH", "Play Audio");
        fsm.ChangeTransition("Mask 2", "CONVO_FINISH", "Play Audio");
        fsm.ChangeTransition("Mask 3", "CONVO_FINISH", "Play Audio");
        fsm.ChangeTransition("Mask 4", "CONVO_FINISH", "Play Audio");
        
        fsm.InsertMethod("Play Audio", () =>
        {
            fsm.gameObject.transform.Find("Voice Loop").GetComponent<AudioSource>().volume = 1f;
            fsm.gameObject.transform.Find("Voice Loop").GetComponent<AudioSource>().pitch = 1f;
            fsm.gameObject.transform.Find("Voice Loop").GetComponent<AudioSource>().Play();
        }, 0);
    }

    private static void UnMuteAudios(PlayMakerFSM fsm)
    {
        Action unmute = () =>
        {
            ToMuteAudioSources.Values.ForEach(data =>
            {
                if (data.muted)
                {
                    data.UnMute();
                }
            });
        };

        fsm.AddMethod("Box Down", unmute);
        fsm.AddMethod("Box Down No HUD", unmute);
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
        "Sly_talk",
        "Sly_talk_02",
        "Sly_talk_03",
        "Sly_talk_04",
        "Sly_talk_05",
        "Banker_talk_01",
        "Stag_ambient_loop",
        "Mr_Mush_Talk_Loop",
        "junk_fluke_long_loop",
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