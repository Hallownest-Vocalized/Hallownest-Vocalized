using UnityEngine.Audio;

namespace HKVocals;

/// <summary>
/// contains references to the various gameobjects in the audio menu in options
/// </summary>
public static class AudioOptionsMenu
{
    private static MenuScreen _audioMenuScreen;
    private static GameObject _audioMenuContent;
    private static GameObject _musicVolume;
    private static GameObject _musicSlider;
    private static AudioMixer _masterMixer;
    private static GameObject _defaultButton;
    
    public static MenuScreen AudioMenuScreen
    {
        get
        {
            if (_audioMenuScreen == null)
            {

                _audioMenuScreen = UIManager.instance.UICanvas.Find("AudioMenuScreen").GetComponent<MenuScreen>();
            }

            return _audioMenuScreen;
        }
    } 
    public static GameObject AudioMenuContent
    {
        get
        {
            if (_audioMenuContent == null)
            {

                _audioMenuContent = AudioMenuScreen.Find("Content");
            }

            return _audioMenuContent;
        }
    } 
    public static GameObject MusicVolume
    {
        get
        {
            if (_musicVolume == null)
            {
                _musicVolume = AudioMenuContent.Find("MusicVolume");
            }

            return _musicVolume;
        }
    }
    
    public static GameObject MusicSlider
    {
        get
        {
            if (_musicSlider == null)
            {
                _musicSlider = MusicVolume.Find("MusicSlider");
            }

            return _musicSlider;
        }
    }
    public static AudioMixer MasterMixer
    {
        get
        {
            if (_masterMixer == null)
            {
                _masterMixer = AudioMenuContent.Find("MasterVolume").Find("MasterSlider").GetComponent<MenuAudioSlider>().masterMixer;
            }

            return _masterMixer;
        }
    }
    
    public static GameObject DefaultButton
    {
        get
        {
            if (_defaultButton == null)
            {
                _defaultButton = AudioMenuScreen.Find("Controls/DefaultsButton");
            }

            return _defaultButton;
        }
    }
}