using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

// TrackColor 特性的命名空间
using UnityEngine.Timeline;

[TrackColor(1f,1f,0.5f)]//指定轨道的颜色
[TrackClipType(typeof(TimelineTestClip))]//指定轨道类型
[TrackBindingType(typeof(GameObject))] //指定什么类型的物体能够拖拽到 轨道的槽中
/// <summary>
/// Timeline 轨道 Track
/// </summary>
public class TimelineTestTrack : TrackAsset
{

    //这边就是点击 轨道上的 非Clip 区域时,Inspector显示的属性
    public Transform trackTrs;
    public int trackIndex;


    //这边 Transform 想赋值 也是有 Asset引用 MonoBehaviour 的问题
    public ExposedReference<Transform> trackTrs_ExposedReference;

    /// <summary>
    /// 当创建 Mixer 时调用
    /// </summary>
    /// <param name="graph"></param>
    /// <param name="go"></param>
    /// <param name="inputCount"></param>
    /// <returns></returns>
    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        return ScriptPlayable<TimelineTestMixer>.Create(graph, inputCount);
    }
}
