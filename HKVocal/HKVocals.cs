using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFCore.Generics;
using Modding;
using UnityEngine;
using System.Reflection;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using System.Collections;
using System.IO;
using SFCore.Utils;

namespace HKVocals
{ 
    public class HKVocals: Mod, IGlobalSettings<GlobalSettings>, ILocalSettings<SaveSettings>, IMenuMod
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
        public const bool RemoveOrigNPCSounds = true;

        private static int[] GetRange(int start, int end)
        {
            int[] array = new int[end - start + 1];
            for (int i = 0; i < array.Length; i++)
                array[i] = start + i;
            return array;
        }
        public AudioClip GetAudioFor(string convName) => CustomAudioClips.ContainsValue(convName) ? CustomAudioClips.First(a => a.Value == convName).Key : CustomAudioBundles.Any(a => a.Value.Any(s => s == convName)) ? CustomAudioBundles.First(a => a.Value.Any(s => s == convName)).Key.LoadAsset<AudioClip>(convName) : audioBundle.LoadAsset<AudioClip>(convName.Replace("_generic", "_town_generic"));
        public void TryPlayAudioFor(string convName) { if (HasAudioFor(convName)) PlayAudioFor(convName); }
        public bool HasAudioFor(string convName) => CustomAudioBundles.Any(a => a.Value.Contains(convName)) || CustomAudioClips.ContainsValue(convName) || audioNames.Contains(convName.Replace("_GENERIC", "_TOWN_GENERIC"));
        public void PlayAudioFor(string convName) => PlayAudio(GetAudioFor(convName.ToLower()));
        private void PlayAudio(AudioClip clip)
        {
            if (!audioSource.transform.parent)
            {
                audioSource.transform.parent = HeroController.instance.transform;
                audioSource.transform.localPosition = Vector3.zero;
            }
            if (!audioSource.outputAudioMixerGroup) // might need to be rewritten if this changes, don't think it does
                audioSource.outputAudioMixerGroup = ObjectPool.instance.startupPools.First(o => o.prefab.name == "Audio Player Actor").prefab.GetComponent<AudioSource>().outputAudioMixerGroup;
            audioSource.volume = _globalSettings.Volume / 100f;
            audioSource.PlayOneShot(clip, 1f);
        }

        public HKVocals() : base("Hollow Knight Vocalized") { }
        public override string GetVersion() => "0.0.0.1";

        public List<IMenuMod.MenuEntry> GetMenuData(IMenuMod.MenuEntry? toggleButtonEntry)
        {
            return new List<IMenuMod.MenuEntry>
            {
                new IMenuMod.MenuEntry("test",
                    new string[] {"On", "Off", "On 2?", "1, 5, or 7"},
                    "description or something",
                    i => _globalSettings.testSetting = i, () => _globalSettings.testSetting),
                //i => { }, () => 0)};
                new IMenuMod.MenuEntry("Volume",
                    GetRange(0, 100).Select(i => i.ToString()).ToArray(),
                    "Controls the Volume of Voice Lines",
                    i => _globalSettings.Volume = i, () => _globalSettings.Volume)
            };
        }

        public AssetBundle audioBundle;
        public List<string> audioNames = new List<string>();
        public NonBouncer coroutineSlave;
        public AudioSource audioSource;
        public Coroutine autoTextRoutine;
        internal static HKVocals instance;
        public bool ToggleButtonInsideMenu => false;

        public override void Initialize()
        {
            instance = this;
            On.DialogueBox.ShowPage += ShowPage;
            On.PlayMakerFSM.Awake += FSMAwake;
            On.HutongGames.PlayMaker.Actions.AudioPlayerOneShot.DoPlayRandomClip += PlayRandomClip;
            On.EnemyDreamnailReaction.Start += EDNRStart;
            On.EnemyDreamnailReaction.ShowConvo += ShowConvo;
            On.HealthManager.TakeDamage += TakeDamage;
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += SceneChanged;

            Assembly asm = Assembly.GetExecutingAssembly();
            audioBundle = AssetBundle.LoadFromStream(asm.GetManifestResourceStream("HKVocals.audiobundle"));
            string[] allAssetNames = audioBundle.GetAllAssetNames();
            for (int i = 0; i < allAssetNames.Length; i++)
            {
                if (audioExtentions.Any(ext => allAssetNames[i].EndsWith(ext)))
                    audioNames.Add(Path.GetFileNameWithoutExtension(allAssetNames[i]).ToUpper());
#if DEBUG
                Log("Object in audiobundle: " + allAssetNames[i] + Path.GetFileNameWithoutExtension(allAssetNames[i]).ToUpper().Replace("KNGHT", "KNIGHT").Replace("_TOWN_GENERIC", "_GENERIC"));
#endif
            }

            GameObject audioGO = new GameObject("HK Vocals Audio");
            audioSource = audioGO.AddComponent<AudioSource>();
            GameObject coroutineGO = new GameObject("HK Vocals Coroutines");
            coroutineSlave = coroutineGO.AddComponent<NonBouncer>();
            GameObject.DontDestroyOnLoad(audioGO);
            GameObject.DontDestroyOnLoad(coroutineGO);
        }

        private void SceneChanged(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.Scene arg1)
        {
            if (arg1.name == "GG_Workshop") coroutineSlave.StartCoroutine(SetUpZoteRoom());
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
                coroutineSlave.StartCoroutine(FadeOutClip(ReflectionHelper.GetField<AudioPlayerOneShot, AudioSource>(self, "audio")));
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
                    coroutineSlave.StopCoroutine(autoTextRoutine);
                autoTextRoutine = coroutineSlave.StartCoroutine(AutoChangePage(self));
            }
        }

        private void SetConversation(On.DialogueBox.orig_SetConversation orig, DialogueBox self, string convName, string sheetName)
        {
            orig(self, convName, sheetName);
            Log("Started Conversation " + convName + " " + sheetName);
            //if (_globalSettings.testSetting == 0)
            TryPlayAudioFor(convName);
        }

        public IEnumerator SetUpZoteRoom()
        {
            yield return null;
            //GameObject zoteLever = GameObject.Instantiate(GameObject.Find(BossStatueLever));
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
