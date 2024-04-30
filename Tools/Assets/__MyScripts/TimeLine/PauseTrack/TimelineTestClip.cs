using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

//ITimelineClipAsset 所需命名空间
using UnityEngine.Timeline;

/// <summary>
/// Timeline 轨道上的一个片段 也称为clip
/// 可以控制 选中clip后,在Inspector 面板上展示的属性字段
/// </summary>
public class TimelineTestClip : PlayableAsset, ITimelineClipAsset
{
    //这边可以编写想暴露出去的属性
    //但是这么写无法进行拖拽赋值
    public Transform trs;
    public int index;
    public string str;

    public ExposedReference<Transform> trs_ExposedReference;
    public ExposedReference<TimelineTestController>  controller;

    TImelineTestBehaviour template = new TImelineTestBehaviour();


    /// <summary>
    /// 指定 Timeline 之间 融合方式
    /// </summary>
    public ClipCaps clipCaps
    {
        get { return ClipCaps.None; }
    }

    /// <summary>
    /// 这个函数在 timeline 播放到对应clip 时,会调用,然后函数对clip上的数据进行赋值或者初始化
    /// </summary>
    /// <param name="graph"></param>
    /// <param name="owner"></param>
    /// <returns></returns>
    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<TImelineTestBehaviour>.Create(graph, template);
        var clone = playable.GetBehaviour();//这边获取到的克隆,才是timeline 播放中实例数据,
        //获取到运行实例中的数据后,对实例上的字段进行赋值
        //clone.trs = trs;
        clone.trs = trs_ExposedReference.Resolve(graph.GetResolver());//通过这个方式进行赋值 解决了无法在 Inspector 上拖拽赋值的问题(问题核心在于 Asset资源无法引用 MonoBehaviour资源(也就是场景中资源))

        clone.index = index;
        clone.str = str;

        clone.controller = controller.Resolve(graph.GetResolver());

        return playable;//将赋值完的 Playable 实例,返回出去
    }
}
