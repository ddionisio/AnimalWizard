using UnityEngine;
using System.Collections;

public class RavenController : MonoBehaviour {
    public enum State {
        Standby,
        Move,
        Attached
    }

    public float speed;
    public tk2dBaseSprite sprite;
    public Collider subCollider;

    private LayerMask mCollideMask;

    private Vector2 mDir;
    private Transform mAttach;
    private Vector3 mLocalAttachPoint;
    private Vector3 mLocalAttachNormal;

    private Animal mAnimal;
    private State mState = State.Standby;
    private float mSpriteSizeX;

    private Vector3 mAttachLastLocalPos;
    private Quaternion mAttachLastLocalRot;


    void Awake() {
        mAnimal = GetComponent<Animal>();

        mAnimal.summonInitCallback += OnAnimalInit;
        mAnimal.setStateCallback += OnAnimalSetState;
        mAnimal.releaseCallback += OnAnimalRelease;

        subCollider.isTrigger = true;

        mSpriteSizeX = sprite.GetBounds().size.x;
    }

    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void FixedUpdate() {
        Vector3 movePos;
        float dt = Time.fixedDeltaTime;

        switch(mState) {
            case State.Move:
                movePos = transform.position;

                float dist = speed * dt;

                RaycastHit hit;
                if(Physics.Raycast(movePos, mDir, out hit, dist + mSpriteSizeX, mCollideMask)) {
                    mAttach = hit.collider.transform;
                    mAttachLastLocalPos = mAttach.localPosition;
                    mAttachLastLocalRot = mAttach.localRotation;

                    Vector3 ofs = hit.normal * mSpriteSizeX;

                    Matrix4x4 mtx = mAttach.worldToLocalMatrix;

                    mLocalAttachPoint = mtx.MultiplyPoint(hit.point + ofs);
                    mLocalAttachNormal = mtx.MultiplyVector(hit.normal);

                    mDir = -hit.normal;

                    ApplyDir();

                    movePos = hit.point + ofs;

                    mState = State.Attached;

                    subCollider.isTrigger = false;
                }
                else {
                    movePos.x += mDir.x * dist;
                    movePos.y += mDir.y * dist;
                }

                transform.position = movePos;
                break;

            case State.Attached:
                if(mAttach != null && (mAttach.localPosition != mAttachLastLocalPos || mAttach.localRotation != mAttachLastLocalRot)) {
                    mAttachLastLocalPos = mAttach.localPosition;
                    mAttachLastLocalRot = mAttach.localRotation;

                    Matrix4x4 mtx = mAttach.localToWorldMatrix;
                    movePos = mtx.MultiplyPoint(mLocalAttachPoint);
                    mDir = -mtx.MultiplyVector(mLocalAttachNormal);

                    ApplyDir();

                    transform.position = movePos;
                }
                break;
        }
    }

    void OnAnimalInit(Animal animal, Player player) {
        //get cursor
        RavenSummonCursor cursor = AnimalSummon.instance.GetCursor(mAnimal.spawnType) as RavenSummonCursor;

        mCollideMask = cursor.wallMask;
        mDir = cursor.curDir;
        transform.position = cursor.spritePos;

        ApplyDir();
    }

    void ApplyDir() {
        transform.right = mDir;

        sprite.FlipY = M8.MathUtil.CheckSide(Vector2.up, mDir) == M8.MathUtil.Side.Left;

        if(transform.up.y < 0.0f) {
            subCollider.transform.up = -transform.up;
        }
        else {
            subCollider.transform.up = transform.up;
        }
    }

    void OnAnimalRelease(EntityBase ent) {
        mAttach = null;
        subCollider.isTrigger = true;
        mState = State.Standby;
        sprite.FlipY = false;
    }

    void OnAnimalSetState(EntityBase ent, int state) {
        switch(state) {
            case Animal.StateNormal:
                if(mState == State.Standby)
                    mState = State.Move;
                break;
        }
    }
}
