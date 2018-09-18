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

            if(mTrans.GetComponent<Rigidbody>() != null && !mTrans.GetComponent<Rigidbody>().isKinematic) {
                mVelocity = mTrans.GetComponent<Rigidbody>().velocity; //for rigidbody
                mAngleVelocity = mTrans.GetComponent<Rigidbody>().angularVelocity; //for rigidbody
            }
        }

        public void Restore() {
            if(mTrans != null) {
                bool applyRigidBody = mTrans.GetComponent<Rigidbody>() != null && !mTrans.GetComponent<Rigidbody>().isKinematic;

                if(applyRigidBody)
                    mTrans.GetComponent<Rigidbody>().isKinematic = true;

                mTrans.position = mPosition;
                mTrans.rotation = mRotation;
                mTrans.localScale = mScale;

                if(applyRigidBody) {
                    mTrans.GetComponent<Rigidbody>().isKinematic = false;

                    mTrans.GetComponent<Rigidbody>().velocity = mVelocity; //for rigidbody
                    mTrans.GetComponent<Rigidbody>().angularVelocity = mAngleVelocity; //for rigidbody
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
        if(!gameObject.activeSelf)
            return null;

		return new State(transform);
	}
	
	public void ActionRestore(object dat) {
		((State)dat).Restore();
	}
}
