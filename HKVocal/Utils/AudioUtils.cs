﻿namespace HKVocals;
public static class AudioUtils
{
    public static AudioClip GetAudioFor(string convName)
    {
        if (Dictionaries.CustomAudioClips.ContainsValue(convName))
        {
            return Dictionaries.CustomAudioClips.First(a => a.Value == convName).Key;
        }
        else
        {
            if (Dictionaries.CustomAudioBundles.Any(a => a.Value.Any(s => s == convName)))
            {
                return Dictionaries.CustomAudioBundles.First(a => a.Value.Any(s => s == convName)).Key
                    .LoadAsset<AudioClip>(convName);
            }
            else
            {
                return HKVocals.instance.audioBundle.LoadAsset<AudioClip>(convName);
            }
        }
    }

    public static void TryPlayAudioFor(string convName, float removeTime = 0f)
    {
        HKVocals.instance.LogDebug($"Trying to play audio for {convName}");
        if (HasAudioFor(convName))
        {
            if (removeTime == 0f)
            {
                PlayAudioFor(convName);
            }
            else
            {
                PlayAudioForWithTrim(convName, removeTime);
            }
        }
        else
        {
            HKVocals.instance.LogWarn($"Audio doesn't exits {convName}");
        }

    }

    public static bool HasAudioFor(string convName)
    {

        //Dictionaries.CustomAudioBundles.Any(a => { HKVocals.instance.Log(a.Value); return false; });
        //Dictionaries.CustomAudioClips.Any(a => { HKVocals.instance.Log(a.Value); return false; });
        //Dictionaries.audioNames.Any(a => { HKVocals.instance.Log(a); return false; });

        return 
            Dictionaries.CustomAudioBundles.Any(a => a.Value.Contains(convName)) ||
            Dictionaries.CustomAudioClips.ContainsValue(convName) ||
            Dictionaries.audioNames.Contains(convName);
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

        if (Dictionaries.NoAudioMixer.Contains(clip.name))
        {
            asrc.outputAudioMixerGroup = null;
        }
        else if (!asrc.outputAudioMixerGroup) // might need to be rewritten if this changes, don't think it does
        {
            asrc.outputAudioMixerGroup = ObjectPool.instance.startupPools.First(o => o.prefab.name == "Audio Player Actor").prefab.GetComponent<AudioSource>().outputAudioMixerGroup;
        }
        
        asrc.volume = HKVocals._globalSettings.Volume / 10f;

        
        HKVocals.instance.LogDebug($"Playing {clip.name}");
        asrc.PlayOneShot(clip, 1f);
        
        HKVocals.instance.LogDebug("");
    }

    public static bool IsPlaying() => HKVocals.instance.audioSource.isPlaying;
}