using UnityEngine;
using System.Collections;

public class MonkeyController : MonoBehaviour {
    public tk2dTiledSprite tail;
    public tk2dBaseSprite tailEnd;

    private Animal mAnimal;

    void Awake() {
        mAnimal = GetComponent<Animal>();
        mAnimal.summonInitCallback += OnAnimalInit;
    }

    void OnAnimalInit(Animal animal, Player player) {
        //get cursor
        MonkeySummonCursor cursor = AnimalSummon.instance.GetCursor(mAnimal.spawnType) as MonkeySummonCursor;

        //sync tail and collider
        Vector3 cTailPos = cursor.tail.transform.localPosition;
        Vector3 ctailEndPos = cursor.tailEnd.transform.localPosition;

        tail.dimensions = cursor.tail.dimensions;

        cTailPos.z = tail.transform.localPosition.z;
        tail.transform.localPosition = cTailPos;

        ctailEndPos.z = tailEnd.transform.localPosition.z;
        tailEnd.transform.localPosition = ctailEndPos;

        BoxCollider boxColl = GetComponent<Collider>() as BoxCollider;
        boxColl.center = cursor.collideBounds.center;
        boxColl.size = cursor.collideBounds.size;
    }
}
