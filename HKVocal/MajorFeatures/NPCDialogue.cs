namespace HKVocals.MajorFeatures;

public static class NPCDialogue
{
    public static bool DidPlayAudioOnDialogueBox = false;
    
    public static void Hook()
    {
        OnDialogueBox.AfterOrig.ShowPage += PlayNPCDialogue;
        OnDialogueBox.BeforeOrig.HideText += StopAudioOnDialogueBoxClose;
    }
    
    private static void StopAudioOnDialogueBoxClose(OnDialogueBox.Delegates.Params_HideText args)
    {
        AudioUtils.StopPlaying();
    }

    private static void PlayNPCDialogue(OnDialogueBox.Delegates.Params_ShowPage args)
    {
        var convo = args.self.currentConversation + "_" + (args.self.currentPage - 1);

        float removeTime = args.self.currentPage - 1 == 0 ? 37f / 60f : 3f / 4f;

        //this controls scroll lock and autoscroll
        DidPlayAudioOnDialogueBox = AudioUtils.TryPlayAudioFor(convo, removeTime);
    }
    
}