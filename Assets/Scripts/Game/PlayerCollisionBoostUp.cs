using UnityEngine;
using System.Collections;

public class PlayerCollisionBoostUp : PlayerCollisionBase {
    public float tolerance = 1;
    public float boost = 10;

    public override void PlayerCollide(PlayerController pc, ControllerColliderHit hit) {
        Vector3 vel = rigidbody.GetPointVelocity(hit.point);
        if(vel.y > tolerance && pc.charCtrl.collisionFlags == CollisionFlags.Below) {
            Vector2 playerVel = pc.curVel;
            pc.AddMove(new Vector2(0.0f, playerVel.y < 0.0f ? boost - playerVel.y : boost));
        }
    }

    void Awake() {
    }

    // Use this for initialization
    void Start() {
    }
}
