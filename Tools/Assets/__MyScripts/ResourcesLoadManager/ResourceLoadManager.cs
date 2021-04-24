using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
# if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 资源加载管理
/// unity资源的加载有几种方式
/// 1.通过public的形式声明后,在inspector面板上赋值
/// 2.将资源放在Resources文件夹底下,通过Resource.Load的形式加载
/// 3.通过www方式从网络或者本地加载数据
/// 4.通过Assetbundle的形式加载
/// 5.通过UnityWebRequest的形式加载,这个新的加载方式替换原本www旧的加载方式
/// </summary>
public class ResourceLoadManager : BaseMonoSingleClass<ResourceLoadManager> {

    public enum LoadAssetType
    {
        Assets,
        AssetBundle
    }
    

    public LoadAssetType assetType = LoadAssetType.Assets;

    

    private void Awake()
    {
        DontDestroyOnLoad(this);

        AssetBundleManager.Instance.Init();
    }

    public T Load<T>(string path) where T : UnityEngine.Object
    {
#if UNITY_EDITOR
        if (assetType == LoadAssetType.AssetBundle)
        {
            return LoadAssetBundle<T>(path);
        }

        //编辑器状态下使用AssetDataBase.
        T asset = AssetDatabase.LoadAssetAtPath<T>(path);
        if (asset == null)
        {
            Debug.LogError("资源路径:" + path + ",加载不到资源");
        }
        return asset;

#else
        //其他平台下,包括Windows,Android,通过AB包进行加载
        return LoadAssetBundle<T>(path);
#endif

    }

    // 同步加载资源
    T LoadAssetBundle<T>(string path) where T : UnityEngine.Object
    {
        return AssetBundleManager.Instance.LoadAssetBundle<T>(path);
    }

    void LoadAssetBundleAsync()
    {
        //todo:异步加载 ab
        //todo:异步加载资源
    }

    

}
