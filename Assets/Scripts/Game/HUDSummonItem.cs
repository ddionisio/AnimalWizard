using UnityEngine;
using System.Collections;

public class HUDSummonItem : MonoBehaviour {

    public UISprite icon;
    public UILabel bind;
    public UILabel count;

    public Color activeColor = Color.white;
    public Color inactiveColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

    private int mCurCount = -1;

    public void Init(int ind, LevelInfo.SummonItem summonInfo) {
        icon.spriteName = summonInfo.type + "_icon";
        icon.color = inactiveColor;
        icon.MakePixelPerfect();

        bind.text = (ind + 1).ToString(); //TODO: actual binding when customizing keys

        UpdateCount(summonInfo.max);
    }

    public void SetAsSelect(bool select) {
        icon.color = select && mCurCount > 0 ? activeColor : inactiveColor;
    }

    public void UpdateCount(int num) {
        if(mCurCount != num) {
            mCurCount = num;
            count.text = num.ToString();

            if(mCurCount == 0)
                icon.color = inactiveColor;
        }
    }
}
