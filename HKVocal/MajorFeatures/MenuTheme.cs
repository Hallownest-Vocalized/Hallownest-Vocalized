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
        GameObject go = Instantiate(StyleLoader.styleGo, transform);
        _video = go.GetComponent<VideoPlayer>();
        _video.targetCameraAlpha = 0.5f;
        _video.targetCamera = Camera.main;
        _video.Play();
        _audio = go.GetComponent<AudioSource>();
        _audio.outputAudioMixerGroup = MixerLoader.HKVAudioGroup;
        _audio.Play();
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
