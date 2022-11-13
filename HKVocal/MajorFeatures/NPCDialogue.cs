using MonoMod.RuntimeDetour;

namespace HKVocals.MajorFeatures;

public static class NPCDialogue
{
    public static bool DidPlayAudioOnDialogueBox = false;
    public static void Hook()
    {
        OnDialogueBox.AfterOrig.ShowPage += PlayNPCDialogue;
        OnDialogueBox.BeforeOrig.HideText += StopAudioOnDialogueBoxClose;
        
        FSMEditData.AddAnyFsmEdit("Conversation Control", RemoveOriginalNPCSounds);
        
        _ = new Hook(typeof(AudioSource).GetMethod(nameof(AudioSource.Play), new Type[]{}), ExcludeSomeAudios_Play);
        _ = new Hook(typeof(AudioSource).GetMethod(nameof(AudioSource.Play), new Type[] { typeof(ulong) }), ExcludeSomeAudios_Play_delay);
        _ = new Hook(typeof(AudioSource).GetMethod(nameof(AudioSource.PlayDelayed)), ExcludeSomeAudios_PlayDelayed);
        _ = new Hook(typeof(AudioSource).GetMethod(nameof(AudioSource.PlayScheduled)), ExcludeSomeAudios_PlayScheduled);
        _ = new Hook(typeof(AudioSource).GetMethod(nameof(AudioSource.PlayOneShot), new Type[] { typeof(AudioClip)}), ExcludeSomeAudios_PlayOneShot_AudioClip);
        _ = new Hook(typeof(AudioSource).GetMethod(nameof(AudioSource.PlayOneShot), new Type[] { typeof(AudioClip), typeof(float)}), ExcludeSomeAudios_PlayOneShot_AudioClip_float);
    }

    private static void StopAudioOnDialogueBoxClose(OnDialogueBox.Delegates.Params_HideText args)
    {
        AudioUtils.StopPlaying();
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
    }
    
    public static void RemoveOriginalNPCSounds(PlayMakerFSM fsm)
    {
        foreach (FsmState state in fsm.FsmStates)
        {
            state.DisableActionsThatAre(action => action.IsAudioAction());
        }
    }

    public static bool ShouldPlayClip(AudioClip clip)
    {
        if (clip != null)
        {
            if (ExcludedClipNames.Contains(clip.name))
            {
                HKVocals.instance.LogDebug($"Not playing clip with name {clip.name}");
                return false; // dont play clip
            }
        }

        return true; //play clip
    }
    
    private static void ExcludeSomeAudios_Play(Action<AudioSource> orig, AudioSource self)
    {
        if (ShouldPlayClip(self.clip)) orig(self);
    }
    private static void ExcludeSomeAudios_Play_delay(Action<AudioSource, ulong> orig, AudioSource self, ulong delay)
    {
        if (ShouldPlayClip(self.clip)) orig(self, delay);
    }
    private static void ExcludeSomeAudios_PlayDelayed(Action<AudioSource, float> orig, AudioSource self, float delay)
    {
        if (ShouldPlayClip(self.clip)) orig(self, delay);
    }
    private static void ExcludeSomeAudios_PlayScheduled(Action<AudioSource, double> orig, AudioSource self, double time)
    {
        if (ShouldPlayClip(self.clip)) orig(self, time);
    }
    private static void ExcludeSomeAudios_PlayOneShot_AudioClip(Action<AudioSource, AudioClip> orig, AudioSource self, AudioClip clip)
    {
        if (clip != null && ReduceVolumeClips.Contains(clip.name))
        {
            HKVocals.instance.LogDebug($"Reducing volume for {clip.name}");
            self.PlayOneShot(clip, 1 / 2f);
        }
        else if (ShouldPlayClip(clip)) orig(self, clip);
    }
    
    private static void ExcludeSomeAudios_PlayOneShot_AudioClip_float(Action<AudioSource, AudioClip, float> orig, AudioSource self, AudioClip clip, float volumeScale)
    {
        if (clip != null && ReduceVolumeClips.Contains(clip.name))
        {
            HKVocals.instance.LogDebug($"Reducing volume for {clip.name}");
            orig(self, clip, volumeScale / 2f);
        }
        else if (ShouldPlayClip(clip)) orig(self, clip, volumeScale);
    }

    private static List<string> ExcludedClipNames = new List<string>()
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
    
    private static List<string> ReduceVolumeClips = new List<string>()
    {
        "Iselda_Shop_Open", 
    };
}