using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Video;
using Satchel;

namespace HKVocals.MajorFeatures;

public class MenuTheme : MonoBehaviour
{
    private static AudioSource _audio;
    private static VideoPlayer _video;

    static MenuTheme()
    {
        ModHooks.LanguageGetHook += ModHooks_LanguageGetHook;
    }

    private static string ModHooks_LanguageGetHook(string key, string sheetTitle, string orig)
    {
        if (key == "STYLE_HKVOCALS")
            return "Reclaim Hallownest";
        return orig;
    }

    public static (string, GameObject, int, string, string[], MenuStyles.MenuStyle.CameraCurves, AudioMixerSnapshot) AddTheme(MenuStyles self)
    {
        //styleBundle ??= AssetBundle.LoadFromMemory(AssemblyUtils.GetBytesFromResources("Resources.stylebundle"));
        // GameObject go = Instantiate(styleBundle.LoadAsset<GameObject>("HKVMenuVideo"));
        GameObject go = new("HKVStyle");
        go.SetActive(false);
        go.transform.SetParent(self.gameObject.transform);
        go.transform.localPosition = new Vector3(0, -1.2f, 0);

        go.AddComponent<AudioSource>();
        go.AddComponent<VideoPlayer>();
        go.AddComponent<AudioSource>();
        go.AddComponent<MenuTheme>();

        var cameraCurves = new MenuStyles.MenuStyle.CameraCurves()
        {
            saturation = 1f,
            redChannel = new(),
            greenChannel = new(),
            blueChannel = new()
        };

        cameraCurves.redChannel.AddKey(new(0, 0));
        cameraCurves.redChannel.AddKey(new(1, 1));

        cameraCurves.greenChannel.AddKey(new(0, 0));
        cameraCurves.greenChannel.AddKey(new(1, 1));

        cameraCurves.blueChannel.AddKey(new(0, 0));
        cameraCurves.blueChannel.AddKey(new(1, 1));

        DontDestroyOnLoad(go);
        go.SetActive(true);

        return ("STYLE_HKVOCALS", go, -1, "", null, cameraCurves, Resources.FindObjectsOfTypeAll<AudioMixer>().First(x => x.name == "Music").FindSnapshot("Silent"));
    }

    bool ready = false;
    bool finished = false;

    private IEnumerator Start()
    {
        yield return new WaitUntil(() => StyleLoader.loaded);
        ready = true;

        var aSourceVid = gameObject.GetComponents<AudioSource>()[0];
        aSourceVid.clip = null;
        aSourceVid.outputAudioMixerGroup = Resources.FindObjectsOfTypeAll<AudioMixerGroup>().First(x => x.name == "Atmos");
        aSourceVid.mute = false;
        aSourceVid.bypassEffects = false;
        aSourceVid.bypassListenerEffects = false;
        aSourceVid.bypassReverbZones = false;
        aSourceVid.playOnAwake = false;
        aSourceVid.loop = true;
        aSourceVid.priority = 128;
        aSourceVid.volume = 0.9f;
        aSourceVid.pitch = 1;
        aSourceVid.panStereo = 0;
        aSourceVid.spatialBlend = 0;
        aSourceVid.reverbZoneMix = 1;
        aSourceVid.dopplerLevel = 0;
        aSourceVid.spread = 0;
        aSourceVid.rolloffMode = AudioRolloffMode.Custom;
        aSourceVid.maxDistance = 58.79711f;
        aSourceVid.SetCustomCurve(AudioSourceCurveType.CustomRolloff, new AnimationCurve(new []
        {
            new Keyframe(45.86174f, 1),
            new Keyframe(55.33846f, 0)
        }));
        _video = gameObject.GetComponent<VideoPlayer>();
        _video.playOnAwake = false;
        _video.audioOutputMode = VideoAudioOutputMode.AudioSource;
        _video.renderMode = VideoRenderMode.CameraFarPlane;
        _video.isLooping = true;
        _video.targetCamera = GameCameras.instance.mainCamera;
        _video.source = VideoSource.VideoClip;
        _video.clip = StyleLoader.styleBundle.LoadAsset<VideoClip>("Sequence_03");
        _video.SetTargetAudioSource(0, aSourceVid);
        DontDestroyOnLoad(_video.clip);

        _audio = gameObject.GetComponents<AudioSource>()[1];
        _audio.clip = StyleLoader.styleBundle.LoadAsset<AudioClip>("HV_Title_Card_music_compressed");
        _audio.outputAudioMixerGroup = Resources.FindObjectsOfTypeAll<AudioMixer>().First(x => x.name == "Music").outputAudioMixerGroup;
        _audio.mute = false;
        _audio.bypassEffects = false;
        _audio.bypassListenerEffects = false;
        _audio.bypassReverbZones = false;
        _audio.playOnAwake = false;
        _audio.loop = false;
        _audio.priority = 128;
        _audio.volume = 0.9f;
        _audio.pitch = 1;
        _audio.panStereo = 0;
        _audio.spatialBlend = 0;
        _audio.reverbZoneMix = 1;
        _audio.dopplerLevel = 0;
        _audio.spread = 0;
        _audio.rolloffMode = AudioRolloffMode.Custom;
        _audio.maxDistance = 58.79711f;
        _audio.SetCustomCurve(AudioSourceCurveType.CustomRolloff, new AnimationCurve(new []
        {
            new Keyframe(45.86174f, 1),
            new Keyframe(55.33846f, 0)
        }));
        DontDestroyOnLoad(_audio.clip);

        OnEnable();
    }

    private void OnEnable()
    {
        if (ready)
        {
            _video.Play();
            _audio.Play();
        }
    }
}
