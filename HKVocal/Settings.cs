namespace HKVocals;

public class GlobalSettings
{
    public int volume = 10;
    public bool ordealZoteSpeak = true;
    public bool autoScroll = false;
    public MajorFeatures.AutoScroll.ScrollSpeed scrollSpeed = MajorFeatures.AutoScroll.ScrollSpeed.Normal;
    public bool dnDialogue = false;
    public bool automaticBossDialogue = false;
    public bool scrollLock = false;
    public bool dampenAudio = true;
    public bool settingsOpened = false;
}
public class SaveSettings
{
    public int LurkerState = 0;
    public int OrdealFails = 0;
    public int Precepts = 1;
    public List<string> FinshedOrdealLines = new List<string>();
    public int GrubConvo = -1;
    public List<string> FinishedConvos = new List<string>();
    public Dictionary<string, int> PersistentVoiceActors = new Dictionary<string, int>();
}
