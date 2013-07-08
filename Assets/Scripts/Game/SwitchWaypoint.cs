using UnityEngine;
using System.Collections;

public class SwitchWaypoint : SwitchBase {
    public enum Action {
        None,
        Flip,
        True,
        False
    }

    public WaypointMover mover;

    public Action actPauseON;
    public Action actReverseON;

    protected override void StateChanged(State prevState) {
        switch(type) {
            case Type.Trigger:
                //only apply if on
                if(state == State.on)
                    ApplyState();
                break;

            default:
                ApplyState();
                break;
        }
    }

    void ApplyState() {
        switch(actPauseON) {
            case Action.Flip:
                mover.pause = !mover.pause;
                break;

            case Action.True:
                mover.pause = state == State.on ? true : false;
                break;

            case Action.False:
                mover.pause = state == State.on ? false : true;
                break;
        }

        switch(actReverseON) {
            case Action.Flip:
                mover.reverse = !mover.reverse;
                break;

            case Action.True:
                mover.reverse = state == State.on ? true : false;
                break;

            case Action.False:
                mover.reverse = state == State.on ? false : true;
                break;
        }
    }
}
