using Satchel;

namespace HKVocals;

public static class StyleLoader
{
    public static AssetBundle styleBundle;
    public static bool loaded = false;

    public static void LoadAssetBundle()
    {
        styleBundle = AssetBundle.LoadFromMemory(SystemInfo.operatingSystemFamily == OperatingSystemFamily.Linux ? AssemblyUtils.GetBytesFromResources("Resources.stylebundlelinux") : AssemblyUtils.GetBytesFromResources("Resources.stylebundle"));
        loaded = true;
    }
}
