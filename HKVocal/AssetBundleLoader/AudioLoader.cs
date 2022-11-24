using UnityEngine.Audio;

namespace HKVocals;

public static class AudioLoader
{
    public static AssetBundle audioBundle;
    
    public static List<string> audioNames = new List<string>();
    public static List<string> audioExtentions = new List<string>() { ".mp3", ".wav" };

    public static void LoadAssetBundle()
    {
        string[] allAssetNames = audioBundle.GetAllAssetNames();
        foreach (var asset in allAssetNames)
        {
            if (audioExtentions.Any(ext => asset.EndsWith(ext)))
            {
                audioNames.Add(Path.GetFileNameWithoutExtension(asset).ToUpper());
            }
            HKVocals.instance.LogDebug($": {asset} {Path.GetFileNameWithoutExtension(asset)?.ToUpper()}");
        }
    }
}
