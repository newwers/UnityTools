using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugFPS : MonoBehaviour {
    void OnGUI()
    {
        //GUILayout.Label(" " + fps.ToString("f2"));
        if (GUI.Button(new Rect(100, 100, 100, 100), (1f / Time.smoothDeltaTime).ToString("0")))
        {
            Destroy(this);
        }
    }
}
