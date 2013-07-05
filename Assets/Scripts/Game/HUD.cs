using UnityEngine;
using System.Collections;

public class HUD : MonoBehaviour {
    public GameObject summonItemPrefab;
    public GameObject totemItemPrefab;

    public GameObject deathMessage;

    public Transform summonItemHolder;

    public Transform totemItemHolder;
    public string totemEmptyRef;
    public string totemFilledRef;

    public NGUIPointAt goalPointer;

    private HUDSummonItem[] mSummons;
    private UISprite[] mTotems;
    
    public void RefreshCollection(Player player) {
        int ind = 0;
        for(; ind < player.collected; ind++) {
            mTotems[ind].spriteName = totemFilledRef;
            mTotems[ind].MakePixelPerfect();
        }

        bool goalActive = ind >= mTotems.Length;

        if(goalActive) {
            LevelInfo level = LevelInfo.instance;
            goalPointer.SetPOI(level.goal);

            SoundPlayerGlobal.instance.Play("exit");
        }
        else {
            goalPointer.SetPOI(null);

            for(; ind < mTotems.Length; ind++) {
                mTotems[ind].spriteName = totemEmptyRef;
                mTotems[ind].MakePixelPerfect();
            }
        }

        
    }

    public void RefreshSummons(Player player) {
        for(int i = 0; i < mSummons.Length; i++) {
            mSummons[i].UpdateCount(player.SummonGetAvailableCount(i));
        }
    }

    public void SetSummonSelect(int ind, bool select) {
        mSummons[ind].SetAsSelect(select);
    }
    
    void Awake() {
    }

    // Use this for initialization
    void Start() {
        //populate the summon items
        LevelInfo level = LevelInfo.instance;

        mSummons = new HUDSummonItem[level.summonItems.Length];

        for(int i = 0; i < mSummons.Length; i++) {
            GameObject itm = (GameObject)Instantiate(summonItemPrefab);
            itm.transform.parent = summonItemHolder;
            itm.transform.localPosition = Vector3.zero;
            itm.transform.localRotation = Quaternion.identity;
            itm.transform.localScale = Vector3.one;

            mSummons[i] = itm.GetComponent<HUDSummonItem>();
            mSummons[i].Init(i, level.summonItems[i]);
        }

        //populate totems
        int numTotems = level.totemCount;
        mTotems = new UISprite[numTotems];
        for(int i = 0; i < numTotems; i++) {
            GameObject itm = (GameObject)Instantiate(totemItemPrefab);
            itm.transform.parent = totemItemHolder;
            itm.transform.localPosition = Vector3.zero;
            itm.transform.localRotation = Quaternion.identity;
            itm.transform.localScale = Vector3.one;

            mTotems[i] = itm.GetComponentInChildren<UISprite>();
            mTotems[i].spriteName = totemEmptyRef;
            mTotems[i].MakePixelPerfect();
        }

        NGUILayoutBase.RefreshNow(transform);
    }

}
