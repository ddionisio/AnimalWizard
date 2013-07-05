using UnityEngine;
using System.Collections;

public class Collectible : MonoBehaviour, IActionStateListener {
    public enum State {
        Invalid,

        Active,
        Collected
    }

    public enum Type {
        Goal,
        Animal
    }

    public Type type;
    public string animalType; //if type is animal

    private State mState = State.Active;

    private bool mActiveWait = false;

    void OnTriggerEnter(Collider col) {
        if(mActiveWait)
            return;

        if(mState == State.Active) { //shouldn't get here if not active...

            Player player = col.GetComponent<Player>();
            if(player != null) {
                SoundPlayerGlobal.instance.Play("collect");

                mState = State.Collected;
                gameObject.SetActive(false);
                
                switch(type) {
                    case Type.Goal:
                        player.AddCollect();
                        break;

                    case Type.Animal:
                        player.AddSummonCount(animalType);
                        break;
                }
            }
        }
    }

    void OnDestroy() {
        if(ActionStateManager.instance != null)
            ActionStateManager.instance.Unregister(this);
    }

    void Start() {
        if(ActionStateManager.instance != null)
            ActionStateManager.instance.Register(this);
    }

    public object ActionSave() {
        if(gameObject.activeSelf) {
            return mState;
        }
        else
            return null;
    }

    public void ActionRestore(object dat) {
        State newState = (State)dat;
        if(mState != newState) {
            State prevState = mState;
            mState = newState;

            switch(prevState) {
                case State.Collected:
                    //undo
                    Player player = Player.instance;
                    switch(type) {
                        case Type.Goal:
                            player.RemoveCollect();
                            break;

                        case Type.Animal:
                            player.RemoveSummonCount(animalType);
                            break;
                    }
                    break;
            }

            switch(mState) {
                case State.Collected:
                    gameObject.SetActive(false);
                    mActiveWait = false;
                    CancelInvoke();
                    break;

                case State.Active:
                    gameObject.SetActive(true);
                    mActiveWait = true;
                    Invoke("ActiveWaitDelay", 0.2f);
                    break;
            }
        }
    }

    void ActiveWaitDelay() {
        mActiveWait = false;
    }
}
