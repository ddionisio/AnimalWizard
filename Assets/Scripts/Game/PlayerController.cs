using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour {
    public const int maxMoves = 10;

    public float moveSpeed;
    public float moveAirSpeed;
    public float jumpSpeed;
    public float gravity;

    public float ladderSpeed;

    public float maxSpeed;

    public float pushForce;
    public float pushedDelay;

    public LayerMask animalLayerMask;
    public LayerMask ladderLayerMask;

    private Player mPlayer;
    private CharacterController mCharCtrl;

    private bool mInputEnabled = false;
    private Vector2 mInputAxis;
    private Vector2 mCurVel;
    private Vector2 mDir;
    private float mCurSpeed;
    private HashSet<Collider> mLadderTriggers = new HashSet<Collider>();
    private bool mIsLadderJumping;

    //these are for moving the player by other influence, such as platforms
    private Vector2[] mMoves = new Vector2[maxMoves];
    private int mNumMoves = 0;

    private Animal mAnimalCancel; //use for unsummoning an animal

    //private HashSet<

    public bool isOnLadder { get { return mLadderTriggers.Count > 0; } }

    public bool inputEnabled {
        get { return mInputEnabled; }
        set {
            if(mInputEnabled != value) {
                mInputEnabled = value;

                InputManager input = Main.instance != null ? Main.instance.input : null;
                if(input != null) {
                    if(mInputEnabled) {
                        input.AddButtonCall(0, InputAction.Action, OnInputAction);
                        input.AddButtonCall(0, InputAction.Jump, OnInputJump);
                        input.AddButtonCall(0, InputAction.Undo, OnInputUndo);

                        for(int i = InputAction.Select1; i <= InputAction.Select5; i++) {
                            input.AddButtonCall(0, i, OnInputSummonSelect);
                        }
                    }
                    else {
                        input.RemoveButtonCall(0, InputAction.Action, OnInputAction);
                        input.RemoveButtonCall(0, InputAction.Jump, OnInputJump);
                        input.RemoveButtonCall(0, InputAction.Undo, OnInputUndo);

                        for(int i = InputAction.Select1; i <= InputAction.Select5; i++) {
                            input.RemoveButtonCall(0, i, OnInputSummonSelect);
                        }
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

        mPlayer.setStateCallback += OnPlayerSetState;
        mPlayer.restoreStateCallback += OnPlayerRestoreState;

        mCharCtrl = collider as CharacterController;
    }

    // Use this for initialization
    void Start() {
        //rigidbody.AddForce(Vector3.zero, ForceMode.
    }

    // Update is called once per frame
    void Update() {
        float dt = Time.deltaTime;

        float inputXVel = 0.0f;

        InputManager input = Main.instance.input;
        
        if(mInputEnabled) {
            mInputAxis.x = input.GetAxis(0, InputAction.DirX);
            mInputAxis.y = input.GetAxis(0, InputAction.DirY);

            //compute input velocity
            if(Mathf.Abs(mInputAxis.x) > float.Epsilon) {
                inputXVel = mInputAxis.x * (mCharCtrl.isGrounded ? moveSpeed : moveAirSpeed);
            }
        }
        else {
            mInputAxis = Vector2.zero;
        }

        Vector2 vel = mCurVel;

        if(isOnLadder) {
            if(mIsLadderJumping) {
                vel.y -= gravity * dt;
                mIsLadderJumping = vel.y > 0.0f;
            }
            else
                vel.y = mInputAxis.y * ladderSpeed;
        }
        else {
            mIsLadderJumping = false;

            vel.y -= gravity * dt;
        }

        if(mNumMoves > 0) {
            for(int i = 0; i < mNumMoves; i++)
                vel += mMoves[i];
            mNumMoves = 0;
        }

        //add input move, cancel x velocity if input moving opposite direction
        if(inputXVel != 0.0f) {
            if(Mathf.Abs(vel.x) < Mathf.Abs(inputXVel) || Mathf.Sign(inputXVel) != Mathf.Sign(vel.x)) {
                vel.x = inputXVel;
            }
        }

        Vector3 pos = transform.position;

        curVel = vel;

        Vector3 dpos = new Vector3(vel.x * dt, vel.y * dt, -pos.z);

        mCharCtrl.Move(dpos);

        bool updateVel = false;

        if(isOnLadder) {
            vel.x = inputXVel;

            if(!mIsLadderJumping)
                vel.y = 0.0f;

            updateVel = true;
        }
        else if(mCharCtrl.isGrounded) {
            if(vel.y < 0.0f)
                vel.y = 0.0f;

            vel.x = inputXVel;

            updateVel = true;
        }
        else if((mCharCtrl.collisionFlags & CollisionFlags.Above) != 0 && vel.y > 0.0f) {
            vel.y = 0.0f;
            updateVel = true;
        }

        if(updateVel)
            curVel = vel;
    }

    void OnTriggerEnter(Collider col) {
        mLadderTriggers.Clear(); //refresh by stay callback
    }

    void OnTriggerStay(Collider col) {
        if(((1 << col.gameObject.layer) & ladderLayerMask) != 0)
            mLadderTriggers.Add(col);
    }

    void OnTriggerExit(Collider col) {
        if(((1 << col.gameObject.layer) & ladderLayerMask) != 0)
            mLadderTriggers.Remove(col);
    }

    void OnControllerColliderHit(ControllerColliderHit hit) {
        Rigidbody hitbody = hit.collider.rigidbody;
        GameObject hitGO = hit.collider.gameObject;

        if(hitbody != null) {
            PlayerCollisionBase collideInteract = hitGO.GetComponent<PlayerCollisionBase>();

            if(collideInteract != null) {
                CollisionFlags flags = GetCollisionFlagsFromHit(hit);

                if(collideInteract.pushBackFlags != CollisionFlags.None && (collideInteract.pushBackFlags & flags) != 0) {
                    //push player
                    Vector2 vel = hitbody.GetPointVelocity(hit.point);

                    if(vel != Vector2.zero) {
                        AddMove(vel);
                    }
                }

                if(!hitbody.isKinematic && collideInteract.pushFlags != CollisionFlags.None && (collideInteract.pushFlags & flags) != 0) {
                    //push hit object
                    hitbody.AddForceAtPosition(hit.moveDirection * pushForce, hit.point);
                }

                //custom collision
                collideInteract.PlayerCollide(this, flags, hit);
            }
        }
    }


    #region input

    void OnInputUndo(InputManager.Info dat) {
        if(dat.state == InputManager.State.Pressed) {
            mPlayer.RestoreLastState();
        }
    }

    void OnInputAction(InputManager.Info dat) {
        if(dat.state == InputManager.State.Pressed) {
            if(mPlayer.summonCurSelect != -1) {
                mPlayer.SummonCurrent();

                if(mPlayer.SummonGetAvailableCount(mPlayer.summonCurSelect) == 0)
                    mPlayer.SummonSetSelect(-1);
            }
            else {
                //cancel an animal
                /*Camera cam = mPlayer.followCamera.mainCamera;
                Ray camRay = cam.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if(Physics.Raycast(camRay, out hit, Mathf.Infinity, animalLayerMask)) {
                    Animal animalHit = hit.collider.GetComponent<Animal>();
                    if(mAnimalCancel != animalHit) {
                        if(mAnimalCancel != null)
                            mAnimalCancel.Despawn(false);

                        mAnimalCancel = animalHit;

                        if(mAnimalCancel != null) {
                            mAnimalCancel.Despawn(true);
                        }
                    }
                }*/
            }
        }
        else if(dat.state == InputManager.State.Released) {
            //cancel animal despawn
            if(mAnimalCancel != null) {
                mAnimalCancel.Despawn(false);
                mAnimalCancel = null;
            }
        }
    }

    void OnInputJump(InputManager.Info dat) {
        if(dat.state == InputManager.State.Pressed) {
            if(mCharCtrl.isGrounded || isOnLadder) {
                mIsLadderJumping = isOnLadder;    
                mCurVel.y = jumpSpeed;
            }
        }
    }

    void OnInputSummonSelect(InputManager.Info dat) {
        if(dat.state == InputManager.State.Pressed) {
            if(mPlayer.summonCurSelect == dat.index || mPlayer.summonCount <= dat.index) {
                mPlayer.SummonSetSelect(-1);
            }
            else if(mPlayer.SummonGetAvailableCount(dat.index) > 0) {
                //only select if there's available count
                mPlayer.SummonSetSelect(dat.index);

                //cancel animal despawn
                if(mAnimalCancel != null) {
                    mAnimalCancel.Despawn(false);
                    mAnimalCancel = null;
                }
            }
        }
    }

    #endregion

    void OnPlayerRelease(EntityBase ent) {
        inputEnabled = false;
    }

    void OnPlayerSetState(EntityBase ent, int state) {
        switch(state) {
            case Player.StateNormal:
                inputEnabled = true;
                break;

            case Player.StateDead:
                inputEnabled = false;
                break;
        }
    }

    void OnPlayerRestoreState(Player p) {
        mInputAxis = Vector2.zero;
        curVel = Vector2.zero;
        mNumMoves = 0;
        mLadderTriggers.Clear();
    }

    private CollisionFlags GetCollisionFlagsFromHit(ControllerColliderHit hit) {
        CollisionFlags ret = CollisionFlags.None;

        Vector2 pos = transform.position;
        pos.x += mCharCtrl.center.x;
        pos.y += mCharCtrl.center.y;

        float hExt = mCharCtrl.height * 0.5f - mCharCtrl.radius;

        if(pos.y + hExt < hit.point.y)
            ret = CollisionFlags.Above;
        else if(pos.y - hExt > hit.point.y)
            ret = CollisionFlags.Below;
        else
            ret = CollisionFlags.Sides;

        //if(

        return ret;
    }
}
