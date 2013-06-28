using UnityEngine;
using System.Collections;

public class PlayerCollisionBoostUp : PlayerCollisionBase {
    public float tolerance = 1;
    public float boost = 10;

    public override void PlayerCollide(PlayerController pc, CollisionFlags flags, ControllerColliderHit hit) {
        Vector3 vel = rigidbody.GetPointVelocity(hit.point);

        if(vel.y > tolerance && (flags & CollisionFlags.Below) != 0) {
            //Vector2 playerVel = pc.curVel;
            //pc.AddMove(new Vector2(0.0f, playerVel.y < 0.0f ? boost - playerVel.y : boost));

            pc.velocityMod.y += boost;
        }
    }

}
