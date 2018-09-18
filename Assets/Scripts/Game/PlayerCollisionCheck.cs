using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerCollisionCheck : MonoBehaviour {
    public float heightOfs;
    public float radiusOfs;

    private PlayerController mController;
    private HashSet<Collider> mCols = new HashSet<Collider>();

    void Start() {
        mController = transform.parent.GetComponent<PlayerController>();

        CapsuleCollider col = GetComponent<Collider>() as CapsuleCollider;
        col.height = mController.charCtrl.height + heightOfs;
        col.radius = mController.charCtrl.radius + radiusOfs;
        col.center = mController.charCtrl.center;
    }

    /*void OnTriggerStay(Collider col) {
        //Debug.Log("col: " + col.name);
        if(mController.lastHit == null || mController.lastHit.collider != col) {
            PlayerCollisionBase playerCol = col.GetComponent<PlayerCollisionBase>();
            if(playerCol != null) {
            }
        }
    }*/

    void OnCollisionStay(Collision inf) {
        //mController.AddMove(-inf.relativeVelocity);

        foreach(ContactPoint contact in inf.contacts) {
            if((mController.lastHit == null || mController.lastHit.collider != contact.otherCollider) && !mCols.Contains(contact.otherCollider)) {
                //mCols.Add(contact.otherCollider);

                Rigidbody hitbody = contact.otherCollider.GetComponent<Rigidbody>();

                PlayerCollisionBase collideInteract = contact.otherCollider.GetComponent<PlayerCollisionBase>();
                if(collideInteract != null) {
                    CollisionFlags flags = GetCollisionFlagsFromContact(contact);

                    if(hitbody != null) {
                        if(collideInteract.pushBackFlags != CollisionFlags.None && (collideInteract.pushBackFlags & flags) != 0) {

                            //push player
                            //mController.curVel = Vector2.zero;

                            Vector2 vel = hitbody.GetPointVelocity(contact.point);

                            if(vel != Vector2.zero && Mathf.Abs(Mathf.Acos(Vector2.Dot(vel.normalized, contact.normal))) <= 90.0f * Mathf.Deg2Rad) {
                                //if(mController.moveCount == 0)
                                    mController.AddMove(vel);
                            }
                        }
                    }

                    //custom collision
                    collideInteract.PlayerCollisionStay(mController, flags, contact);
                }
            }
            //Debug.Log("contact: " + contact.otherCollider.name);
        }
    }

    void FixedUpdate() {
        GetComponent<Rigidbody>().MovePosition(mController.transform.position);
    }

    void LateUpdate() {
        mCols.Clear();
    }

    private CollisionFlags GetCollisionFlagsFromContact(ContactPoint contact) {
        CharacterController charCtrl = mController.charCtrl;
        CollisionFlags ret = CollisionFlags.None;

        Vector2 pos = mController.transform.position;
        pos.x += charCtrl.center.x;
        pos.y += charCtrl.center.y;

        float hExt = charCtrl.height * 0.5f - charCtrl.radius;

        if(pos.y + hExt < contact.point.y)
            ret = CollisionFlags.Above;
        else if(pos.y - hExt > contact.point.y)
            ret = CollisionFlags.Below;
        else
            ret = CollisionFlags.Sides;

        //if(

        return ret;
    }
}
