using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LevelList : MonoBehaviour {
    public const string levelSceneStateKey = "level";

    [System.Serializable]
    public class Item {
        public string title;
        public string cutscene;

    }

    [System.Serializable]
    public class Data {
        public string lockedString;
        public string unlockStringFormat;
        public string completeStringFormat;

        public Item[] items;
    }

    public TextAsset file;

    private static LevelList mInstance;

    private Data mData;

    public static LevelList instance { get { return mInstance; } }

    public LevelStatus GetLevelStatus(int levelInd) {
        //first level is always unlocked
        LevelStatus ret = (LevelStatus)UserData.instance.GetInt("lvls" + levelInd, (int)LevelStatus.Locked);
        return levelInd == 0 && ret == LevelStatus.Locked ? LevelStatus.Unlocked : ret;
    }

    public void SetLevelStatus(int levelInd, LevelStatus status) {
        UserData.instance.SetInt("lvls" + levelInd, (int)status);
    }

    public int levelCount { get { return mData.items.Length; } }

    public string levelLockedString { get { return mData.lockedString; } }

    public string GetLevelCutscene(int levelInd) {
        return mData.items[levelInd].cutscene;
    }

    public string GetLevelTitle(int levelInd) {
        return mData.items[levelInd].title;
    }

    public string GetLevelTitleUnlocked(int levelInd) {
        return string.Format(mData.unlockStringFormat, levelInd + 1, mData.items[levelInd].title);
    }

    public string GetLevelTitleCompleted(int levelInd) {
        return string.Format(mData.completeStringFormat, levelInd + 1, mData.items[levelInd].title);
    }

    void OnDestroy() {
        if(mInstance == this)
            mInstance = null;
    }

    void Awake() {
        if(mInstance == null) {
            mInstance = this;

            fastJSON.JSON.Instance.Parameters.UseExtensions = false;
            mData = fastJSON.JSON.Instance.ToObject<Data>(file.text);
        }
    }
}
