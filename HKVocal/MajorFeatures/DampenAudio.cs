using static HKVocals.AudioOptionsMenu;

namespace HKVocals.MajorFeatures;
public static class DampenAudio
{
    private static readonly float DampenValue = 30f; //a percentage 
    private static readonly float DampenTime = 1f;
    private static bool AudioDampened = false;

    public static void Hook()
    {
        NPCDialogue.OnPlayNPCDialogue += StartDampingAudio;
        OnDialogueBox.BeforeOrig.HideText += _ => StopDampenAudio();
    }

    private static void StartDampingAudio()
    {
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
        
        AudioDampened = dampen;
        
        float currentTime = 0f;
        float multiplier = (100 - DampenValue) / 100f;
        float soundVolume = GameManager.instance.gameSettings.soundVolume;
        float musicVolume = GameManager.instance.gameSettings.musicVolume;

        while (currentTime <= DampenTime)
        {
            currentTime += Time.deltaTime;

            //if dampen = true, we get the first value of lerp as the original volume and 2nd value as the reduced and vice versa
            
            MasterMixer.SetFloat("MusicVolume",
                Mathf.Lerp(MiscUtils.GetDecibelVolume(musicVolume * (dampen ? 1f: multiplier)),
                    MiscUtils.GetDecibelVolume(musicVolume * (dampen ? multiplier : 1f)), currentTime / DampenTime));

            MasterMixer.SetFloat("SFXVolume",
                Mathf.Lerp(MiscUtils.GetDecibelVolume(soundVolume * (dampen ? 1f: multiplier)),
                    MiscUtils.GetDecibelVolume(soundVolume * (dampen ? multiplier : 1f)), currentTime / DampenTime));
            

            yield return null;
        }
    }
}