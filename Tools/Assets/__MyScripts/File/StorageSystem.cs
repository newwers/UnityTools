/*
 StreamingAssets
 会将里面文件复制到打包应用下,但是在安卓平台中,只能读,不能写

persistentDataPath
在安卓平台上，persistentDataPath默认指向应用的内部存储目录，通常是/data/data/your.package.name/files。
可读可写,通常存放配置文件夹,ab资源,下载的热更新资源

PlayerPrefs
pc上储存在注册表中
在安卓平台上，PlayerPrefs将数据存储在 Android的SharedPreferences中。


如果是微信小游戏,需要使用微信官方提供的 PlayerPrefs 进行储存
 在微信小游戏中，PlayerPrefs可能不是最佳选择，因为WebGL平台限制，使用Application.persistentDataPath缓存路径来保存文件可能会失败。
一种简单的方式是使用PlayerPrefs来存储内容，但存在读取速度慢的问题。微信小游戏提供了插件来优化PlayerPrefs的使用

persistentDataPath的替代方案是使用WX.env.USER_DATA_PATH

 */

using System.IO;
using System.Text;
using UnityEngine;

/// <summary>
/// 储存系统,管理 streamingAssetsPath,persistentDataPath,PlayerPrefs的存储和读取
/// 
/// </summary>
public static class StorageSystem 
{

    public static string streamingAssetsPath
    {
        get
        {
            return Application.streamingAssetsPath;
        }
    }

    public static string persistentDataPath
    {
        get
        {
            return Application.persistentDataPath;
        }
    }


    public static void SaveStringToStreamingAssets(string data, string filename)
    {
        string path = Path.Combine(streamingAssetsPath, filename);
        File.WriteAllText(path, data, Encoding.UTF8);
    }

    public static string LoadStringFromStreamingAssets(string filename)
    {
        string path = Path.Combine(Application.streamingAssetsPath, filename);
        if (File.Exists(path))
        {
            return File.ReadAllText(path, Encoding.UTF8);
        }
        return null;
    }

    public static void SaveStringToPersistentDataPath(string data, string filename)
    {
        string path = Path.Combine(Application.persistentDataPath, filename);
        File.WriteAllText(path, data, Encoding.UTF8);
    }

    public static string LoadStringFromPersistentDataPath(string filename)
    {
        string path = Path.Combine(Application.persistentDataPath, filename);
        if (File.Exists(path))
        {
            return File.ReadAllText(path, Encoding.UTF8);
        }
        return null;
    }

    public static void SaveStringToPlayerPrefs(string data, string key)
    {
        PlayerPrefs.SetString(key, data);
        PlayerPrefs.Save();
    }

    public static string LoadStringFromPlayerPrefs(string key)
    {
        return PlayerPrefs.GetString(key, string.Empty);
    }

    public static void SaveIntToPlayerPrefs(int data, string key)
    {
        PlayerPrefs.SetInt(key, data);
        PlayerPrefs.Save();
    }

    public static int LoadIntFromPlayerPrefs(string key)
    {
        return PlayerPrefs.GetInt(key, 0);
    }

    public static string ObjectToJson(object obj)
    {
        return JsonUtility.ToJson(obj);
    }

    public static T JsonToObject<T>(string json)
    {
        return JsonUtility.FromJson<T>(json);
    }

    public static void SaveStringToResources(string data, string fileName)
    {
        string path = Path.Combine(Application.dataPath + "/Resources/", fileName);
        File.WriteAllText(path, data, Encoding.UTF8);
    }

    public static string LoadStringFromResources(string fileName)
    {
        TextAsset textAsset = Resources.Load<TextAsset>(fileName);
        if (textAsset)
        {
            return textAsset.text;
        }

        return null;
    }
}
