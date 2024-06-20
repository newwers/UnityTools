/*
	newwer
*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


public static class Tool
{


    /// <summary>
    /// 读取配置的文件
    /// </summary>
    /// <returns></returns>
    public static string GetConfigContent()
    {
        //print(Application.streamingAssetsPath);

        string jsonPath = string.Format("{0}{1}", Application.streamingAssetsPath, "/config.json");
        //print(jsonPath);
        string jsonText = File.ReadAllText(jsonPath);
        //print(jsonText);

        return jsonText;
    }

    /// <summary>
    /// 根据鼠标位置计算当前权重
    /// </summary>
    /// <param name="mousePos"></param>
    /// <returns></returns>
    public static float GetMousePosWeight(Vector3 mousePos)
    {
        Vector3 screenCenter = new Vector3(Screen.width / 2, Screen.height, 0);

        Vector3 offset = mousePos - screenCenter;
        float percentageX = Mathf.Abs(offset.x / Screen.width);
        float percentageY = Mathf.Abs(offset.y / Screen.height);
        var weight = 1 - (percentageX + percentageY) / 2;
        return weight;
    }

    public static string FormatSeconds(int seconds)
    {
        TimeSpan time = TimeSpan.FromSeconds(seconds);

        if (seconds < 3600) // 如果秒数小于1小时
        {
            return time.ToString(@"mm\:ss"); // 格式化成 00:00
        }
        else
        {
            return time.ToString(@"hh\:mm\:ss"); // 格式化成 00:00:00
        }
    }

}

