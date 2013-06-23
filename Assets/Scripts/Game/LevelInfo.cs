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

    private static LevelInfo mInstance;

    private SummonItem[] mSummonItems;

    public static LevelInfo instance { get { return mInstance; } }

    public SummonItem[] summonItems { get { return mSummonItems; } }

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
        }
    }
}