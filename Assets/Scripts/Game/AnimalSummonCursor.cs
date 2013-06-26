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

    public Color defaultColor { get { return mDefaultColor; } }

    public Transform attach { 
        get { return mAttach; } 
        set { 
            mAttach = value;

            if(mAttach != null)
                transform.position = mAttach.position;
        } 
    }

    public virtual bool isValid { get { return mCollisionCount == 0; } }

    protected virtual void OnDisable() {
        mAttach = null;
        mCollisionCount = 0;

        if(sprite != null)
            sprite.color = mDefaultColor;
    }

    protected virtual void OnTriggerEnter(Collider c) {
        mCollisionCount++;

        if(sprite != null)
            sprite.color = invalidColor;
    }

    protected virtual void OnTriggerExit(Collider c) {
        mCollisionCount--;

        if(mCollisionCount <= 0) {
            if(sprite != null)
                sprite.color = mDefaultColor;

            mCollisionCount = 0;
        }
    }

    protected virtual void Awake() {
        mDefaultColor = sprite.color;
    }

    protected virtual void FixedUpdate() {
        //update rigidbody to attach
        if(mAttach != null)
            rigidbody.MovePosition(mAttach.position);
    }
}
