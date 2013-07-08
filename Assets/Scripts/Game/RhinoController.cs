using UnityEngine;
using System.Collections;

public class RhinoController : PlayerCollisionBase, IActionStateListener {
    public float moveForce;

    public float turnMoveableAngle = 45.0f; //angle from up vector, if angle bet. up and normal is greater, then we turn
    public LayerMask turnMask; //layers that will make the rhino move the opposite direction if blocked

    public tk2dBaseSprite sprite;

    public float upForce;

    private Animal mAnimal;
    private ConstantForce mForce;
    private float mXDir;
    private int mCollisionCount = 0;

    private bool mStarted = false;

    void OnDisable() {
        if(mStarted) {
            mForce.force = Vector3.zero;
            mCollisionCount = 0;
        }
    }

    protected override void Awake() {
        base.Awake();

        mAnimal = GetComponent<Animal>();
        mForce = GetComponent<ConstantForce>();
        mAnimal.summonInitCallback += OnSummonInit;
        mAnimal.setStateCallback += OnSetState;
    }

    // Use this for initialization
    void Start() {
        mStarted = true;
    }

    void OnCollisionEnter(Collision col) {
        mCollisionCount += col.contacts.Length;

        if(mForce.force == Vector3.zero) {
            Player player = Player.instance;
            mXDir = Mathf.Sign(transform.position.x - player.transform.position.x);
            mForce.force = new Vector3(mXDir * moveForce, upForce, 0.0f);
        }
    }

    void OnCollisionStay(Collision col) {
        bool doOpposite = false;

        foreach(ContactPoint contact in col.contacts) {
            if(((1 << contact.otherCollider.gameObject.layer) & turnMask) != 0) {
                if(!doOpposite) {
                    Vector2 contactNormal = contact.normal;

                    float a = Vector2.Angle(Vector2.up, contactNormal);
                    if(a > turnMoveableAngle && Mathf.Sign(contactNormal.x) != mXDir)
                        doOpposite = true;
                }
            }
        }

        if(doOpposite) {
            Vector3 vel = rigidbody.velocity;
            vel.x = 0.0f;
            rigidbody.velocity = vel;
            mXDir *= -1.0f;
            mForce.force = new Vector3(mXDir * moveForce, upForce, 0.0f);
        }
    }

    void OnCollisionExit(Collision col) {
        mCollisionCount -= col.contacts.Length;

        if(mCollisionCount <= 0) {
            Vector3 vel = rigidbody.velocity;
            vel.y = 0.0f;
            rigidbody.velocity = vel;

            mCollisionCount = 0;
            mForce.force = Vector3.zero;
        }
    }

    public override void PlayerCollide(PlayerController pc, CollisionFlags flags, ControllerColliderHit hit) {
        if((flags & CollisionFlags.Sides) != CollisionFlags.None) {
            rigidbody.velocity = Vector3.zero;
            mXDir = Mathf.Sign(transform.position.x - pc.transform.position.x);
            mForce.force = new Vector3(mXDir * moveForce, upForce, 0.0f);
        }
    }

    void OnSetState(EntityBase ent, int state) {
        switch(state) {
            case Animal.StateSpawning:
            case Animal.StateDespawning:
            case Animal.StateNormal:
                mForce.force = new Vector3(0.0f, 0.0f, 0.0f);
                break;

            //case Animal.StateNormal:
               // mForce.force = new Vector3(mXDir * moveForce, 0.0f, 0.0f);
                //break;
        }
    }

    void OnSpawned() {
        ActionStateManager.instance.Register(this);
    }

    void OnDespawned() {
        ActionStateManager.instance.Unregister(this);
    }

    void OnSummonInit(Animal animal, Player player) {
        mXDir = Mathf.Sign(transform.position.x - player.transform.position.x);
    }

    public object ActionSave() {
        return mXDir;
    }

    public void ActionRestore(object dat) {
        mXDir = (float)dat;
        mForce.force = new Vector3(mXDir * moveForce, upForce, 0.0f);
    }

    void Update() {
        if(mXDir != 0.0f)
            sprite.FlipX = mXDir < 0.0f;
    }
}
