using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class AssetBundleManager : BaseSingleClass<AssetBundleManager>
{
    public delegate void LoadAsset<T>(T asset);

    struct ABInfo
    {
        /// <summary>
        /// 包名,在 GetAllDependencies 时,使用
        /// </summary>
        public string abPackageName;
        /// <summary>
        /// 包结尾后缀名
        /// </summary>
        public string abVariant;
        /// <summary>
        /// 包名全称.在AssetBundle.LoadFromFile时,使用
        /// </summary>
        public string abFullPath;

        public AssetBundle ab;

        public ABInfo(string abPackageName, string abVariant, string abFullPath,AssetBundle ab)
        {
            this.abPackageName = abPackageName;
            this.abVariant = abVariant;
            this.abFullPath = abFullPath;
            this.ab = ab;
        }
    }


    Dictionary<string, ABInfo> m_LoadAbDic = new Dictionary<string, ABInfo>();

    AssetBundle m_RootAB;
    AssetBundleManifest m_RootManifest;
    bool m_IsLoaded = false;

    /// <summary>
    /// 包后缀,要注意是需要逗号
    /// </summary>
    public string abVariant = ".ab";

    string m_RootPath
    {
        get
        {
            return Application.streamingAssetsPath + "/AssetBundles/";
        }
    }

    string m_Platform
    {
        get
        {
#if UNITY_ANDROID
            return "Andorid";
#elif UNITY_IOS
            return "IOS";
#else
            return "PC";
#endif
        }
    }


    public void Init()
    {
        LoadManifest();
        Debug.Log("AssetBundle 初始化完毕");
    }

    void LoadManifest()
    {
        if (m_IsLoaded)
        {
            return;
        }
        m_RootAB = AssetBundle.LoadFromFile(m_RootPath + m_Platform + "/PC");
        if (m_RootAB == null)
        {
            Debug.LogError("主 Manifest 加载失败");
            return;
        }
        m_RootManifest = m_RootAB.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        m_IsLoaded = true;
    }

    /// <summary>
    /// 获取依赖
    /// </summary>
    /// <param name="abName"></param>
    /// <returns></returns>
    string[] GetDepend(string abName)
    {
        if (m_RootManifest == null)
        {
            Debug.LogError("manifest对象为mull,未进行初始化");
            return null;
        }
        return m_RootManifest.GetAllDependencies(abName);
    }

    /// <summary>
    /// 加载依赖
    /// </summary>
    /// <param name="abName"></param>
    void LoadDepend(string abName)
    {
        string[] abNames = GetDepend(abName);//这边需要从Assets开始
        if (abNames == null)
        {
            Debug.LogError("获取不到依赖文件");
            return;
        }
        foreach (var item in abNames)
        {
            AddLoadABDic(item);
        }
        
    }

    ABInfo AddLoadABDic(string name)
    {
        if (!m_LoadAbDic.ContainsKey(name))
        {
            ABInfo info = new ABInfo(name, abVariant, m_RootPath + m_Platform + "/" + name, null);
            AssetBundle ab = AssetBundle.LoadFromFile(info.abFullPath);//这边需要全称
            if (ab != null)
            {
                info.ab = ab;
                m_LoadAbDic.Add(name, info);
                Debug.Log("加载依赖文件:" + name);
                return info;
            }
        }
        return default;
    }


    /// <summary>
    /// 同步资源加载
    /// </summary>
    /// <typeparam name="T">这边的资源路径就填 Assets的相对路径即可</typeparam>
    /// <param name="path"></param>
    /// <returns></returns>
    public T LoadAssetBundle<T>(string path) where T : UnityEngine.Object
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }
        path = path.ToLower();

        string[] str = path.Split('/');

        string name;
        if (str.Length == 0)
        {
            name = str[0];
        }
        else
        {
            name = str[str.Length - 1];
        }

        int index = path.LastIndexOf("/");

        string abFileName = path.Substring(0, index) + abVariant;

        LoadDepend(abFileName);

        if (m_LoadAbDic.ContainsKey(abFileName))
        {
            return m_LoadAbDic[abFileName].ab.LoadAsset<T>(name);
        }
        ABInfo info = AddLoadABDic(abFileName);
        Debug.Log("加载AB文件:" + abFileName);

        if (info.ab != null)
        {
            return info.ab.LoadAsset<T>(name);
        }


        return null;
    }

    #region 异步加载


    /// <summary>
    /// 异步加载AssetBundle资源
    /// </summary>
    /// <typeparam name="T">资源类型</typeparam>
    /// <param name="path">资源路径（Assets的相对路径）</param>
    /// <param name="callback">加载完成回调</param>
    public void LoadAssetBundleAsync<T>(string path, LoadAsset<T> callback) where T : UnityEngine.Object
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            callback?.Invoke(null);
            return;
        }

        path = path.ToLower();

        string[] str = path.Split('/');

        string name;
        if (str.Length == 0)
        {
            name = str[0];
        }
        else
        {
            name = str[str.Length - 1];
        }

        int index = path.LastIndexOf("/");

        string abFileName = path.Substring(0, index) + abVariant;

        LoadDepend(abFileName);

        if (m_LoadAbDic.ContainsKey(abFileName))
        {
            AssetBundleRequest request = m_LoadAbDic[abFileName].ab.LoadAssetAsync<T>(name);
            request.completed += (asyncRequest) =>
            {
                T asset = (asyncRequest as AssetBundleRequest).asset as T;
                callback?.Invoke(asset);
            };
        }
        else
        {
            ABInfo info = AddLoadABDic(abFileName);
            Debug.Log("加载AB文件:" + abFileName);

            if (info.ab != null)
            {
                AssetBundleRequest request = info.ab.LoadAssetAsync<T>(name);
                request.completed += (asyncRequest) =>
                {
                    T asset = (asyncRequest as AssetBundleRequest).asset as T;
                    callback?.Invoke(asset);
                };
            }
            else
            {
                callback?.Invoke(null);
            }
        }
    }


    #endregion

    public void UnloadAssetBundle(string bundlePath)
    {
        if (m_LoadAbDic.ContainsKey(bundlePath))
        {
            ABInfo info = m_LoadAbDic[bundlePath];
            AssetBundle assetBundle = info.ab;
            assetBundle.Unload(true);
            m_LoadAbDic.Remove(bundlePath);
            Debug.Log("AssetBundle unloaded: " + bundlePath);
        }
        else
        {
            Debug.LogWarning("AssetBundle not found: " + bundlePath);
        }
    }

    public void UnloadAllAssetBundles()
    {
        foreach (var kvp in m_LoadAbDic)
        {
            kvp.Value.ab.Unload(true);
        }
        m_LoadAbDic.Clear();
        Debug.Log("All AssetBundles unloaded");
    }


    void Dispose()
    {
        //AssetBundle.UnloadAllAssetBundles(false);//只释放掉已经没有引用的资源
        AssetBundle.UnloadAllAssetBundles(true);//释放掉所有资源,包括还在引用的资源
        if (m_LoadAbDic != null)
        {
            m_LoadAbDic.Clear();
        }

        m_RootAB = null;
        m_RootManifest = null;
    }
}