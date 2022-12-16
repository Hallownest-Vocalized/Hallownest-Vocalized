using GlobalEnums;

namespace HKVocals.MajorFeatures;

public static class RollCredits
{
    private static float fadeTime = 1f;
    public static float RollSpeed = 150f;
    public static float MouseScrollSpeed = 60f;
    public static float UpDownSpeed = 20f;
    public static float ScrollMaxY = 46600f;
    private const string CreditsSceneName = "CreditsScene";
    private static bool isFromMenu;
    private static bool doWantToLoadEndCredits;

    public static void Hook()
    {
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged += (from, to) =>
        {
            if (to.name == CreditsSceneName)
            {
                GameManagerR.SetState(GameState.CUTSCENE);
                UIManagerR.SetState(UIState.CUTSCENE);
                HKVocals.CoroutineHolder.StartCoroutine(CreditsRoll());
            }
        };

        ModHooks.BeforeSceneLoadHook += scene =>
        {
            if (scene == "End_Credits" && !doWantToLoadEndCredits)
            {
                doWantToLoadEndCredits = false;
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

    private static IEnumerator CreditsRoll()
    {
        if (!isFromMenu) yield return new WaitForSeconds(2f);

        //increase title size
        ModName.GetComponentInChildren<Image>(true).rectTransform.sizeDelta = new Vector2(1315, 512);
        
        yield return ModName.FadeInAndOut();
        yield return Director.FadeInAndOut();
        yield return Programmer.FadeInAndOut();
        yield return Audio.FadeInAndOut();
 
        ScrollParent.FixFonts();
        ScrollParent.SetActive(true);
        yield return ScrollParent.GetAddComponent<ScrollMainCredits>().WaitForScrollEnd();
        ScrollParent.SetActive(false);
        
        GoBackToGame();
    }

    private static void GoBackToGame()
    {
        if (isFromMenu)
        {
            HKVocals.CoroutineHolder.StartCoroutine(GameManager.instance.ReturnToMainMenu(GameManager.ReturnToMainMenuSaveModes.DontSave));
        }
        else
        {
            doWantToLoadEndCredits = true;
            GameManager.instance.LoadScene("End_Credits");
        }

        isFromMenu = false;
    }

    private static IEnumerator FadeInAndOut(this GameObject go)
    {
        yield return go.FadeIn();
        yield return new WaitForSeconds(5f);
        yield return go.FadeOut();
        go.SetActive(false);
    }

    private static IEnumerator FadeIn(this GameObject go)
    {
        go.FixFonts();
        go.SetAlphaZero();
        go.SetActive(true);

        yield return go.Fade(fadeTime, true);
    }
    private static IEnumerator FadeOut(this GameObject go)
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
            HKVocals.DoLog(t.transform.parent.gameObject.name + "/" + t.gameObject.name + ": " + t.font.name);
            if (t.font.name == "TrajanPro-Regular")
            {
                t.font = MenuResources.TrajanRegular;
            }
            else if (t.font.name == "TrajanPro-Bold")
            {
                t.font = MenuResources.TrajanBold;
            }
            else if (t.font.name == "NotoSerifCJKsc-Regular")
            {
                t.font = MenuResources.NotoSerifCJKSCRegular;
            }
            else
            {
                t.font = MenuResources.Perpetua;
            }
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
                transform.Translate(Vector3.up * Time.deltaTime * RollCredits.RollSpeed);
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
        while (transform.position.y < RollCredits.ScrollMaxY)
        {
            yield return null;
        }
    }
}