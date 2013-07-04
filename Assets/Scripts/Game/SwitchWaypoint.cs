using UnityEngine;
using System.Collections;

public class SwitchWaypoint : SwitchBase {
    public enum Action {
        None,
        Flip,
        Active,
        Inactive
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

            case Action.Active:
                mover.pause = state == State.on ? true : false;
                break;

            case Action.Inactive:
                mover.pause = state == State.on ? false : true;
                break;
        }

        switch(actReverseON) {
            case Action.Flip:
                mover.reverse = !mover.reverse;
                break;

            case Action.Active:
                mover.reverse = state == State.on ? true : false;
                break;

            case Action.Inactive:
                mover.reverse = state == State.on ? false : true;
                break;
        }
    }
}
