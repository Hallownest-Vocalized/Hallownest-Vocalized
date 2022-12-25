using UnityEngine.Audio;
using Satchel;

namespace HKVocals;

/// <summary>
/// contains references to the various gameobjects in the audio menu in options
/// </summary>
public static class RefVanillaMenu
{
    private static MenuScreen _audioMenuScreen;
    private static GameObject _audioMenuScreenContent;
    private static GameObject _musicVolumeSliderHolder;
    private static GameObject _musicVolumeSlider;
    private static AudioMixer _masterMixer;
    private static GameObject _defaultAudioSettingsButton;
    
    private static MenuScreen _extrasMenuScreen;
    private static GameObject _extrasMenuScreenContent;
    private static GameObject _creditButtonHolder;
    private static List<GameObject> _packDetailButtons;

    #region AudioMenu
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
    public static GameObject AudioMenuScreenContent
    {
        get
        {
            if (_audioMenuScreenContent == null)
            {

                _audioMenuScreenContent = AudioMenuScreen.Find("Content");
            }

            return _audioMenuScreenContent;
        }
    } 
    public static GameObject MusicVolumeSliderHolder
    {
        get
        {
            if (_musicVolumeSliderHolder == null)
            {
                _musicVolumeSliderHolder = AudioMenuScreenContent.Find("MusicVolume");
            }

            return _musicVolumeSliderHolder;
        }
    }
    
    public static GameObject MusicVolumeSlider
    {
        get
        {
            if (_musicVolumeSlider == null)
            {
                _musicVolumeSlider = MusicVolumeSliderHolder.Find("MusicSlider");
            }

            return _musicVolumeSlider;
        }
    }
    public static AudioMixer MasterMixer
    {
        get
        {
            if (_masterMixer == null)
            {
                _masterMixer = AudioMenuScreenContent.Find("MasterVolume").Find("MasterSlider").GetComponent<MenuAudioSlider>().masterMixer;
            }

            return _masterMixer;
        }
    }
    
    public static GameObject DefaultAudioSettingsButton
    {
        get
        {
            if (_defaultAudioSettingsButton == null)
            {
                _defaultAudioSettingsButton = AudioMenuScreen.Find("Controls/DefaultsButton");
            }

            return _defaultAudioSettingsButton;
        }
    }
    
    #endregion

    #region ExtrasMenu

    public static MenuScreen ExtrasMenuScreen
    {
        get
        {
            if (_extrasMenuScreen == null)
            {

                _extrasMenuScreen = UIManager.instance.UICanvas.Find("ExtrasMenuScreen").GetComponent<MenuScreen>();
            }

            return _extrasMenuScreen;
        }
    } 
    public static GameObject ExtrasMenuScreenContent
    {
        get
        {
            if (_extrasMenuScreenContent == null)
            {

                _extrasMenuScreenContent = ExtrasMenuScreen.Find("Content");
            }

            return _extrasMenuScreenContent;
        }
    } 
    
    
    public static GameObject CreditButtonHolder
    {
        get
        {
            if (_creditButtonHolder == null)
            {

                _creditButtonHolder = ExtrasMenuScreenContent.Find("Credits");
            }

            return _creditButtonHolder;
        }
    }

    public static List<GameObject> PackDetailButtons
    {
        get
        {
            _packDetailButtons ??= new List<GameObject>();
            
            _packDetailButtons.RemoveNullValues();

            if (_packDetailButtons.Count < 4)
            {
                _packDetailButtons.Clear();

                _packDetailButtons = new List<GameObject>()
                {
                    ExtrasMenuScreenContent.Find("PackDetails_1"),
                    ExtrasMenuScreenContent.Find("PackDetails_2"),
                    ExtrasMenuScreenContent.Find("PackDetails_Lifeblood"), //why
                    ExtrasMenuScreenContent.Find("PackDetails_3"),

                };
            }

            return _packDetailButtons;
        }
    }
    
    #endregion
}