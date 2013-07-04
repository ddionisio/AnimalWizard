using UnityEngine;
using System.Collections;

public class ModalMainMenu : UIController {
    public UIEventListener play;
    public UIEventListener options;
    public UIEventListener credits;

    protected override void OnActive(bool active) {
        if(active) {
        }
        else {
        }
    }

    protected override void OnOpen() {
    }

    protected override void OnClose() {
    }

    void Awake() {
        play.onClick += OnPlay;
        options.onClick += OnOptions;
        credits.onClick += OnCredits;
    }

    void OnPlay(GameObject go) {
        Main.instance.sceneManager.LoadScene(LevelList.sceneLevelSelect);
    }

    void OnOptions(GameObject go) {
        UIModalManager.instance.ModalOpen("options");
    }

    void OnCredits(GameObject go) {
        UIModalManager.instance.ModalOpen("credits");
    }
}
