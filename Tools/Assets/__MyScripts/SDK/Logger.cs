using UnityEngine;
using System.Collections.Generic;

public class Logger : MonoBehaviour
{
	public int ShowLogCount = 10;
	public int LogHeight = 30;
	public int fontSize = 30;

    //#if !UNITY_EDITOR
    Queue<string> queue;
	GUIStyle style;

    private void Awake()
    {
        queue = new Queue<string>(ShowLogCount);

        
    }

    void OnEnable() {
		Application.logMessageReceived +=HandleLog;
	}

	void OnDisable() {
        Application.logMessageReceived -= HandleLog;
    }

	void OnGUI() {
		if (style == null || style.fontSize != fontSize)
		{
            style = new GUIStyle(GUI.skin.label);
            style.fontSize = fontSize; // 设置字体大小
        }
		GUILayout.BeginArea(new Rect(0, Screen.height - ShowLogCount* LogHeight-200, Screen.width, ShowLogCount * LogHeight));
		foreach (string s in queue) {
			GUILayout.Label(s, style);
		}
		GUILayout.EndArea();
	}

	void HandleLog(string message, string stackTrace, LogType type) {

		if (type == LogType.Exception || type == LogType.Exception)
		{
            queue.Enqueue(Time.time + " - " + message + "\n" + stackTrace);
        }
		else
		{
            queue.Enqueue(Time.time + " - " + message);
        }

		

		if (queue.Count > ShowLogCount) {
			queue.Dequeue();
		}
	}
//#endif
}