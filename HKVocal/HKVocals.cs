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
    public class HKVocalsGD
    {
        public int testSetting = 0;
        public int Volume = 50;
    }
    public class HKVocalsPD
    {
        public bool ZoteOn = true;
        public bool UnlockedZoteOpt = false;
    }
    public partial class HKVocals : FullSettingsMod<HKVocalsPD, HKVocalsGD>, IMenuMod
    {
        public static Dictionary<AssetBundle, string[]> CustomAudioBundles = new Dictionary<AssetBundle, string[]>();
        public static Dictionary<AudioClip, string> CustomAudioClips = new Dictionary<AudioClip, string>();
        private static readonly List<string> audioExtentions = new List<string>() { ".mp3" };
        private const bool RemoveOrigNPCSounds = true;

        private static int[] GetRange(int start, int end)
        {
            int[] array = new int[end - start + 1];
            for (int i = 0; i < array.Length; i++)
                array[i] = start + i;
            return array;
        }
        private AudioClip GetAudioFor(string convName) => CustomAudioClips.ContainsValue(convName) ? CustomAudioClips.First(a => a.Value == convName).Key : CustomAudioBundles.Any(a => a.Value.Any(s => s == convName)) ? CustomAudioBundles.First(a => a.Value.Any(s => s == convName)).Key.LoadAsset<AudioClip>(convName) : audioBundle.LoadAsset<AudioClip>(convName.Replace("_generic", "_town_generic"));
        private void TryPlayAudioFor(string convName) { if (HasAudioFor(convName)) PlayAudioFor(convName); }
        private bool HasAudioFor(string convName) => CustomAudioBundles.Any(a => a.Value.Contains(convName)) || CustomAudioClips.ContainsValue(convName) || audioNames.Contains(convName.Replace("_GENERIC", "_TOWN_GENERIC"));
        private void PlayAudioFor(string convName) => PlayAudio(GetAudioFor(convName.ToLower()));
        private void PlayAudio(AudioClip clip)
        {
            if (HeroController.instance)
                audioSource.transform.position = HeroController.instance.transform.position;
            audioSource.volume = _globalSettings.Volume / 100f;
            audioSource.PlayOneShot(clip, 1f);
        }

        public HKVocals() : base("Hollow Knight Vocalized") { }
        public override string GetVersion() => "0.0.0.1";

        public List<IMenuMod.MenuEntry> GetMenuData(IMenuMod.MenuEntry? toggleButtonEntry)
            => new List<IMenuMod.MenuEntry>(){
                new IMenuMod.MenuEntry("test",
                    new string[] { "On", "Off", "On 2?", "1, 5, or 7"},
                    "description or something",
                    i => _globalSettings.testSetting = i, () => _globalSettings.testSetting),
                    //i => { }, () => 0)};
                new IMenuMod.MenuEntry("Volume",
                    GetRange(0, 100).Select(i => i.ToString()).ToArray(),
                    "Controls the Volume of Voice Lines",
                    i => _globalSettings.Volume = i, () => _globalSettings.Volume)};

        private AssetBundle audioBundle;
        private List<string> audioNames = new List<string>();
        private NonBouncer coroutineSlave;
        private AudioSource audioSource;
        private Coroutine autoTextRoutine;
        private static HKVocals instance;
        public bool ToggleButtonInsideMenu => false;

        public override void Initialize()
        {
            if (_globalSettings == null)
                _globalSettings = new HKVocalsGD();
            foreach (string s in PlayMakerGlobals.Instance.Events)
            {
                Log(s);
            }
            SaveGlobalSettings();
            instance = this;
            //On.AudioManager.ApplyMusicCue += AudioManager_ApplyMusicCue;
            //.AudioManager.ApplyAtmosCue += AudioManager_ApplyAtmosCue;
            //var infos = (MusicCue.MusicChannelInfo[])musicCue.GetType().GetField("channelInfos", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(musicCue);
            //On.DialogueBox.SetConversation += SetConversation;
            On.DialogueBox.ShowPage += ShowPage;
            On.PlayMakerFSM.Awake += FSMAwake;
            On.HutongGames.PlayMaker.Actions.AudioPlayerOneShot.DoPlayRandomClip += PlayRandomClip;
            On.EnemyDreamnailReaction.Start += EDNRStart;
            On.EnemyDreamnailReaction.ShowConvo += ShowConvo;
            On.HealthManager.TakeDamage += TakeDamage;
            ModHooks.AfterSavegameLoadHook += AfterSaveLoad;
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

        private void AudioManager_ApplyAtmosCue(On.AudioManager.orig_ApplyAtmosCue orig, AudioManager self, AtmosCue atmosCue, float transitionTime)
        {
            Log(atmosCue.Snapshot.name);
            orig(self, atmosCue, transitionTime);
        }

        private void AudioManager_ApplyMusicCue(On.AudioManager.orig_ApplyMusicCue orig, AudioManager self, MusicCue musicCue, float delayTime, float transitionTime, bool applySnapshot)
        {
            var infos = (MusicCue.MusicChannelInfo[])musicCue.GetType().GetField("channelInfos", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(musicCue);
            foreach (MusicCue.MusicChannelInfo mcmci in infos)
            {
                Log(mcmci.Clip.name);
            }
            orig(self, musicCue, delayTime, transitionTime, applySnapshot);
        }

        private void SceneChanged(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.Scene arg1)
        {
            if (arg1.name == "GG_Workshop") coroutineSlave.StartCoroutine(SetUpZoteRoom());
        }

        private void AfterSaveLoad(SaveGameData obj)
        {
            if (_saveSettings == null)
                _saveSettings = new HKVocalsPD();
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
            if (SceneFSMEdits.TryGetValue((UnityEngine.SceneManagement.SceneManager.GetActiveScene().name, self.gameObject.name, self.FsmName), out var sceneAction))
                sceneAction(self);
            if (GoFSMEdits.TryGetValue((self.gameObject.name, self.FsmName), out var goAction))
                goAction(self);
            if (FSMEdits.TryGetValue(self.FsmName, out var action))
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

        private IEnumerator SetUpZoteRoom()
        {
            yield return null;
            //GameObject zoteLever = GameObject.Instantiate(GameObject.Find(BossStatueLever));
        }

        private void CreateDreamDialogue(string convName, string sheetName, string enemyType = "", string enemyVariant = "", GameObject enemy = null)
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

        private IEnumerator AutoChangePage(DialogueBox dialogueBox)
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

        public class DreamDialogueAction : FsmStateAction
        {
            public string convName { get => convNames[0]; set => convNames = new string[] { value }; }
            public string sheetName { get => sheetNames[0]; set => sheetNames = new string[] { value }; }
            public string[] convNames;
            public string[] sheetNames;
            public float waitTime = 0;
            public ConvoMode convoMode = ConvoMode.Once;
            public int[] convoOccurances = new int[] { 0 };
            public bool isEnemy = true;
            private int lastIndex = int.MaxValue;
            private int realLastIndex = -1;
            public DreamDialogueAction(string convName, string sheetName)
            {
                this.convName = convName;
                this.sheetName = sheetName;
            }
            public DreamDialogueAction(string[] convNames, string sheetName)
            {
                this.convNames = convNames;
                this.sheetName = sheetName;
            }
            public DreamDialogueAction(List<(string, string)> Conversations)
            {
                convNames = Conversations.Select(tup => tup.Item1).ToArray();
                sheetNames = Conversations.Select(tup => tup.Item2).ToArray();
            }
            public override void OnEnter()
            {
                //if (_globalSettings.testSetting == 0)
                StartShow();
                if (Fsm != null)
                    Finish();
            }
            private void StartShow()
            {
                realLastIndex++;
                int currentOccurance;
                if (convoOccurances.Length <= realLastIndex)
                    currentOccurance = convoOccurances.Last();
                else
                    currentOccurance = convoOccurances[realLastIndex];
                if (currentOccurance == -1)
                    return;
                if (waitTime == 0)
                    ShowDialogue();
                else
                    instance.coroutineSlave.StartCoroutine(WaitShowDialogue());
            }
            private IEnumerator WaitShowDialogue()
            {
                yield return new WaitForSeconds(waitTime);
                ShowDialogue();
            }
            private void ShowDialogue()
            {
                switch (convoMode)
                {
                    case ConvoMode.Once:
                        if (lastIndex > convNames.Length - 2)
                            return;
                        if (lastIndex == int.MaxValue || lastIndex < -1)
                            lastIndex = -1;
                        lastIndex++;
                        break;
                    case ConvoMode.Repeat:
                        if (lastIndex == int.MaxValue || lastIndex > convNames.Length - 2 || lastIndex < -1)
                            lastIndex = -1;
                        lastIndex++;
                        break;
                    case ConvoMode.Random:
                        lastIndex = UnityEngine.Random.Range(0, convNames.Length);
                        break;
                }
                instance.CreateDreamDialogue(convNames.Length > lastIndex ? convNames[lastIndex] : convNames.Last(), sheetNames.Length == 0 ? "Enemy Dreams" : sheetNames.Length > lastIndex ? sheetNames[lastIndex] : sheetNames.Last(), "", "", isEnemy ? Owner : null);
            }
            public enum ConvoMode
            {
                Once,
                Repeat,
                Random
            }
        }
        public class ExDNailReaction : MonoBehaviour
        {
            internal string Variation = "";
            internal string PDName = "";
            private EnemyDeathEffects ede;
            private void Awake()
            {
                ede = GetComponent<EnemyDeathEffects>();
                if (ede)
                {
                    PDName = ede.GetAttr<EnemyDeathEffects, string>("playerDataName");
                    Variation = EnemyVariants.ContainsKey(PDName) ? EnemyVariants[PDName][UnityEngine.Random.Range(0, EnemyVariants[PDName].Length)] : "";
                }
            }
            private void OnDestroy()
            {
                PlayMakerFSM fsm = FsmVariables.GlobalVariables.GetFsmGameObject("Enemy Dream Msg").Value.LocateMyFSM("Display");
                if (fsm.FsmVariables.GetFsmGameObject("Last Enemy").Value == gameObject)
                    fsm.SendEvent("CANCEL ENEMY DREAM");
            }
        }
    }
}
