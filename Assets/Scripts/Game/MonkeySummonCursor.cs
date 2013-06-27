using UnityEngine;
using System.Collections;

public class MonkeySummonCursor : AnimalSummonCursor {
    public tk2dTiledSprite tail;
    public tk2dBaseSprite tailEnd;

    public Bounds collideBounds;

    void OnDrawGizmos() {
        Gizmos.color = Color.magenta;

        Gizmos.DrawWireCube(transform.position + collideBounds.center, collideBounds.size);
    }
}
