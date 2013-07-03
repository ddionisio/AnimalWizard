using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Animal : EntityBase, IActionStateListener {
    public class ActionState : ActionStateTransform.State {
        private int mEntityState;

        public ActionState(Animal animal)
            : base(animal.transform) {
                mEntityState = animal.state;
        }

        public void Restore(Animal animal) {
            base.Restore();

            animal.state = mEntityState;
        }
    }

    public const int StateSpawning = 1;
    public const int StateSpawned = 2;
    public const int StateNormal = 3;
    public const int StateDespawning = 4;

    public const int maxState = 64;

    public const float despawnDelay = 0.5f;

    public delegate void GenericPlayerCallback(Animal animal, Player player);

    public GameObject despawnGO;

    public event GenericPlayerCallback summonInitCallback;

    public void SummonInit(Player player) {
        //we are requested to summon by player, use this to initialize movement data (for when spawn finishes) based on current player's and cursor's position
        if(summonInitCallback != null)
            summonInitCallback(this, player);
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
        if(rigidbody != null && !rigidbody.isKinematic) {
            rigidbody.velocity = Vector3.zero;
            rigidbody.angularVelocity = Vector3.zero;
        }

        if(ActionStateManager.instance != null) {
            ActionStateManager.instance.Unregister(this);
        }

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
        if(ActionStateManager.instance != null) {
            ActionStateManager.instance.Register(this);
        }

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

    public object ActionSave() {
        if(gameObject.activeSelf && state == StateNormal) {
            if(FSM != null)
                FSM.SendEvent(EntityEvent.Save);

            return new ActionState(this);
        }
        else {
            return null;
        }
    }

    public void ActionRestore(object dat) {
        ((ActionState)dat).Restore(this);

        if(FSM != null)
            FSM.SendEvent(EntityEvent.Restore);
    }
}
