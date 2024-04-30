using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class ChangeTextPlayableAsset : PlayableAsset
{
    public string newText;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject go)
    {
        var playable = ScriptPlayable<ChangeTextPlayableBehaviour>.Create(graph);
        var textBehaviour = playable.GetBehaviour();
        textBehaviour.newText = newText;

        return playable;
    }
}