using UnityEngine;
using System.Collections;

public class ActionStateWaypointMover : MonoBehaviour, IActionStateListener {
    private WaypointMover mWPMover;

    void OnDestroy() {
        if(ActionStateManager.instance != null)
            ActionStateManager.instance.Unregister(this);
    }

    void Awake() {
        mWPMover = GetComponent<WaypointMover>();
    }

    void Start() {
        ActionStateManager.instance.Register(this);
    }

    public object ActionSave() {
        if(!gameObject.activeSelf)
            return null;

        if(mWPMover != null)
            return new WaypointMover.SaveState(mWPMover);
        else
            return new ActionStateTransform.State(transform);
    }

    public void ActionRestore(object dat) {
        if(mWPMover != null)
            ((WaypointMover.SaveState)dat).Restore();
        else
            ((ActionStateTransform.State)dat).Restore();
    }
}
