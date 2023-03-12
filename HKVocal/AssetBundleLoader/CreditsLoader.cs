using Satchel;

namespace HKVocals;

public static class CreditsLoader
{
    private static AssetBundle creditsBundle;

    public static void LoadAssetBundle()
    {
        creditsBundle = AssetBundle.LoadFromMemory(AssemblyUtils.GetBytesFromResources("Resources.creditsbundle"));
    }
}
