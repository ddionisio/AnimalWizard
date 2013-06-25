using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AnimalState {
    private int mEntityState;
    private Vector3 mPosition;
    private Quaternion mRotation;
    private Vector3 mScale;
    private Vector3 mVelocity; //for rigidbody
    private Vector3 mAngleVelocity; //for rigidbody

    private Dictionary<int, object> mData;

    public AnimalState(Animal animal) {
        mPosition = animal.transform.position;
        mRotation = animal.transform.rotation;
        mScale = animal.transform.localScale;
        if(animal.rigidbody != null && !animal.rigidbody.isKinematic) {
            mVelocity = animal.rigidbody.velocity; //for rigidbody
            mAngleVelocity = animal.rigidbody.angularVelocity; //for rigidbody
        }
        else {
            mVelocity = Vector3.zero;
            mAngleVelocity = Vector3.zero;
        }

        mEntityState = animal.state;

        mData = new Dictionary<int, object>();
    }

    public void SetData(int id, object obj) {
        mData.Add(id, obj);
    }

    public object GetData(int id) {
        return mData[id];
    }

    public void Restore(Animal animal) {
        animal.transform.position = mPosition;
        animal.transform.rotation = mRotation;
        animal.transform.localScale = mScale;
        if(animal.rigidbody != null && !animal.rigidbody.isKinematic) {
            animal.rigidbody.velocity = mVelocity; //for rigidbody
            animal.rigidbody.angularVelocity = mAngleVelocity; //for rigidbody
        }

        animal.state = mEntityState;
    }
}

public class Animal : EntityBase {
    public const int StateSpawning = 1;
    public const int StateSpawned = 2;
    public const int StateNormal = 3;
    public const int StateDespawning = 4;

    public const int maxState = 64;

    public const float despawnDelay = 0.5f;

    public delegate void GenericPlayerCallback(Animal animal, Player player);
    public delegate void GenericStateCallback(Animal animal, Player player, AnimalState state);

    public GameObject despawnGO;

    public event GenericPlayerCallback summonInitCallback;
    public event GenericStateCallback stateSaveCallback;
    public event GenericStateCallback stateRestoreCallback;

    private Queue<AnimalState> mStates = new Queue<AnimalState>(maxState);

    public int numSaveStates { get { return mStates.Count; } }

    public void SummonInit(Player player) {
        //we are requested to summon by player, use this to initialize movement data (for when spawn finishes) based on current player's and cursor's position
        if(summonInitCallback != null)
            summonInitCallback(this, player);
    }

    public void SaveState(Player player) {
        //only save state if we are in normal mode
        if(state == StateNormal) {
            AnimalState newAnimalState = new AnimalState(this);

            //request to save state
            if(stateSaveCallback != null) {
                stateSaveCallback(this, player, newAnimalState);
            }

            mStates.Enqueue(newAnimalState);
        }
    }

    public void RestoreState(Player player) {
        if(mStates.Count > 0) {
            AnimalState animalState = mStates.Dequeue();

            if(stateRestoreCallback != null) {
                stateRestoreCallback(this, player, animalState);
            }

            animalState.Restore(this);

            if(FSM != null)
                FSM.SendEvent(EntityEvent.Restore);
        }
    }
        
    //set despawn = false to cancel
    public void Despawn(bool despawn) {
        //call to start despawn, then make sure to call release when done (after animation, etc)

        if(despawn) {
            if(state == StateNormal)
                state = StateDespawning;
        }
        else {
            if(state == StateDespawning)
                state = StateNormal;
        }
    }

    protected override void OnDespawned() {
        //reset stuff here
        mStates.Clear();

        base.OnDespawned();
    }

    protected override void OnDestroy() {
        //dealloc here
        summonInitCallback = null;

        base.OnDestroy();
    }

    public override void SpawnFinish() {
        //start ai, player control, etc

        state = StateSpawned;

        //now to wait till state = StateNormal
    }

    protected override void SpawnStart() {
        //initialize some things

        state = StateSpawning;
    }

    protected override void Awake() {
        base.Awake();

        //initialize variables
        despawnGO.SetActive(false);
    }

    // Use this for initialization
    protected override void Start() {
        base.Start();

        //initialize variables from other sources (for communicating with managers, etc.)
    }

    protected override void StateChanged() {
        switch(prevState) {
            case StateDespawning:
                StopCoroutine("DoDespawn");
                despawnGO.SetActive(false);
                break;
        }

        switch(state) {
            case StateDespawning:
                StartCoroutine("DoDespawn");
                despawnGO.SetActive(true);
                break;
        }
    }

    IEnumerator DoDespawn() {
        yield return new WaitForSeconds(despawnDelay);

        if(state == StateDespawning)
            Release();
    }
}
