using UnityEngine;
using System.Collections;

public class Goal : MonoBehaviour {
    public Transform goalHolder; //put goal icons in here to be activated
    public GameObject goalFindMore; //when goal hasn't been met

    void OnTriggerEnter(Collider col) {
        Player player = col.GetComponent<Player>();
        if(player != null) {
            if(ApplyGoals(player)) {
                goalFindMore.SetActive(false);

                player.state = Player.StateVictory;

                //open victory dialog
                UIModalManager.instance.ModalOpen("victory");
            }
            else {
                goalFindMore.SetActive(true);
            }
        }
    }

    void OnTriggerExit(Collider col) {
        Player player = col.GetComponent<Player>();
        if(player != null) {
            goalFindMore.SetActive(false);
        }
    }

    void Awake() {
        //disable goals
        foreach(Transform t in goalHolder) {
            t.gameObject.SetActive(false);
        }

        goalFindMore.SetActive(false);
    }

    //true if all goals are met
    private bool ApplyGoals(Player player) {
        bool ret = player.collected >= goalHolder.childCount;

        if(ret) {
            foreach(Transform t in goalHolder) {
                t.gameObject.SetActive(true);
            }
        }
        
        return ret;
    }
}
