using UnityEngine;
using System.Collections;

public class PlayerSpriteController : MonoBehaviour {
    public enum State {
        idle,
        move,
        jump,
        fall,
        climbIdle,
        climb,
        dead
    }

    public tk2dSpriteAnimator anim;

    private tk2dSpriteAnimationClip[] mClips;
    private PlayerController mController;

    void Awake() {
        mController = GetComponent<PlayerController>();

        mClips = M8.tk2dUtil.GetSpriteClips(anim, typeof(State));
    }

    // Use this for initialization
    void Start() {
        mController.player.setStateCallback += OnPlayerSetState;
    }

    // Update is called once per frame
    void Update() {
        anim.Sprite.FlipX = mController.isFacingLeft;

        switch(mController.player.state) {
            case Player.StateNormal:
                if(mController.isOnLadder) {
                    if(mController.isOnLadderJumping) {
                        anim.Play(mClips[(int)State.jump]);
                    }
                    else if(Mathf.Abs(mController.inputAxis.y) > float.Epsilon || Mathf.Abs(mController.inputAxis.x) > float.Epsilon) {
                        anim.Play(mClips[(int)State.climb]);
                    }
                    else {
                        anim.Play(mClips[(int)State.climbIdle]);
                    }
                }
                else if(mController.isGrounded) {
                    if(Mathf.Abs(mController.inputAxis.x) > float.Epsilon) {
                        anim.Play(mClips[(int)State.move]);
                    }
                    else {
                        anim.Play(mClips[(int)State.idle]);
                    }
                }
                else {
                    if(mController.curVel.y > 0.0f)
                        anim.Play(mClips[(int)State.jump]);
                    else
                        anim.Play(mClips[(int)State.fall]);
                }
                break;
        }
    }

    void OnPlayerSetState(EntityBase ent, int state) {
        switch(state) {
            case Player.StateDead:
                anim.Play(mClips[(int)State.dead]);
                break;
        }
    }
}
