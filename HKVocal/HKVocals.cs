using System;
using System.Collections.Generic;
using System.Linq;
using Modding;
using UnityEngine;
using System.Reflection;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using System.Collections;
using System.IO;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace HKVocals
{ 
    public class HKVocals: Mod, IGlobalSettings<GlobalSettings>, ILocalSettings<SaveSettings>, ICustomMenuMod
    {
        public static GlobalSettings _globalSettings { get; set; } = new GlobalSettings();
        public void OnLoadGlobal(GlobalSettings s) => _globalSettings = s;
        public GlobalSettings OnSaveGlobal() => _globalSettings;
        public SaveSettings _saveSettings { get; set; } = new SaveSettings();
        public void OnLoadLocal(SaveSettings s) => _saveSettings = s;
        public SaveSettings OnSaveLocal() => _saveSettings;
        
        public static  List<Func<HealthManager, bool>> HpListeners = new List<Func<HealthManager, bool>>();
        public static Dictionary<AssetBundle, string[]> CustomAudioBundles = new Dictionary<AssetBundle, string[]>();
        public static Dictionary<AudioClip, string> CustomAudioClips = new Dictionary<AudioClip, string>();
        private static readonly List<string> audioExtentions = new List<string>() { ".mp3" };
        public static string AudioSliderKey = "HKVOCALS_VOL";
        private static string AudioSliderText = "HK Vocals Volume: ";
        public const bool RemoveOrigNPCSounds = true;

        private static int[] GetRange(int start, int end)
        {
            int[] array = new int[end - start + 1];
            for (int i = 0; i < array.Length; i++)
                array[i] = start + i;
            return array;
        }
        public AudioClip GetAudioFor(string convName) => CustomAudioClips.ContainsValue(convName) ? CustomAudioClips.First(a => a.Value == convName).Key : CustomAudioBundles.Any(a => a.Value.Any(s => s == convName)) ? CustomAudioBundles.First(a => a.Value.Any(s => s == convName)).Key.LoadAsset<AudioClip>(convName) : audioBundle.LoadAsset<AudioClip>(convName.Replace("_generic", "_town_generic"));

        public void TryPlayAudioFor(string convName)
        {
            if (HasAudioFor(convName))
            {
                PlayAudioFor(convName);
            }
            
        }
        public bool HasAudioFor(string convName) => CustomAudioBundles.Any(a => a.Value.Contains(convName)) || CustomAudioClips.ContainsValue(convName) || audioNames.Contains(convName.Replace("_GENERIC", "_TOWN_GENERIC"));
        public void PlayAudioFor(string convName) => PlayAudio(GetAudioFor(convName.ToLower()));
        public void PlayAudioFor(string convName, AudioSource asrc) => PlayAudio(GetAudioFor(convName.ToLower()), asrc);
        private void PlayAudio(AudioClip clip)
        {
            if (audioSource == null)
            {
                CreateAudioSource();
            }

            if (HeroController.instance != null)
            {
                audioSource.transform.localPosition = HeroController.instance.transform.localPosition;
            }

            if (Dictionaries.NoAudioMixer.Contains(clip.name))
            {
                audioSource.outputAudioMixerGroup = null;
            }
            else if (!audioSource.outputAudioMixerGroup) // might need to be rewritten if this changes, don't think it does
            {
                audioSource.outputAudioMixerGroup = ObjectPool.instance.startupPools.First(o => o.prefab.name == "Audio Player Actor").prefab.GetComponent<AudioSource>().outputAudioMixerGroup;
            }
            
            audioSource.volume = _globalSettings.Volume/10f;
            audioSource.PlayOneShot(clip, 1f);
        }
        
        private void PlayAudio(AudioClip clip, AudioSource asrc)
        {
            if (asrc == null) return;
            
            if (HeroController.instance != null)
            {
                asrc.transform.localPosition = HeroController.instance.transform.localPosition;
            }

            if (Dictionaries.NoAudioMixer.Contains(clip.name))
            {
                asrc.outputAudioMixerGroup = null;
            }
            else if (!asrc.outputAudioMixerGroup) // might need to be rewritten if this changes, don't think it does
            {
                asrc.outputAudioMixerGroup = ObjectPool.instance.startupPools.First(o => o.prefab.name == "Audio Player Actor").prefab.GetComponent<AudioSource>().outputAudioMixerGroup;
            }
            
            asrc.volume = _globalSettings.Volume/10f;
            asrc.PlayOneShot(clip, 1f);
        }

        public bool IsPlaying() => audioSource.isPlaying;


        public HKVocals() : base("Hollow Knight Vocalized") { }
        public override string GetVersion() => "0.0.0.1";

        public AssetBundle audioBundle;
        public List<string> audioNames = new List<string>();
        public AudioSource audioSource;
        public Coroutine autoTextRoutine;
        internal static HKVocals instance;
        public bool ToggleButtonInsideMenu => false;
        public bool IsGrubRoom = false;
        public string GrubRoom = "Crossroads_48";
        private static GameObject ZoteLever;

        public MenuScreen GetMenuScreen(MenuScreen modListMenu, ModToggleDelegates? toggleDelegates) => ModMenu.CreateModMenuScreen(modListMenu);

        public override void Initialize()
        {
            instance = this;
            On.DialogueBox.ShowPage += ShowPage;
            On.PlayMakerFSM.Awake += FSMAwake;
            On.HutongGames.PlayMaker.Actions.AudioPlayerOneShot.DoPlayRandomClip += PlayRandomClip;
            On.EnemyDreamnailReaction.Start += EDNRStart;
            On.EnemyDreamnailReaction.ShowConvo += ShowConvo;
            On.HealthManager.TakeDamage += TakeDamage;
            UIManager.EditMenus +=  ModMenu.AddAudioSlider;
            ModHooks.LanguageGetHook += AddKey;
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += SceneChange;
            On.BossStatueLever.OnTriggerEnter2D += UseZoteLever;
            ModHooks.HeroUpdateHook += () =>
            {
                //do not question this code please
                if (InputHandler.Instance.inputActions.pause.WasPressed)
                {
                    Log("Break");
                }
            };

            Assembly asm = Assembly.GetExecutingAssembly();
            audioBundle = AssetBundle.LoadFromStream(asm.GetManifestResourceStream("HKVocals.audiobundle"));
            string[] allAssetNames = audioBundle.GetAllAssetNames();
            for (int i = 0; i < allAssetNames.Length; i++)
            {
                if (audioExtentions.Any(ext => allAssetNames[i].EndsWith(ext)))
                {
                    audioNames.Add(Path.GetFileNameWithoutExtension(allAssetNames[i]).ToUpper());
                }
                #if DEBUG
                Log("Object in audiobundle: " + allAssetNames[i] + Path.GetFileNameWithoutExtension(allAssetNames[i]).ToUpper().Replace("KNGHT", "KNIGHT").Replace("_TOWN_GENERIC", "_GENERIC"));
                #endif
            }

            CreateAudioSource();
        }

        private void UseZoteLever(On.BossStatueLever.orig_OnTriggerEnter2D orig, BossStatueLever self, Collider2D collision)
        {
            if (self.gameObject.name != "ZoteLever")
            {
                orig(self, collision);
            }
            else
            {
                if (ZoteLever == null) return;

                if (!ZoteLever.activeInHierarchy || (!collision.CompareTag("Nail Attack"))) return;

                BossStatueLever ZoteLeverComponent = ZoteLever.GetComponent<BossStatueLever>();
                
                
                if(ReflectionHelper.GetField<BossStatueLever, bool>(ZoteLeverComponent, "canToggle") == false) return;
                
                ReflectionHelper.SetField(ZoteLeverComponent, "canToggle", false);
                
                ReflectionHelper.GetField<BossStatueLever, AudioEvent>(ZoteLeverComponent, "switchSound")
                    .SpawnAndPlayOneShot(FSMEdits.GetZoteAudioPlayer(), HeroController.instance.transform.position);
                
                GameManager.instance.FreezeMoment(1);
                GameCameras.instance.cameraShakeFSM.SendEvent("EnemyKillShake");

                GameObject strikeNailPrefab =
                    ReflectionHelper.GetField<BossStatueLever, GameObject>(ZoteLeverComponent, "strikeNailPrefab");
                Transform hitOrigin =
                    ReflectionHelper.GetField<BossStatueLever, Transform>(ZoteLeverComponent, "hitOrigin");
                

                if (strikeNailPrefab && hitOrigin)
                {
                    strikeNailPrefab.Spawn(hitOrigin.transform.position);
                }

                if (!ZoteLeverComponent.leverAnimator) return;
                
                ZoteLeverComponent.leverAnimator.Play("Hit");

                _globalSettings.OrdealZoteSpeak = !_globalSettings.OrdealZoteSpeak;

                GameManager.instance.StartCoroutine(EnableToggle());

                IEnumerator EnableToggle()
                {
                    yield return new WaitForSeconds(1f);
                    ReflectionHelper.SetField(ZoteLeverComponent, "canToggle", true);
                    ZoteLeverComponent.leverAnimator.Play("Shine");
                }
            }
        }
        
        private void SceneChange(Scene from, Scene to)
        {
            FSMEdits.DeleteZoteAudioPlayers();
            if (to.name == "GG_Workshop")
            {
                ZoteLever = GameObject.Instantiate(
                    GameObject.Find("GG_Statue_MantisLords/alt_lever/GG_statue_switch_lever"));

                ZoteLever.transform.position = new Vector3(196.8f, 63.5f, 1);
                ZoteLever.name = "ZoteLever";
                ZoteLever.GetComponent<BossStatueLever>().SetOwner(null);
            }

            IsGrubRoom = (to.name == GrubRoom);
        }

        private void CreateAudioSource()
        {
            GameObject audioGO = new GameObject("HK Vocals Audio");
            audioSource = audioGO.AddComponent<AudioSource>();
            GameObject.DontDestroyOnLoad(audioGO);
        }
        
        private string AddKey(string key, string sheettitle, string orig)
        {
            //We change the key of the object we clone so we also need to tell game what the new key should return
            if (key == AudioSliderKey) return AudioSliderText;
            return orig;
        }

        private void TakeDamage(On.HealthManager.orig_TakeDamage orig, HealthManager self, HitInstance hitInstance)
        {
            orig(self, hitInstance);
            for (int i = 0; i < HpListeners.Count; i++)
            {
                if (HpListeners[i](self))
                {
                    HpListeners.RemoveAt(i);
                    i--;
                }
            }
        }

        private void ShowConvo(On.EnemyDreamnailReaction.orig_ShowConvo orig, EnemyDreamnailReaction self)
        {
            ExDNailReaction ex = self.GetComponent<ExDNailReaction>();
            PlayMakerFSM fsm = FsmVariables.GlobalVariables.GetFsmGameObject("Enemy Dream Msg").Value.LocateMyFSM("Display");
            fsm.FsmVariables.GetFsmGameObject("Last Enemy").Value = self.gameObject;
            fsm.FsmVariables.GetFsmString("Enemy Type").Value = ex ? ex.PDName : "";
            fsm.FsmVariables.GetFsmString("Enemy Variant").Value = ex ? ex.Variation : "";
            fsm.FsmVariables.GetFsmBool("Is Enemy").Value = true;
            orig(self);
        }

        private void EDNRStart(On.EnemyDreamnailReaction.orig_Start orig, EnemyDreamnailReaction self)
        {
            orig(self);
            //if (self.GetComponent<EnemyDeathEffects>() != null)
            self.gameObject.AddComponent<ExDNailReaction>();
        }

        private void PlayRandomClip(On.HutongGames.PlayMaker.Actions.AudioPlayerOneShot.orig_DoPlayRandomClip orig, AudioPlayerOneShot self)
        {
            orig(self);
            if (!RemoveOrigNPCSounds /*&& _globalSettings.testSetting == 0*/ && self.Fsm.Name == "Conversation Control")
            {
                GameManager.instance.StartCoroutine(FadeOutClip(ReflectionHelper.GetField<AudioPlayerOneShot, AudioSource>(self, "audio")));
            }
        }

        private void FSMAwake(On.PlayMakerFSM.orig_Awake orig, PlayMakerFSM self)
        {
            orig(self);
            /*if (self.FsmGlobalTransitions.Any(t => t.EventName.ToLower().Contains("dream")))
            {
                self.MakeLog();
                foreach (FsmTransition t in self.FsmGlobalTransitions)
                    Log(t.EventName);
            }*/
            if (Dictionaries.SceneFSMEdits.TryGetValue((UnityEngine.SceneManagement.SceneManager.GetActiveScene().name, self.gameObject.name, self.FsmName), out var sceneAction))
                sceneAction(self);
            if (Dictionaries.GoFSMEdits.TryGetValue((self.gameObject.name, self.FsmName), out var goAction))
                goAction(self);
            if (Dictionaries.FSMChanges.TryGetValue(self.FsmName, out var action))
                action(self);
            /*if (self.gameObject.name.ToLower().Contains("elderbug"))
            {
                foreach (FsmVar v in self.FsmStates.SelectMany(s => s.Actions.Where(a => a is CallMethodProper call && call.behaviour.Value.ToLower() == "dialoguebox").Cast<CallMethodProper>().SelectMany(c => c.parameters)))
                    Log(v.variableName + "  " + v.Type + "  " + v.GetValue());
            }*/
        }

        private void ShowPage(On.DialogueBox.orig_ShowPage orig, DialogueBox self, int pageNum)
        {
            orig(self, pageNum);
            //Log("Started Conversation " + self.currentConversation + "_" + self.currentPage);
            //if (_globalSettings.testSetting == 0)
            TryPlayAudioFor(self.currentConversation + "_" + (self.currentPage - 1));
            if (audioSource.isPlaying)
            {
                if (autoTextRoutine != null)
                {
                    GameManager.instance.StopCoroutine(autoTextRoutine);
                }
                autoTextRoutine = GameManager.instance.StartCoroutine(AutoChangePage(self));
            }
        }

        private void SetConversation(On.DialogueBox.orig_SetConversation orig, DialogueBox self, string convName,
            string sheetName)
        {
            orig(self, convName, sheetName);
            Log("Started Conversation " + convName + " " + sheetName);
            //if (_globalSettings.testSetting == 0)
            TryPlayAudioFor(convName);
        }

        public void CreateDreamDialogue(string convName, string sheetName, string enemyType = "", string enemyVariant = "", GameObject enemy = null)
        {
            PlayMakerFSM fsm = FsmVariables.GlobalVariables.GetFsmGameObject("Enemy Dream Msg").Value.LocateMyFSM("Display");
            fsm.Fsm.GetFsmString("Convo Title").Value = convName;
            fsm.Fsm.GetFsmString("Sheet").Value = sheetName;
            fsm.FsmVariables.GetFsmString("Enemy Type").Value = enemyType;
            fsm.FsmVariables.GetFsmString("Enemy Variant").Value = enemyVariant;
            fsm.Fsm.GetFsmGameObject("Last Enemy").Value = enemy;
            fsm.Fsm.GetFsmBool("Is Enemy").Value = enemy;
            fsm.SendEvent("DISPLAY DREAM MSG");
        }

        public IEnumerator AutoChangePage(DialogueBox dialogueBox)
        {
            int newPageNum = dialogueBox.currentPage + 1;
            string oldConvoName = dialogueBox.currentConversation;
            yield return new WaitWhile(() => audioSource.isPlaying && dialogueBox && dialogueBox.currentPage < newPageNum && dialogueBox.currentConversation == oldConvoName);
            if (dialogueBox && dialogueBox.currentPage < newPageNum && dialogueBox.currentConversation == oldConvoName)
                dialogueBox.ShowNextPage();
        }

        private IEnumerator FadeOutClip(AudioSource source)
        {
            float volumeChange = source.volume / 100f;
            yield return new WaitForSeconds(1f);
            for (int i = 0; i < 100; i++)
                source.volume -= volumeChange;
        }
    }
}
