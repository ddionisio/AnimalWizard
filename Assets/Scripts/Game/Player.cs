using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Player : EntityBase {

    private PlayerController mController;
    private FollowCamera mFollowCamera;

    private HUD mHUD;

    private Dictionary<string, List<Animal>> mSummons; //tracking summoned animals
    private int mSummonCurSelect = -1;

    public FollowCamera followCamera { get { return mFollowCamera; } }

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

            animal.releaseCallback += OnAnimalRelease;

            animals.Add(animal);

            mHUD.RefreshSummons(this);
        }
    }

    protected override void OnDespawned() {
        //reset stuff here
        mController.inputEnabled = false;

        base.OnDespawned();
    }

    protected override void OnDestroy() {
        //dealloc here

        base.OnDestroy();
    }

    public override void SpawnFinish() {
        //start ai, player control, etc
        mController.inputEnabled = true;

        mFollowCamera.target = transform;
        mFollowCamera.focusEnable = true;
    }

    protected override void SpawnStart() {
        //initialize some things
    }

    protected override void Awake() {
        base.Awake();

        autoSpawnFinish = true;

        //initialize variables
        mController = GetComponent<PlayerController>();

        GameObject camGO = GameObject.FindGameObjectWithTag("MainCamera");
        mFollowCamera = camGO.GetComponent<FollowCamera>();

        GameObject hudGO = GameObject.FindGameObjectWithTag("HUD");
        mHUD = hudGO.GetComponent<HUD>();
    }

    // Use this for initialization
    protected override void Start() {
        base.Start();

        //initialize variables from other sources (for communicating with managers, etc.)
        LevelInfo level = LevelInfo.instance;

        //level bounds
        mFollowCamera.bounds = level.bounds;

        //initialize summon container
        mSummons = new Dictionary<string, List<Animal>>(level.summonItems.Length);
        foreach(LevelInfo.SummonItem itm in level.summonItems)
            mSummons[itm.type] = new List<Animal>(itm.max);
    }

    //void LateUpdate() {
    //}

    void OnAnimalRelease(EntityBase ent) {
        mSummons[ent.spawnType].Remove(ent as Animal);
        ent.releaseCallback -= OnAnimalRelease;

        mHUD.RefreshSummons(this);
    }
}
