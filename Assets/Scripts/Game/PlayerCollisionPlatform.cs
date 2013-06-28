using UnityEngine;
using System.Collections;

//need rigidbody
public class PlayerCollisionPlatform : PlayerCollisionBase {
    private WaypointMover mWPMover;

    void DoCollision(PlayerController pc, CollisionFlags flags, Vector3 hitPos, Vector3 hitNormal) {
        //determine if we want to move the other way
        //also move the player based on flags
        if(mWPMover != null) {
            if(pushFlags != CollisionFlags.None && (pushFlags & flags) != 0) {
                if(Mathf.Abs(Mathf.Acos(Vector3.Dot(hitNormal, mWPMover.dir))) <= Mathf.PI * 0.5f) {
                    mWPMover.reverse = !mWPMover.reverse;
                }
            }
        }

        //nudge player
        /*if(pushBackFlags != CollisionFlags.None && (pushBackFlags & flags) != 0) {
            if(flags == CollisionFlags.Below) {
                Vector3 pos = pc.transform.position;
                pos.y = hitPos.y + pc.charCtrl.height * 0.5f + pc.nudgeOfs;
                pc.transform.position = pos;
            }
        }*/
    }

    public override void PlayerCollide(PlayerController pc, CollisionFlags flags, ControllerColliderHit hit) {
        DoCollision(pc, flags, hit.point, hit.normal);
        
        //pc.player.state = Player.StateDead;
    }

    public override void PlayerCollisionStay(PlayerController pc, CollisionFlags flags, ContactPoint hit) {
        DoCollision(pc, flags, hit.point, hit.normal);
    }

    void OnEnable() {
    }

    protected override void Awake() {
        base.Awake();

        mWPMover = GetComponent<WaypointMover>();
    }

    void Start() {
    }
}
