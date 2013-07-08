using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LevelInfo : MonoBehaviour {
    [System.Serializable]
    public class SummonItem {
        public string type;
        public int max;
    }

    [System.Serializable]
    public class Data {
        public SummonItem[] summons;

        //some other BS
    }

    public TextAsset file;

    public Transform totemHolder;

    public Transform goal;

    private static LevelInfo mInstance;

    private tk2dTileMap mTileMap;

    private SummonItem[] mSummonItems;

    public static LevelInfo instance { get { return mInstance; } }

    public tk2dTileMap tileMap { get { return mTileMap; } }

    public int totemCount { get { return totemHolder != null ? totemHolder.childCount : 0; } }

    public SummonItem[] summonItems { get { return mSummonItems; } }

    public Bounds levelBounds {
        get {
            float boundW = mTileMap.width * mTileMap.data.tileSize.x;
            float boundH = mTileMap.height * mTileMap.data.tileSize.y;

            return new Bounds(
                new Vector3(mTileMap.data.tileOrigin.x + boundW * 0.5f, mTileMap.data.tileOrigin.y + boundH * 0.5f),
                new Vector3(boundW, boundH));
        }
    }

    public SummonItem GetSummonItem(string type) {
        foreach(SummonItem itm in mSummonItems) {
            if(itm.type == type)
                return itm;
        }

        return null;
    }

    void OnDestroy() {
        if(mInstance == this) {
            mInstance = null;
        }
    }

    void Awake() {
        if(mInstance == null) {
            mInstance = this;

            fastJSON.JSON.Instance.Parameters.UseExtensions = false;
            Data fileData = fastJSON.JSON.Instance.ToObject<Data>(file.text);

            mSummonItems = fileData.summons;

            mTileMap = GetComponentInChildren<tk2dTileMap>();
        }
    }
}
