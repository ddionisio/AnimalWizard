using UnityEngine;
using System.Collections;

//make sure there's a collider as trigger with the proper layer to determine where this can't spawn to
//rigidbody must be set to kinematic
public class AnimalSummonCursor : MonoBehaviour {
    public Bounds boundCheck;

    public tk2dBaseSprite sprite;

    public Color invalidColor;

    private Transform mAttach;
    private Collider mCollider;

    public Transform attach { get { return mAttach; } set { mAttach = value; } }

    public bool isValid { get { return mCollider != null; } }

    void OnTriggerEnter(Collider c) {
        mCollider = c;

        if(sprite != null)
            sprite.color = invalidColor;
    }

    void OnTriggerExit(Collider c) {
        if(mCollider == c) {
            if(sprite != null)
                sprite.color = Color.white;

            mCollider = null;
        }
    }

    void FixedUpdate() {
        //update rigidbody to attach
        rigidbody.MovePosition(mAttach.position);
    }
}
