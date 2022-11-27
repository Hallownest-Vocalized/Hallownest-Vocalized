using UnityEngine.Audio;

namespace HKVocals;

public static class AudioLoader
{
    public static AssetBundle audioBundle;
    
    public static List<string> audioNames = new List<string>();
    public static List<string> audioExtentions = new List<string>() { ".mp3", ".wav" };

    public static void LoadAssetBundle()
    {
        audioBundle = AssetBundle.LoadFromStream(File.OpenRead(HKVocals.BundleLocation));
        
        foreach (var audio in audioBundle.GetAllAssetNames())
        {
            if (audioExtentions.Any(extension => audio.EndsWith(extension)))
            {
                audioNames.Add(Path.GetFileNameWithoutExtension(audio).ToUpper());
                HKVocals.DoLogDebug($"{audio} {Path.GetFileNameWithoutExtension(audio)?.ToUpper()}");
            }
        }
    }
}
