using GlobalEnums;
using UMenuButton = UnityEngine.UI.MenuButton;
using MenuButton = Satchel.BetterMenus.MenuButton;

namespace HKVocals.UI;
public static class ModMenu
{
    public static Menu MenuRef;
    
    public static MenuScreen CreateModMenuScreen(MenuScreen modListMenu)
    {
        MenuRef ??= new Menu("Hallownest Vocalized", new Element[]
        {
            //new TextPanel("To change volume, please use Audio Menu"),
            new MenuButton("Change Volume", "Change volume of voice actors", _ =>
            {
                UIManager.instance.UILeaveDynamicMenu(RefVanillaMenu.AudioMenuScreen, MainMenuState.AUDIO_MENU);
            }, proceed:true),

            Blueprints.HorizontalBoolOption("Scroll Lock", 
                "Should first time dialogues be scroll locked until audio has finished?",
                (i) => HKVocals._globalSettings.scrollLock = i,
                () => HKVocals._globalSettings.scrollLock),
            
            Blueprints.HorizontalBoolOption("Auto Scroll", 
                "Should dialogue autoscroll after the audio finishes?",
                (i) => 
                    {
                        HKVocals._globalSettings.autoScroll = i;
                        MenuRef.Find("Auto Scroll Speed").isVisible = i;
                        MenuRef.Update();
                    }
                ,
                () => HKVocals._globalSettings.autoScroll),
            
            new HorizontalOption("Auto Scroll Speed", 
                "How fast should it autoscroll after audio finishes playing?",
                Enum.GetNames(typeof(MajorFeatures.AutoScroll.ScrollSpeed)).ToArray(),
                (i) => HKVocals._globalSettings.scrollSpeed = (MajorFeatures.AutoScroll.ScrollSpeed) i,
                () => (int) HKVocals._globalSettings.scrollSpeed,
                Id: "Auto Scroll Speed")
                {
                    isVisible = HKVocals._globalSettings.autoScroll
                },
            
            Blueprints.HorizontalBoolOption("Dream Nail Dialogue", 
                "Should dream nail dialogue be voiced?",
                (i) =>
                {
                    HKVocals._globalSettings.dnDialogue = i;
                    MenuRef.Find("Automatic Boss Dialogue").isVisible = !i;
                    MenuRef.Update();
                },
                () => HKVocals._globalSettings.dnDialogue),
            
            new HorizontalOption("Automatic Boss Shouts",
                "Should some bosses automatically do shouts?",
                new []{"On", "Off"},
                i => HKVocals._globalSettings.automaticBossDialogue = i == 0,
                () => HKVocals._globalSettings.automaticBossDialogue ? 0 : 1,
                Id: "Automatic Boss Dialogue")
            {
                isVisible = !HKVocals._globalSettings.dnDialogue
            },
            
            Blueprints.HorizontalBoolOption("Dampen Audio",
                "Should audio be dampened when audio is played?",
                i =>
                {
                    HKVocals._globalSettings.dampenAudio = i;
                    if (!HKVocals._globalSettings.dampenAudio)
                    {
                        MajorFeatures.DampenAudio.ForceStopDampenAudio();
                    }
                },
                () => HKVocals._globalSettings.dampenAudio),
            
            Blueprints.HorizontalBoolOption("Shop",
                "Should shops be voiced?",
                i =>
                {
                    HKVocals._globalSettings.shopAudio = i;
                },
                () => HKVocals._globalSettings.shopAudio),
            
            Blueprints.HorizontalBoolOption("Inventory",
                "Should the inventory be voiced?",
                i =>
                {
                    HKVocals._globalSettings.invAudio = i;
                },
                () => HKVocals._globalSettings.invAudio),
            
            new MenuButton("Join the HNVocals Discord!","",(_)=>
            {
                Application.OpenURL("https://discord.gg/p72F2St3RF");
            }),
        });
        return MenuRef.GetMenuScreen(modListMenu);
    }

    public static void GoToModMenu()
    {
        // we cant garuntee our modmenu has been created yet so our MenuRef.menuScreen might be null. hence we manually find modist menu and use that
        UIManager.instance.UIGoToDynamicMenu(ModMenu.MenuRef.menuScreen != null 
            ? ModMenu.MenuRef.menuScreen 
            : ModMenu.MenuRef.GetMenuScreen(UIManager.instance.UICanvas.Find("ModListMenu").GetComponent<MenuScreen>()));
    }
}
