using GlobalEnums;
using Satchel;
using Image = UnityEngine.UI.Image;
using JetBrains.Annotations;
using UnityEngine.EventSystems;
using UMenuButton = UnityEngine.UI.MenuButton;

namespace HKVocals.MajorFeatures;

public static class RollCredits
{
    //for 1080p screens
    private static readonly float _rollSpeed = 120f;
    private static readonly float _scrollMaxY = 50_050;
    public static readonly float MouseScrollSpeed = 75f;
    public static readonly float UpDownSpeed = 25f;
    
    //scale to screen height
    public static float RollSpeed => _rollSpeed * (Screen.height/1080f);
    public static float ScrollMaxY => _scrollMaxY * (Screen.height/1080f);
    
    private const string CreditsSceneName = "HKV_Credits";
    private static bool isFromMenu;
    private static bool doWantToLoadVanillaCredits;

    [CanBeNull] private static GameObject MMButton;
    [CanBeNull] private static GameObject OGButton;
    [CanBeNull] private static GameObject SkipButton;

    public static void Hook()
    {
        //hook to get to know when to start our credits
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged += (_, to) =>
        {
            if (to.name == CreditsSceneName)
            {
                GameManagerR.SetState(GameState.CUTSCENE);
                UIManagerR.SetState(UIState.CUTSCENE);
                var aSource = CreditsParent.gameObject.AddComponent<AudioSource>();
                aSource.clip = HallownestVocalizedAudioLoaderMod.AudioBundle.LoadAsset<AudioClip>("Creditaudio");
                aSource.mute = false;
                aSource.bypassEffects = false;
                aSource.bypassListenerEffects = false;
                aSource.bypassReverbZones = false;
                aSource.playOnAwake = false;
                aSource.loop = false;
                aSource.priority = 128;
                aSource.volume = 1;
                aSource.pitch = 1;
                aSource.panStereo = 0;
                aSource.spatialBlend = 0;
                aSource.reverbZoneMix = 1;
                aSource.dopplerLevel = 0;
                aSource.spread = 0;
                HKVocals.CoroutineHolder.StartCoroutine(CreditsRoll());
                if (isFromMenu == false)
                {
                    aSource.PlayDelayed(2.5f);
                }
                //CreateSkipButton();
            }
        };
        
        //load correct scene after game ends
        ModHooks.BeforeSceneLoadHook += scene =>
        {
            if (scene == "End_Credits" && !doWantToLoadVanillaCredits)
            {
                doWantToLoadVanillaCredits = false;
                return CreditsSceneName;
            }

            return scene;
        };

        //to make GameManager.IsNonGamePlayScene return correct value
        OnInGameCutsceneInfo.WithOrig.get_IsInCutscene += MakeCreditsSceneNonGamePlay;
        
        //without doing this my fps tanks to like 6 not too inclined to find out why so idm doing this
        OnGameManager.WithOrig.SetupHeroRefs += PreventNREsInCreditsScene_SetupHeroRefs;
        OnGameManager.WithOrig.LevelActivated += PreventNREsInCreditsScene_LevelActivated;
        OnInputHandler.WithOrig.AttachHeroController += PreventNREsInCreditsScene_AttachHeroController;
    }

    private static bool MakeCreditsSceneNonGamePlay(Func<bool> orig)
    {
       return orig() || MiscUtils.GetCurrentSceneName() == CreditsSceneName;
    }
    private static Transform CreditsParent => GameObject.Find("Canvas").transform.GetChild(1);
    private static GameObject ModName => CreditsParent.GetChild(0).gameObject;
    private static GameObject Director => CreditsParent.GetChild(1).gameObject;
    private static GameObject Programmer => CreditsParent.GetChild(2).gameObject;
    private static GameObject Audio => CreditsParent.GetChild(3).gameObject;
    private static GameObject ScrollParent => CreditsParent.GetChild(4).gameObject;
    private static GameObject Thanks => CreditsParent.GetChild(5).gameObject;

    private static IEnumerator CreditsRoll()
    {
        yield return ModName.FadeInAndOut(3f, 0.1f, 5.5f, 0.5f);
        yield return Director.FadeInAndOut(0f, 0.3f, 5.5f, 0.3f);
        yield return Programmer.FadeInAndOut(0f, 0.3f, 6.5f, 0.3f);
        yield return Audio.FadeInAndOut(0f, 0.3f, 6f, 0.1f);
 
        ScrollParent.FixFonts();
        ScrollParent.SetActive(true);
        // yield return SkipButton.FadeIn(1.5f);
        yield return ScrollParent.GetAddComponent<ScrollMainCredits>().WaitForScrollEnd();
        ScrollParent.SetActive(false);

        if (isFromMenu == false)
        {
            yield return Thanks.FadeIn(2.5f);
            CreateButtons();
        }
        else
        {
            GoBackToGame();
        }
    }

    public static void CreateButtons()
    {
        var CreditsScreen = CreditsParent;
        var backButton = UIManager.instance.UICanvas.Find("SaveProfileScreen").Find("Controls").Find("BackButton");

        MMButton = Object.Instantiate(backButton, CreditsScreen.transform);
        MMButton.name = "Credits Main Menu Button";
        OGButton = Object.Instantiate(backButton, CreditsScreen.transform);
        OGButton.name = "Original Credits Button";

        MMButton.transform.localPosition = new Vector3(-270, -425, 0);
        OGButton.transform.localPosition = new Vector3(265, -425, 0);

        MMButton.RemoveComponent<EventTrigger>();
        OGButton.RemoveComponent<EventTrigger>();

        MMButton.Find("Text").RemoveComponent<AutoLocalizeTextUI>();
        OGButton.Find("Text").RemoveComponent<AutoLocalizeTextUI>();

        MMButton.Find("Text").GetComponent<Text>().text =
            "Main Menu";
        OGButton.Find("Text").GetComponent<Text>().text =
            "OG Credits";

        var mbmm = MMButton.GetComponent<UMenuButton>();
        var mbog = OGButton.GetComponent<UMenuButton>();
        mbmm.proceed = true;
        mbmm.buttonType = UMenuButton.MenuButtonType.CustomSubmit;
        mbmm.submitAction = _ =>
        {
            isFromMenu = true;
            GoBackToGame();
        };
        mbog.proceed = true;
        mbog.buttonType = UMenuButton.MenuButtonType.CustomSubmit;
        mbog.submitAction = _ =>
        {
            isFromMenu = false;
            GoBackToGame();
        };
    }

    /*public static void CreateSkipButton()
    {
        var CreditsScreen = CreditsParent;
        var backButton = UIManager.instance.UICanvas.Find("SaveProfileScreen").Find("Controls").Find("BackButton");
        
        SkipButton = Object.Instantiate(backButton, CreditsScreen.transform);
        SkipButton.name = "Skip Button";
        SkipButton.SetActive(false);
        
        SkipButton.transform.localPosition = new Vector3(830, 480, 0);
        
        SkipButton.RemoveComponent<EventTrigger>();
        
        SkipButton.Find("Text").RemoveComponent<AutoLocalizeTextUI>();
        
        SkipButton.Find("Text").GetComponent<Text>().text =
            "Skip";
        
        SkipButton.Find("Text").Find("CursorLeft").SetActive(false);
        SkipButton.Find("Text").Find("CursorRight").SetActive(false);
        
        var mbskip = SkipButton.GetComponent<UMenuButton>();
        mbskip.proceed = true;
        mbskip.buttonType = UMenuButton.MenuButtonType.CustomSubmit;
        mbskip.submitAction = _ =>
        {
            if (isFromMenu == true)
            {
                SkipButton.SetActive(false);
                HKVocals.CoroutineHolder.StartCoroutine(GameManager.instance.ReturnToMainMenu(GameManager.ReturnToMainMenuSaveModes.DontSave));
            }
            else if (isFromMenu == false)
            {
                SkipButton.SetActive(false);
                // stop scroll somehow and fade out
                Thanks.FadeIn(2.5f);
                CreateButtons();
            }
        };
    }*/

    private static void GoBackToGame()
    {
        if (isFromMenu)
        {
            HKVocals.CoroutineHolder.StartCoroutine(GameManager.instance.ReturnToMainMenu(GameManager.ReturnToMainMenuSaveModes.DontSave));
        }
        else
        {
            doWantToLoadVanillaCredits = true;
            GameManager.instance.LoadScene("End_Credits");
        }

        isFromMenu = false;
    }

    private static IEnumerator FadeInAndOut(this GameObject go, float notSeenWait, float fadeInTime, float seenWait, float fadeOutTime)
    {
        yield return new WaitForSeconds(notSeenWait);
        yield return go.FadeIn(fadeInTime);
        yield return new WaitForSeconds(seenWait);
        yield return go.FadeOut(fadeOutTime);
        go.SetActive(false);
    }

    private static IEnumerator FadeIn(this GameObject go, float fadeTime)
    {
        go.FixFonts();
        go.SetAlphaZero();
        go.SetActive(true);

        yield return go.Fade(fadeTime, true);
    }
    private static IEnumerator FadeOut(this GameObject go, float fadeTime)
    {
        yield return go.Fade(fadeTime, false);
        go.SetActive(false);
    }

    private static IEnumerator Fade(this GameObject go, float time, bool fadeIn)
    {
        //if fade in, expect alpha to be 0
        float alphaChangeCounter = 0;
        
        var texts = go.GetComponentsInChildren<Text>(true).Cast<Graphic>().ToArray();
        var sprites = go.GetComponentsInChildren<Image>(true).Cast<Graphic>().ToArray();

        var graphics = texts.Concat(sprites).ToArray();
        
        while (alphaChangeCounter < 1f)
        {
            //add when fade in, subract when fade out
            foreach (var graphic in graphics)
            {
                if (fadeIn)
                {
                    graphic.SetAlpha(graphic.color.a + Time.deltaTime / time);
                }
                else
                {
                    graphic.SetAlpha(graphic.color.a - Time.deltaTime / time);
                }
            }

            alphaChangeCounter += Time.deltaTime / time;
            yield return null;
        }
    }

    private static  void SetAlpha(this Graphic t, float a) => t.color = new Color(t.color.r, t.color.g, t.color.b, a);

    private static void FixFonts(this GameObject go)
    {
        go.GetComponentsInChildren<Text>().ForEach(t =>
        {
            t.font = t.font.name switch
            {
                "TrajanPro-Regular" => MenuResources.TrajanRegular,
                "TrajanPro-Bold" => MenuResources.TrajanBold,
                "NotoSerifCJKsc-Regular" => MenuResources.NotoSerifCJKSCRegular,
                _ => MenuResources.Perpetua
            };
        });
    }
    
    private static void SetAlphaZero(this GameObject go)
    {
        go.GetComponentsInChildren<Text>(true).ForEach(g => g.SetAlpha(0));
        go.GetComponentsInChildren<Image>(true).ForEach(g => g.SetAlpha(0));
    }
    
    private static void PreventNREsInCreditsScene_AttachHeroController(On.InputHandler.orig_AttachHeroController orig, InputHandler self, HeroController herocontroller)
    {
        if (MiscUtils.GetCurrentSceneName() != CreditsSceneName) orig(self, herocontroller);
    }

    private static void PreventNREsInCreditsScene_LevelActivated(On.GameManager.orig_LevelActivated orig, GameManager self, Scene scenefrom, Scene sceneto)
    {
        if (MiscUtils.GetCurrentSceneName() != CreditsSceneName) orig(self, scenefrom, sceneto);
    }

    private static void PreventNREsInCreditsScene_SetupHeroRefs(On.GameManager.orig_SetupHeroRefs orig, GameManager self)
    {
        if (MiscUtils.GetCurrentSceneName() != CreditsSceneName) orig(self);
    }

    public static IEnumerator LoadCreditsFromMenu()
    {
        InputHandler.Instance.StopUIInput();
        yield return HKVocals.CoroutineHolder.StartCoroutine(UIManagerR.HideCurrentMenu());
        GameCameras.instance.cameraController.FadeOut(CameraFadeType.START_FADE);
        yield return new WaitForSeconds(2.5f);
        isFromMenu = true;
        GameManagerR.LoadScene(CreditsSceneName);
    }
}

public class ScrollMainCredits : MonoBehaviour
{
    private bool stopScrolling;
    private Coroutine stopScrollingCoroutine;
    private HeroActions InputActions;

    public void Start()
    {
        InputActions = InputHandler.Instance.inputActions;
    }

    public void FixedUpdate()
    {
        if (Input.mouseScrollDelta.y == 0f 
            && !InputActions.up.IsPressed 
            && !InputActions.down.IsPressed)
        {
            if (!stopScrolling)
            {
                transform.Translate(Vector3.up * (Time.deltaTime * RollCredits.RollSpeed));
            }
        }
        else
        {
            if (stopScrollingCoroutine != null)
            {
                StopCoroutine(stopScrollingCoroutine);
            }
            stopScrolling = true;
            stopScrollingCoroutine = StartCoroutine(ResetStopScrolling());

            if (Input.mouseScrollDelta.y != 0)
            {
                transform.Translate(Vector3.up * Input.mouseScrollDelta.y * RollCredits.MouseScrollSpeed);
            }
            else if (InputActions.down.IsPressed)
            {
                transform.Translate(Vector3.up * RollCredits.UpDownSpeed);
            }
            else if (InputActions.up.IsPressed)
            {
                transform.Translate(Vector3.down * RollCredits.UpDownSpeed);
            }
        }
    }

    IEnumerator ResetStopScrolling()
    {
        yield return new WaitForSeconds(0.8f);
        stopScrolling = false;
    }

    public IEnumerator WaitForScrollEnd()
    {
        while (transform.position.y < RollCredits.ScrollMaxY + 1250f)
        {
            yield return null;
        }
    }
}