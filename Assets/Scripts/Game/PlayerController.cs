using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {
    public const int maxMoves = 10;

    public float moveSpeed;
    public float jumpSpeed;
    public float gravity;

    public float maxSpeed;

    public float pushForce;
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

        float inputXVel = 0.0f;

        if(mInputEnabled) {
            InputManager input = Main.instance.input;

            mInputAxis.x = input.GetAxis(0, InputAction.DirX);
            mInputAxis.y = input.GetAxis(0, InputAction.DirY);

            //compute input velocity
            if(Mathf.Abs(mInputAxis.x) > float.Epsilon) {
                inputXVel = mInputAxis.x * moveSpeed;
            }
        }
        else {
            mInputAxis = Vector2.zero;
        }

        Vector2 vel = mCurVel;

        vel.x = mInputAxis.x * moveSpeed;

        vel.y -= gravity * dt;
                
        if(mNumMoves > 0) {
            for(int i = 0; i < mNumMoves; i++)
                vel += mMoves[i];
            mNumMoves = 0;
        }

        //add input move, cancel x velocity if input moving opposite direction
        if(inputXVel != 0.0f) {
            if(Mathf.Sign(inputXVel) != Mathf.Sign(vel.x))
                vel.x = inputXVel;
            else
                vel.x += inputXVel;
        }
                                
        curVel = vel;

        mCharCtrl.Move(vel * dt);

        bool updateVel = false;

        if(mCharCtrl.isGrounded && vel.y < 0.0f) {
            vel.y = 0.0f;
            updateVel = true;
        }
        else if((mCharCtrl.collisionFlags & CollisionFlags.Above) != 0 && vel.y > 0.0f) {
            vel.y = 0.0f;
            updateVel = true;
        }

        if(updateVel)
            curVel = vel;
    }

    void FixedUpdate() {

        
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

                if(!hitbody.isKinematic && collideInteract.pushFlags != CollisionFlags.None && (collideInteract.pushBackFlags & flags) != 0) {
                    //push hit object
                    hitbody.AddForceAtPosition(hit.moveDirection * pushForce, hit.point);
                }

                //custom collision
                collideInteract.PlayerCollide(this, flags, hit);
            }
        }   
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
