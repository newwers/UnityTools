using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

public class ChangeTextPlayableBehaviour : PlayableBehaviour
{
    public string newText;

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        Text textComponent = playerData as Text;
        if (textComponent != null)
        {
            textComponent.text = newText;
        }
    }
}