using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public class AssetBundleManager : BaseSingleClass<AssetBundleManager>
{

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

    static AssetBundleManager()
    {
        //if (Instance != null)
        //{
        //    Instance.Init();
        //}
        //else
        //{
        //    Debug.LogError("未进行初始化");
        //}
        //在这边进行初始化变量会被清空
    }

    public void Init()
    {
        LoadManifest();
    }

    void LoadManifest()
    {
        m_RootAB = AssetBundle.LoadFromFile(m_RootPath + m_Platform + "/PC");
        if (m_RootAB == null)
        {
            Debug.LogError("主 Manifest 加载失败");
            return;
        }
        m_RootManifest = m_RootAB.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
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


    void Dispose()
    {
        AssetBundle.UnloadAllAssetBundles(false);
        if (m_LoadAbDic != null)
        {
            m_LoadAbDic.Clear();
        }
    }
}