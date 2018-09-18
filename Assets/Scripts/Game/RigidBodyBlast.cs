using UnityEngine;
using System.Collections;

public class RigidBodyBlast : MonoBehaviour {
    public bool atPoint;
    public float force;

    void OnCollisionStay(Collision c) {
        foreach(ContactPoint contact in c.contacts) {
            if(contact.otherCollider.GetComponent<Rigidbody>() != null && !contact.otherCollider.GetComponent<Rigidbody>().isKinematic) {
                if(atPoint) {
                    contact.otherCollider.GetComponent<Rigidbody>().AddForceAtPosition(-contact.normal * force, contact.point);
                }
                else {
                    contact.otherCollider.GetComponent<Rigidbody>().AddForce(-contact.normal * force);
                }
            }
        }
    }
}
