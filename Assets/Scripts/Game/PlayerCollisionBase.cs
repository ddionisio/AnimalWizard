using UnityEngine;
using System.Collections;

public class PlayerCollisionBase : MonoBehaviour {
    public bool solidPushBack = false; //if we are going to use velocity of rigidbody
    public bool pushBackAbove; //which collision flag from player to push back
    public bool pushBackSide;
    public bool pushBackBelow;

    public bool pushAbove; //which collision flag from player for this object to get pushed
    public bool pushSide;
    public bool pushBelow;
        
    private CollisionFlags mPushBackFlags = CollisionFlags.None;
    private CollisionFlags mPushFlags = CollisionFlags.None;

    public CollisionFlags pushBackFlags { get { return mPushBackFlags; } }
    public CollisionFlags pushFlags { get { return mPushFlags; } }

    protected virtual void Awake() {
        if(pushBackAbove)
            mPushBackFlags |= CollisionFlags.Above;
        if(pushBackSide)
            mPushBackFlags |= CollisionFlags.Sides;
        if(pushBackBelow)
            mPushBackFlags |= CollisionFlags.Below;

        if(pushAbove)
            mPushFlags |= CollisionFlags.Above;
        if(pushSide)
            mPushFlags |= CollisionFlags.Sides;
        if(pushBelow)
            mPushFlags |= CollisionFlags.Below;
    }

    public virtual void PlayerCollisionStay(PlayerController pc, CollisionFlags flags, ContactPoint hit) {
    }

    public virtual void PlayerCollide(PlayerController pc, CollisionFlags flags, ControllerColliderHit hit) {
    }
}
