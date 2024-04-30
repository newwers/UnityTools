using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.UI;

[TrackColor(0.855f, 0.8623f, 0.87f)]
[TrackClipType(typeof(ChangeTextPlayableAsset))]
[TrackBindingType(typeof(Text))]
public class ChangeTextTrackAsset : TrackAsset
{
    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {

        //ScriptPlayable<ChangeTextPlayableBehaviour> result = ScriptPlayable<ChangeTextPlayableBehaviour>.Create(graph, inputCount);
        //foreach (TimelineClip clip in GetClips())
        //{
        //    ChangeTextPlayableAsset textAsset = clip.asset as ChangeTextPlayableAsset;
        //    if (textAsset != null)
        //    {
        //        ChangeTextPlayableBehaviour textBehaviour = result.GetBehaviour();
        //        textBehaviour.newText = textAsset.newText;
        //    }
        //}

        //return result;

        return ScriptPlayable<TimelineTestMixer>.Create(graph, inputCount);
    }
}