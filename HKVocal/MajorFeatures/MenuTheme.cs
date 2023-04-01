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

    public static (string, GameObject, int, string, string[], MenuStyles.MenuStyle.CameraCurves, AudioMixerSnapshot) AddTheme(MenuStyles arg)
    {
        //styleBundle ??= AssetBundle.LoadFromMemory(AssemblyUtils.GetBytesFromResources("Resources.stylebundle"));
        // GameObject go = Instantiate(styleBundle.LoadAsset<GameObject>("HKVMenuVideo"));
        GameObject go = new("HKVStyle");
        go.AddComponent<MenuTheme>();
        go.transform.parent = arg.transform;

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

        return ("STYLE_HKVOCALS", go, -1, "", null, cameraCurves, Resources.FindObjectsOfTypeAll<AudioMixer>().First(x => x.name == "Music").FindSnapshot("Silent")); // todo: replace snapshot with some none snapshot
    }

    bool ready = false;
    bool finished = false;

    private IEnumerator Start()
    {
        yield return new WaitUntil(() => StyleLoader.loaded);
        ready = true;

        var aSourceVid = gameObject.AddComponent<AudioSource>();
        aSourceVid.clip = null;
        aSourceVid.outputAudioMixerGroup = Resources.FindObjectsOfTypeAll<AudioMixerGroup>().First(x => x.name == "Atmos");
        aSourceVid.mute = false;
        aSourceVid.bypassEffects = false;
        aSourceVid.bypassListenerEffects = false;
        aSourceVid.bypassReverbZones = false;
        aSourceVid.playOnAwake = false;
        aSourceVid.loop = true;
        aSourceVid.priority = 128;
        aSourceVid.volume = 1;
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
        var vp = gameObject.AddComponent<VideoPlayer>();
        vp.playOnAwake = false;
        vp.audioOutputMode = VideoAudioOutputMode.AudioSource;
        vp.renderMode = VideoRenderMode.CameraFarPlane;
        vp.isLooping = true;
        vp.targetCamera = GameCameras.instance.mainCamera;
        vp.source = VideoSource.VideoClip;
        vp.clip = StyleLoader.styleBundle.LoadAsset<VideoClip>("Sequence_03");
        vp.SetTargetAudioSource(0, aSourceVid);

        var aSourceMusic = gameObject.AddComponent<AudioSource>();
        aSourceMusic.clip = StyleLoader.styleBundle.LoadAsset<AudioClip>("HV_Title_Card_music_compressed");
        aSourceMusic.outputAudioMixerGroup = Resources.FindObjectsOfTypeAll<AudioMixer>().First(x => x.name == "Music").outputAudioMixerGroup;
        aSourceMusic.mute = false;
        aSourceMusic.bypassEffects = false;
        aSourceMusic.bypassListenerEffects = false;
        aSourceMusic.bypassReverbZones = false;
        aSourceMusic.playOnAwake = false;
        aSourceMusic.loop = false;
        aSourceMusic.priority = 128;
        aSourceMusic.volume = 1;
        aSourceMusic.pitch = 1;
        aSourceMusic.panStereo = 0;
        aSourceMusic.spatialBlend = 0;
        aSourceMusic.reverbZoneMix = 1;
        aSourceMusic.dopplerLevel = 0;
        aSourceMusic.spread = 0;
        aSourceMusic.rolloffMode = AudioRolloffMode.Custom;
        aSourceMusic.maxDistance = 58.79711f;
        aSourceMusic.SetCustomCurve(AudioSourceCurveType.CustomRolloff, new AnimationCurve(new []
        {
            new Keyframe(45.86174f, 1),
            new Keyframe(55.33846f, 0)
        }));

        StopAllCoroutines();
        StartCoroutine(WaitForAudio());
    }

    private void OnEnable()
    {
        if (ready)
        {
            _video.Play();
            if (!finished)
            {
                _audio.Play();
                StopAllCoroutines();
                StartCoroutine(WaitForAudio());
            }
        }
    }

    private IEnumerator WaitForAudio()
    {
        yield return new WaitWhile(() => _audio.isPlaying);
        finished = true;
    }
}
