/*
	newwer
*/
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


public static class TimeEventTool
{
    static string m_ConfigJsonPath;
    static string m_TimeEventJsonPath;
    static TimeEventTool()
    {
        m_ConfigJsonPath = string.Format("{0}{1}", Application.streamingAssetsPath, "/config.json");
        m_TimeEventJsonPath = string.Format("{0}{1}", Application.streamingAssetsPath, "/timeEvent.json");
    }
    /// <summary>
    /// 读取配置的文件
    /// </summary>
    /// <returns></returns>
    public static string GetConfigContent()
    {
        //print(Application.streamingAssetsPath);
        
        //print(jsonPath);
        string jsonText = File.ReadAllText(m_ConfigJsonPath);
        //print(jsonText);

        return jsonText;
    }

    public static void SaveTimeEvents()
    {
        string path = m_TimeEventJsonPath;
        var datas = TimeManager.Instance.GetSaveData();
        string json = JsonUtility.ToJson(datas,true);
        if (File.Exists(path))
        {
            File.Delete(path);
        }
        File.WriteAllText(path, json);
    }

    public static TimeEventSaveData ReadTimeEvents()
    {
        if (!File.Exists(m_TimeEventJsonPath))
        {
            return null;
        }

        string json = File.ReadAllText(m_TimeEventJsonPath);
        Debug.Log("读取到时间事件:" + json);
        return JsonUtility.FromJson<TimeEventSaveData>(json);
    }
}

