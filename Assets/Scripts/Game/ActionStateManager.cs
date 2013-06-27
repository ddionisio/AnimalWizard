using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public interface IActionStateListener {
    /// <summary>
    /// Return null to not save
    /// </summary>
    object ActionSave();
        
    void ActionRestore(object dat);
}

public class ActionStateManager : MonoBehaviour {
    public const int maxStates = 32;

    private struct Pair {
        public IActionStateListener listener;
        public object dat;
    }

    private static ActionStateManager mInstance = null;

    private HashSet<IActionStateListener> mListeners = new HashSet<IActionStateListener>();
    private Stack<List<Pair>> mStates = new Stack<List<Pair>>(maxStates);

    public static ActionStateManager instance { get { return mInstance; } }

    public void Register(IActionStateListener listener) {
        mListeners.Add(listener);
    }

    public void Unregister(IActionStateListener listener) {
        mListeners.Remove(listener);
    }

    public void Save() {
        List<Pair> newStates = new List<Pair>(mListeners.Count);
        foreach(IActionStateListener listener in mListeners) {
            object obj = listener.ActionSave();
            if(obj != null) {
                newStates.Add(new Pair() { listener = listener, dat = obj });
            }
        }

        if(newStates.Count > 0)
            mStates.Push(newStates);
    }

    public void RestoreLast() {
        if(mStates.Count > 0) {
            List<Pair> states = mStates.Pop();
            foreach(Pair pair in states) {
                //make sure listener still exists
                if(mListeners.Contains(pair.listener)) {
                    pair.listener.ActionRestore(pair.dat);
                }
            }
        }
    }

    void OnDestroy() {
        if(mInstance == this)
            mInstance = null;

        mListeners.Clear();
        mStates.Clear();
    }

    void Awake() {
        if(mInstance == null) {
            mInstance = this;
        }
    }
}
