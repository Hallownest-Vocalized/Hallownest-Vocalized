using System;
using Modding.Menu;
using Modding.Menu.Config;
using UnityEngine;
using UnityEngine.UI;

namespace HKVocals
{
    public static class ModMenu
    {
        public static MenuScreen CreateModMenuScreen(MenuScreen modListMenu)
        {
            return new MenuBuilder(UIManager.instance.UICanvas.gameObject, "HKVocal")
                .CreateTitle("HK Vocal", MenuTitleStyle.vanillaStyle)
                .CreateContentPane(RectTransformData.FromSizeAndPos(
                    new RelVector2(new Vector2(1920f, 903f)),
                    new AnchoredPosition(
                        new Vector2(0.5f, 0.5f),
                        new Vector2(0.5f, 0.5f),
                        new Vector2(0f, -60f)
                    )
                ))
                .CreateControlPane(RectTransformData.FromSizeAndPos(
                    new RelVector2(new Vector2(1920f, 259f)),
                    new AnchoredPosition(
                        new Vector2(0.5f, 0.5f),
                        new Vector2(0.5f, 0.5f),
                        new Vector2(0f, -502f)
                    )
                ))
                .SetDefaultNavGraph(new ChainedNavGraph())
                .AddContent(
                    RegularGridLayout.CreateVerticalLayout(105f),
                    c =>
                    {
                        c.AddTextPanel
                        ("AudioMenuText",
                            new RelVector2(new Vector2(500,200)),
                            new TextPanelConfig
                            {
                                Text = "To change Volume go to audio menu",
                                Size = 46,
                                Anchor = TextAnchor.MiddleCenter,
                                Font = TextPanelConfig.TextFont.TrajanRegular
                            });
                    }).AddControls(
                    new SingleContentLayout(new AnchoredPosition(
                        new Vector2(0.5f, 0.5f),
                        new Vector2(0.5f, 0.5f),
                        new Vector2(0f, -64f)
                    )), c => c.AddMenuButton(
                        "BackButton",
                        new MenuButtonConfig
                        {
                            Label = "Back",
                            CancelAction = _ => UIManager.instance.UIGoToDynamicMenu(modListMenu),
                            SubmitAction = _ => UIManager.instance.UIGoToDynamicMenu(modListMenu),
                            Style = MenuButtonStyle.VanillaStyle,
                            Proceed = true
                        })).Build();
        }

        public static void AddAudioSlider()
        {
            //get go of a current slider
            GameObject MusicSlider = UIManager.instance.gameObject.transform.Find("UICanvas/AudioMenuScreen/Content/MusicVolume/MusicSlider").gameObject;
            
            //make clone
            GameObject VolumeSlider = GameObject.Instantiate(MusicSlider, MusicSlider.transform.parent);
            
            MenuAudioSlider VolumeSlider_MenuAudioSlider = VolumeSlider.GetComponent<MenuAudioSlider>();
            Slider VolumeSlider_Slider = VolumeSlider.GetComponent<Slider>();
            
            //all the other sliders are 0.6 down from each other
            VolumeSlider.transform.position -= new Vector3(0, 0.6f, 0f);
            VolumeSlider.name = "HkVocalsSlider";

            Action<float> StoreValue = f =>
            {
                VolumeSlider_MenuAudioSlider.UpdateTextValue(f);
                HKVocals._globalSettings.Volume = (int)f;
            };

            // stuff to happen whenever slider is moved
            var SliderEvent = new Slider.SliderEvent();
            SliderEvent.AddListener(StoreValue.Invoke);

            VolumeSlider_Slider.onValueChanged = SliderEvent ;

            //change the key of the text so it can be changed
            VolumeSlider.transform.Find("Label").GetComponent<AutoLocalizeTextUI>().textKey = HKVocals.AudioSliderKey;
            VolumeSlider.SetActive(true);
            
            //to make sure when go is cloned, it gets the value of the previous session not the value of the music slider
            VolumeSlider_MenuAudioSlider.UpdateTextValue(HKVocals._globalSettings.Volume);
            VolumeSlider_Slider.value = HKVocals._globalSettings.Volume;
        }
    }
}