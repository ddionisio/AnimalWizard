using UnityEngine;
using System.Collections;

public class ActionStateTransform : MonoBehaviour, IActionStateListener {
    public class State {
        private Rigidbody mBody;
        private Vector3 mPosition;
        private Quaternion mRotation;
        private Vector3 mScale;
        private Vector3 mVelocity; //for rigidbody
        private Vector3 mAngleVelocity; //for rigidbody

        public State(Rigidbody body) {
            mBody = body;
            if(mBody != null) {
                mPosition = mBody.transform.position;
                mRotation = mBody.transform.rotation;
                mScale = mBody.transform.localScale;

                if(!mBody.isKinematic) {
                    mVelocity = mBody.velocity; //for rigidbody
                    mAngleVelocity = mBody.angularVelocity; //for rigidbody
                }
            }
        }

        public void Restore() {
            if(mBody != null) {
                mBody.transform.position = mPosition;
                mBody.transform.rotation = mRotation;
                mBody.transform.localScale = mScale;

                if(!mBody.isKinematic) {
                    mBody.velocity = mVelocity; //for rigidbody
                    mBody.angularVelocity = mAngleVelocity; //for rigidbody
                }
            }
        }
    }

    void OnDestroy() {
        if(ActionStateManager.instance != null)
            ActionStateManager.instance.Unregister(this);
    }

    // Use this for initialization
    void Start() {
        ActionStateManager.instance.Register(this);
    }

    // Update is called once per frame
    void Update() {

    }
	
	public object ActionSave() {
		return new State(rigidbody);
	}
	
	public void ActionRestore(object dat) {
		((State)dat).Restore();
	}
}
