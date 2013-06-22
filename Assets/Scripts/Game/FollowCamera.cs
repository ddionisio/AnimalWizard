using UnityEngine;
using System.Collections;

public class FollowCamera : MonoBehaviour {

    public Camera mainCamera;
    public Transform focus;

    public float threshold = 0.25f;
    public float focusMaxDistance = 5.0f;
    public float delay = 0.1f;

    public Rect bounds;

    private Transform mTarget;
    private bool mFocusEnable;

    private float mThresholdSq;
    private float mFocusMaxDistanceSq;
    private Vector2 mLastTargetPos;
    private Vector2 mLastDirSign;

    private Vector3 mDestPos;
    private Vector3 mCurVel;

    public Transform target {
        get { return mTarget; }
        set {
            if(mTarget != value) {
                mTarget = value;

                if(mTarget != null) {
                    mLastTargetPos = mTarget.position;
                    mDestPos = mLastTargetPos;
                    mCurVel = Vector3.zero;
                    ApplyPos(mDestPos);
                }
            }
        }
    }

    public bool focusEnable {
        get { return mFocusEnable; }
        set {
            if(mFocusEnable != value) {
                mFocusEnable = value;

                if(focus != null) {
                    focus.gameObject.SetActive(mFocusEnable);

                    if(!mFocusEnable)
                        mDestPos = mLastTargetPos = focus.position;
                }
            }
        }
    }

    void Awake() {
        if(focus != null) {
            focus.gameObject.SetActive(mFocusEnable);
        }

        mThresholdSq = threshold * threshold;
        mFocusMaxDistanceSq = focusMaxDistance * focusMaxDistance;
    }

    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {
#if UNITY_EDITOR
        mThresholdSq = threshold * threshold;
        mFocusMaxDistanceSq = focusMaxDistance * focusMaxDistance;
#endif
        
        if(mTarget != null) {
            Vector2 targetPos;
            if(mFocusEnable && focus != null) {
                //TODO: find a better way to do this
                Vector3 lastCamPos = mainCamera.transform.position;
                mainCamera.transform.position = mTarget.position;
                focus.position = mainCamera.ScreenToWorldPoint(Input.mousePosition);
                mainCamera.transform.position = lastCamPos;

                Vector2 dFocus = focus.position - mTarget.position;
                if(dFocus.sqrMagnitude > mFocusMaxDistanceSq) {
                    dFocus = dFocus.normalized * focusMaxDistance;
                    targetPos = mTarget.position;
                    targetPos += dFocus;
                }
                else {
                    targetPos = focus.position;
                }

                ApplyPos(targetPos);
            }
            else {
                targetPos = mTarget.position;

                Vector2 delta = new Vector2(targetPos.x - mDestPos.x, targetPos.y - mDestPos.y);
                Vector2 deltaLast = targetPos - mLastTargetPos;

                if(delta.sqrMagnitude > mThresholdSq) {
                    mDestPos.x += deltaLast.x;
                    mDestPos.y += deltaLast.y;

                    Vector2 deltaSign = new Vector2(Mathf.Sign(deltaLast.x), Mathf.Sign(deltaLast.y));
                    if(mLastDirSign.x != deltaSign.x) {
                        mLastDirSign.x = deltaSign.x;
                        mDestPos.x = targetPos.x;
                    }

                    if(mLastDirSign.y != deltaSign.y) {
                        mLastDirSign.y = deltaSign.y;
                        mDestPos.y = targetPos.y;
                    }
                }

                Vector3 newPos = Vector3.SmoothDamp(transform.position, mDestPos, ref mCurVel, delay);

                ApplyPos(newPos);

                mLastTargetPos = targetPos;
            }
        }
    }

    void OnDrawGizmos() {
        if(bounds.width > 0 && bounds.height > 0) {
            Gizmos.color = Color.cyan;

            Gizmos.DrawWireCube(new Vector3(bounds.x, bounds.y, 0.0f), new Vector3(bounds.width * 0.5f, bounds.height * 0.5f, 1.0f));
        }
    }

    private void ApplyPos(Vector3 pos) {
        if(bounds.width > 0.0f && bounds.height > 0.0f) {
            float camWidthRatio = mainCamera.pixelWidth / mainCamera.pixelHeight;
            float camHalfWidth = mainCamera.orthographicSize * camWidthRatio;
            float camHalfHeight = mainCamera.orthographicSize;

            float hBoundW = bounds.width * 0.5f;
            float hBoundH = bounds.height * 0.5f;

            float boundLeft = bounds.x - hBoundW*0.5f;
            float boundBot = bounds.y - hBoundH*0.5f;

            if(pos.x - camHalfWidth < boundLeft) {
                pos.x = boundLeft + camHalfWidth;
            }
            else if(pos.x + camHalfWidth > boundLeft + hBoundW) {
                pos.x = boundLeft + hBoundW - camHalfWidth;
            }

            if(pos.y - camHalfHeight < boundBot) {
                pos.y = boundBot + camHalfHeight;
            }
            else if(pos.y + camHalfHeight > boundBot + hBoundH) {
                pos.y = boundBot + hBoundH - camHalfHeight;
            }
        }

        transform.position = pos;
    }
}
