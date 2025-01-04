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
        //public string province;//测试省份
    }

    public Button btn;

    public class DownLoadData
    {
        public int level;
    }

    #region 服务器返回数据
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
    public string testData;

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
        CallGetUserData();
    }

    void GetNameAndAvator(int curChapterIndex)
    {
        WX.GetSetting(new GetSettingOption()
        {
            success = (result) =>
            {
                if (result.authSetting.ContainsKey("scope.userInfo") && result.authSetting["scope.userInfo"])//如果授权,可以直接获取
                {
                    print("GetUserInfo 有授权");
                    WX.GetUserInfo(new GetUserInfoOption
                    {
                        success = (res) =>
                        {
                            SendDataToServer(res.userInfo.nickName, res.userInfo.avatarUrl, curChapterIndex);
                        }
                    });
                }
                else//没有授权需要手动授权
                {
                    //todo:生成按钮让玩家点击?
                    print("GetUserInfo 没有授权");
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
            //province = "测试省份",
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
        //获取到数据后,展示排行榜信息
        ShowRankUI(result.result);
    }

    void ShowRankUI(string result)
    {
        var response = LoadData(result);
        if (response == null)
        {
            print("服务器数据不对!,不能加载全国排行榜");
            return ;
        }

        //将排行榜名称和头像,关卡加载到UI
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
