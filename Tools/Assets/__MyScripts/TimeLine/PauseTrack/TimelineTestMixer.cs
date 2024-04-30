using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

/// <summary>
/// Timeline 轨道(Track)上的混合器(Mixer)
/// </summary>
public class TimelineTestMixer : PlayableBehaviour
{

    //这边就是点击 轨道上的 非Clip 区域时,Inspector显示的属性
    //未展示
    public Transform mixerTrs;
    public int mixerIndex;



    /// <summary>
    /// 轨道执行时,每帧都调用此函数
    /// </summary>
    /// <param name="playable"></param>
    /// <param name="info"></param>
    /// <param name="playerData"></param>
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        //playerData as 这边通过 as 将引用槽 上的数据进行转换得到对应对象

        //这边通过 playable.GetInputCount() 获取整个轨道上 有几个 Clip 输入
        int inputCount = playable.GetInputCount();

        //然后需要通过 playable.GetInputWeight(inputCount); 获取每个Clip上播放的权重
        //这边权重概念主要 一个是 判断是否该Clip处于播放状态,播放中是1 ,未播放为0,如果是混合则在0到1之间
        //float weight = playable.GetInputWeight(inputCount);

        //根据权重进行处理
    }
}
