using Satchel;
using HKVocals.Utils;
using System.Linq;
using FrogCore;
using FrogCore.Ext;

namespace HKVocals.EasterEggs;
public static class PaleFlower
{
    private static tk2dSpriteCollectionData LurkerFlowerIdleSC;
    private static tk2dSpriteCollectionData LurkerFlowerSnatchSC;

    public static void Hook()
    {
        /*On.HealthManager.TakeDamage += TakeDamage;*/
        On.HealthManager.Awake += CreateAnimations;
        On.HealthManager.Die += PaleLurkerDie;
        ModHooks.LanguageGetHook += LurkerText;
        CreateCollections();
    }

    private static void CreateAnimations(On.HealthManager.orig_Awake orig, HealthManager self)
    {
        orig(self);
        if (self.name == "Pale Lurker")
        {
            AddAnimations(self.GetComponent<tk2dSpriteAnimator>().Library);
            On.HealthManager.Awake -= CreateAnimations;
        }
    }
    private static void CreateCollections()
    {
        Texture2D Idle = AssemblyUtils.GetTextureFromResources("HKVocals.Resources.Pale_Lurker_Flower_Idle.png");
        Texture2D Snatch = AssemblyUtils.GetTextureFromResources("HKVocals.Resources.Pale_Lurker_Flower_Snatch.png");
        GameObject IdleGo = new GameObject("Custom Pale Lurker Idle Col");
        GameObject SnatchGo = new GameObject("Custom Pale Lurker Snatch Col");
        IdleGo.DontDestroyOnLoad();
        SnatchGo.DontDestroyOnLoad();
        float width = (float)Idle.width / 6f;
        float height = (float)Idle.height;
        string[] names = new string[6];
        Rect[] rects = new Rect[6];
        Vector2[] anchors = new Vector2[6];
        for (int i = 0; i < 6; i++)
        {
            names[i] = i.ToString();
            rects[i] = new Rect(width * (float)i, 0, width, height);
            anchors[i] = new Vector2(0.5f, 1f);
        }
        LurkerFlowerIdleSC = FrogCore.Utils.CreateFromTexture(IdleGo, Idle, tk2dSpriteCollectionSize.PixelsPerMeter(66f), new Vector2(width * 6, height), names, rects, null, anchors, new bool[6]); //FrogCore.Utils.CreateTk2dSpriteCollection(Idle, names, rects, anchors, IdleGo);
        LurkerFlowerIdleSC.hasPlatformData = false;
        width = (float)Snatch.width / 5f;
        height = (float)Snatch.height;
        names = new string[5];
        rects = new Rect[5];
        anchors = new Vector2[5];
        for (int i = 0; i < 5; i++)
        {
            names[i] = i.ToString();
            rects[i] = new Rect(width * (float)i, 0, width, height);
            anchors[i] = new Vector2(0.5f, 1f);
        }
        LurkerFlowerSnatchSC = FrogCore.Utils.CreateFromTexture(SnatchGo, Snatch, tk2dSpriteCollectionSize.PixelsPerMeter(66f), new Vector2(width * 5, height), names, rects, null, anchors, new bool[6]);
        LurkerFlowerSnatchSC.hasPlatformData = false;
    }
    public static void AddAnimations(tk2dSpriteAnimation animation)
    {
        tk2dSpriteAnimationClip idleClip = new tk2dSpriteAnimationClip(animation.GetClipByName("Idle"))
        {
            name = "IdleFlower",
            frames = new tk2dSpriteAnimationFrame[] {
            new() {spriteCollection = LurkerFlowerIdleSC, spriteId = 0},
            new() {spriteCollection = LurkerFlowerIdleSC, spriteId = 2},
            new() {spriteCollection = LurkerFlowerIdleSC, spriteId = 3},
            new() {spriteCollection = LurkerFlowerIdleSC, spriteId = 4},
            new() {spriteCollection = LurkerFlowerIdleSC, spriteId = 5},
            new() {spriteCollection = LurkerFlowerIdleSC, spriteId = 6}}
        };
        tk2dSpriteAnimationClip snatchClip = new tk2dSpriteAnimationClip()
        {
            name = "SnatchFlower",
            fps = 60,
            wrapMode = tk2dSpriteAnimationClip.WrapMode.Once,
            frames = new tk2dSpriteAnimationFrame[] {
            new() {spriteCollection = LurkerFlowerSnatchSC, spriteId = 0},
            new() {spriteCollection = LurkerFlowerSnatchSC, spriteId = 0},
            new() {spriteCollection = LurkerFlowerSnatchSC, spriteId = 0},
            new() {spriteCollection = LurkerFlowerSnatchSC, spriteId = 0},
            new() {spriteCollection = LurkerFlowerSnatchSC, spriteId = 1},
            new() {spriteCollection = LurkerFlowerSnatchSC, spriteId = 1},
            new() {spriteCollection = LurkerFlowerSnatchSC, spriteId = 1},
            new() {spriteCollection = LurkerFlowerSnatchSC, spriteId = 1},
            new() {spriteCollection = LurkerFlowerSnatchSC, spriteId = 1},
            new() {spriteCollection = LurkerFlowerSnatchSC, spriteId = 2},
            new() {spriteCollection = LurkerFlowerSnatchSC, spriteId = 2},
            new() {spriteCollection = LurkerFlowerSnatchSC, spriteId = 2},
            new() {spriteCollection = LurkerFlowerSnatchSC, spriteId = 2},
            new() {spriteCollection = LurkerFlowerSnatchSC, spriteId = 2},
            new() {spriteCollection = LurkerFlowerSnatchSC, spriteId = 3},
            new() {spriteCollection = LurkerFlowerSnatchSC, spriteId = 3},
            new() {spriteCollection = LurkerFlowerSnatchSC, spriteId = 3},
            new() {spriteCollection = LurkerFlowerSnatchSC, spriteId = 4},
            new() {spriteCollection = LurkerFlowerSnatchSC, spriteId = 4},
            new() {spriteCollection = LurkerFlowerSnatchSC, spriteId = 4},
            new() {spriteCollection = LurkerFlowerSnatchSC, spriteId = 4}}
        };
        animation.clips = animation.clips.Append(idleClip).Append(snatchClip).ToArray();
    }

    private static string LurkerText(string key, string sheetTitle, string orig)
    {
        switch (key)
        {
            case "LURKER_DEFEAT":
                return "...King!... Treasure!... Mine! Mine!...<page>" +
                    "Tired... Treasure! Shiny treasure...";
            case "LURKER_REQUEST":
                return "Give an item?";
            case "LURKER_UNINTERESTED":
                return "Trinkets! ...Huaghhh, dull treasures...<page>" +
                    "Key! Mine! Begone, begone!";
            case "LURKER_BROKENFLOWER":
                return "...Huagh?... Stem! Stripped. Sad flower...<page>" +
                    "Ooagh! Sad Treasure! To dirt!<page>" +
                    "King! Mine! Begone, begone!";
            case "LURKER_FLOWER":
                return "Oooogh! Treasure! Pretty treasure! Shiny flower...<page>" +
                    "Pale thing! Frail? Careful! Careful, careful!<page>" +
                    "Delicate thing. Soft petals. Oooogh...";
            case "LURKER_FLOWER_REPEAT":
                return "Pale thing! Shiny treasure...";
            case "LURKER_FLOWER_DREAM":
                return "...Oooooh, aaaaah...";
            case "LURKER_IDLE_0":
                return "Flower! Treasure!... Soft flower...";
            case "LURKER_IDLE_1":
                return "Never, never wilt. Pale thing...";
            case "LURKER_IDLE_2":
                return "Grow? No! Mine, mine!";
            case "LURKER_IDLE_3":
                return "For me! For you? For... King?";
            case "LURKER_IDLE_4":
                return "No. For me. For me!";
            case "LURKER_IDLE_5":
                return "Frail... Soft... Pure...";
            case "LURKER_IDLE_REPEAT":
                return "Ooogh... Pale thing...";
            case "LURKER_FLOWER_DREAM2":
                return "Hehehehehee. Small thing, tiny thing!<page>" +
                    "But warm...";
            default:
                return orig;
        }
    }

    private static void PaleLurkerDie(On.HealthManager.orig_Die orig, HealthManager self, float? attackDirection, AttackTypes attackType, bool ignoreEvasion)
    {
        if (self.gameObject.name != "Pale Lurker")
        {
            orig(self, attackDirection, attackType, ignoreEvasion);
        }
        else if (!self.gameObject.GetComponent<PLMono>())
        {
            //self.gameObject.GetComponent<tk2dSpriteAnimator>().Play("Idle");
            self.gameObject.AddComponent<PLMono>().StartWaitForRecoil();

            /*     self.gameObject.RemoveComponent<DamageHero>();
            self.gameObject.RemoveComponent<HealthManager>();*/
        }
    }

    private class PLMono : MonoBehaviour
    {
        Utils.DialogueNPC npc;
        bool givenFlower = false;
        tk2dSpriteAnimator _anim;
        Rigidbody2D _rb2d;
        Coroutine _turnCo;

        private void Awake()
        {
            UnityEngine.Object.Destroy(gameObject.GetComponent<DamageHero>());
            _anim = GetComponent<tk2dSpriteAnimator>();
            _rb2d = GetComponent<Rigidbody2D>();
            Invincible(true);
        }

        private void Invincible(bool invincible) => GetComponent<HealthManager>().enabled = !invincible;

        private void SetUpFirstNPC()
        {
            Invincible(true);
            npc = Utils.DialogueNPC.CreateInstance();
            npc.transform.position = transform.position;
            npc.DialogueSelector = GetDialogueFirst;
            npc.GetComponent<MeshRenderer>().enabled = false;
            npc.SetDreamKey("LURKER_0");
            npc.SetUp();
            npc.gameObject.LocateMyFSM("npc_control").GetBoolVariable("Hero Always Right").Value = false;
            npc.gameObject.LocateMyFSM("Conversation Control").AddCustomAction("End", () => { if (!givenFlower) StartCoroutine(RunAwayAnim()); });
        }

        private DialogueOptions GetDialogueFirst(DialogueCallbackOptions prev)
        {
            if (!prev.Continue)
            {
                if (givenFlower)
                    return new() { Key = "LURKER_FLOWER_REPEAT", Sheet = "", Type = DialogueType.Normal, Continue = true };
                return new() { Key = "LURKER_DEFEAT", Sheet = "", Type = DialogueType.Normal, Continue = true };
            }
            switch (prev.Key)
            {
                case "LURKER_DEFEAT":
                    return new() { Key = "LURKER_REQUEST", Sheet = "", Cost = 0, Type = DialogueType.YesNo, Continue = true };
                case "LURKER_REQUEST":
                    if (prev.Response == DialogueResponse.Yes)
                    {
                        if (PlayerData.instance.hasXunFlower)
                        {
                            if (!PlayerData.instance.xunFlowerBroken)
                            {
                                givenFlower = true;
                                npc.SetDreamKey("LURKER_FLOWER_DREAM");
                                return new() { Key = "LURKER_FLOWER", Sheet = "", Wait = TakeFlowerAnim(), Type = DialogueType.Normal, Continue = true };
                            }
                            return new() { Key = "LURKER_BROKENFLOWER", Sheet = "", Wait = TakePause(), Type = DialogueType.Normal, Continue = true };
                        }
                        return new() { Key = "LURKER_UNINTERESTED", Sheet = "", Wait = TakePause(), Type = DialogueType.Normal, Continue = true };
                    }
                    return new() { Continue = false };
                default:
                    return new() { Continue = false };
            }
        }

        private IEnumerator TakePause()
        {
            yield return new WaitForSeconds(2f);
        }

        private IEnumerator TakeFlowerAnim()
        {
            _anim.Play("SnatchFlower");
            yield return new WaitForSeconds(2f); // replace with starting/waiting for animation
            _anim.Play("IdleFlower");
        }

        private IEnumerator RunAwayAnim()
        {
            Invincible(false);
            GetComponent<Collider2D>().isTrigger = false;
            yield return new WaitForSeconds(5f);
            StopCoroutine(_turnCo);
            // hide anim, open gates. This will be canceled if lurker dies, and the normal events will take place
        }

        internal void StartWaitForRecoil() => StartCoroutine(WaitForRecoil());

        private IEnumerator WaitForRecoil()
        {
            gameObject.LocateMyFSM("Lurker Control").enabled = false;
            transform.GetComponentsInChildren<Transform>().Where(t => t != transform).ForEach(t => t.gameObject.SetActive(false));
            //gameObject.layer = 13;
            _rb2d.velocity = new Vector2(-7f * transform.localScale.x, 10f);
            _anim.Play("Death Air");
            GetComponent<SpriteFlash>().flashWhiteLong();
            GameManager.instance.FreezeMoment(2);
            while (_rb2d.velocity != Vector2.zero)
                _rb2d.velocity = new Vector2(_rb2d.velocity.y, _rb2d.velocity.x / 1.1f);
                yield return new WaitForFixedUpdate();
            _rb2d.velocity = Vector2.zero;
            yield return _anim.PlayAnimWait("Land");
            gameObject.layer = 13;
            _rb2d.isKinematic = false;
            _rb2d.gravityScale = 0f;
            GetComponent<Collider2D>().isTrigger = true;
            _anim.Play("Idle");
            _turnCo = StartCoroutine(AutoTurn());
            SetUpFirstNPC();
        }

        private IEnumerator AutoTurn()
        {
            while (true)
            {
                if (transform.position.x - HeroController.instance.transform.position.x > 0)
                {
                    // facing right
                    if (transform.localScale.x < 0)
                    {
                        transform.localScale = new Vector3(1, 1, 1);
                        yield return _anim.PlayAnimWait("Turn");
                        _anim.Play(givenFlower ? "IdleFlower" : "Idle");
                    }
                }
                // hero is right of lurker
                else
                {
                    // facing left
                    if (transform.localScale.x > 0)
                    {
                        transform.localScale = new Vector3(-1, 1, 1);
                        yield return _anim.PlayAnimWait("Turn");
                        _anim.Play(givenFlower ? "IdleFlower" : "Idle");
                    }
                }
            }
        }
    }
}