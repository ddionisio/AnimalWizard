using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Platform : MonoBehaviour {
    public const float nudgeOffset = -0.01f;

    void OnTriggerStay(Collider col) {
        //character collider?

        //rigidbody, zero out velocity y, kinda hacky.....
        /*if(col.rigidbody != null && !col.rigidbody.isKinematic) {
            //check if platform is below
            Vector3 pos = transform.position;
            Vector3 colPos = col.transform.position;

            if(colPos.y - col.bounds.extents.y > pos.y) {
                Vector3 vel = col.rigidbody.velocity;
                if(vel.y < 0.0f) {
                    col.rigidbody.AddForce(new Vector3(0.0f, -vel.y, 0.0f), ForceMode.VelocityChange);
                }
            }
        }*/
        //rigidbody, zero out velocity y, kinda hacky.....
        if(col.rigidbody != null && !col.rigidbody.isKinematic) {
            //check if bottom of col is above us
            Vector3 pos = transform.position;
            Vector3 colPos = col.transform.position;

            if(colPos.y - col.bounds.extents.y > pos.y) {
                //correct the y position //nudgeOffset
                RaycastHit hit;
                if(Physics.Raycast(colPos, -Vector3.up, out hit, Mathf.Abs(pos.y - colPos.y), (1 << gameObject.layer))) {
                    colPos.y = hit.point.y + col.bounds.extents.y;

                    col.rigidbody.MovePosition(colPos);
                }
            }

            Vector3 vel = col.rigidbody.velocity;
            vel.y = 0.0f;
            col.rigidbody.velocity = vel;
        }
    }

    void OnTriggerEnter(Collider col) {
        //character collider?

        //rigidbody, zero out velocity y, kinda hacky.....
        if(col.rigidbody != null && !col.rigidbody.isKinematic) {
            //check if bottom of col is above us
            Vector3 pos = transform.position;
            Vector3 colPos = col.transform.position;

            if(colPos.y - col.bounds.extents.y > pos.y) {
                //correct the y position //nudgeOffset
                RaycastHit hit;
                if(Physics.Raycast(colPos, -Vector3.up, out hit, Mathf.Abs(pos.y - colPos.y), (1 << gameObject.layer))) {
                    colPos.y = hit.point.y + col.bounds.extents.y;

                    col.rigidbody.MovePosition(colPos);
                }
            }
        }
    }

    void OnTriggerExit(Collider col) {
    }

    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }
}
