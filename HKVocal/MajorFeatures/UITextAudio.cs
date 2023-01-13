namespace HKVocals.MajorFeatures;
public class UITextAudio
{
    private static bool HunterNotesUnlocked = true;
    public static bool OpenShopMenu = false;
    public static bool OpenInvMenu = false;
    public static bool ShopMenuClosed = true;
    public static bool InvMenuClosed = true;
    
    public static void Hook()
    {
        FSMEditData.AddGameObjectFsmEdit("Enemy List", "Item List Control", PlayJournalText );
        FSMEditData.AddGameObjectFsmEdit ("Inv", "Update Text", PlayInventoryText );
        FSMEditData.AddGameObjectFsmEdit ("Charms", "Update Text", PlayCharmText );
        FSMEditData.AddGameObjectFsmEdit ("Item List", "Item List Control", PlayShopText );
        FSMEditData.AddGameObjectFsmEdit ("Shop Menu", "shop_control", ShopMenuOpenClose );
        FSMEditData.AddGameObjectFsmEdit ("Inventory", "Inventory Control", InventoryOpenClose );
    }
    
    public static void PlayCharmText(PlayMakerFSM fsm)
    {
        fsm.AddFsmMethod("Change Text", () => { fsm.PlayUIText("Convo Desc", UIAudioType.Other); });
    }

    public static void PlayInventoryText(PlayMakerFSM fsm)
    {
        fsm.AddFsmMethod("Change Text", () => { fsm.PlayUIText("Convo Desc", UIAudioType.Other); });
    }
    
    public static void PlayJournalText(PlayMakerFSM fsm)
    {
        fsm.AddFsmMethod("Get Details", () =>
        {
            AudioPlayer.StopPlaying();
            HKVocals.CoroutineHolder.StartCoroutine(JournalText(fsm));
            HKVocals.instance.Log("generic audio for " + fsm.FsmVariables.GetFsmString("Item Desc Convo").Value + "_0");
            /*fsm.PlayUIText("Item Desc Convo", UIAudioType.Other);*/
        });

        fsm.AddFsmMethod("Display Kills", () => { HunterNotesUnlocked = false; } );
        fsm.AddFsmMethod("Get Notes", () => { HunterNotesUnlocked = true; } );

        IEnumerator JournalText(PlayMakerFSM fsm)
        {
            fsm.PlayUIText("Item Desc Convo", UIAudioType.Other);
            yield return new WaitForSeconds(2f);
            yield return new WaitUntil(AudioPlayer.IsPlaying);
            if (HunterNotesUnlocked == false)
            {
                yield break;
            }
            else if (HunterNotesUnlocked == true) 
            {
                if (InvMenuClosed == true)
                {
                    AudioPlayer.StopPlaying();
                } 
                else
                {
                    yield return new WaitWhile(AudioPlayer.IsPlaying);
                    yield return new WaitForSeconds(1f);
                    fsm.PlayUIText("Item Notes Convo", UIAudioType.Other);
                    HKVocals.instance.Log("hunter note audio for " + fsm.FsmVariables.GetFsmString("Item Notes Convo").Value + "_0");
                }
            }
        }

    }

    
    public static void ShopMenuOpenClose(PlayMakerFSM fsm)
    {
        //Checks when you open the shop keeper menu
        fsm.AddFsmMethod("Open Window", () => { OpenShopMenu = true; ShopMenuClosed = false; });
        //Checks when you close a shop keeper menu
        fsm.AddFsmMethod("Down", () => { AudioPlayer.StopPlaying(); ShopMenuClosed = true; OpenShopMenu = false; });
    }

    public static void InventoryOpenClose(PlayMakerFSM fsm)
    {
        fsm.AddFsmMethod("Open", () => { OpenInvMenu = true; InvMenuClosed = false; });
        fsm.AddFsmMethod("Close", () => { AudioPlayer.StopPlaying(); InvMenuClosed = true; OpenInvMenu = false; });
    }

    public static void PlayShopText(PlayMakerFSM fsm)
    {
        fsm.AddFsmMethod("Get Details Init", () => { fsm.PlayUIText("Item Desc Convo", UIAudioType.Shop); });
        fsm.AddFsmMethod("Get Details", () => { fsm.PlayUIText("Item Desc Convo", UIAudioType.Shop); });
    }
}

public enum UIAudioType
{
    Shop,
    Other
}