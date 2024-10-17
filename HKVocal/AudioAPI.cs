namespace HKVocals;

public static class AudioAPI
{
    public static readonly string[] AudioExtensions = new string[] {".mp3", ".wav"};
    private static Dictionary<int, List<IAudioProvider>> AudioProviders = new();
    public static List<string> AudioNames { get; } = new();
    public static IAudioProvider[] OrderedProviers {get; private set;} = new IAudioProvider[0];
    public static string[] CachedAudioNames = new string[0];
    public static bool ShouldUpdateOrderedProviders {get; private set;} = false;
    public static bool ShouldUpdateCachedAudioNames {get; private set;} = false;
    internal static List<string> CustomMuteList = new();

    /// <summary>
    /// Adds an audio clip name to the mute list.
    /// </summary>
    /// <param name="name">Name of the audio clip to mute.</param>
    public static void AddMuteAudio(string name) => CustomMuteList.Add(name);

    /// <summary>
    /// Inserts an IAudioProvider into the priority list at the specified level.
    /// </summary>
    /// <param name="optimalPriority">The priority of the IAudioProvider. Use -1 for base language packs and use 0 and above for addons.</param>
    /// <param name="provider">The IAudioProvider to be added.</param>
    public static void AddAudioProvider(int optimalPriority, IAudioProvider provider)
    {
        List<IAudioProvider> similarPriority;

        if (!AudioProviders.TryGetValue(optimalPriority, out similarPriority) || similarPriority is null)
        {
            similarPriority = new List<IAudioProvider>();
            AudioProviders[optimalPriority] = similarPriority;
        }

        similarPriority.Add(provider);

        ShouldUpdateOrderedProviders = true;
        ShouldUpdateCachedAudioNames = true;
    }

    public static AudioClip GetAudioFor(string convName)
    {
        UpdateOrderedPriority();
        foreach (IAudioProvider provider in OrderedProviers)
        {
            AudioClip clip = provider.GetAudioClip(convName);
            if (clip)
                return clip;
        }
        return null;
    }

    public static bool HasAudioFor(string convName)
    {
        UpdateAudioNames();
        return CachedAudioNames.Contains(convName);
    }

    public static void UpdateOrderedPriority()
    {
        if (ShouldUpdateOrderedProviders)
            OrderedProviers = AudioProviders.OrderBy(pair => -pair.Key).SelectMany(pair => pair.Value).ToArray();
    }

    public static void UpdateAudioNames()
    {
        if (ShouldUpdateCachedAudioNames || AudioProviders.SelectMany(pair => pair.Value).Any(provider => provider.UpdateAudioNames()))
        {
            CachedAudioNames = AudioProviders.SelectMany(pair => pair.Value).SelectMany(provider => provider.GetAudioNames()).ToArray();
        }
    }
}

public interface IAudioProvider
{
    public AudioClip GetAudioClip(string convName);
    public IEnumerable<string> GetAudioNames();
    public bool UpdateAudioNames();
}

public class AssetBundleAudioProvider : IAudioProvider
{
    public AssetBundle Bundle {get; protected set;}
    public string[] AudioNames {get; protected set;} = new string[0];
    public bool HasCachedAudio {get; protected set;} = false;

    public AssetBundleAudioProvider(AssetBundle bundle, bool autoCacheAudio = true)
    {
        Bundle = bundle;
        if (autoCacheAudio)
            GetAudioNames();
    }

    public virtual AudioClip GetAudioClip(string convName) => Bundle.LoadAsset<AudioClip>(convName);

    public virtual IEnumerable<string> GetAudioNames()
    {
        if (!HasCachedAudio)
            AudioNames = Bundle.GetAllAssetNames().Where(s => AudioAPI.AudioExtensions.Any(ext => s.EndsWith(ext))).Select(s => Path.GetFileNameWithoutExtension(s).ToUpper()).ToArray();
        return AudioNames;
    }

    public virtual bool UpdateAudioNames() => !HasCachedAudio;
}

public class AssetBundleCreateRequestAudioProvider : AssetBundleAudioProvider
{
    public AssetBundleCreateRequest Request {get; protected set;}
    public bool AutoCacheAudioOnCompletion {get; protected set;}

    public AssetBundleCreateRequestAudioProvider(AssetBundleCreateRequest request, bool autoCacheAudioOnCompletion) : base(null, false)
    {
        Request = request;
        AutoCacheAudioOnCompletion = autoCacheAudioOnCompletion;
        if (Request.isDone)
            OnBundleLoadComplete(Request);
        else
            Request.completed += OnBundleLoadComplete;
    }

    public override AudioClip GetAudioClip(string convName)
    {
        if (Bundle == null)
            return null;
        return base.GetAudioClip(convName);
    }

    public override IEnumerable<string> GetAudioNames()
    {
        if (Bundle == null)
            return new string[0];
        return base.GetAudioNames();
    }

    private void OnBundleLoadComplete(AsyncOperation _)
    {
        Bundle = Request.assetBundle;
        if (AutoCacheAudioOnCompletion)
            GetAudioNames();
    }
}