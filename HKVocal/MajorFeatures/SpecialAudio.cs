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
        if (key == "ELDERBUG_INTRO_MAIN_ALT" && sheettitle == "Elderbug")
        {
            orig = Language.Language.Get("ELDERBUG_INTRO_MAIN", sheettitle);
        }
        if (key == "KING_SOUL_PICKUP_KING_FINAL_WORDS" && sheettitle == "Minor NPC")
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
        msg.DisableFsmAction(2);
        msg.GetFirstActionOfType<CallMethodProper>().parameters[0].stringValue = "KING_SOUL_PICKUP_KING_FINAL_WORDS";
    }
}
