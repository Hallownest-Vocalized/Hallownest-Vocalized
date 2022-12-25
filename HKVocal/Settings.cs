namespace HKVocals;

public class GlobalSettings
{
    public int Volume = 10;
    public bool OrdealZoteSpeak = true;
    public bool autoScroll = false;
    public MajorFeatures.AutoScroll.ScrollSpeed ScrollSpeed = MajorFeatures.AutoScroll.ScrollSpeed.Normal;
    public bool dnDialogue = false;
    public bool automaticBossDialogue = false;
    public bool scrollLock = false;
    public bool dampenAudio = true;
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
