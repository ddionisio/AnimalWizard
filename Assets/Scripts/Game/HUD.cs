using UnityEngine;
using System.Collections;

public class HUD : MonoBehaviour {
    public GameObject summonItemPrefab;

    public Transform summonItemHolder;

    private HUDSummonItem[] mSummons;

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

        NGUILayoutBase.RefreshNow(transform);
    }

}
