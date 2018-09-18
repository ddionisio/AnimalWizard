using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;

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

            Data fileData = new Data();

            var jsonNode = JSON.Parse(file.text);

            if(jsonNode["summons"] != null) {
                var summonsNode = jsonNode["summons"].AsArray;
                fileData.summons = new SummonItem[summonsNode.Count];
                for(int i = 0; i < summonsNode.Count; i++) {
                    string type = summonsNode[i]["type"].Value;
                    int max = summonsNode[i]["max"].AsInt;
                    fileData.summons[i] = new SummonItem() { type = type, max = max };
                }
            }
            else
                fileData.summons = new SummonItem[0];

            mSummonItems = fileData.summons;

            mTileMap = GetComponentInChildren<tk2dTileMap>();
        }
    }
}
