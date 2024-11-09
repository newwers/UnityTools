/*
 StreamingAssets
 �Ὣ�����ļ����Ƶ����Ӧ����,�����ڰ�׿ƽ̨��,ֻ�ܶ�,����д

persistentDataPath
�ڰ�׿ƽ̨�ϣ�persistentDataPathĬ��ָ��Ӧ�õ��ڲ��洢Ŀ¼��ͨ����/data/data/your.package.name/files��
�ɶ���д,ͨ����������ļ���,ab��Դ,���ص��ȸ�����Դ

PlayerPrefs
pc�ϴ�����ע�����
�ڰ�׿ƽ̨�ϣ�PlayerPrefs�����ݴ洢�� Android��SharedPreferences�С�


�����΢��С��Ϸ,��Ҫʹ��΢�Źٷ��ṩ�� PlayerPrefs ���д���
 ��΢��С��Ϸ�У�PlayerPrefs���ܲ������ѡ����ΪWebGLƽ̨���ƣ�ʹ��Application.persistentDataPath����·���������ļ����ܻ�ʧ�ܡ�
һ�ּ򵥵ķ�ʽ��ʹ��PlayerPrefs���洢���ݣ������ڶ�ȡ�ٶ��������⡣΢��С��Ϸ�ṩ�˲�����Ż�PlayerPrefs��ʹ��

persistentDataPath�����������ʹ��WX.env.USER_DATA_PATH

 */

using System.IO;
using System.Text;
using UnityEngine;

/// <summary>
/// ����ϵͳ,���� streamingAssetsPath,persistentDataPath,PlayerPrefs�Ĵ洢�Ͷ�ȡ
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

    public static string ObjectToJson(object obj)
    {
        return JsonUtility.ToJson(obj);
    }

    public static T JsonToObject<T>(string json)
    {
        return JsonUtility.FromJson<T>(json);
    }
}
