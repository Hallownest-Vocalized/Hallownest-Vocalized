using System;
using System.Collections.Generic;
using System.Linq;
using Modding;
using UnityEngine;
using System.Reflection;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using System.Collections;
using System.Diagnostics;
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
        
        public const bool RemoveOrigNPCSounds = true;

        private static int[] GetRange(int start, int end)
        {
            int[] array = new int[end - start + 1];
            for (int i = 0; i < array.Length; i++)
                array[i] = start + i;
            return array;
        }

        public AudioClip GetAudioFor(string convName)
        {
            if (Dictionaries.CustomAudioClips.ContainsValue(convName))
            {
                return Dictionaries.CustomAudioClips.First(a => a.Value == convName).Key;
            }
            else
            {
                if (Dictionaries.CustomAudioBundles.Any(a => a.Value.Any(s => s == convName)))
                {
                    return Dictionaries.CustomAudioBundles.First(a => a.Value.Any(s => s == convName)).Key
                        .LoadAsset<AudioClip>(convName);
                }
                else
                {
                    return audioBundle.LoadAsset<AudioClip>(convName);
                }
            }
        }

        public void TryPlayAudioFor(string convName, float removeTime = 0f)
        {
            LogDebug($"Trying to play audio for {convName}");
            if (HasAudioFor(convName))
            {
                if (removeTime == 0f)
                {
                    PlayAudioFor(convName);
                }
                else
                {
                    PlayAudioForWithTrim(convName, removeTime);
                }
            }
            else
            {
                LogWarn($"Audio doesn't exits {convName}");
            }

        }

        public bool HasAudioFor(string convName)
        {
            return 
                Dictionaries.CustomAudioBundles.Any(a => a.Value.Contains(convName)) ||
                Dictionaries.CustomAudioClips.ContainsValue(convName) ||
                audioNames.Contains(convName);
        }

        public void PlayAudioFor(string convName) => PlayAudio(GetAudioFor(convName.ToLower())); 
        public void PlayAudioForWithTrim(string convName, float removeTime) => PlayAudioWithTrim(GetAudioFor(convName.ToLower()), removeTime);
        public void PlayAudioFor(string convName, AudioSource asrc) => PlayAudio(GetAudioFor(convName.ToLower()), asrc);
        
        public void PlayAudioWithTrim(AudioClip clip, float removeTime, AudioSource asrc = null)
        {
            LogDebug($"Trimming {clip.name}");
            int remove =(int) (clip.frequency * removeTime);
            int size = clip.samples - remove;
            float[] samples = new float[size * clip.channels];
            clip.GetData(samples, remove);

            AudioClip newclip = AudioClip.Create(clip.name, size , clip.channels, clip.frequency, false);
            newclip.SetData(samples, 0);
                    
            PlayAudio(newclip, asrc);
        }
        private void PlayAudio(AudioClip clip, AudioSource asrc = null)
        {
            //if supplied is null, use default
            if (asrc == null)
            {
                asrc = audioSource;
            }
            //if its still null create new audio source go
            if (asrc == null)
            {
                CreateAudioSource();
            }

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

            if (Dictionaries.SpecialAudios.Keys.Contains(clip.name))
            {
                LogDebug($"Playing {clip.name} (special)");
                asrc.PlayOneShot(clip, Dictionaries.SpecialAudios[clip.name]);
            }
            else
            {
                LogDebug($"Playing {clip.name}");
                asrc.PlayOneShot(clip, 1f);
            }
            LogDebug("");
        }

        public bool IsPlaying() => audioSource.isPlaying;

        public HKVocals() : base("Hollow Knight Vocalized")
        {
            var go = new GameObject("HK Vocals Coroutine Holder");
            CoroutineHolder = go.AddComponent<NonBouncer>();
            Object.DontDestroyOnLoad(CoroutineHolder);
        }
        public override string GetVersion() => "0.0.0.1";

        public AssetBundle audioBundle;
        public List<string> audioNames = new List<string>();
        public AudioSource audioSource;
        public Coroutine autoTextRoutine;
        internal static HKVocals instance;
        public bool ToggleButtonInsideMenu => false;
        public bool IsGrubRoom = false;
        public string GrubRoom = "Crossroads_48";
        public static NonBouncer CoroutineHolder;

        public MenuScreen GetMenuScreen(MenuScreen modListMenu, ModToggleDelegates? toggleDelegates) => ModMenu.CreateModMenuScreen(modListMenu);
        
        public override void Initialize()
        {
            instance = this;
            On.DialogueBox.ShowPage += ShowPage;
            On.DialogueBox.ShowNextPage += StopConvo_NextPage;
            On.DialogueBox.HideText += StopConvo_HideText;
            On.PlayMakerFSM.Awake += FSMAwake;
            On.HutongGames.PlayMaker.Actions.AudioPlayerOneShot.DoPlayRandomClip += PlayRandomClip;
            On.EnemyDreamnailReaction.Start += EDNRStart;
            On.EnemyDreamnailReaction.ShowConvo += ShowConvo;
            On.HealthManager.TakeDamage += TakeDamage;
            UIManager.EditMenus +=  ModMenu.AddAudioSlider;
            
            
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += EternalOrdeal.DeleteZoteAudioPlayersOnSceneChange;
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += ZoteLever.SetZoteLever;
            On.BossStatueLever.OnTriggerEnter2D += ZoteLever.UseZoteLever;

            Assembly asm = Assembly.GetExecutingAssembly();
            audioBundle = AssetBundle.LoadFromStream(asm.GetManifestResourceStream("HKVocals.audiobundle"));
            string[] allAssetNames = audioBundle.GetAllAssetNames();
            for (int i = 0; i < allAssetNames.Length; i++)
            {
                if (Dictionaries.audioExtentions.Any(ext => allAssetNames[i].EndsWith(ext)))
                {
                    audioNames.Add(Path.GetFileNameWithoutExtension(allAssetNames[i]).ToUpper());
                }
                LogDebug($"Object in audiobundle: {allAssetNames[i]} {Path.GetFileNameWithoutExtension(allAssetNames[i])?.ToUpper().Replace("KNGHT", "KNIGHT")}");
            }

            CreateAudioSource();
        }

        private void StopConvo_HideText(On.DialogueBox.orig_HideText orig, DialogueBox self)
        {
            audioSource.Stop();
            orig(self);
        }

        private void StopConvo_NextPage(On.DialogueBox.orig_ShowNextPage orig, DialogueBox self)
        {
            audioSource.Stop();
            orig(self);
        }

        private void CreateAudioSource()
        {
            Log("creating asrc");
            GameObject audioGO = new GameObject("HK Vocals Audio");
            audioSource = audioGO.AddComponent<AudioSource>();
            Object.DontDestroyOnLoad(audioGO);
        }

        private void TakeDamage(On.HealthManager.orig_TakeDamage orig, HealthManager self, HitInstance hitInstance)
        {
            orig(self, hitInstance);
            for (int i = 0; i < Dictionaries.HpListeners.Count; i++)
            {
                if (Dictionaries.HpListeners[i](self))
                {
                    Dictionaries.HpListeners.RemoveAt(i);
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
                HKVocals.CoroutineHolder.StartCoroutine(FadeOutClip(ReflectionHelper.GetField<AudioPlayerOneShot, AudioSource>(self, "audio")));
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
            var convo = self.currentConversation + "_" + (self.currentPage - 1);
            LogDebug($"Showing page in {convo}");
            if (self.currentPage - 1 == 0)
            {
                TryPlayAudioFor(convo,37f/60f);
            }
            else
            {
                TryPlayAudioFor(convo, 3f/4f);
            }
            if (audioSource.isPlaying)
            {
                if (autoTextRoutine != null)
                {
                    HKVocals.CoroutineHolder.StopCoroutine(autoTextRoutine);
                }
                autoTextRoutine = HKVocals.CoroutineHolder.StartCoroutine(AutoChangePage(self));
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
            yield return new WaitWhile(() => IsPlaying() && dialogueBox && dialogueBox.currentPage < newPageNum && dialogueBox.currentConversation == oldConvoName);
            yield return new WaitForSeconds(1f/6f);//wait additional 1/6th second
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
