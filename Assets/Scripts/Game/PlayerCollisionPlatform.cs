using UnityEngine;
using System.Collections;

//need rigidbody
public class PlayerCollisionPlatform : PlayerCollisionBase, IActionStateListener {
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

    void OnCollisionEnter(Collision col) {
        if(mWPMover != null) {
            bool doReverse = false;

            foreach(ContactPoint contact in col.contacts) {
                if(Mathf.Abs(Mathf.Acos(Vector3.Dot(contact.normal, mWPMover.dir))) > Mathf.PI * 0.5f) {
                    doReverse = true;
                    break;
                }
            }

            if(doReverse)
                mWPMover.reverse = !mWPMover.reverse;
        }
    }

    void OnEnable() {
    }

    void OnDestroy() {
        if(ActionStateManager.instance != null)
            ActionStateManager.instance.Unregister(this);
    }

    protected override void Awake() {
        base.Awake();

        mWPMover = GetComponent<WaypointMover>();
    }

    void Start() {
        ActionStateManager.instance.Register(this);
    }

    public object ActionSave() {
        if(!gameObject.activeSelf)
            return null;

        if(mWPMover != null)
            return new WaypointMover.SaveState(mWPMover);
        else
            return new ActionStateTransform.State(transform);
    }

    public void ActionRestore(object dat) {
        if(mWPMover != null)
            ((WaypointMover.SaveState)dat).Restore();
        else
            ((ActionStateTransform.State)dat).Restore();
    }
}
