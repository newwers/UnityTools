using System;
using UnityEngine;

public struct TimeEventData 
{
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

    public TimeEventData(DateTime date, string showTips, AudioClip playAudio)
    {
        this.date = date;
        this.showTips = showTips ?? throw new ArgumentNullException(nameof(showTips));
        this.playAudio = playAudio;
    }
}
