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

        CheckForAchivements(clip.name);


        asrc.Stop();
        asrc.transform.localPosition = HeroController.instance != null 
            ? HeroController.instance.transform.localPosition :
            new Vector3(15f, 10f, 1f); //for monomon audio

        asrc.SetMixerGroup();

        MixerLoader.SetMixerVolume();
        asrc.PlayOneShot(clip, 1f);
    }

    private static void CheckForAchivements(string clip) {
        HKVocals._globalSettings.FinishedNPCDialoge.RemoveAll(v => v.Equals(clip, StringComparison.OrdinalIgnoreCase));
        HKVocals._globalSettings.FinishedDNailDialoge.RemoveAll(v => v.Equals(clip, StringComparison.OrdinalIgnoreCase));
        HKVocals._globalSettings.FinishedLoreTabletDialoge.RemoveAll(v => v.Equals(clip, StringComparison.OrdinalIgnoreCase));
        HKVocals._globalSettings.FinishedUIDialoge.RemoveAll(v => v.Equals(clip, StringComparison.OrdinalIgnoreCase));

        if (!GameManager.instance.IsAchievementAwarded("Consideration") && HKVocals._globalSettings.FinishedNPCDialoge.Count == 0)
            GameManager.instance.AwardAchievement("Consideration");

        if (!GameManager.instance.IsAchievementAwarded("Ambition") && HKVocals._globalSettings.FinishedDNailDialoge.Count == 0)
            GameManager.instance.AwardAchievement("Ambition");
            
        if (!GameManager.instance.IsAchievementAwarded("Chronology") && HKVocals._globalSettings.FinishedLoreTabletDialoge.Count == 0)
            GameManager.instance.AwardAchievement("Chronology");

        if (!GameManager.instance.IsAchievementAwarded("Acquisition") && HKVocals._globalSettings.FinishedUIDialoge.Count == 0)
            GameManager.instance.AwardAchievement("Acquisition");

        if (
            GameManager.instance.IsAchievementAwarded("Consideration") &&
            GameManager.instance.IsAchievementAwarded("Ambition") &&
            GameManager.instance.IsAchievementAwarded("Chronology") &&
            GameManager.instance.IsAchievementAwarded("Acquisition") &&
            
            !GameManager.instance.IsAchievementAwarded("CompendiumVocalization")
        ) GameManager.instance.AwardAchievement("CompendiumVocalization");
    }

    public static bool IsPlaying() => HKVocals.instance.audioSource.isPlaying;
    public static void StopPlaying() => HKVocals.instance.audioSource.Stop();
    public static bool HasAudioFor(string convName) => HallownestVocalizedAudioLoaderMod.AudioNames.Contains(convName);
    public static AudioClip GetAudioFor(string convName) => HallownestVocalizedAudioLoaderMod.AudioBundle.LoadAsset<AudioClip>(convName);
}
