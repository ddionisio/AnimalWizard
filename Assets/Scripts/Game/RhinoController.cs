using UnityEngine;
using System.Collections;

public class RhinoController : PlayerCollisionBase {
    public float moveForce;

    public float turnMoveableAngle = 45.0f; //angle from up vector, if angle bet. up and normal is greater, then we turn
    public LayerMask turnMask; //layers that will make the rhino move the opposite direction if blocked

    private Animal mAnimal;
    private ConstantForce mForce;
    private float mXDir;

    protected override void Awake() {
        base.Awake();

        mAnimal = GetComponent<Animal>();
        mForce = GetComponent<ConstantForce>();
        mAnimal.summonInitCallback += OnSummonInit;
        mAnimal.setStateCallback += OnSetState;
        mAnimal.stateSaveCallback += OnSaveState;
        mAnimal.stateRestoreCallback += OnRestoreState;
    }

    // Use this for initialization
    void Start() {

    }

    void OnCollisionEnter(Collision col) {
        bool doOpposite = false;

        foreach(ContactPoint contact in col.contacts) {
            if(((1 << contact.otherCollider.gameObject.layer) & turnMask) != 0) {
                if(!doOpposite) {
                    Vector2 contactNormal = contact.normal;

                    float a = Vector2.Angle(Vector2.up, contactNormal);
                    if(a > turnMoveableAngle)
                        doOpposite = true;
                }
            }
        }

        if(doOpposite) {
            rigidbody.velocity = Vector3.zero;
            mXDir *= -1.0f;
            mForce.force = new Vector3(mXDir * moveForce, 0.0f, 0.0f);
        }
    }

    public override void PlayerCollide(PlayerController pc, CollisionFlags flags, ControllerColliderHit hit) {
        if((flags & CollisionFlags.Sides) != CollisionFlags.None) {
            rigidbody.velocity = Vector3.zero;
            mXDir = Mathf.Sign(transform.position.x - pc.transform.position.x);
            mForce.force = new Vector3(mXDir * moveForce, 0.0f, 0.0f);
        }
    }

    void OnSetState(EntityBase ent, int state) {
        switch(state) {
            case Animal.StateSpawning:
            case Animal.StateDespawning:
                mForce.force = new Vector3(0.0f, 0.0f, 0.0f);
                break;

            case Animal.StateNormal:
                mForce.force = new Vector3(mXDir * moveForce, 0.0f, 0.0f);
                break;
        }
    }

    void OnSummonInit(Animal animal, Player player) {
        mXDir = Mathf.Sign(transform.position.x - player.transform.position.x);
    }

    void OnSaveState(Animal animal, Player player, AnimalState state) {
        state.SetData(1, (object)mXDir);
    }

    void OnRestoreState(Animal animal, Player player, AnimalState state) {
        mXDir = (float)state.GetData(1);
        mForce.force = new Vector3(mXDir * moveForce, 0.0f, 0.0f);
    }
}
