using UnityEngine;
using System.Collections;

public class ModalCredits : UIController {
    public UIEventListener back;

    protected override void OnActive(bool active) {
        if(active) {
            back.onClick = OnBackClick;
        }
        else {
            back.onClick = null;
        }
    }

    protected override void OnOpen() {
    }

    protected override void OnClose() {
    }

    void Awake() {
    }

    void OnBackClick(GameObject go) {
        UIModalManager.instance.ModalCloseTop();
    }
}
