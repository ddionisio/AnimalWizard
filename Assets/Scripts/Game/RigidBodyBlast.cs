using UnityEngine;
using System.Collections;

public class RigidBodyBlast : MonoBehaviour {
    public bool atPoint;
    public float force;

    void OnCollisionStay(Collision c) {
        foreach(ContactPoint contact in c.contacts) {
            if(contact.otherCollider.rigidbody != null && !contact.otherCollider.rigidbody.isKinematic) {
                if(atPoint) {
                    contact.otherCollider.rigidbody.AddForceAtPosition(-contact.normal * force, contact.point);
                }
                else {
                    contact.otherCollider.rigidbody.AddForce(-contact.normal * force);
                }
            }
        }
    }
}
