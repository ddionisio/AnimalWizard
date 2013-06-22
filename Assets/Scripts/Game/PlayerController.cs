using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {
    public const int maxMoves = 10;

    public float moveSpeed;
    public float jumpSpeed;
    public float gravity;

    public float maxSpeed;

    public float pushForce;
    public LayerMask pushMask; //for pushing things, like an animal, etc
    public LayerMask pushByMask; //for platforms, or things that push the play

    public float pushedDelay;

    private Player mPlayer;
    private CharacterController mCharCtrl;

    private bool mInputEnabled = false;
    private Vector2 mInputAxis;
    private Vector2 mCurVel;
    private Vector2 mDir;
    private float mCurSpeed;

    //these are for moving the player by other influence, such as platforms
    private Vector2[] mMoves = new Vector2[maxMoves];
    private int mNumMoves = 0;

    public bool inputEnabled {
        get { return mInputEnabled; }
        set {
            if(mInputEnabled != value) {
                mInputEnabled = value;

                InputManager input = Main.instance != null ? Main.instance.input : null;
                if(input != null) {
                    if(mInputEnabled) {
                        input.AddButtonCall(0, InputAction.Jump, OnInputJump);
                    }
                    else {
                        input.RemoveButtonCall(0, InputAction.Jump, OnInputJump);
                    }
                }
            }
        }
    }

    public Player player { get { return mPlayer; } }

    public CharacterController charCtrl { get { return mCharCtrl; } }

    public Vector2 curVel {
        get { return mCurVel; }
        set {
            if(mCurVel != value) {
                mCurVel = value;
                mCurSpeed = mCurVel.magnitude;

                if(mCurSpeed > 0.0f) {
                    mDir = mCurVel / mCurSpeed;
                    if(mCurSpeed > maxSpeed) {
                        mCurVel = mDir * maxSpeed;
                        mCurSpeed = maxSpeed;
                    }
                }
            }
        }
    }

    public void AddMove(Vector2 move) {
        if(mNumMoves < maxMoves) {
            mMoves[mNumMoves] = move;
            mNumMoves++;
        }
    }

    void OnDestroy() {
        inputEnabled = false;
    }

    void Awake() {
        mPlayer = GetComponent<Player>();
        mCharCtrl = collider as CharacterController;
    }

    // Use this for initialization
    void Start() {
        //rigidbody.AddForce(Vector3.zero, ForceMode.
    }

    // Update is called once per frame
    void Update() {
        float dt = Time.deltaTime;

        if(mInputEnabled) {
            InputManager input = Main.instance.input;

            mInputAxis.x = input.GetAxis(0, InputAction.DirX);
            mInputAxis.y = input.GetAxis(0, InputAction.DirY);
        }
        else {
            mInputAxis = Vector2.zero;
        }

        Vector2 vel = mCurVel;

        vel.x = mInputAxis.x * moveSpeed;

        vel.y -= gravity * dt;

        //animation update

        if(mNumMoves > 0) {
            for(int i = 0; i < mNumMoves; i++)
                vel += mMoves[i];
            mNumMoves = 0;
        }

        curVel = vel;

        mCharCtrl.Move(curVel * dt);
    }

    void FixedUpdate() {

        
    }

    void OnControllerColliderHit(ControllerColliderHit hit) {
        Rigidbody hitbody = hit.collider.rigidbody;
        GameObject hitGO = hit.collider.gameObject;

        if(hitbody != null) {
            if(((1 << hitGO.layer) & pushByMask.value) != 0) {
                //push player
                Vector2 vel = hitbody.GetPointVelocity(hit.point);
                if(vel != Vector2.zero)
                    AddMove(vel);
            }

            if(!hitbody.isKinematic) {
                //push hit object
                if(((1 << hitGO.layer) & pushMask.value) != 0) {
                    hitbody.AddForceAtPosition(hit.moveDirection * pushForce, hit.point);
                }
            }
        }

        //custom collision
        PlayerCollisionBase collideInteract = hitGO.GetComponent<PlayerCollisionBase>();
        if(collideInteract != null)
            collideInteract.PlayerCollide(this, hit);
    }


    #region input

    void OnInputJump(InputManager.Info dat) {
        if(dat.state == InputManager.State.Pressed) {
            if(mCharCtrl.isGrounded) {
                mCurVel.y = jumpSpeed;
            }
        }
    }

    #endregion

}
