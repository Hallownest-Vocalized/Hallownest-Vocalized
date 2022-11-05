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
        ModHooks.LanguageGetHook += AddSpecialElderbugAudioKey;
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
        if (chainSeq.CurrentSequence != null && !chainSeq.CurrentSequence.IsPlaying && !chainSeq.isSkipped && AudioUtils.IsPlaying())
        {
            chainSeq.Next();
        }
    }
    private static void PlayMonomonIntroPoem(OnAnimatorSequence.Delegates.Params_Begin args)
    {
        MiscUtils.WaitForFramesBeforeInvoke(100, () => AudioUtils.TryPlayAudioFor("RANDOM_POEM_STUFF_0"));
    }
    
    private static string AddSpecialElderbugAudioKey(string key, string sheettitle, string orig)
    {
        if (key == "ELDERBUG_INTRO_MAIN_ALT" && sheettitle == "Elderbug")
        {
            orig = Language.Language.Get("ELDERBUG_INTRO_MAIN", sheettitle);
        }

        return orig;
    }
    
    public static void MakeElderbugPlaySpecialAudio(PlayMakerFSM fsm)
    {
        //the idea is to add a new statewhich plays the alt audio and we change the transitions of the intros that dont work well to the alt audio playing state
        var introMain = fsm.GetState("Intro Main");
        var alt = fsm.CopyFsmState(introMain.Name, introMain.Name + " Alt");

        alt.Actions = Array.Empty<FsmStateAction>();
        
        alt.AddMethod(() =>
        {
            PlayerDataAccess.metElderbug = true;
            var dialogueText = Satchel.FsmUtil.GetAction<CallMethodProper>(introMain, 1).gameObject.GameObject.Value;
            dialogueText.GetComponent<DialogueBox>().StartConversation("ELDERBUG_INTRO_MAIN_ALT", "Elderbug");
        });
        
        fsm.GetState("Intro 2").ChangeTransition("CONVO_FINISH", alt.Name);
        fsm.GetState("Intro 3").ChangeTransition("CONVO_FINISH", alt.Name);
    }
}
