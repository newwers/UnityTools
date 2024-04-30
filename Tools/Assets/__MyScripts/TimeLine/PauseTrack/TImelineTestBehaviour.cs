using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//1.添加命名空间
using UnityEngine.Playables;

/// <summary>
/// Timeline Data
/// 相当于一个模板，装在clip中
/// </summary>
public class TImelineTestBehaviour : PlayableBehaviour//2.继承 PlayableBehaviour
{
    public Transform trs;
    public int index;
    public string str;


    public TimelineTestController controller;

    /// <summary>
    /// Timeline 播放到Clip 时,每帧调用的函数
    /// </summary>
    /// <param name="playable"></param>
    /// <param name="info"></param>
    /// <param name="playerData"></param>
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        base.ProcessFrame(playable, info, playerData);

        //Debug.Log("trs:" + trs.name + ",index:" + index + ",str:" + str);
    }

    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        base.OnBehaviourPause(playable, info);

        Debug.Log("OnBehaviourPause time:" + playable.GetTime());

        if (controller && playable.GetTime() > 0)
        {
            controller.PauseTimeline();
        }
    }
}
