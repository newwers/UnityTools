using System;
using UnityEngine;
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
    public class DataList
    {
        public string _id;
        public string openid;
        public Data gamedata;
    }

    #endregion

    public GlobalRankManager globalRankManager;
    public string testData = "{\"code\":1,\"data\":[{\"_id\":\"a00247e96773fa69009cc03a78144c7a\",\"openid\":\"oa03u64dE56wRZawxsTvc9DL8G2k\",\"gamedata\":{\"avatarUrl\":\"测试头像地址3\",\"nickName\":\"测试用户名字3\",\"userInfo\":{\"appId\":\"wx31ea30b346ccda2b\",\"openId\":\"oa03u64dE56wRZawxsTvc9DL8G2k\"},\"weekTime\":1,\"level\":220}},{\"_id\":\"a00247e96773fa69009cc03a78144c6a\",\"openid\":\"oa03u64dE56wRZawxsTvc9DL8G1k\",\"gamedata\":{\"avatarUrl\":\"测试头像地址2\",\"nickName\":\"测试用户名字2\",\"userInfo\":{\"appId\":\"wx31ea30b346ccda1b\",\"openId\":\"oa03u64dE56wRZawxsTvc9DL8G1k\"},\"weekTime\":1,\"level\":110}},{\"_id\":\"a00247e96773fa69009cc03a78144c5a\",\"openid\":\"oa03u64dE56wRZawxsTvc9DL8GOk\",\"gamedata\":{\"avatarUrl\":\"https://thirdwx.qlogo.cn/mmopen/vi_32/PiajxSqBRaEKeoDznpVpMF1iaXpru8QV4ickKVxzWqesQouW7VDB8FEu3kK7e3tmHXL5LyOEpKQUQibuibGxeWDiaCFvk59ias27k1ic6gbOL0S1ZdP3ian0RnrTnqQ/132\",\"nickName\":\"借点时间\",\"userInfo\":{\"appId\":\"wx31ea30b346ccda0b\",\"openId\":\"oa03u64dE56wRZawxsTvc9DL8GOk\"},\"weekTime\":1,\"level\":3,\"time\":0}}]}";
    private string m_RankResult;
    private double m_Timer;
    private const double RefreshRankTime = 60;

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
        if ((m_Timer + RefreshRankTime) > Time.realtimeSinceStartupAsDouble)
        {
            Debug.Log("获取全国排行榜数据");
            CallGetUserData();
        }
        else
        {
            Debug.Log("使用缓存全国排行榜数据");
            ShowRankUI(m_RankResult);
        }
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
                    var wxBtn = WX.CreateUserInfoButton((int)btn.transform.position.x, Screen.height - (int)btn.transform.position.y,
                        (int)rect.rect.width, (int)rect.rect.height, "zh_CN", false);
                    wxBtn.OnTap((WXUserInfoResponse userInfo) =>
                    {
                        SendDataToServer(userInfo.userInfo.nickName, userInfo.userInfo.avatarUrl, curChapterIndex);
                        wxBtn.Destroy();
                    });
                }
            },
            fail = (e) =>
            {
                print("GetNameAndAvator fail:" + e.errMsg);
            },
            complete = (res) =>
            {
                print("GetNameAndAvator complete");
            }
        });
    }

    private void SendDataToServer(string name, string avatar, int level)
    {
        Data data = new Data()
        {
            level = level + 1,
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
        m_RankResult = result.result;
        m_Timer = Time.realtimeSinceStartupAsDouble;
        //获取到数据后,展示排行榜信息
        ShowRankUI(result.result);
    }

    void ShowRankUI(string result)
    {
        var response = LoadData(result);
        if (response == null)
        {
            print("服务器数据不对!,不能加载全国排行榜");
            return;
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
