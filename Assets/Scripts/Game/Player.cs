using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerState {
    private Vector2 mPosition;
    private Animal mAnimalSummon = null;
    private List<RigidState> mRigids = null;

    public PlayerState(Player player, Animal animalSummon) {
        Vector3 pos = player.transform.position;
        mPosition = pos;

        mAnimalSummon = animalSummon;

        foreach(KeyValuePair<string, List<Animal>> dat in player.summons) {
            foreach(Animal animal in dat.Value) {
                if(animal != animalSummon)
                    animal.SaveState(player);
            }
        }

        pos.z -= 1.0f;
        RaycastHit[] hits = Physics.SphereCastAll(pos, player.saveRigidRadius, Vector3.forward, 2.0f, player.saveRigidMask);
        if(hits.Length > 0) {
            mRigids = new List<RigidState>(hits.Length);
            foreach(RaycastHit hit in hits) {
                if(hit.collider.rigidbody != null && !hit.collider.rigidbody.isKinematic)
                    mRigids.Add(new RigidState(hit.collider.rigidbody));
            }
        }
    }

    public void Restore(Player player) {
        player.transform.position = mPosition;

        foreach(KeyValuePair<string, List<Animal>> dat in player.summons) {
            foreach(Animal animal in dat.Value) {
                if(animal != mAnimalSummon)
                    animal.RestoreState(player);
            }
        }

        if(mAnimalSummon != null && !mAnimalSummon.isReleased) {
            Debug.Log("restore remove animal: " + mAnimalSummon.gameObject.name);
            mAnimalSummon.Release();
        }

        if(mRigids != null) {
            foreach(RigidState rstate in mRigids) {
                rstate.Restore();
            }
        }
    }
}

public class RigidState {
    private Rigidbody mBody;
    private Vector3 mPosition;
    private Quaternion mRotation;
    private Vector3 mScale;
    private Vector3 mVelocity; //for rigidbody
    private Vector3 mAngleVelocity; //for rigidbody

    public RigidState(Rigidbody body) {
        mBody = body;
        mPosition = mBody.transform.position;
        mRotation = mBody.transform.rotation;
        mScale = mBody.transform.localScale;
        mVelocity = mBody.velocity; //for rigidbody
        mAngleVelocity = mBody.angularVelocity; //for rigidbody
    }

    public void Restore() {
        if(mBody != null) {
            mBody.transform.position = mPosition;
            mBody.transform.rotation = mRotation;
            mBody.transform.localScale = mScale;
            mBody.velocity = mVelocity; //for rigidbody
            mBody.angularVelocity = mAngleVelocity; //for rigidbody
        }
    }
}

public class Player : EntityBase {
    public delegate void GenericCallback(Player player);

    public float saveRigidRadius; //for when saving states, check surrounding bodies
    public LayerMask saveRigidMask; //which layers are checked to save bodies

    public event GenericCallback restoreStateCallback;

    private PlayerController mController;
    private FollowCamera mFollowCamera;

    private HUD mHUD;

    private Dictionary<string, List<Animal>> mSummons; //tracking summoned animals
    private List<Animal> mSummoning; //tracking animals currently summoning
    private int mSummonCurSelect = -1;

    private Stack<PlayerState> mStates = new Stack<PlayerState>(Animal.maxState);

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
        if(mStates.Count > 0) {
            Debug.Log("restoring player state");

            SummonSetSelect(-1);

            PlayerState playerState = mStates.Pop();
            playerState.Restore(this);

            //release animals that are currently being summoned
            foreach(Animal summonAnimal in mSummoning) {
                summonAnimal.Release();
            }

            Debug.Log("removed animal summonings: " + mSummoning.Count);

            mSummoning.Clear();

            if(restoreStateCallback != null)
                restoreStateCallback(this);
        }
    }

    protected override void OnDespawned() {
        //reset stuff here
        mSummons.Clear();
        mSummoning.Clear();
        mController.inputEnabled = false;

        base.OnDespawned();
    }

    protected override void OnDestroy() {
        //dealloc here
        restoreStateCallback = null;

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
        int maxSummonable = 0;

        mSummons = new Dictionary<string, List<Animal>>(level.summonItems.Length);
        foreach(LevelInfo.SummonItem itm in level.summonItems) {
            maxSummonable += itm.max;
            mSummons[itm.type] = new List<Animal>(itm.max);
        }

        mSummoning = new List<Animal>(maxSummonable);
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
                mStates.Push(new PlayerState(this, animal));
                break;
        }
    }

    void OnAnimalRelease(EntityBase ent) {
        mSummons[ent.spawnType].Remove(ent as Animal);
        ent.releaseCallback -= OnAnimalRelease;

        mHUD.RefreshSummons(this);
    }

    void OnDrawGizmosSelected() {
        Gizmos.color = Color.magenta;

        if(saveRigidRadius > 0.0f)
            Gizmos.DrawWireSphere(transform.position, saveRigidRadius);
    }
}
