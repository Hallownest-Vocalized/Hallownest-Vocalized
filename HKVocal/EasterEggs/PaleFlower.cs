using System.Linq;
using FrogCore;
using FrogCore.Ext;

namespace HKVocals.EasterEggs;
public static class PaleFlower
{
    private static tk2dSpriteCollectionData LurkerFlowerIdleSC;
    private static tk2dSpriteCollectionData LurkerFlowerSnatchSC;
    private static bool addedAnimations = false;

    public static void Hook()
    {
        new MonoMod.RuntimeDetour.Hook(ReflectionHelper.GetMethodInfo(typeof(SFCore.MonoBehaviours.CustomItemList), "CheckBool"), ModifySFCoreFlower1);
        new MonoMod.RuntimeDetour.Hook(ReflectionHelper.GetMethodInfo(typeof(SFCore.MonoBehaviours.CustomItemList), "GetDescConvo"), ModifySFCoreFlower2);
        On.HealthManager.Awake += PaleLurkerAwake;
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged += SceneChanged;
        On.HealthManager.Die += PaleLurkerDie;
        ModHooks.LanguageGetHook += LurkerText;
        CreateCollections();
    }

    private static bool ModifySFCoreFlower1(Func<SFCore.MonoBehaviours.CustomItemList, SFCore.ItemHelper.Item, bool> orig, SFCore.MonoBehaviours.CustomItemList self, SFCore.ItemHelper.Item item)
    {
        if (item.type == SFCore.ItemType.Flower)
            return PlayerData.instance.GetBool(item.playerdataBool1);
        return orig(self, item);
    }

    private static string ModifySFCoreFlower2(Func<SFCore.MonoBehaviours.CustomItemList, int, string> orig, SFCore.MonoBehaviours.CustomItemList self, int itemNum)
    {
        SFCore.ItemHelper.Item item = self.List.First((SFCore.ItemHelper.Item x) => x.uniqueName.Equals(ReflectionHelper.GetField<SFCore.MonoBehaviours.CustomItemList, GameObject[]>(self, "_currentList")[itemNum].name));
        if (item.type == SFCore.ItemType.Flower)
        {
            switch ((PlayerData.instance.GetBool(item.playerdataBool2), PlayerData.instance.GetBool(item.playerdataInt)))
            {
                case (false, false):
                    return item.descConvo1;
                case (true, false):
                    return item.nameConvoBoth;
                case (false, true):
                    return item.descConvo2;
                case (true, true):
                    return item.descConvoBoth;
            }
        }
        return orig(self, itemNum);
    }

    private static void PaleLurkerAwake(On.HealthManager.orig_Awake orig, HealthManager self)
    {
        orig(self);
        if (self.name == "Pale Lurker")
        {
            if (!addedAnimations)
            {
                AddAnimations(self.GetComponent<tk2dSpriteAnimator>().Library);
                addedAnimations = true;
            }
        }
    }

    private static void SceneChanged(Scene from, Scene to)
    {
        if (HKVocals._saveSettings.LurkerFlower)
        {
            GameObject lurker = to.GetRootGameObjects().First(g => g.name == "Lurker Control").transform.Find("Pale Lurker").gameObject;
            lurker.AddComponent<LurkerNPCSecond>();
            lurker.transform.parent = null;
            lurker.SetActive(true);
            to.GetRootGameObjects().First(g => g.name == "Corpse Pale Lurker").SetActive(false);
        }
    }

    private static void CreateCollections()
    {
        Texture2D Idle = Satchel.AssemblyUtils.GetTextureFromResources("HKVocals.Resources.Pale_Lurker_Flower_Idle.png");
        Texture2D Snatch = Satchel.AssemblyUtils.GetTextureFromResources("HKVocals.Resources.Pale_Lurker_Flower_Snatch.png");
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
            anchors[i] = new Vector2(139f, 128f);
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
            anchors[i] = new Vector2(139f, 128f);
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
            new() {spriteCollection = LurkerFlowerIdleSC, spriteId = 1},
            new() {spriteCollection = LurkerFlowerIdleSC, spriteId = 2},
            new() {spriteCollection = LurkerFlowerIdleSC, spriteId = 3},
            new() {spriteCollection = LurkerFlowerIdleSC, spriteId = 4},
            new() {spriteCollection = LurkerFlowerIdleSC, spriteId = 5}}
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
            case "LURKER_IDLE_REPEAT_0":
                return "Ooogh... Pale thing...";
            case "LURKER_IDLE_REPEAT_1":
                return "Flower! Treasure!... Soft flower...";
            case "LURKER_IDLE_REPEAT_2":
                return "Never, never wilt. Pale thing...";
            case "LURKER_IDLE_DREAM":
                return "Hehehehehee. Small thing, tiny thing!<page>" +
                    "But warm...";
            case "LURKER_B4_0":
                return "Key! Treasure! Begone, begone!";
            case "LURKER_NPC_SUPER":
                return "";
            case "LURKER_NPC_MAIN":
                return "Pale Lurker";
            case "LURKER_NPC_SUB":
                return "";
            default:
                return orig;
        }
    }

    private static void PaleLurkerDie(On.HealthManager.orig_Die orig, HealthManager self, float? attackDirection, AttackTypes attackType, bool ignoreEvasion)
    {
        if (self.gameObject.name == "Pale Lurker")
        {
            if (!self.gameObject.GetComponent<LurkerNPCFirst>())
                self.gameObject.AddComponent<LurkerNPCFirst>().StartWaitForRecoil();
            else if (self.GetComponent<HealthManager>().enabled)
                orig(self, attackDirection, attackType, ignoreEvasion);
        }
        else
            orig(self, attackDirection, attackType, ignoreEvasion);
    }

    private class LurkerNPCFirst : MonoBehaviour
    {
        DialogueNPC npc;
        tk2dSpriteAnimator _anim;
        Rigidbody2D _rb2d;
        Collider2D _col;
        HealthManager _hm;
        Coroutine _turnCo;
        bool waitForLeave = true;
        GameObject audioPlayer;

        private void PlayAudioOneShot(AudioClip clip)
        {
            AudioSource source = audioPlayer.Spawn(HeroController.instance.transform.position).GetComponent<AudioSource>();
            source.volume = 0.7f;
            source.pitch = 1.1f;
            source.PlayOneShot(clip);
        }

        private void Awake()
        {
            UnityEngine.Object.Destroy(gameObject.GetComponent<DamageHero>());
            _anim = GetComponent<tk2dSpriteAnimator>();
            _rb2d = GetComponent<Rigidbody2D>();
            _col = GetComponent<Collider2D>();
            _hm = GetComponent<HealthManager>();
            _hm.enabled = false;
            audioPlayer = gameObject.LocateMyFSM("Lurker Control").GetAction<AudioPlayerOneShotSingle>("Slash 1", 1).audioPlayer.Value;
        }

        private void Invincible(bool invincible)
        {
            gameObject.layer = invincible ? 13 : 11;
            _hm.enabled = !invincible;
        }

        private void SetUpNPC()
        {
            Invincible(true);
            npc = DialogueNPC.CreateInstance();
            npc.transform.position = transform.position;
            npc.DialogueSelector = GetDialogue;
            npc.GetComponent<MeshRenderer>().enabled = false;
            npc.SetDreamKey("LURKER_B4_0");
            npc.SetTitle("LURKER_NPC");
            npc.SetUp();
            PlayMakerFSM npc_control = npc.gameObject.LocateMyFSM("npc_control");
            npc_control.GetBoolVariable("Hero Always Right").Value = false;
            npc_control.GetFloatVariable("Move To Offset").Value = 2f;
            npc_control.AddMethod("Convo End", () => { if (!HKVocals._saveSettings.LurkerFlower) StartCoroutine(RunAwayAnim()); });
            npc_control.AddState("Wait For Shiny Collect");
            npc_control.ChangeTransition("Pause", "FINISHED", "Wait For Shiny Collect");
            npc_control.AddTransition("Wait For Shiny Collect", "SHINY PICKED UP", "Idle");
            npc_control.AddMethod("Wait For Shiny Collect", () => npc_control.ChangeTransition("Pause", "FINISHED", "Idle"));
        }

        private DialogueOptions GetDialogue(DialogueCallbackOptions prev)
        {
            if (!prev.Continue)
            {
                if (HKVocals._saveSettings.LurkerFlower)
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
                        waitForLeave = false;
                        if (PlayerData.instance.hasXunFlower)
                        {
                            if (!PlayerData.instance.xunFlowerBroken)
                            {
                                HKVocals._saveSettings.LurkerFlower = true;
                                npc.SetDreamKey("LURKER_FLOWER_DREAM");
                                PlayerData.instance.hasXunFlower = false;
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
            _anim.Play("Idle");
            yield return new WaitForSeconds(1f);
            PlayAudioOneShot(HeroController.instance.normalSlash.Reflect().audio.clip);
            yield return _anim.PlayAnimWait("SnatchFlower");
            _anim.Play("IdleFlower");
            yield return new WaitForSeconds(1f);
            EnemyDeathEffects deathEffects = GetComponent<EnemyDeathEffects>();
            GameObject.Instantiate(deathEffects.Reflect().corpsePrefab.transform.GetChild(0).gameObject, transform);
            deathEffects.RecordJournalEntry();
            StopCoroutine(_turnCo);
            GameManager.instance.AwardAchievement("KindnessPaleLurker");
        }

        private IEnumerator RunAwayAnim()
        {
            yield return new WaitWhile(() => HeroController.instance.controlReqlinquished);
            Destroy(npc.gameObject);
            Invincible(false);
            _col.isTrigger = false;
            if (waitForLeave)
                yield return new WaitForSeconds(5f);
            yield return null;
            StopCoroutine(_turnCo);
            PlayMakerFSM _control = gameObject.LocateMyFSM("Lurker Control");
            _control.enabled = true;
            _control.SetState("Dig 1");
            _control.ChangeTransition("Dig 2", "FINISHED", "Dormant");
            _control.AddMethod("Dormant", () => { PlayMakerFSM.BroadcastEvent("SHINY PICKED UP"); gameObject.SetActive(false); });
        }

        internal void StartWaitForRecoil() => StartCoroutine(WaitForRecoil());

        private IEnumerator WaitForRecoil()
        {
            gameObject.LocateMyFSM("Lurker Control").enabled = false;
            transform.GetComponentsInChildren<Transform>().Where(t => t != transform).ForEach(t => t.gameObject.SetActive(false));
            //gameObject.layer = 13;
            _rb2d.velocity = new Vector2(7f * transform.localScale.x, 10f);
            _anim.Play("Death Air");
            if (!PlayerData.instance.xunFlowerBroken)
                StartCoroutine(SpawnEffects());
            GetComponent<SpriteFlash>().flashWhiteLong();
            GameManager.instance.FreezeMoment(2);
            while (_rb2d.velocity != Vector2.zero)
            {
                _rb2d.velocity = new Vector2(_rb2d.velocity.y, _rb2d.velocity.x / (1 + (Time.fixedDeltaTime / 10)));
                yield return new WaitForFixedUpdate();
            }
            _rb2d.velocity = Vector2.zero;
            yield return _anim.PlayAnimWait("Land");
            gameObject.layer = 13;
            _rb2d.isKinematic = false;
            _rb2d.gravityScale = 0f;
            _col.isTrigger = true;
            _anim.Play("Idle");
            _turnCo = StartCoroutine(AutoTurn());
            try
            {
                GetComponent<EnemyDeathEffects>().Reflect().audioSnapshotOnDeath.TransitionTo(2f);
            }
            catch { }
            SetUpNPC();
        }

        private IEnumerator SpawnEffects()
        {
            EnemyHitEffectsUninfected hitEffects = GetComponent<EnemyHitEffectsUninfected>();
            FlingUtils.Config config = new()
            {
                AmountMin = 20,
                AmountMax = 30,
                SpeedMin = 20f,
                SpeedMax = 35f,
                AngleMin = 140f,
                AngleMax = 220f,
                OriginVariationX = 0f,
                OriginVariationY = 0f
            };
            for (int i = 0; i < 4; i++)
            {
                config.Prefab = hitEffects.slashEffectGhost1;
                FlingUtils.SpawnAndFling(config, transform, hitEffects.effectOrigin);
                config.Prefab = hitEffects.slashEffectGhost2;
                FlingUtils.SpawnAndFling(config, transform, hitEffects.effectOrigin);
                yield return null;
            }
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
                        yield return _anim.PlayAnimWait("Turn");
                        transform.localScale = new Vector3(1, 1, 1);
                        _anim.Play(HKVocals._saveSettings.LurkerFlower ? "IdleFlower" : "Idle");
                    }
                }
                // hero is right of lurker
                else
                {
                    // facing left
                    if (transform.localScale.x > 0)
                    {
                        yield return _anim.PlayAnimWait("Turn");
                        transform.localScale = new Vector3(-1, 1, 1);
                        _anim.Play(HKVocals._saveSettings.LurkerFlower ? "IdleFlower" : "Idle");
                    }
                }
                yield return null;
            }
        }
    }
    private class LurkerNPCSecond : MonoBehaviour
    {
        DialogueNPC npc;
        tk2dSpriteAnimator _anim;
        int repeat = -1;

        private void Awake()
        {
            transform.SetPosition2D(224.6f, 52.5f);
            transform.parent = null;
            UnityEngine.Object.Destroy(gameObject.GetComponent<DamageHero>());
            foreach (PlayMakerFSM fsm in GetComponents<PlayMakerFSM>())
                Destroy(fsm);
            _anim = GetComponent<tk2dSpriteAnimator>();
            gameObject.layer = 13;
            Rigidbody2D _rb2d = GetComponent<Rigidbody2D>();
            _rb2d.isKinematic = false;
            _rb2d.gravityScale = 0f;
            GetComponent<Collider2D>().isTrigger = true;
            GetComponent<HealthManager>().enabled = false;
            SetUpNPC();
            _anim.Play("IdleFlower");
        }

        private void SetUpNPC()
        {
            npc = DialogueNPC.CreateInstance();
            npc.transform.position = transform.position;
            npc.DialogueSelector = GetDialogue;
            npc.GetComponent<MeshRenderer>().enabled = false;
            npc.SetDreamKey("LURKER_IDLE_DREAM");
            npc.SetTitle("LURKER_NPC");
            npc.SetUp();
            PlayMakerFSM npc_control = npc.gameObject.LocateMyFSM("npc_control");
            npc_control.GetBoolVariable("Hero Always Right").Value = false;
            npc_control.GetBoolVariable("Hero Always Left").Value = true;
            npc_control.GetFloatVariable("Move To Offset").Value = 2f;
        }

        private DialogueOptions GetDialogue(DialogueCallbackOptions prev)
        {
            if (prev.Continue)
                return new() { Continue = false };
            if (HKVocals._saveSettings.LurkerConversation <= 4)
            {
                HKVocals._saveSettings.LurkerConversation++;
                return new() { Key = "LURKER_IDLE_" + HKVocals._saveSettings.LurkerConversation, Sheet = "", Cost = 0, Type = DialogueType.Normal, Continue = true };
            }
            repeat++;
            if (repeat > 2)
                repeat = 0;
            return new() { Key = "LURKER_IDLE_REPEAT_" + repeat, Sheet = "", Cost = 0, Type = DialogueType.Normal, Continue = true };
        }
    }
}