using Satchel;

namespace HKVocals.MajorFeatures;

/// <summary>
/// For types of audio that need special handling to work
/// </summary>
public static class SpecialAudio
{
    public static void Hook()
    {
        //intro dialogue
        OnAnimatorSequence.AfterOrig.Begin += PlayMonomonIntroPoem;
        OnAnimatorSequence.WithOrig.Skip += LockSkippingMonomonIntro;
        OnChainSequence.WithOrig.Update += WaitForAudioBeforeNextCutscene;
        
        //elderbugs intro audio doesnt transition well so we add special audio for that
        FSMEditData.AddGameObjectFsmEdit("Elderbug", "Conversation Control", MakeElderbugPlaySpecialAudio);
        FSMEditData.AddGameObjectFsmEdit("Shiny Item RoyalCharm", "Shiny Control", ChangeNameOfClashingKey);
        FSMEditData.AddGameObjectFsmEdit("Dreamer Plaque Inspect", "Conversation Control", PlayDreamerPlaqueDialogue);
        FSMEditData.AddGameObjectFsmEdit("Fountain Inspect", "Conversation Control", PlayTHKPlaqueDialogue);
        ModHooks.LanguageGetHook += AddSpecialAudioKeys;
    }

    private static void LockSkippingMonomonIntro(On.AnimatorSequence.orig_Skip orig, AnimatorSequence self)
    {
        if (!HKVocals._globalSettings.scrollLock)
        {
            orig(self);
            HKVocals.instance.audioSource.Stop();
        }
    }
    private static void WaitForAudioBeforeNextCutscene(On.ChainSequence.orig_Update orig, ChainSequence self)
    {
        ChainSequenceR chainSeq = self.Reflect();
        if (!(chainSeq.CurrentSequence == null || 
            chainSeq.CurrentSequence.IsPlaying || 
            chainSeq.isSkipped ||
            AudioPlayer.IsPlaying()))
        {
            chainSeq.Next();
        }
    }
    private static void PlayMonomonIntroPoem(OnAnimatorSequence.Delegates.Params_Begin args)
    {
        CoroutineHelper.WaitForSecondsBeforeInvoke(1.164f, () =>
        {
            MixerLoader.SetSnapshot(MiscUtils.GetCurrentSceneName());
            AudioPlayer.TryPlayAudioFor("RANDOM_POEM_STUFF_0");
        });
    }
    
    private static string AddSpecialAudioKeys(string key, string sheettitle, string orig)
    {
        // makes sure text displays correctly
        if (key == "ELDERBUG_INTRO_MAIN_ALT" && sheettitle == "Elderbug")
        {
            orig = Language.Language.Get("ELDERBUG_INTRO_MAIN", sheettitle);
        }
        else if (key == "KING_SOUL_PICKUP_KING_FINAL_WORDS" && sheettitle == "Minor NPC")
        {
            orig = Language.Language.Get("KING_FINAL_WORDS", sheettitle);
        }

        return orig;
    }
    
    public static void MakeElderbugPlaySpecialAudio(PlayMakerFSM fsm)
    {
        //the idea is to add a new state which plays the alt audio and we change the transitions of the intros that dont work well to the alt audio playing state
        var introMain = fsm.GetFsmState("Intro Main");
        var alt = fsm.CopyFsmState(introMain.Name, introMain.Name + " Alt");

        alt.Actions = Array.Empty<FsmStateAction>();
        
        alt.AddFsmMethod(() =>
        {
            PlayerDataAccess.metElderbug = true;
            var dialogueText = introMain.GetFsmAction<CallMethodProper>(1).gameObject.GameObject.Value;
            dialogueText.GetComponent<DialogueBox>().StartConversation("ELDERBUG_INTRO_MAIN_ALT", "Elderbug");
        });
        
        fsm.GetFsmState("Intro 2").ChangeFsmTransition("CONVO_FINISH", alt.Name);
        fsm.GetFsmState("Intro 3").ChangeFsmTransition("CONVO_FINISH", alt.Name);
    }
    
    public static void ChangeNameOfClashingKey(PlayMakerFSM fsm)
    {
        var msg = fsm.GetFsmState("King Msg");
        msg.GetFirstActionOfType<CallMethodProper>().parameters[0].stringValue = "KING_SOUL_PICKUP_KING_FINAL_WORDS";
    }

    public static void PlayDreamerPlaqueDialogue(PlayMakerFSM fsm)
    {
        IEnumerator DreamerAudio()
        {
            for (int i = 1; i < 6; i++)
            {
                MixerLoader.SetSnapshot(Snapshots.Arena);
                AudioPlayer.TryPlayAudioFor("DREAMERS_INSPECT_RG" + i);
                yield return new WaitWhile(() => AudioPlayer.IsPlaying());
            }
        }
        Coroutine dreamerCo = null;
        void Stop()
        {
            AudioPlayer.StopPlaying();
            fsm.StopCoroutine(dreamerCo);
        }
        fsm.InsertCustomAction("Fade Up", () => dreamerCo = fsm.StartCoroutine(DreamerAudio()), 0);
        fsm.InsertCustomAction("Msg Down", Stop, 0);
        fsm.AddCustomAction("Update Map?", () =>
        {
            MixerLoader.SetSnapshot(Snapshots.Arena);
            AudioPlayer.TryPlayAudioFor("DREAMERS_INSPECT_RG6");
        });
    }

    public static void PlayTHKPlaqueDialogue(PlayMakerFSM fsm)
    {
        fsm.InsertCustomAction("Stop", () =>
        {
            MixerLoader.SetSnapshot(Snapshots.Cliffs);
            AudioPlayer.TryPlayAudioFor("RUINS_FOUNTAIN");
        }, 0);
        fsm.InsertCustomAction("Fade Down", () => AudioPlayer.StopPlaying(), 0);
        fsm.AddCustomAction("Update Map?", () =>
        {
            MixerLoader.SetSnapshot(Snapshots.Cliffs);
            AudioPlayer.TryPlayAudioFor("PROMPT_MAP_BLACKEGG");
        });
    }
}
