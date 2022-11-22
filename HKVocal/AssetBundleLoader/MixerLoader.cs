using UnityEngine.Audio;

namespace HKVocals;

public static class MixerLoader
{
    
    public static AssetBundle mixerBundle;
    public static AudioMixerGroup HKVAudioGroup;
    public static AudioMixer HKVMixer;
    
    public static void LoadAssetBundle()
    {
        mixerBundle = AssetBundle.LoadFromStream(File.OpenRead(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/mixerbundle"));
        foreach (var obj in mixerBundle.LoadAllAssets())
        {
            HKVocals.instance.LogDebug($"Object in mixer: {obj}");
        }
    }

    public static void SetMixerVolume(float value)
    {
        HKVMixer.SetFloat("Volume", Mathf.Log10(value) * 20f);
    }
}

public enum Snapshots
{
    No_Effect,
    Dream,
    Cave,
    Spa,
    Cliffs,
    Room,
    Arena,
    Sewerpipe,
    Fog_Canyon,
}