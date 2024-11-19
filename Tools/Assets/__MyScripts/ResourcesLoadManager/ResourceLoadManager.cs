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



    protected override void Awake()
    {

        DontDestroyOnLoad(this);

        Init();

        print("ResourceLoadManager Awake");
    }

    public void Init()
    {
        if (assetType == LoadAssetType.AssetBundle)
        {
            AssetBundleManager.Instance.Init();
        }
    }
    /// <summary>
    /// 从Resources文件夹下加载 例如 Csv/DataConfig 不需要后缀
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path"></param>
    /// <returns></returns>
    public T ResourceLoad<T>(string path) where T : UnityEngine.Object
    {
        return Resources.Load<T>(path);
    }

    /// <summary>
    /// 加载资源
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path">例如:Assets/Textures/myTexture.png</param>
    /// <returns></returns>
    public T Load<T>(string path) where T : UnityEngine.Object
    {
        if (string.IsNullOrEmpty(path)) return null;
        path = path.ToLower();
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

    // 同步加载AB
    T LoadAssetBundle<T>(string path) where T : UnityEngine.Object
    {
        return AssetBundleManager.Instance.LoadAssetBundle<T>(path);
    }

    /// <summary>
    /// 异步加载AB资源
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path"></param>
    /// <param name="loadAsset"></param>
    public void LoadAssetAsync<T>(string path,AssetBundleManager.LoadAsset<T> loadAsset) where T : UnityEngine.Object
    {
        AssetBundleManager.Instance.LoadAssetBundleAsync<T>(path,loadAsset);
    }

    

}
