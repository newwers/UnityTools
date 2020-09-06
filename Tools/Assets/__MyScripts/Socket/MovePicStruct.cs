using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 移动时消息结构体
/// </summary>
public struct MovePicStruct 
{
    /// <summary>
    /// 图片超出屏幕的偏移
    /// </summary>
    public float offsetPosX;
    public float offsetPosY;

    public Vector2 offsetPos;

    public MovePicStruct(float offsetPosX, float offsetPosY) : this()
    {
        this.offsetPosX = offsetPosX;
        this.offsetPosY = offsetPosY;
        offsetPos = new Vector2(offsetPosX, offsetPosY);
    }
}
