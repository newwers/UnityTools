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

    public ResourceLoadManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
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
            yield return request.Send();
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
