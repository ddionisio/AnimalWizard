using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;

public class LevelList : MonoBehaviour {
    public const string sceneEnding = "end";
    public const string sceneLevelSelect = "levelSelect";
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
        LevelStatus ret = (LevelStatus)UserData.instance.GetInt(GetLevelStatusUserDataKey(levelInd), (int)LevelStatus.Locked);
        return levelInd == 0 && ret == LevelStatus.Locked ? LevelStatus.Unlocked : ret;
    }

    public void SetLevelStatus(int levelInd, LevelStatus status) {
        UserData.instance.SetInt(GetLevelStatusUserDataKey(levelInd), (int)status);
    }

    public int levelCount { get { return mData.items.Length; } }

    public string levelLockedString { get { return mData.lockedString; } }

    public string GetLevelStatusUserDataKey(int levelInd) {
        return "lvls" + levelInd;
    }

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

    public void LoadLevel(int levelInd) {
        SceneState.instance.SetGlobalValue(levelSceneStateKey, levelInd, false);

        //Debug.Log("level: " + levelInd);
        string cutscene = GetLevelCutscene(levelInd);

        if(!string.IsNullOrEmpty(cutscene)) {
            Main.instance.sceneManager.LoadScene(cutscene);
            //cutscene should load the level after its done
        }
        else {
            //go straight to play
            Main.instance.sceneManager.LoadLevel(levelInd);
        }
    }

    public void CompleteCurrentLevel() {
        //save level as complete and unlock the next one
        int curLevel = SceneState.instance.GetGlobalValue(levelSceneStateKey);

        UserData.instance.SetInt(GetLevelStatusUserDataKey(curLevel), (int)LevelStatus.Complete);

        UserData.instance.SetInt(GetLevelStatusUserDataKey(curLevel+1), (int)LevelStatus.Unlocked);

        //all level complete?
        if(curLevel + 1 >= mData.items.Length) {
            Main.instance.sceneManager.LoadScene(sceneEnding);
        }
        else {
            //then load that level
            LoadLevel(curLevel + 1);
        }
    }

    void OnDestroy() {
        if(mInstance == this)
            mInstance = null;
    }

    void Awake() {
        if(mInstance == null) {
            mInstance = this;
            
            mData = new Data();

            var json = JSON.Parse(file.text);

            string lockedString = json["lockedString"].Value;
            string unlockStringFormat = json["unlockStringFormat"].Value;
            string completeStringFormat = json["completeStringFormat"].Value;

            Item[] items;
            if(json["items"] != null) {
                var itemsNode = json["items"].AsArray;
                items = new Item[itemsNode.Count];
                for(int i = 0; i < itemsNode.Count; i++) {
                    string title = itemsNode[i]["title"].Value;
                    string cutscene = itemsNode[i]["cutscene"].Value;
                    items[i] = new Item() { title=title, cutscene=cutscene };
                }
            }
            else
                items = new Item[0];

            mData.lockedString = lockedString;
            mData.unlockStringFormat = unlockStringFormat;
            mData.completeStringFormat = completeStringFormat;
            mData.items = items;
        }
    }
}
