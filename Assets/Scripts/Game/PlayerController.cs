using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using HutongGames.PlayMaker;

public class PlayerController : MonoBehaviour {
    public const int maxMoves = 10;

    public float moveSpeed;
    public float moveAirSpeed;
    public float jumpSpeed;
    public float jumpCancelSpeed;
    public float gravity;

    public float ladderSpeed;

    public float maxFallSpeed;
    public float maxSpeed;

    public float pushForce;
    public float pushedDelay;

    public float nudgeOfs;

    public float dropPlatformDelay;
    public LayerMask dropPlatformMask;

    public float interactDelay;

    public LayerMask animalLayerMask;
    public LayerMask ladderLayerMask;
    public LayerMask deathLayerMask;
    public LayerMask interactLayerMask;

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

    [System.NonSerialized]
    public Vector2 velocityMod; //fill this up during collisions and triggers, will be added to curVel during move and reset afterwards

    private float mLastInputYUpTime;
    private float mLastInputYDownTime;

    private Animal mAnimalCancel; //use for unsummoning an animal

    private ControllerColliderHit mLastHit;
    private bool mLastGrounded;
    private bool mFacingLeft = false;

    private PlayMakerFSM mInteractFSM;

    //private HashSet<
    public bool isFacingLeft { get { return mFacingLeft; } }

    public bool isGrounded { get { return mCharCtrl.isGrounded || mLastGrounded; } }

    public bool isOnLadder { get { return mLadderTriggers.Count > 0; } }
    public bool isOnLadderJumping { get { return mIsLadderJumping; } }

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

    public ControllerColliderHit lastHit { get { return mLastHit; } }

    public Vector2 curVel {
        get { return mCurVel; }
        set {
            if(mCurVel != value) {
                mCurVel = value;

                if(mCurVel.y < -maxFallSpeed)
                    mCurVel.y = -maxFallSpeed;

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

    public Vector2 dir { get { return mDir; } }

    public float curSpeed { get { return mCurSpeed; } }

    public int moveCount { get { return mNumMoves; } }

    public Vector2 inputAxis { get { return mInputAxis; } }

    public void AddMove(Vector2 move) {
        if(mNumMoves < maxMoves) {
            mMoves[mNumMoves] = move;
            mNumMoves++;

            //Debug.Log("moves: " + mNumMoves + " vel: " + move);
        }
    }

    void OnDestroy() {
        inputEnabled = false;
    }

    void Awake() {
        mPlayer = GetComponent<Player>();

        mPlayer.setStateCallback += OnPlayerSetState;
        mPlayer.restoreStateCallback += OnPlayerRestoreState;

        mCharCtrl = GetComponent<Collider>() as CharacterController;
    }

    // Use this for initialization
    void Start() {
        //rigidbody.AddForce(Vector3.zero, ForceMode.
        //mCharCtrl.plat
    }

    // Update is called once per frame
    void FixedUpdate() {
        Vector3 pos = transform.position;
        float dt = Time.smoothDeltaTime;

        InputManager input = Main.instance.input;

        if(mInputEnabled) {
            mInputAxis.x = input.GetAxis(0, InputAction.DirX);
            mInputAxis.y = input.GetAxis(0, InputAction.DirY);

            if(mInputAxis.y < 0.0f && isGrounded && mLastHit != null && ((1 << mLastHit.gameObject.layer) & dropPlatformMask) != 0) {
                if(Time.fixedTime - mLastInputYDownTime >= dropPlatformDelay) {
                    pos.y -= mCharCtrl.height + mCharCtrl.stepOffset;
                    mCharCtrl.detectCollisions = false;
                    transform.position = pos;

                    mLastInputYDownTime = Time.fixedTime;

                    return;
                }
            }
            else {
                mLastInputYDownTime = Time.fixedTime;
            }

            if(mInputAxis.y > 0.0f && mPlayer.isInteract) {
                if(Time.fixedTime - mLastInputYUpTime >= interactDelay) {
                    //interact
                    if(mInteractFSM != null) {
                        mInteractFSM.SendEvent(EntityEvent.Interact);
                    }

                    mLastInputYUpTime = Time.fixedTime;
                    return;
                }
            }
            else {
                mLastInputYUpTime = Time.fixedTime;
            }
        }
        else {
            mInputAxis = Vector2.zero;
        }

        mCharCtrl.detectCollisions = true;

        Vector2 vel = mCurVel;// +velocityMod;

        vel.x += velocityMod.x;

        if(vel.y < 0.0f && velocityMod.y > 0.0f)
            vel.y = velocityMod.y;
        else
            vel.y += velocityMod.y;

        velocityMod = Vector2.zero;

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

        /*if(mNumMoves > 0) {
            for(int i = 0; i < mNumMoves; i++)
                vel += mMoves[i];
            mNumMoves = 0;
        }*/

        //add input move, cancel x velocity if input moving opposite direction
        /*float inputXVel = 0.0f;
        if(Mathf.Abs(mInputAxis.x) > float.Epsilon) {// && (mCharCtrl.collisionFlags & CollisionFlags.Sides) == CollisionFlags.None) {
            inputXVel = mInputAxis.x * (isGrounded ? moveSpeed : moveAirSpeed);

            if(Mathf.Sign(inputXVel) != Mathf.Sign(vel.x)) {
                if(Mathf.Abs(vel.x) < Mathf.Abs(inputXVel)) {
                    vel.x = inputXVel;
                }
            }
            else if(mCharCtrl.isGrounded) {
                vel.x += inputXVel;
            }
        }*/

        mLastHit = null;

        //current velocity move
        pos = transform.position;
                
        curVel = vel;

        Vector3 dpos = new Vector3(vel.x * dt, vel.y * dt, -pos.z);

        CollisionFlags collFlags = mCharCtrl.Move(dpos);

        mLastGrounded = mCharCtrl.isGrounded;
        //

        //x input move
        float inputXVel = 0.0f;
        if(Mathf.Abs(mInputAxis.x) > float.Epsilon) {// && (mCharCtrl.collisionFlags & CollisionFlags.Sides) == CollisionFlags.None) {
            if(mCharCtrl.isGrounded) {
                inputXVel = mInputAxis.x * moveSpeed;
            }
            else {
                inputXVel = mInputAxis.x * moveAirSpeed;
            }

            //inputXVel = mInputAxis.x * (mCharCtrl.isGrounded ? moveSpeed : moveAirSpeed);
        }

        //move
        Vector2 mvel = new Vector2(isGrounded ? inputXVel : 0.0f, 0.0f);
        if(mNumMoves > 0) {
            for(int i = 0; i < mNumMoves; i++)
                mvel += mMoves[i];

            mNumMoves = 0;
        }

        //curVel = mvel;

        if((isOnLadder || isGrounded) && inputXVel != 0.0f) {
            if(Mathf.Abs(mvel.x) < Mathf.Abs(inputXVel) || Mathf.Sign(inputXVel) != Mathf.Sign(mvel.x))
                mvel.x = inputXVel;
        }

        mCharCtrl.Move(mvel * dt);
        mNumMoves = 0;
        //
        /*
        if(mNumMoves > 0) {
            mvel = new Vector2(0.0f, 0.0f);
            for(int i = 0; i < mNumMoves; i++)
                mvel += mMoves[i];

            mCharCtrl.Move(mvel * dt);
            mNumMoves = 0;
        }*/

        //adjust
        if(isOnLadder) {
            vel.x = 0.0f;

            if(!mIsLadderJumping)
                vel.y = 0.0f;
        }
        else if(isGrounded) {
            vel.x = 0.0f;
        }
        else {
            if(inputXVel != 0.0f) {
                if(Mathf.Abs(vel.x) < Mathf.Abs(inputXVel) || Mathf.Sign(inputXVel) != Mathf.Sign(vel.x))
                    vel.x = inputXVel;
            }

            if(((mCharCtrl.collisionFlags | collFlags) & CollisionFlags.Above) != 0 && vel.y > 0.0f) {
                vel.y = 0.0f;
            }
        }

        curVel = vel;

        if(vel.x != 0.0f || inputXVel != 0.0f)
            mFacingLeft = vel.x < 0.0f || inputXVel < 0.0f;
    }

    void OnTriggerEnter(Collider col) {
        mLadderTriggers.Clear(); //refresh by stay callback

        //die
        bool die = false;

        if(((1 << col.gameObject.layer) & deathLayerMask) != 0) {
            die = true;
        }
        else {
            PlayerCollisionDeath death = col.GetComponent<PlayerCollisionDeath>();
            if(death != null) {
                die = true;
            }
        }

        if(die)
            player.state = Player.StateDead;
        else if(((1 << col.gameObject.layer) & interactLayerMask) != 0) {
            //interact?
            PlayMakerFSM fsm = col.GetComponent<PlayMakerFSM>();
            if(fsm != null) {
                mPlayer.isInteract = true;
                mInteractFSM = fsm;
            }
        }
        //if(
    }

    void OnTriggerStay(Collider col) {
        if(((1 << col.gameObject.layer) & ladderLayerMask) != 0)
            mLadderTriggers.Add(col);
    }

    void OnTriggerExit(Collider col) {
        if(((1 << col.gameObject.layer) & interactLayerMask) != 0) {
            if(mInteractFSM != null && mInteractFSM.gameObject == col.gameObject) {
                mPlayer.isInteract = false;
                mInteractFSM = null;
            }
        }

        if(((1 << col.gameObject.layer) & ladderLayerMask) != 0)
            mLadderTriggers.Remove(col);
    }

    void OnControllerColliderHit(ControllerColliderHit hit) {
        mLastHit = hit;

        Rigidbody hitbody = hit.collider.GetComponent<Rigidbody>();
        GameObject hitGO = hit.gameObject;

        if(((1 << hitGO.layer) & deathLayerMask) != 0) {
            player.state = Player.StateDead;
            return;
        }

        PlayerCollisionBase collideInteract = hitGO.GetComponent<PlayerCollisionBase>();

        if(collideInteract != null) {
            CollisionFlags flags = GetCollisionFlagsFromHit(hit);

            if(hitbody != null) {
                if(collideInteract.pushBackFlags != CollisionFlags.None && (collideInteract.pushBackFlags & flags) != 0) {

                    //push player
                    Vector2 vel = collideInteract.solidPushBack ? hitbody.velocity : hitbody.GetPointVelocity(hit.point);

                    if(vel != Vector2.zero){// && Mathf.Abs(Mathf.Acos(Vector2.Dot(vel.normalized, hit.normal))) <= 95.0f * Mathf.Deg2Rad) {
                        AddMove(vel);
                    }
                }

                if(!hitbody.isKinematic && collideInteract.pushFlags != CollisionFlags.None && (collideInteract.pushFlags & flags) != 0) {
                    //push hit object
                    hitbody.AddForceAtPosition(hit.moveDirection * pushForce, hit.point);
                }
            }

            //custom collision
            collideInteract.PlayerCollide(this, flags, hit);
        }
    }


    #region input

    void OnInputUndo(InputManager.Info dat) {
        if(dat.state == InputManager.State.Pressed) {
            //if we were dead, remove this input, it will be added back after restore (hopefully)
            if(mPlayer.state == Player.StateDead)
                Main.instance.input.RemoveButtonCall(0, InputAction.Undo, OnInputUndo);

            mPlayer.RestoreLastState();
        }
    }

    void OnInputAction(InputManager.Info dat) {
        if(dat.state == InputManager.State.Pressed) {
            if(mPlayer.summonCurSelect != -1) {
                SoundPlayerGlobal.instance.Play("summon");

                mPlayer.SummonCurrent();

                //if(mPlayer.SummonGetAvailableCount(mPlayer.summonCurSelect) == 0)
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
            if(isGrounded || isOnLadder) {
                mIsLadderJumping = isOnLadder;    
                mCurVel.y = jumpSpeed;

                SoundPlayerGlobal.instance.Play("jump");
            }
        }
        else if(dat.state == InputManager.State.Released) {
            if(mCurVel.y > jumpCancelSpeed && !isGrounded) {
                mCurVel.y = jumpCancelSpeed;
            }
        }
    }

    void OnInputSummonSelect(InputManager.Info dat) {
        if(dat.state == InputManager.State.Pressed) {
            //SoundPlayerGlobal.instance.Play("click");

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

    void OnPlayerSetState(EntityBase ent, int state) {
        switch(ent.prevState) {
            case Player.StateDead:
                if(Main.instance.input != null)
                    Main.instance.input.RemoveButtonCall(0, InputAction.Undo, OnInputUndo);
                break;
        }

        switch(state) {
            case Player.StateNormal:
                inputEnabled = true;
                mCharCtrl.detectCollisions = true;
                break;

            case Player.StateInvalid:
            case Player.StateVictory:
                inputEnabled = false;
                break;

            case Player.StateDead:
                inputEnabled = false;
                mCharCtrl.detectCollisions = false;

                //allow undo
                Main.instance.input.AddButtonCall(0, InputAction.Undo, OnInputUndo);
                break;
        }
    }

    void OnPlayerRestoreState(Player p) {
        mInputAxis = Vector2.zero;
        curVel = Vector2.zero;
        mNumMoves = 0;
        mLadderTriggers.Clear();
        mLastHit = null;
        mLastInputYDownTime = mLastInputYUpTime = Time.fixedTime;
        mInteractFSM = null;
        mPlayer.isInteract = false;
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

    void OnUIModalActive() {
        switch(mPlayer.state) {
            case Player.StateNormal:
                inputEnabled = false;
                break;

            case Player.StateDead:
                Main.instance.input.RemoveButtonCall(0, InputAction.Undo, OnInputUndo);
                break;
        }
    }

    void OnUIModalInactive() {
        switch(mPlayer.state) {
            case Player.StateNormal:
                inputEnabled = true;
                break;

            case Player.StateDead:
                Main.instance.input.AddButtonCall(0, InputAction.Undo, OnInputUndo);
                break;
        }
    }
}
