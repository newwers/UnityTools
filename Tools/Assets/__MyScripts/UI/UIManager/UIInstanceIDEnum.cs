using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 存放所有注册界面对应唯一实例ID枚举
/// </summary>
public enum EUIInstanceID 
{
    None = 0,
    /// <summary>
    /// 主界面菜单
    /// </summary>
    MainMenu = 1,
    /// <summary>
    /// 植物信息
    /// </summary>
    Loading = 2,
    /// <summary>
    /// 背包
    /// </summary>
    Bag = 3,
    /// <summary>
    /// 主界面
    /// </summary>
    MainUIPanel = 4,
    /// <summary>
    /// 黑屏
    /// </summary>
    BlackScreen = 5,
    /// <summary>
    /// 通用弹窗界面
    /// </summary>
    CommonTips = 6,
    /// <summary>
    /// 商店
    /// </summary>
    Shop = 7,
}
