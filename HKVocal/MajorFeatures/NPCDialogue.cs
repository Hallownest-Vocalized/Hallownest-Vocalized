namespace HKVocals.MajorFeatures;

/// <summary>
/// Plays dialogue when NPC speaks
/// </summary>
public static class NPCDialogue
{
    public static bool DidPlayAudioOnDialogueBox;

    public delegate void OnPlayNPCDialogueHandler();

    public static event OnPlayNPCDialogueHandler OnPlayNPCDialogue;

    public static void Hook()
    {
        OnDialogueBox.AfterOrig.ShowPage += PlayAudioForNPCDialogue;
        OnDialogueBox.BeforeOrig.HideText += _ => AudioPlayer.StopPlaying();
    }

    private static void PlayAudioForNPCDialogue(OnDialogueBox.Delegates.Params_ShowPage args)
    {
        //DialogueBox is a component of DialogueManager/Text
        var dialogueManager = args.self.gameObject.transform.parent.gameObject;

        bool isDreamBoxOpen = dialogueManager.Find("Box Dream").GetComponent<MeshRenderer>().enabled;
        bool isDialogueBoxOpen = dialogueManager.Find("DialogueBox").Find("backboard").GetComponent<SpriteRenderer>().enabled;

        if (isDialogueBoxOpen)
        {
            MixerLoader.SetSnapshot(MiscUtils.GetCurrentSceneName());
        }
        else if (isDreamBoxOpen)
        {
            // we dont wanna play dn dialogue when toggled off
            if (!HKVocals._globalSettings.dnDialogue)
            {
                return;
            }
            else
            {
                MixerLoader.SetSnapshot(Snapshots.Dream);
            }
        }
         

        //convos start at _0 but page numbers start from 1
        int convoNumber = args.self.currentPage - 1;
        string convo = args.self.currentConversation + "_" + convoNumber;

        float removeTime = convoNumber == 0 ? 3 / 5f : 3 / 4f;

        //this controls scroll lock and autoscroll
        DidPlayAudioOnDialogueBox = AudioPlayer.TryPlayAudioFor(convo, removeTime);

        if (DidPlayAudioOnDialogueBox)
        {
            OnPlayNPCDialogue?.Invoke();
        }
    }
}