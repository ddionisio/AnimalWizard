using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SwitchBase : MonoBehaviour, IActionStateListener {
    public enum Type {
        Trigger,
        Sticky,
        Stay,
    }

    public enum State {
        off,
        on
    }

    public Type type = Type.Trigger;
    public tk2dSpriteAnimator anim;
    public float checkDelay = 0.1f;

    private State mState = State.off;
    private tk2dSpriteAnimationClip[] mAnimClips;
    private HashSet<Collider> mColliders = new HashSet<Collider>();
    private WaitForSeconds mWait;
    private bool mCheckStarted = false;
    private bool mStarted = false;
    private bool mRestore = false;

    public State state { get { return mState; } }

    protected virtual void StateChanged(State prevState) {
    }

    void OnTriggerEnter(Collider c) {
        mColliders.Clear();

        if(!mCheckStarted) {
            if(type == Type.Trigger) {
                StartCoroutine(DoCheck(State.on));
            }
            else if(mState == State.off)
                StartCoroutine(DoCheck(State.off));
        }
    }

    void OnTriggerStay(Collider c) {
        mColliders.Add(c);
    }

    void OnTriggerExit(Collider c) {
        mColliders.Remove(c);
    }

    protected virtual void OnEnable() {
        if(mStarted)
            anim.Play(mAnimClips[(int)mState]);
    }

    protected virtual void OnDisable() {
        mState = State.off;
        mCheckStarted = false;
        mColliders.Clear();
    }

    protected virtual void OnDestroy() {
        if(ActionStateManager.instance != null)
            ActionStateManager.instance.Unregister(this);
    }

    protected virtual void Awake() {
        mWait = new WaitForSeconds(checkDelay);
        mAnimClips = M8.tk2dUtil.GetSpriteClips(anim, typeof(State));
    }

    // Use this for initialization
    protected virtual void Start() {
        if(ActionStateManager.instance != null)
            ActionStateManager.instance.Register(this);

        mStarted = true;
        anim.Play(mAnimClips[(int)mState]);
    }

    void ChangeState(State toState) {
        if(mState != toState) {
            //Debug.Log("state: " + toState);

            State prev = mState;
            mState = toState;

            StateChanged(prev);
        }

        anim.Play(mAnimClips[(int)mState]);
    }

    IEnumerator DoCheck(State toState) {
        mCheckStarted = true;

        if(mRestore) {
            mState = toState;
            anim.Play(mAnimClips[(int)mState]);
            mRestore = false;
        }
        else {
            ChangeState(toState);
        }

        while(mCheckStarted) {
            yield return mWait;

            switch(type) {
                case Type.Trigger:
                    ChangeState(State.off);
                    mCheckStarted = false;
                    break;

                case Type.Sticky:
                    if(mColliders.Count > 0) {
                        ChangeState(State.on);
                        mCheckStarted = false;
                    }
                    break;

                case Type.Stay:
                    ChangeState(mColliders.Count > 0 ? State.on : State.off);
                    break;
            }
        }
    }

    public object ActionSave() {
        if(gameObject.activeSelf) {
            return mState;
        }
        else
            return null;
    }

    public void ActionRestore(object dat) {
        State toState = (State)dat;

        mColliders.Clear();
        mCheckStarted = false;
        StopAllCoroutines();

        mRestore = true;

        StartCoroutine(DoCheck(toState));
    }
}
