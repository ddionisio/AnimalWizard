using UnityEngine;
using System.Collections;

public class Animal : EntityBase {
    public const int StateSpawning = 1;
    public const int StateSpawned = 2;
    public const int StateNormal = 3;
    public const int StateDespawning = 4;

    public delegate void OnSummonInit(Animal animal, Player player);

    public event OnSummonInit summonInitCallback;

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
        //reset stuff here

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
    }

    protected override void SpawnStart() {
        //initialize some things

        state = StateSpawning;
    }

    protected override void Awake() {
        base.Awake();

        //initialize variables
    }

    // Use this for initialization
    protected override void Start() {
        base.Start();

        //initialize variables from other sources (for communicating with managers, etc.)
    }
}
