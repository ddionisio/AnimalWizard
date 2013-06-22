using UnityEngine;
using System.Collections;

public abstract class PlayerCollisionBase : MonoBehaviour {
    public abstract void PlayerCollide(PlayerController pc, ControllerColliderHit hit);
}
