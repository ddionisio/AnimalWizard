using UnityEngine;
using System.Collections;

/// <summary>
/// Make sure this is on an object with a rigidbody!
/// </summary>
[ExecuteInEditMode]
[AddComponentMenu("M8/Physics/RigidBodyMoveToTarget")]
public class RigidBodyMoveToTarget : MonoBehaviour {
    public Transform target;

#if UNITY_EDITOR
    // Update is called once per frame
    void Update() {
        if(!Application.isPlaying && target != null) {
            if(GetComponent<Collider>() != null) {
                Vector3 ofs = transform.worldToLocalMatrix.MultiplyPoint(GetComponent<Collider>().bounds.center);

                transform.position = target.localToWorldMatrix.MultiplyPoint(-ofs);
            }
            else {
                transform.position = target.position;
            }

            transform.rotation = target.rotation;
        }
    }
#endif

    void FixedUpdate() {
        if(GetComponent<Collider>() != null) {
            Vector3 ofs = transform.worldToLocalMatrix.MultiplyPoint(GetComponent<Collider>().bounds.center);
            GetComponent<Rigidbody>().MovePosition(target.localToWorldMatrix.MultiplyPoint(-ofs));
        }
        else
            GetComponent<Rigidbody>().MovePosition(target.position);

        GetComponent<Rigidbody>().MoveRotation(target.rotation);
    }
}
