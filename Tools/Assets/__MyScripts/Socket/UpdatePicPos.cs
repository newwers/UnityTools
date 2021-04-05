using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdatePicPos : MonoBehaviour
{
    RectTransform rectTransform;

    public bool isServer;

    private void Awake()
    {
        Notification.Subscribe("UpdatePos", UpdatePos);
        rectTransform = GetComponent<RectTransform>();
    }

    /// <summary>
    /// todo:这边的位置跟新需要计算出超过屏幕多少,然后让超过部分显示到另一个屏幕上
    /// </summary>
    /// <param name="obj"></param>
    private void UpdatePos(object obj)
    {
        Loom.QueueOnMainThread(() =>
        {
            MovePicStruct data = (MovePicStruct)obj;

            if (isServer)//服务器图片的位置 = 客户端发送的坐标 + 屏幕的一半 + 图片宽度的一半
            {
                
                rectTransform.anchoredPosition = data.offsetPos + new Vector2(Screen.width, 0);
            }
            else//客户端,更新坐标,位置 = 服务器发送的坐标 - 屏幕的一半 - 图片宽度的一半
            {
                rectTransform.anchoredPosition = data.offsetPos - new Vector2(Screen.width,0) ;
            }
            
        });
    }
}
