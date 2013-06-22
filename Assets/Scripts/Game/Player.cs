using UnityEngine;
using System.Collections;

public class Player : EntityBase {
    private PlayerController mController;
    private FollowCamera mFollowCamera;
        
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
    }

    // Use this for initialization
    protected override void Start() {
        base.Start();

        //initialize variables from other sources (for communicating with managers, etc.)
    }

    //void LateUpdate() {
    //}
}
