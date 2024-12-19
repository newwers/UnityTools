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

    void Start()
    {
        // ��ʼ��΢��С��ϷSDK
        WX.InitSDK(Callback);
    }

    private void Callback(int code)
    {
        print("��ʼ�� code:" + code);
        SetRankData();
    }

    // ��ʾ���а�
    public void ShowRank()
    {
        RankRawImage.gameObject.SetActive(true);
        var resolution = RankCanvasScaler.referenceResolution;
        var p = RankRawImage.transform.position;
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

    // �������а�����
    public void SetRankData()
    {
        OpenDataMessage data = new OpenDataMessage();
        data.type = "setUserRecord";
        data.score = ����Ѱ��_Main.instance.��ǰ�ؿ�����+1;

        string json = JsonUtility.ToJson(data);
        WX.GetOpenDataContext().PostMessage(json);
        print($"SetRankData type:{data.type},score:{data.score}");
    }
}
