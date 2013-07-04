using UnityEngine;
using System.Collections;

public class ModalPause : UIController {
    public UIEventListener resume;
    public UIEventListener restart;
    public UIEventListener options;
    public UIEventListener exit;

    protected override void OnActive(bool active) {
        if(active) {
            resume.onClick = OnResumeClick;
            restart.onClick = OnRestartClick;
            options.onClick = OnOptionsClick;
            exit.onClick = OnExitClick;
        }
        else {
            resume.onClick = null;
            restart.onClick = null;
            options.onClick = null;
            exit.onClick = null;
        }
    }

    protected override void OnOpen() {
        Main.instance.sceneManager.Pause();
    }

    protected override void OnClose() {
        Main.instance.sceneManager.Resume();
    }

    void Awake() {
    }

    void OnResumeClick(GameObject go) {
        UIModalManager.instance.ModalCloseTop();
    }

    void OnRestartClick(GameObject go) {
        UIModalConfirm.Open("RESTART", null,
            delegate(bool yes) {
                if(yes)
                    Main.instance.sceneManager.Reload();
            });
    }

    void OnOptionsClick(GameObject go) {
        UIModalManager.instance.ModalOpen("options");
    }

    void OnExitClick(GameObject go) {
        UIModalConfirm.Open("EXIT", null,
            delegate(bool yes) {
                if(yes)
                    Main.instance.sceneManager.LoadScene(LevelList.sceneLevelSelect);
            });
    }
}
