using GlobalEnums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HKVocals;
public static class AudioPlayer
{
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

    public static void PlayAudioFor(string convName) => PlayAudio(GetAudioFor(convName.ToLower())); 
    public static void PlayAudioForWithTrim(string convName, float removeTime) => PlayAudioWithTrim(GetAudioFor(convName.ToLower()), removeTime);
    public static void PlayAudioFor(string convName, AudioSource asrc) => PlayAudio(GetAudioFor(convName.ToLower()), asrc);
    
    public static void PlayAudioWithTrim(AudioClip clip, float removeTime)
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
        
        asrc.Stop();

        asrc.transform.localPosition = HeroController.instance != null 
            ? HeroController.instance.transform.localPosition :
            new Vector3(15f, 10f, 1f); //for monomon audio

        asrc.SetMixerGroup();

        MixerLoader.SetMixerVolume();
        asrc.PlayOneShot(clip, 1f);
        
        HKVocals.instance.LogDebug($"Playing {clip.name}");
    }

    public static bool IsPlaying() => HKVocals.instance.audioSource.isPlaying;
    public static void StopPlaying() => HKVocals.instance.audioSource.Stop();
    public static bool HasAudioFor(string convName) => AudioLoader.audioNames.Contains(convName);
    public static AudioClip GetAudioFor(string convName) => AudioLoader.audioBundle.LoadAsset<AudioClip>(convName);
}
