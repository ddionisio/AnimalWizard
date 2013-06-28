using UnityEngine;
using System.Collections;

public class PlayerCollisionDeath : PlayerCollisionBase {
    public override void PlayerCollide(PlayerController pc, CollisionFlags flags, ControllerColliderHit hit) {
        //kill player!
        pc.player.state = Player.StateDead;
    }
}
