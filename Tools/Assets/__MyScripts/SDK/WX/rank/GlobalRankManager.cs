using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using WeChatWASM;
using Z.UI;

public class GlobalRankManager : MonoBehaviour
{
    public ListView listView;
    public UIReferenceComponent selfRankUI;

    WXCloundFunc.DataList[] m_Data;
    Dictionary<string, Texture2D> m_AvatarTextures = new Dictionary<string, Texture2D>();
    private RawImage m_avatar_rawimage;
    private TextMeshProUGUI m_name_textmeshprougui;
    private TextMeshProUGUI m_level_textmeshprougui;
    private TextMeshProUGUI m_rank_textmeshprougui;

    private void Start()
    {
        listView.onItemRender.AddListener(OnShowItem);

        m_avatar_rawimage = selfRankUI.GetUI<UnityEngine.UI.RawImage>("avatar_RawImage");
        m_name_textmeshprougui = selfRankUI.GetUI<TMPro.TextMeshProUGUI>("name_TextMeshProUGUI");
        m_level_textmeshprougui = selfRankUI.GetUI<TMPro.TextMeshProUGUI>("Level_TextMeshProUGUI");
        m_rank_textmeshprougui = selfRankUI.GetUI<TMPro.TextMeshProUGUI>("rank_TextMeshProUGUI");
    }

    private void OnShowItem(int index, UIReferenceComponent ui)
    {
        if (index >= m_Data.Length)
        {
            return;
        }

        var data = m_Data[index];

        UnityEngine.UI.RawImage avatar_rawimage = ui.GetUI<UnityEngine.UI.RawImage>("avatar_RawImage");
        TMPro.TextMeshProUGUI name_textmeshprougui = ui.GetUI<TMPro.TextMeshProUGUI>("name_TextMeshProUGUI");
        TMPro.TextMeshProUGUI level_textmeshprougui = ui.GetUI<TMPro.TextMeshProUGUI>("Level_TextMeshProUGUI");
        TMPro.TextMeshProUGUI rank_textmeshprougui = ui.GetUI<TMPro.TextMeshProUGUI>("rank_TextMeshProUGUI");

        LoadAvatar(data.gamedata.avatarUrl, avatar_rawimage);
        name_textmeshprougui.text = data.gamedata.nickName;
        level_textmeshprougui.text = data.gamedata.level.ToString();
        rank_textmeshprougui.text = (index + 1).ToString();
    }

    public void ShowRank(WXCloundFunc.DataList[] data)
    {
        listView.numItems = (uint)data.Length;
        m_Data = data;

        GetSelfUseInfo();
    }

    void GetSelfUseInfo()
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
                            for (int i = 0; i < m_Data.Length; i++)
                            {
                                if (m_Data[i].gamedata.nickName == res.userInfo.nickName && m_Data[i].gamedata.avatarUrl == res.userInfo.avatarUrl)
                                {
                                    LoadSelfRank(m_Data[i], i);
                                }
                            }
                            print($"m_SelfUserName:{res.userInfo.nickName},m_SelfUserAvatarUrl:{res.userInfo.avatarUrl} ");
                        }
                    });
                }
                else//没有授权需要手动授权
                {
                    //生成按钮让玩家点击?
                    print("GetUserInfo 没有授权");
                    //var rect = btn.transform as RectTransform;
                    //var wxBtn = WX.CreateUserInfoButton((int)btn.transform.position.x, Screen.height - (int)btn.transform.position.y,
                    //    (int)rect.rect.width, (int)rect.rect.height, "zh_CN", false);
                    //wxBtn.OnTap((WXUserInfoResponse userInfo) =>
                    //{
                    //    m_SelfUserName = userInfo.userInfo.nickName;
                    //    m_SelfUserAvatarUrl = userInfo.userInfo.avatarUrl;
                    //    print($"m_SelfUserName:{m_SelfUserName},m_SelfUserAvatarUrl:{m_SelfUserAvatarUrl} ");
                    //    SendDataToServer(userInfo.userInfo.nickName, userInfo.userInfo.avatarUrl, curChapterIndex);
                    //    wxBtn.Destroy();
                    //});
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

    void LoadSelfRank(WXCloundFunc.DataList data, int rank)
    {
        print("加载自己全国排行榜排名:" + rank);
        LoadAvatar(data.gamedata.avatarUrl, m_avatar_rawimage);
        m_name_textmeshprougui.text = data.gamedata.nickName;
        m_level_textmeshprougui.text = data.gamedata.level.ToString();
        m_rank_textmeshprougui.text = (rank + 1).ToString();
    }

    public void HideRank()
    {

    }


    IEnumerator LoadAvatarFromUrl(string url, RawImage rawImage)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            print("不能加载头像地址:" + url);
        }
        else
        {
            var tex = DownloadHandlerTexture.GetContent(request);
            m_AvatarTextures[url] = tex;
            rawImage.texture = tex;
            //var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new UnityEngine.Vector2(0.5f, 0.5f));
            //img.sprite = sprite;
        }

    }

    void LoadAvatar(string url, RawImage rawImage)
    {
        Texture2D avatar = null;
        if (m_AvatarTextures.TryGetValue(url, out avatar))
        {
            rawImage.texture = avatar;
        }
        else
        {
            StartCoroutine(LoadAvatarFromUrl(url, rawImage));
        }

    }

}
