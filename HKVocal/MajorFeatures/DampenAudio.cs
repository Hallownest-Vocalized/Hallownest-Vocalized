namespace HKVocals.MajorFeatures;
public static class DampenAudio
{
    private static float DampenValue; //a percentage 
    private static readonly float DampenValueNormal = 30f;  
    private static readonly float DampenValueDream = 30f;  
    private static readonly float DampenTime = 1f;
    private static bool AudioDampened = false;

    public static void Hook()
    {
        NPCDialogue.OnPlayNPCDialogue += StartDampingAudioNormal;
        DreamNailDialogue.OnPlayDreamDialogue += StartDampingAudioDream;
        
        OnDialogueBox.BeforeOrig.HideText += _ => StopDampenAudio();
        Hooks.HookStateEntered(new FSMData("DialogueManager","Box Open Dream", "Stop Audio"), _ => StopDampenAudio());
        Hooks.HookStateEntered(new FSMData("Dream Msg","Display", "Full Cancel"), _ => StopDampenAudio());
        Hooks.HookStateEntered(new FSMData("Dream Msg","Display", "Text Down"), _ => StopDampenAudio());
        Hooks.HookStateEntered(new FSMData("DialogueManager","Box Open Dream", "Stop Audio"), _ => StopDampenAudio());
        
        FSMEditData.AddGameObjectFsmEdit("Text","Dialogue Page Control", DontCallHideTextOnHalfConvoFinish);
    }

    private static void DontCallHideTextOnHalfConvoFinish(PlayMakerFSM fsm)
    {
        var endConversation = fsm.GetFsmState("End Conversation");
        
        //replace the action that calls hide text so we can manually call our orig_HideText
        endConversation.ReplaceFsmAction(new Core.FsmUtil.Actions.MethodAction()
        {
            Method = () =>
            {
                var useStop = fsm.GetFsmBoolVariable("Use Stop");
                var dialogueBox = fsm.Fsm.GameObject.gameObject.GetComponent<DialogueBox>().Reflect();
                
                //if its a half convo
                if (!useStop.Value)
                {
                    //this avoids our on hook
                    orig_DialogueBox_HideText(dialogueBox);
                }
                else
                {
                    //this goes through our on hook
                    dialogueBox.HideText();
                }

                useStop.Value = true;
            }
        }, 3);
        
        //remove the one that sets use stop (that controls half convo) and set it ourself so we can read the value
        endConversation.RemoveFsmAction(0);
    }

    private static void orig_DialogueBox_HideText(DialogueBoxR box)
    {
        if (box.typing)
        {
            box.StopTypewriter();
        }
        box.textMesh.maxVisibleCharacters = 0;
        box.hidden = true;
    }
    

    private static void StartDampingAudioNormal()
    {
        DampenValue = DampenValueNormal;
        if (HKVocals._globalSettings.dampenAudio)
        {
            HKVocals.CoroutineHolder.StartCoroutine(DoDampenAudio(dampen: true));
        }
    }
    private static void StartDampingAudioDream()
    {
        DampenValue = DampenValueDream;
        if (HKVocals._globalSettings.dampenAudio)
        {
            HKVocals.CoroutineHolder.StartCoroutine(DoDampenAudio(dampen: true));
        }
    }
    
    private static void StopDampenAudio()
    {
        if (HKVocals._globalSettings.dampenAudio)
        {
            HKVocals.CoroutineHolder.StartCoroutine(DoDampenAudio(dampen: false));
        }
    }
    
    private static IEnumerator DoDampenAudio(bool dampen)
    {
        //we shouldn't re dampen if already dampened or un dampen if it wasnt dampned
        if (dampen && AudioDampened || !dampen && !AudioDampened)
        {
            yield break;
        }
        
        HKVocals.DoLogDebug((dampen ? "Dampening" : "Undampening") + " audio");
        AudioDampened = dampen;
        
        float currentTime = 0f;
        float multiplier = (100 - DampenValue) / 100f;
        float masterVolume = GameManager.instance.gameSettings.masterVolume;
        while (currentTime <= DampenTime)
        {
            currentTime += Time.deltaTime;

            //if dampen = true, we get the first value of lerp as the original volume and 2nd value as the reduced and vice versa
            
            AudioOptionsMenu.MasterMixer.SetFloat("MasterVolume",
                Mathf.Lerp(MiscUtils.GetDecibelVolume(masterVolume * (dampen ? 1f: multiplier)),
                    MiscUtils.GetDecibelVolume(masterVolume * (dampen ? multiplier : 1f)), currentTime / DampenTime));

            yield return null;
        }
    }
}