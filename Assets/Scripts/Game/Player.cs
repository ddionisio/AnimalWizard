using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Player : EntityBase, IActionStateListener {
    public const int StateNormal = 1;
    public const int StateDead = 2;

    public delegate void GenericCallback(Player player);

    public class ActionState {
        private Vector2 mPosition;
        private Animal mAnimalSummon = null;
        private int mState;

        public ActionState(Player player) {
            Vector3 pos = player.transform.position;
            mPosition = pos;

            mState = player.state;
            mAnimalSummon = player.mLastSummoning;
        }

        public void Restore(Player player) {
            Debug.Log("restoring player state");

            player.transform.position = mPosition;

            //release animals that are currently being summoned
            foreach(Animal summonAnimal in player.mSummoning) {
                summonAnimal.Release();
            }

            Debug.Log("removed animal summonings: " + player.mSummoning.Count);

            player.mSummoning.Clear();

            if(mAnimalSummon != null && !mAnimalSummon.isReleased) {
                Debug.Log("restore remove animal: " + mAnimalSummon.gameObject.name);
                mAnimalSummon.Release();
            }

            player.SummonSetSelect(-1);

            player.state = mState;

            if(player.restoreStateCallback != null)
                player.restoreStateCallback(player);
        }
    }

    public event GenericCallback restoreStateCallback;

    private static Player mInstance;

    private FollowCamera mFollowCamera;

    private HUD mHUD;

    private Dictionary<string, List<Animal>> mSummons; //tracking summoned animals
    private List<Animal> mSummoning; //tracking animals currently summoning
    private int mSummonCurSelect = -1;

    private Animal mLastSummoning;

    public static Player instance { get { return mInstance; } }

    public FollowCamera followCamera { get { return mFollowCamera; } }

    public Dictionary<string, List<Animal>> summons { get { return mSummons; } } //only for restore stuff

    public int summonCurSelect { get { return mSummonCurSelect; } }

    public int summonCount { get { return LevelInfo.instance.summonItems.Length; } }

    public int SummonGetAvailableCount(int ind) {
        LevelInfo level = LevelInfo.instance;
        LevelInfo.SummonItem summonItem = level.summonItems[ind];

        List<Animal> animals = null;
        if(!mSummons.TryGetValue(summonItem.type, out animals)) {
            Debug.LogError("Type not found: " + summonItem.type);
            return 0;
        }

        return summonItem.max - animals.Count;
    }

    public int SummonGetAvailableCount(string type) {
        List<Animal> animals = null;
        if(!mSummons.TryGetValue(type, out animals)) {
            Debug.LogError("Type not found: " + type);
            return 0;
        }

        LevelInfo level = LevelInfo.instance;

        return level.GetSummonItem(type).max - animals.Count;
    }

    /// <summary>
    /// set ind = -1 to unselect
    /// </summary>
    public void SummonSetSelect(int ind) {
        if(mSummonCurSelect != ind) {
            if(mSummonCurSelect != -1) {
                mHUD.SetSummonSelect(mSummonCurSelect, false);
            }

            mSummonCurSelect = ind;

            if(mSummonCurSelect != -1) {
                LevelInfo level = LevelInfo.instance;
                LevelInfo.SummonItem summonItem = level.summonItems[ind];

                AnimalSummon.instance.Select(summonItem.type, mFollowCamera.focusCursor);

                mHUD.SetSummonSelect(mSummonCurSelect, true);
            }
            else {
                AnimalSummon.instance.Select(null, null);
            }
        }
    }

    public void SummonCurrent() {
        //can only summon if cursor is valid
        if(AnimalSummon.instance.curCursor != null && AnimalSummon.instance.curCursor.isValid) {
            Animal animal = AnimalSummon.instance.SummonCurrent(this, mFollowCamera.focusCursor.position);

            List<Animal> animals = null;
            if(!mSummons.TryGetValue(animal.spawnType, out animals)) {
                Debug.LogError("Type not found: " + animal.spawnType);
                return;
            }

            mSummoning.Add(animal);

            animal.setStateCallback += OnAnimalSummoningSetState;
            animal.releaseCallback += OnAnimalRelease;

            animals.Add(animal);

            mHUD.RefreshSummons(this);
        }
    }

    public void RestoreLastState() {
        //if only one restore, do it only if we are dead
        if(ActionStateManager.instance.stateCount == 1) {
            if(state == StateDead)
                ActionStateManager.instance.RestoreLast(true);
        }
        else
            ActionStateManager.instance.RestoreLast();
    }

    void ClearData() {
        state = StateInvalid;

        //reset stuff here
        mSummons.Clear();
        mSummoning.Clear();

        if(ActionStateManager.instance != null)
            ActionStateManager.instance.Unregister(this);
    }

    protected override void OnDespawned() {
        ClearData();

        base.OnDespawned();
    }

    protected override void OnDestroy() {
        ClearData();

        if(mInstance == this)
            mInstance = null;
                
        //dealloc here
        restoreStateCallback = null;

        base.OnDestroy();
    }

    protected override void StateChanged() {
        switch(state) {
            case StateNormal:
                mFollowCamera.target = transform;
                mFollowCamera.focusEnable = true;
                break;

            case StateDead:
                Debug.Log("dead");
                mFollowCamera.target = null;
                mFollowCamera.focusEnable = false;
                break;
        }
    }

    public override void SpawnFinish() {
        //start ai, player control, etc
        ActionStateManager.instance.Register(this);
                
        state = StateNormal;

        ActionStateManager.instance.Save(); //when no other states are recoverable if we die
    }

    protected override void SpawnStart() {
        //initialize some things
    }

    protected override void Awake() {
        if(mInstance == null) {
            mInstance = this;

            base.Awake();

            autoSpawnFinish = true;

            //initialize variables
            GameObject camGO = GameObject.FindGameObjectWithTag("MainCamera");
            mFollowCamera = camGO.GetComponent<FollowCamera>();

            GameObject hudGO = GameObject.FindGameObjectWithTag("HUD");
            mHUD = hudGO.GetComponent<HUD>();
        }
    }

    // Use this for initialization
    protected override void Start() {
        base.Start();

        //initialize variables from other sources (for communicating with managers, etc.)
        LevelInfo level = LevelInfo.instance;

        //level bounds
        mFollowCamera.bounds = level.bounds;

        //initialize summon container
        int maxSummonable = 0;

        mSummons = new Dictionary<string, List<Animal>>(level.summonItems.Length);
        foreach(LevelInfo.SummonItem itm in level.summonItems) {
            maxSummonable += itm.max;
            mSummons[itm.type] = new List<Animal>(itm.max);
        }

        mSummoning = new List<Animal>(maxSummonable);
    }

    void OnApplicationFocus(bool focus) {
        SummonSetSelect(-1);
    }

    //void LateUpdate() {
    //}
        
    //this is only during summoning until it gets to normal state
    void OnAnimalSummoningSetState(EntityBase ent, int state) {
        switch(state) {
            case Animal.StateNormal:
                Animal animal = ent as Animal;

                Debug.Log("saving player state animal: "+animal.gameObject.name);

                ent.setStateCallback -= OnAnimalSummoningSetState;

                mSummoning.Remove(animal);

                //save state
                mLastSummoning = animal;
                ActionStateManager.instance.Save();
                break;
        }
    }

    void OnAnimalRelease(EntityBase ent) {
        mSummons[ent.spawnType].Remove(ent as Animal);
        ent.setStateCallback -= OnAnimalSummoningSetState;
        ent.releaseCallback -= OnAnimalRelease;

        mHUD.RefreshSummons(this);
    }

    void OnDrawGizmosSelected() {
        Gizmos.color = Color.magenta;
    }

    public object ActionSave() {
        ActionState newAction = new ActionState(this);
        mLastSummoning = null;
        return newAction;
    }

    public void ActionRestore(object dat) {
        ((ActionState)dat).Restore(this);
    }
}
