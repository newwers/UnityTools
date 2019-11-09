using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// 资源加载管理
/// unity资源的加载有几种方式
/// 1.通过public的形式声明后,在inspector面板上赋值
/// 2.将资源放在Resources文件夹底下,通过Resource.Load的形式加载
/// 3.通过www方式从网络或者本地加载数据
/// 4.通过Assetbundle的形式加载
/// 5.通过UnityWebRequest的形式加载,这个新的加载方式替换原本www旧的加载方式
/// </summary>
public class ResourceLoadManager : MonoBehaviour {

    public static ResourceLoadManager Instance;

    public Dictionary<string, Texture2D> loadTexture2DResource = new Dictionary<string, Texture2D>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        LoadAllTexture2DResource();
    }
    /// <summary>
    /// 加载所有texture2d资源
    /// </summary>
    private void LoadAllTexture2DResource()
    {
        var allPath = FarmGameManager.Instance.m_TexturePathConfig.allTexturePath;
        foreach (var item in allPath)
        {
            StartCoroutine(LoadTexture2D(OnLoadSuccessTexture2DResource, item.path));
        }
    }

    private void OnLoadSuccessTexture2DResource(Texture2D texture2D,string path)
    {
        if (!loadTexture2DResource.ContainsKey(path))
        {
            loadTexture2DResource.Add(path, texture2D);
        }
    }


    /// <summary>
    /// 从Resources文件夹中加载
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public object Load(string path)
    {
        return Resources.Load(path);
    }

    /// <summary>
    /// 加载Sprite
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public Sprite LoadSprite(string path)
    {
        if (loadTexture2DResource.ContainsKey(path))
        {
            Texture2D texture2D = loadTexture2DResource[path];
            return Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), Vector2.zero);
        }
        return null;
    }

    /// <summary>
    /// 加载Texture2D
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public Texture2D LoadTexture2D(string path)
    {
        if (loadTexture2DResource.ContainsKey(path))
        {
            return loadTexture2DResource[path];
        }
        return null;
    }

    /// <summary>
    /// 获取本地的音频资源
    /// </summary>
    private IEnumerator LoadLocalAudioResource(Action<AudioClip> func)
    {
        //资源加载管理
        var uri = new System.Uri(Path.Combine(Application.dataPath, "_MyAudio/BGM-Ingame.wav"));
        print(uri);

        //这边有个注意,音频尽量用wav格式,然后Cool Edit导出时用的是Windows PCM的Wav 然后再设置Load type 为Streaming的类型(测试过,不改也可以加载)
        using (UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(uri.ToString(), AudioType.WAV))
        {
            yield return request.SendWebRequest();
            if (request.isNetworkError)
            {
                print(request.error);
            }
            else
            {
                AudioClip audioClip = DownloadHandlerAudioClip.GetContent(request);
                //这边获取到资源后,需要将资源传递出去
                func(audioClip);//回调
            }
        }
    }

    /// <summary>
    /// 加载Application.dataPath底下的数据
    /// </summary>
    /// <param name="func"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    private IEnumerator LoadTexture2D(Action<Texture2D,string> func,string path)
    {
        //资源加载管理
        var uri = new System.Uri(Path.Combine(Application.dataPath, path + ".png"));
        print(uri);

        //这边有个注意,音频尽量用wav格式,然后Cool Edit导出时用的是Windows PCM的Wav 然后再设置Load type 为Streaming的类型(测试过,不改也可以加载)
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(uri))
        {
            yield return request.SendWebRequest();
            if (request.isNetworkError)
            {
                print(request.error);
            }
            else
            {
                Texture2D texture2D = DownloadHandlerTexture.GetContent(request);
                //这边获取到资源后,需要将资源传递出去
                func(texture2D, path);//回调
            }
        }
    }

    /// <summary>
    /// 加载游戏预制体,只能通过AssetBundle加载
    /// </summary>
    /// <param name="func"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    private IEnumerator LoadGameObject(Action<GameObject, string> func, string path)
    {
        //资源加载管理
        var uri = new System.Uri(Path.Combine(Application.dataPath, path));
        print(uri);

        //这边有个注意,音频尽量用wav格式,然后Cool Edit导出时用的是Windows PCM的Wav 然后再设置Load type 为Streaming的类型(测试过,不改也可以加载)
        using (UnityWebRequest request = UnityWebRequest.Get(uri))
        {
            yield return request.SendWebRequest();
            if (request.isNetworkError)
            {
                print(request.error);
            }
            else
            {
                string[] names = path.Split('/');
                string name = names[names.Length - 1];
                GameObject go = DownloadHandlerAssetBundle.GetContent(request).LoadAsset<GameObject>(name);
                //这边获取到资源后,需要将资源传递出去
                func(go, path);//回调
            }
        }
    }

    [ContextMenu("测试资源加载")]
    public void TestLoad()
    {
        StartCoroutine(LoadLocalAudioResource(OnLoadSuccess));
    }

    private void OnLoadSuccess(AudioClip audioClip)
    {
        LogManager.Log("OnLoadSuccess!!,audio length = " + audioClip.length);
    }
}
