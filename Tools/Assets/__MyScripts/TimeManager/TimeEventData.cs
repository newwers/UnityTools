using System;
using UnityEngine;

public class TimeEventData 
{
    public int id;
    /// <summary>
    /// 设置的日期,用DateTime就可以表示
    /// </summary>
    public DateTime date;
    /// <summary>
    /// 显示的提示语句
    /// </summary>
    public string showTips;
    /// <summary>
    /// 播放的音频
    /// </summary>
    public AudioClip playAudio;
    /// <summary>
    /// 当时间到后,执行的函数
    /// </summary>
    public Action OnTriggerAction;
    /// <summary>
    /// 下次触发间隔
    /// </summary>
    public long nextTriggerTimeSpan;
}
