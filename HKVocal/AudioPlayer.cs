namespace HKVocals;
public static class AudioPlayer
{
    public static bool TryPlayAudioFor(string convName, float removeTime = 0f, AudioSource asrc = null)
    {
        HKVocals.instance.LogDebug($"Trying to play audio for {convName}");
        if (HasAudioFor(convName))
        {
            if (removeTime != 0f)
            {
                PlayAudioWithTrim(GetAudioClip(convName), removeTime);
                return true;
            }
            
            PlayAudio(GetAudioClip(convName), asrc);
            return true;
        }
        else
        {
            HKVocals.instance.LogWarn($"Audio doesn't exist {convName}");
            return false;
        }
    }

    private static AudioClip GetAudioClip(string convoName) => GetAudioFor(convoName.ToLower());
    
    private static void PlayAudioWithTrim(AudioClip clip, float removeTime)
    {
        HKVocals.instance.LogDebug($"Trimming {clip.name}");
        int remove = (int) (clip.frequency * removeTime);
        int size = clip.samples - remove;
        float[] samples = new float[size * clip.channels];
        clip.GetData(samples, remove);

        AudioClip newclip = AudioClip.Create(clip.name, size, clip.channels, clip.frequency, false);
        newclip.SetData(samples, 0);
                
        PlayAudio(newclip);
    }
    private static void PlayAudio(AudioClip clip, AudioSource asrc = null)
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
        
        asrc.Stop();

        asrc.transform.localPosition = HeroController.instance != null 
            ? HeroController.instance.transform.localPosition :
            new Vector3(15f, 10f, 1f); //for monomon audio

        asrc.SetMixerGroup();

        MixerLoader.SetMixerVolume();
        asrc.PlayOneShot(clip, 1f);
    }

    public static bool IsPlaying() => HKVocals.instance.audioSource.isPlaying;
    public static void StopPlaying() => HKVocals.instance.audioSource.Stop();
    public static bool HasAudioFor(string convName) => AudioLoader.audioNames.Contains(convName);
    public static AudioClip GetAudioFor(string convName) => AudioLoader.audioBundle.LoadAsset<AudioClip>(convName);
}
