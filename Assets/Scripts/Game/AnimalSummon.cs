using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//make sure the pool type matches from LevelInfo
//add each cursor as a child, make sure the game object name matches the types from LevelInfo!
public class AnimalSummon : MonoBehaviour {
    public delegate void OnSelect(AnimalSummon summon, string prevType);

    public Transform cursorHolder;

    public event OnSelect selectCallback;

    private PoolController mPool;

    private static AnimalSummon mInstance = null;
    private string mCurType = null;
    private AnimalSummonCursor mCurCursor = null;
    private Dictionary<string, AnimalSummonCursor> mCursors;

    public static AnimalSummon instance { get { return mInstance; } }

    public string curType { get { return mCurType; } }
    public AnimalSummonCursor curCursor { get { return mCurCursor; } }

    public AnimalSummonCursor GetCursor(string type) {
        return mCursors[type];
    }

    /// <summary>
    /// set type to null or empty to unselect
    /// </summary>
    public void Select(string type, Transform attach) {
        if(mCurType != type) {
            string prevType = mCurType;
            mCurType = type;

            //return and disable cursor
            if(mCurCursor != null) {
                mCurCursor.attach = null;
                mCurCursor.gameObject.SetActive(false);
            }

            if(!string.IsNullOrEmpty(mCurType)) {
                mCurCursor = mCursors[type];

                mCurCursor.attach = attach;
                
                if(!mCursors.TryGetValue(mCurType, out mCurCursor))
                    Debug.LogError("No cursor for: " + mCurType);

                mCurCursor.gameObject.SetActive(true);
            }
            else {
                mCurCursor = null;
            }

            if(selectCallback != null)
                selectCallback(this, prevType);
        }
    }

    public Animal SummonCurrent(Player player, Vector3 position) {
        Animal animal = null;

        //this will summon the currently selected type
        if(!string.IsNullOrEmpty(mCurType)) {
            Transform spawned = mPool.Spawn(mCurType, null, null, position);
            animal = spawned.GetComponent<Animal>();
            animal.SummonInit(player);
        }

        return animal;
    }

    void OnDestroy() {
        if(mInstance == this)
            mInstance = null;

        selectCallback = null;
    }

    void Awake() {
        if(mInstance == null) {
            mInstance = this;

            mPool = GetComponent<PoolController>();

            //cursor init
            AnimalSummonCursor[] cursors = cursorHolder.GetComponentsInChildren<AnimalSummonCursor>(true);
            mCursors = new Dictionary<string, AnimalSummonCursor>(cursors.Length);
            foreach(AnimalSummonCursor cursor in cursors) {
                cursor.gameObject.SetActive(false);
                mCursors.Add(cursor.gameObject.name, cursor);
            }
        }
    }

    void Start() {
        //animal pool init
        LevelInfo level = LevelInfo.instance;
        foreach(LevelInfo.SummonItem dat in level.summonItems) {
            mPool.Expand(dat.type, dat.max);
        }
    }
}
