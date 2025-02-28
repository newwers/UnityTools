using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using WeChatWASM;

public class WXCloundFunc : MonoBehaviour
{
    [Serializable]
    public class Data
    {
        public string avatarUrl;
        public string nickName;
        public int weekTime;
        public float level;
        //public string province;//²âÊÔÊ¡·Ý
    }

    public Button btn;

    public class DownLoadData
    {
        public int level;
    }

    #region ·þÎñÆ÷·µ»ØÊý¾Ý
    [Serializable]
    public class ServerData
    {
        public int code;
        public DataList[] data;
    }
    [Serializable]
    public class  DataList
    {
        public string _id;
        public string openid;
        public Data gamedata;
    }

    #endregion

    public GlobalRankManager globalRankManager;
    public string testData= "{\"code\":1,\"data\":[{\"_id\":\"a00247e96773fa69009cc03a78144c7a\",\"openid\":\"oa03u64dE56wRZawxsTvc9DL8G2k\",\"gamedata\":{\"avatarUrl\":\"测试头像地址3\",\"nickName\":\"测试用户名字3\",\"userInfo\":{\"appId\":\"wx31ea30b346ccda2b\",\"openId\":\"oa03u64dE56wRZawxsTvc9DL8G2k\"},\"weekTime\":1,\"level\":220}},{\"_id\":\"a00247e96773fa69009cc03a78144c6a\",\"openid\":\"oa03u64dE56wRZawxsTvc9DL8G1k\",\"gamedata\":{\"avatarUrl\":\"测试头像地址2\",\"nickName\":\"测试用户名字2\",\"userInfo\":{\"appId\":\"wx31ea30b346ccda1b\",\"openId\":\"oa03u64dE56wRZawxsTvc9DL8G1k\"},\"weekTime\":1,\"level\":110}},{\"_id\":\"a00247e96773fa69009cc03a78144c5a\",\"openid\":\"oa03u64dE56wRZawxsTvc9DL8GOk\",\"gamedata\":{\"avatarUrl\":\"https://thirdwx.qlogo.cn/mmopen/vi_32/PiajxSqBRaEKeoDznpVpMF1iaXpru8QV4ickKVxzWqesQouW7VDB8FEu3kK7e3tmHXL5LyOEpKQUQibuibGxeWDiaCFvk59ias27k1ic6gbOL0S1ZdP3ian0RnrTnqQ/132\",\"nickName\":\"借点时间\",\"userInfo\":{\"appId\":\"wx31ea30b346ccda0b\",\"openId\":\"oa03u64dE56wRZawxsTvc9DL8GOk\"},\"weekTime\":1,\"level\":3,\"time\":0}}]}";
    private string m_RankResult;
    private double m_Timer;
    private const double RefreshRankTime = 60;//60ÃëË¢ÐÂÒ»´Î

    void Start()
    {
        //SendDataToServer();
#if UNITY_EDITOR

        ShowRankUI(testData);
#endif
    }

    public void SendRankData(int curChapterIndex)
    {
        GetNameAndAvator(curChapterIndex);
    }

    public void ShowGlobalRank()
    {
        //Ò»·ÖÖÓË¢ÐÂÒ»´ÎÅÅÐÐ°ñ
        if ((m_Timer + RefreshRankTime) > Time.realtimeSinceStartupAsDouble)
        {
            Debug.Log("ÇëÇóÅÅÐÐ°ñÊý¾Ý");
            CallGetUserData();
            
        }
        else
        {
            Debug.Log("Ê¹ÓÃÅÅÐÐ°ñ»º´æÊý¾Ý");
            ShowRankUI(m_RankResult);
        }
    }

    void GetNameAndAvator(int curChapterIndex)
    {
        WX.GetSetting(new GetSettingOption()
        {
            success = (result) =>
            {
                if (result.authSetting.ContainsKey("scope.userInfo") && result.authSetting["scope.userInfo"])//Èç¹ûÊÚÈ¨,¿ÉÒÔÖ±½Ó»ñÈ¡
                {
                    print("GetUserInfo ÓÐÊÚÈ¨");
                    WX.GetUserInfo(new GetUserInfoOption
                    {
                        success = (res) =>
                        {
                            SendDataToServer(res.userInfo.nickName, res.userInfo.avatarUrl, curChapterIndex);
                        }
                    });
                }
                else//Ã»ÓÐÊÚÈ¨ÐèÒªÊÖ¶¯ÊÚÈ¨
                {
                    //todo:Éú³É°´Å¥ÈÃÍæ¼Òµã»÷?
                    print("GetUserInfo Ã»ÓÐÊÚÈ¨");
                    var rect = btn.transform as RectTransform;
                    var wxBtn = WX.CreateUserInfoButton((int)btn.transform.position.x,Screen.height - (int)btn.transform.position.y,
                        (int)rect.rect.width,(int)rect.rect.height,"zh_CN",false);
                    wxBtn.OnTap((WXUserInfoResponse userInfo) =>
                    {
                        SendDataToServer(userInfo.userInfo.nickName, userInfo.userInfo.avatarUrl, curChapterIndex);
                        wxBtn.Destroy();
                    });
                }
            },
            fail = (e) => {
                print("GetNameAndAvator fail:" + e.errMsg);
            },
            complete = (res) => {
                print("GetNameAndAvator complete");
            }
        });
    }

    private void SendDataToServer(string name,string avatar,int level)
    {
        Data data = new Data()
        {
            level = level,
            nickName = name,
            avatarUrl = avatar,
            //province = "²âÊÔÊ¡·Ý",
            weekTime = 1
        };
        CallSetUserData(data);
    }

    private void CallGetUserData()
    {
        print("CallGetUserData");
        DownLoadData downLoadData = new DownLoadData();
        downLoadData.level = 0;
        WeChatWASM.WX.cloud.CallFunction(new WeChatWASM.CallFunctionParam()
        {
            name = "DownLoadData",
            data = downLoadData,
            success = OnCallGetUserInfoFuncSuccess,
            fail = OnCallFuncFail,
            complete = OnCallFuncComplete
        });
    }


    private void CallSetUserData(Data data)
    {
        print("--- CallSetUserData UpLoadData ---");
        WeChatWASM.WX.cloud.CallFunction(new WeChatWASM.CallFunctionParam()
        {
            name = "UpLoadData",
            data = data,
            success = OnCallFuncSuccess,
            fail = OnCallFuncFail,
            complete = OnCallFuncComplete
        });
    }

    private void OnCallFuncComplete(GeneralCallbackResult result)
    {
        print("OnCallFuncComplete:" + result.errMsg);
    }

    private void OnCallFuncFail(GeneralCallbackResult result)
    {
        print("OnCallFuncFail:" + result.errMsg);
    }

    private void OnCallFuncSuccess(CallFunctionResult result)
    {
        print("OnCallFuncSuccess:" + result.result);
    }
    private void OnCallGetUserInfoFuncSuccess(CallFunctionResult result)
    {
        print("OnCallGetUserInfoFuncSuccess:" + result.result);
        m_RankResult = result.result;
        m_Timer = Time.realtimeSinceStartupAsDouble;
        //»ñÈ¡µ½Êý¾Ýºó,Õ¹Ê¾ÅÅÐÐ°ñÐÅÏ¢
        ShowRankUI(result.result);
    }

    void ShowRankUI(string result)
    {
        var response = LoadData(result);
        if (response == null)
        {
            print("·þÎñÆ÷Êý¾Ý²»¶Ô!,²»ÄÜ¼ÓÔØÈ«¹úÅÅÐÐ°ñ");
            return ;
        }

        //½«ÅÅÐÐ°ñÃû³ÆºÍÍ·Ïñ,¹Ø¿¨¼ÓÔØµ½UI
        globalRankManager.ShowRank(response.data);
    }

    private ServerData LoadData(string result)
    {
        var response = JsonUtility.FromJson<ServerData>(result);
        if (response.code == 1 && response.data != null)
        {
            return response;
        }
        return null;
    }

}
