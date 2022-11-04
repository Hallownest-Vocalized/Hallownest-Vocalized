namespace HKVocals;
public static class AudioUtils
{
    public static AudioClip GetAudioFor(string convName)
    {
        return HKVocals.instance.audioBundle.LoadAsset<AudioClip>(convName);
    }

    public static bool TryPlayAudioFor(string convName, float removeTime = 0f)
    {
        HKVocals.instance.LogDebug($"Trying to play audio for {convName}");
        if (HasAudioFor(convName))
        {
            if (removeTime != 0f)
            {
                PlayAudioForWithTrim(convName, removeTime);
                return true;
            }

            PlayAudioFor(convName);
            return true;
        }
        else
        {
            HKVocals.instance.LogWarn($"Audio doesn't exist {convName}");
            return false;
        }

    }

    public static bool HasAudioFor(string convName)
    {
        return Dictionaries.audioNames.Contains(convName);
    }

    public static void PlayAudioFor(string convName) => PlayAudio(GetAudioFor(convName.ToLower())); 
    public static void PlayAudioForWithTrim(string convName, float removeTime) => PlayAudioWithTrim(GetAudioFor(convName.ToLower()), removeTime);
    public static void PlayAudioFor(string convName, AudioSource asrc) => PlayAudio(GetAudioFor(convName.ToLower()), asrc);
    
    public static void PlayAudioWithTrim(AudioClip clip, float removeTime, AudioSource asrc = null)
    {
        HKVocals.instance.LogDebug($"Trimming {clip.name}");
        int remove =(int) (clip.frequency * removeTime);
        int size = clip.samples - remove;
        float[] samples = new float[size * clip.channels];
        clip.GetData(samples, remove);

        AudioClip newclip = AudioClip.Create(clip.name, size , clip.channels, clip.frequency, false);
        newclip.SetData(samples, 0);
                
        PlayAudio(newclip, asrc);
    }
    public static void PlayAudio(AudioClip clip, AudioSource asrc = null)
    {
        //if supplied is null, use default
        if (asrc == null)
        {
            asrc = HKVocals.instance.audioSource;
        }
        //if its still null create new audio source go
        if (asrc == null)
        {
            HKVocals.instance.CreateAudioSource();
        }

        if (HeroController.instance != null)
        {
            asrc.transform.localPosition = HeroController.instance.transform.localPosition;
        }
        else
        {
            //for monomon audio
            asrc.transform.localPosition = new Vector3(15f, 10f, 1f);
        }

        if (!asrc.outputAudioMixerGroup) // might need to be rewritten if this changes, don't think it does
        {
            asrc.outputAudioMixerGroup = ObjectPool.instance.startupPools.First(o => o.prefab.name == "Audio Player Actor").prefab.GetComponent<AudioSource>().outputAudioMixerGroup;
        }
        
        asrc.volume = HKVocals._globalSettings.Volume / 10f;
        asrc.Stop();
        HKVocals.instance.LogDebug($"Playing {clip.name}");
        asrc.PlayOneShot(clip, 1f);
    }

    public static bool IsPlaying() => HKVocals.instance.audioSource.isPlaying;
    public static void StopPlaying() => HKVocals.instance.audioSource.Stop();
    
    private static IEnumerator FadeOutClip(AudioSource source)
    {
        float volumeChange = source.volume / 100f;
        yield return new WaitForSeconds(1f);
        for (int i = 0; i < 100; i++)
            source.volume -= volumeChange;
    }
    
}
