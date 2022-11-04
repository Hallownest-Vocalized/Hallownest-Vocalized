namespace HKVocals.MajorFeatures;

public class UITextAudio
{
    private static bool HunterNotesUnlocked = true;
    public static bool OpenedShopMenu = false;
    public static bool OpenInvMenu = false;
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
        fsm.AddMethod("Change Text", () => { fsm.PlayUIText("Convo Desc"); });
    }

    public static void PlayInventoryText(PlayMakerFSM fsm)
    {
        fsm.AddMethod("Change Text", () => { fsm.PlayUIText("Convo Desc"); });
    }
    
    public static void PlayJournalText(PlayMakerFSM fsm)
    {
        fsm.AddMethod("Get Details", () =>
        {
            HKVocals.CoroutineHolder.StopCoroutine(JournalWait());
            HKVocals.CoroutineHolder.StopCoroutine(JournalText(fsm));
            HKVocals.CoroutineHolder.StartCoroutine(JournalWait());
            fsm.PlayUIText("Item Desc Convo");
        });

        fsm.AddMethod("Display Kills", () => { HunterNotesUnlocked = false; } );

        IEnumerator JournalWait()
        {
            yield return new WaitForSeconds(1.5f);
            HKVocals.CoroutineHolder.StartCoroutine(JournalText(fsm));
            HKVocals.CoroutineHolder.StopCoroutine(JournalWait());
        }

        IEnumerator JournalText(PlayMakerFSM fsm)
        {
            yield return new WaitWhile(AudioUtils.IsPlaying);
            if (HunterNotesUnlocked == false)
            {
                HunterNotesUnlocked = true;
            }
            else if (HunterNotesUnlocked) 
            {
                if (InvMenuClosed)
                {
                    HKVocals.instance.audioSource.Stop();
                } else
                {
                    fsm.PlayUIText("Item Notes Convo");
                }
            }
            HKVocals.CoroutineHolder.StopCoroutine(JournalText(fsm));
        }

    }

    
    public static void ShopMenuOpenClose(PlayMakerFSM fsm)
    {
        //Checks when you open the shop keeper menu
        fsm.AddMethod("Open Window", () => { OpenedShopMenu = true; });
        //Checks when you close a shop keeper menu
        fsm.AddMethod("Down", () => { AudioUtils.StopPlaying(); });
    }

    public static void InventoryOpenClose(PlayMakerFSM fsm)
    {
        fsm.AddMethod("Open", () => { OpenInvMenu = true; InvMenuClosed = false; });
        fsm.AddMethod("Close", () => { AudioUtils.StopPlaying(); InvMenuClosed = true; });
    }

    public static void PlayShopText(PlayMakerFSM fsm)
    {
        fsm.AddMethod("Get Details Init", () => { fsm.PlayUIText("Item Desc Convo"); });
        fsm.AddMethod("Get Details", () => { fsm.PlayUIText("Item Desc Convo"); });
    }
}