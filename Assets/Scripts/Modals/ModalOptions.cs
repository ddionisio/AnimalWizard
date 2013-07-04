using UnityEngine;
using System.Collections;

public class ModalOptions : UIController {
    public UIEventListener back;

    public UISlider music;
    public UISlider sound;

    protected override void OnActive(bool active) {
        if(active) {
            back.onClick = OnBackClick;
            music.onValueChange = OnMusicValueChange;
            sound.onValueChange = OnSoundValueChange;
        }
        else {
            back.onClick = null;
            music.onValueChange = null;
            sound.onValueChange = null;
        }
    }

    protected override void OnOpen() {
        music.sliderValue = Main.instance.userSettings.musicVolume;
        sound.sliderValue = Main.instance.userSettings.soundVolume;
    }

    protected override void OnClose() {
    }

    void Awake() {
    }

    void OnBackClick(GameObject go) {
        UIModalManager.instance.ModalCloseTop();
    }

    void OnSoundValueChange(float val) {
        Main.instance.userSettings.soundVolume = val;
    }

    void OnMusicValueChange(float val) {
        Main.instance.userSettings.musicVolume = val;
    }
}
