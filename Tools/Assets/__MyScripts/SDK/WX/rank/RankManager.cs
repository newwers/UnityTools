using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WeChatWASM;

[Serializable]
public class OpenDataMessage
{
    public string type;
    public int score;
}

public class RankManager : MonoBehaviour
{
    

    public RawImage RankRawImage;
    public CanvasScaler RankCanvasScaler;
    public WXCloundFunc wXCloundFunc;

    void Start()
    {
        // 初始化微信小游戏SDK
        WX.InitSDK(Callback);
    }

    private void Callback(int code)
    {
        print("初始化 code:" + code);
        int CurChapterIndex = StorageSystem.LoadIntFromPlayerPrefs("当前游戏关卡",1);
        SetRankData(CurChapterIndex);
    }

    // 显示排行榜
    public void ShowRank()
    {
        RankRawImage.gameObject.SetActive(true);
        var resolution = RankCanvasScaler.referenceResolution;
        var p = RankRawImage.transform.position;
        print($"width:{(int)RankRawImage.rectTransform.rect.width},height:{(int)RankRawImage.rectTransform.rect.height}");
        WX.ShowOpenData(RankRawImage.texture, (int)p.x, Screen.height - (int)p.y, (int)RankRawImage.rectTransform.rect.width, (int)RankRawImage.rectTransform.rect.height);
        OpenDataMessage data = new OpenDataMessage();
        data.type = "showFriendsRank";
        string json = JsonUtility.ToJson(data);
        WX.GetOpenDataContext().PostMessage(json);
        print("ShowRank");
    }

    public void HideRank()
    {
        RankRawImage.gameObject.SetActive(false);
        WX.HideOpenData();
        print("HideRank");
    }

    // 设置排行榜数据
    public void SetRankData(int curChapterIndex)
    {
        OpenDataMessage data = new OpenDataMessage();
        data.type = "setUserRecord";
        data.score = curChapterIndex;

        string json = JsonUtility.ToJson(data);
        WX.GetOpenDataContext().PostMessage(json);
        print($"SetRankData type:{data.type},score:{data.score}");

        SetCloundRank(curChapterIndex);
    }

    void SetCloundRank(int curChapterIndex)
    {
        wXCloundFunc.SendRankData(curChapterIndex);
    }
}
