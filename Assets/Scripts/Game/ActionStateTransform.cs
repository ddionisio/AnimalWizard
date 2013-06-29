using UnityEngine;
using System.Collections;

public class ActionStateTransform : MonoBehaviour, IActionStateListener {
    public class State {
        private Transform mTrans;
        private Vector3 mPosition;
        private Quaternion mRotation;
        private Vector3 mScale;
        private Vector3 mVelocity; //for rigidbody
        private Vector3 mAngleVelocity; //for rigidbody

        public State(Transform t) {
            mTrans = t;

            mPosition = mTrans.position;
            mRotation = mTrans.rotation;
            mScale = mTrans.localScale;

            if(mTrans.rigidbody != null && !mTrans.rigidbody.isKinematic) {
                mVelocity = mTrans.rigidbody.velocity; //for rigidbody
                mAngleVelocity = mTrans.rigidbody.angularVelocity; //for rigidbody
            }
        }

        public void Restore() {
            if(mTrans != null) {
                bool applyRigidBody = mTrans.rigidbody != null && !mTrans.rigidbody.isKinematic;

                if(applyRigidBody)
                    mTrans.rigidbody.isKinematic = true;

                mTrans.position = mPosition;
                mTrans.rotation = mRotation;
                mTrans.localScale = mScale;

                if(applyRigidBody) {
                    mTrans.rigidbody.isKinematic = false;

                    mTrans.rigidbody.velocity = mVelocity; //for rigidbody
                    mTrans.rigidbody.angularVelocity = mAngleVelocity; //for rigidbody
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
		return new State(transform);
	}
	
	public void ActionRestore(object dat) {
		((State)dat).Restore();
	}
}
