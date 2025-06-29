using System;
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

    public bool isShow = false;

    void Start()
    {
        // 初始化微信小游戏SDK
        WX.InitSDK(Callback);
    }

    private void Callback(int code)
    {
        print("初始化 code:" + code);
        int CurChapterIndex = StorageSystem.LoadIntFromPlayerPrefs("ChapterIndex");
        //SetRankData(CurChapterIndex);//初始默认数据
    }

    // 显示排行榜
    public void ShowRank()
    {
        RankRawImage.gameObject.SetActive(true);
        var resolution = RankCanvasScaler.referenceResolution;
        var p = RankRawImage.transform.position;
        int width = (int)RankRawImage.rectTransform.rect.width;
        int height = (int)RankRawImage.rectTransform.rect.height;
        int y = Screen.height - (int)p.y - height / 2;
        int x = (int)p.x - width / 2;
        WX.ShowOpenData(RankRawImage.texture, x, y, width, height);
        OpenDataMessage data = new OpenDataMessage();
        data.type = "showFriendsRank";
        string json = JsonUtility.ToJson(data);
        WX.GetOpenDataContext().PostMessage(json);
        print($"ShowRank x:{x},y:{y},width:{width},height:{height}");
        isShow = true;
    }

    public void HideRank()
    {
        RankRawImage.gameObject.SetActive(false);
        WX.HideOpenData();
        print("HideRank");
        isShow = false;
    }

    // 设置排行榜数据
    public void SetRankData(int curChapterIndex)
    {
        OpenDataMessage data = new OpenDataMessage();
        data.type = "setUserRecord";
        data.score = curChapterIndex + 1;

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
