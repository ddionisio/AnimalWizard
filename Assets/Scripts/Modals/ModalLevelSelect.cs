using UnityEngine;
using System.Collections;

public class ModalLevelSelect : UIController {
    public GameObject levelItemPrefab;

    public Transform levelItemHolder;

    public UIEventListener options;

    private UIGrid mLevelItemGrid;

    protected override void OnActive(bool active) {
    }

    protected override void OnOpen() {
    }

    protected override void OnClose() {
    }

    void Awake() {
        mLevelItemGrid = levelItemHolder.GetComponent<UIGrid>();


    }

    // Use this for initialization
    void Start() {
        LevelList levelList = LevelList.instance;

        int levelCount = levelList.levelCount;

        for(int i = 0; i < levelCount; i++) {
            GameObject go = (GameObject)GameObject.Instantiate(levelItemPrefab);
            go.name = i.ToString("D3");

            Transform t = go.transform;
            t.parent = levelItemHolder;
            t.localScale = Vector3.one;


            UIEventListener listener = go.GetComponent<UIEventListener>();
            listener.onClick += OnLevelClick;

            UIButton button = go.GetComponent<UIButton>();

            UILabel label = go.GetComponentInChildren<UILabel>();

            switch(levelList.GetLevelStatus(i)) {
                case LevelStatus.Locked:
                    label.text = levelList.levelLockedString;
                    button.isEnabled = false;
                    break;

                case LevelStatus.Unlocked:
                    label.text = levelList.GetLevelTitleUnlocked(i);
                    break;

                case LevelStatus.Complete:
                    label.text = levelList.GetLevelTitleCompleted(i);
                    break;
            }
        }

        mLevelItemGrid.repositionNow = true;

        options.onClick += OnOptionsClick;
    }

    // Update is called once per frame
    void Update() {

    }

    void OnLevelClick(GameObject go) {
        int levelInd = int.Parse(go.name);

        LevelList.instance.LoadLevel(levelInd);
    }

    void OnOptionsClick(GameObject go) {
        UIModalManager.instance.ModalOpen("options");
    }
}
