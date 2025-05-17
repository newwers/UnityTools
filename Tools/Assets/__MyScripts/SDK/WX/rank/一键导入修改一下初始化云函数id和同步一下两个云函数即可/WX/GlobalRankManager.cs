using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Z.UI;

public class GlobalRankManager : MonoBehaviour
{
    public ListView listView;

    WXCloundFunc.DataList[] m_Data;
    Dictionary<string,Texture2D> m_AvatarTextures = new Dictionary<string, Texture2D>();

    private void Start()
    {
        listView.onItemRender.AddListener(OnShowItem);
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

        LoadAvatar(data.gamedata.avatarUrl,avatar_rawimage);
        name_textmeshprougui.text = data.gamedata.nickName;
        level_textmeshprougui.text = data.gamedata.level.ToString();
        rank_textmeshprougui.text = (index+1).ToString();
    }

    public void ShowRank(WXCloundFunc.DataList[] data)
    {
        listView.numItems = (uint)data.Length;
        m_Data = data;


    }

    public void HideRank()
    {

    }


    IEnumerator LoadAvatarFromUrl(string url,RawImage rawImage)
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

    void LoadAvatar(string url,RawImage rawImage)
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
