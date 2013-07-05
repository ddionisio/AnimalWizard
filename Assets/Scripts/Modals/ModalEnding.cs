using UnityEngine;
using System.Collections;

public class ModalEnding : UIController {
    public UIEventListener click;

    protected override void OnActive(bool active) {
        if(active) {
            click.onClick += OnClick;
        }
        else {
            click.onClick -= OnClick;
        }
    }

    protected override void OnOpen() {
    }

    protected override void OnClose() {
    }

    void Awake() {
    }

    void OnClick(GameObject go) {
        Main.instance.sceneManager.LoadScene("start");
    }
}
