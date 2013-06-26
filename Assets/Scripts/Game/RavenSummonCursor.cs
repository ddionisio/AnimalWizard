using UnityEngine;
using System.Collections;

public class RavenSummonCursor : AnimalSummonCursor {
    public Transform line;
    public tk2dBaseSprite arrow;

    public LayerMask wallMask;

    private Player mPlayer;

    private Color mLineDefaultColor;
    private Color mArrowDefaultColor;

    private float mMinLength;

    private float mCursorLength;

    private Vector2 mCurDir;
    private float mCurLength;
    private bool mValid;

    public override bool isValid {
        get {
            return mValid;
        }
    }

    public Vector2 curDir {
        get {
            return mCurDir;
        }
    }

    public Vector2 spritePos {
        get {
            Vector2 pos = mPlayer.transform.position;
            return pos + mCurDir * mCursorLength;
        }
    }

    void OnEnable() {
        mValid = false;
        ApplyValidity(mValid);
    }

    protected override void OnDisable() {
        line.renderer.material.SetColor("modColor", mLineDefaultColor);
        arrow.color = mArrowDefaultColor;

        base.OnDisable();
    }

    protected override void OnTriggerEnter(Collider c) {

    }

    protected override void OnTriggerExit(Collider c) {

    }

    protected override void Awake() {
        base.Awake();

        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        mPlayer = playerGO.GetComponent<Player>();

        mLineDefaultColor = line.renderer.material.GetColor("modColor");
        mArrowDefaultColor = arrow.color;

        //assume local.y is 0 and x > 0, also sprite facing right
        mCursorLength = sprite.transform.localPosition.x;

        mMinLength = mCursorLength + arrow.GetBounds().size.x;
    }

    protected override void FixedUpdate() {
        //update rigidbody to attach
        //if(mAttach != null)
        //rigidbody.MovePosition(mAttach.position);

        RaycastHit hit;
        bool isHit = false;

        Vector2 pos = mPlayer.transform.position;
        Vector2 mousePos = attach.position;

        Vector2 dpos = mousePos - pos;
        mCurLength = dpos.magnitude;

        if(mCurLength > 0.0f) {
            mCurDir = dpos / mCurLength;

            isHit = Physics.Raycast(pos, mCurDir, out hit, mCurLength, wallMask);

            if(isHit) {
                Vector2 hitPos = hit.point;
                mCurLength = (hitPos - pos).magnitude;
            }

            //
            sprite.transform.right = mCurDir;

            arrow.transform.up = mCurDir;

            line.up = mCurDir;

            Vector3 lineS = line.localScale;
            lineS.y = mCurLength;
            line.localScale = lineS;
        }

        line.position = new Vector3(pos.x, pos.y, line.position.z);

        Vector2 arrowPos = pos + mCurDir * mCurLength;
        arrow.transform.position = new Vector3(arrowPos.x, arrowPos.y, arrow.transform.position.z);

        Vector2 spritePos = pos + mCurDir * mCursorLength;
        sprite.transform.position = new Vector3(spritePos.x, spritePos.y, sprite.transform.position.z);

        sprite.FlipY = M8.MathUtil.CheckSide(Vector2.up, mCurDir) == M8.MathUtil.Side.Left;

        bool valid = mCurLength >= mMinLength && isHit;
        if(mValid != valid) {
            mValid = valid;
            ApplyValidity(mValid);
        }
    }

    private void ApplyValidity(bool valid) {
        if(valid) {
            sprite.color = defaultColor;
            arrow.color = mArrowDefaultColor;
            line.renderer.material.SetColor("modColor", mLineDefaultColor);

        }
        else {
            sprite.color = invalidColor;
            arrow.color = invalidColor;
            line.renderer.material.SetColor("modColor", invalidColor);
        }
    }
}
