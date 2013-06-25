using UnityEngine;
using System.Collections;

//make sure there's a collider as trigger with the proper layer to determine where this can't spawn to
//rigidbody must be set to kinematic
public class AnimalSummonCursor : MonoBehaviour {
    public tk2dBaseSprite sprite;

    public Color invalidColor;

    private Transform mAttach;

    private int mCollisionCount = 0;

    private Color mDefaultColor;

    public Transform attach { 
        get { return mAttach; } 
        set { 
            mAttach = value;

            if(mAttach != null)
                transform.position = mAttach.position;
        } 
    }

    public bool isValid { get { return mCollisionCount == 0; } }

    void OnDisable() {
        mAttach = null;
        mCollisionCount = 0;

        if(sprite != null)
            sprite.color = mDefaultColor;
    }

    void OnTriggerEnter(Collider c) {
        mCollisionCount++;

        if(sprite != null)
            sprite.color = invalidColor;
    }

    void OnTriggerExit(Collider c) {
        mCollisionCount--;

        if(mCollisionCount <= 0) {
            if(sprite != null)
                sprite.color = mDefaultColor;

            mCollisionCount = 0;
        }
    }

    void Awake() {
        mDefaultColor = sprite.color;
    }

    void FixedUpdate() {
        //update rigidbody to attach
        if(mAttach != null)
            rigidbody.MovePosition(mAttach.position);
    }
}
