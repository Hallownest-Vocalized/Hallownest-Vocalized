namespace HKVocals;

public class GlobalSettings
{
    public int Volume = 10;
    public bool OrdealZoteSpeak = true;
    public bool autoScroll = false; //TODO: test
    public bool dnDialogue = false; //TODO: test
    public bool scrollLock = false; //TODO: test
}
public class SaveSettings
{
    public int GrubConvo = -1;
    public bool ZoteOn = true;
    public bool UnlockedZoteOpt = false;
    public List<string> FinishedConvos = new List<string>();
    public Dictionary<string, int> PersistentVoiceActors = new Dictionary<string, int>();
}
