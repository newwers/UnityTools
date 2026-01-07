using UnityEditor;
using UnityEngine;

public class TimeTestWindow : EditorWindow
{
    private float timeScale = 1.0f;
    private bool isPaused = false;
    private float previousTimeScale = 1.0f;


    // 添加菜单项
    [MenuItem("Tools/时间测试窗口")]
    public static void ShowWindow()
    {
        GetWindow<TimeTestWindow>("Time Controller");
    }

    private void OnEnable()
    {
        // 初始化时获取当前的时间缩放
        timeScale = Time.timeScale;
        previousTimeScale = timeScale;

        if (!Application.isPlaying)
        {
            Debug.LogWarning("此功能只能在游戏运行时使用!");
            return;
        }
    }

    private void OnGUI()
    {
        GUILayout.Space(10);

        // 显示当前时间缩放值
        EditorGUILayout.LabelField("当前时间缩放", EditorStyles.boldLabel);
        EditorGUILayout.LabelField(timeScale.ToString("F2"), EditorStyles.helpBox);

        GUILayout.Space(10);

        // 时间缩放滑块
        EditorGUILayout.LabelField("调整时间缩放");
        timeScale = EditorGUILayout.Slider(timeScale, 0f, 10f);

        GUILayout.Space(10);

        // 按钮布局
        EditorGUILayout.BeginHorizontal();
        {
            // 暂停/继续按钮
            if (GUILayout.Button(isPaused ? "继续" : "暂停", GUILayout.Height(30)))
            {
                TogglePause();
            }

            // 重置按钮
            if (GUILayout.Button("重置", GUILayout.Height(30)))
            {
                ResetTimeScale();
            }
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10);

        // 预设按钮
        EditorGUILayout.LabelField("预设", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        {
            if (GUILayout.Button("0.25x"))
            {
                SetTimeScale(0.25f);
            }
            if (GUILayout.Button("0.5x"))
            {
                SetTimeScale(0.5f);
            }
            if (GUILayout.Button("正常"))
            {
                SetTimeScale(1f);
            }
            if (GUILayout.Button("2x"))
            {
                SetTimeScale(2f);
            }
            if (GUILayout.Button("3x"))
            {
                SetTimeScale(3f);
            }
            if (GUILayout.Button("5x"))
            {
                SetTimeScale(5f);
            }
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(20);




        // 应用时间缩放
        if (!Mathf.Approximately(Time.timeScale, timeScale))
        {
            Time.timeScale = timeScale;
            isPaused = Time.timeScale < 0.01f;
            Repaint();
        }
    }



    private void TogglePause()
    {
        if (isPaused)
        {
            // 恢复播放
            Time.timeScale = previousTimeScale;
            timeScale = previousTimeScale;
            isPaused = false;
        }
        else
        {
            // 暂停
            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            timeScale = 0f;
            isPaused = true;
        }
    }

    private void ResetTimeScale()
    {
        SetTimeScale(1.0f);
        isPaused = false;
    }

    private void SetTimeScale(float scale)
    {
        timeScale = scale;
        Time.timeScale = timeScale;
        isPaused = Time.timeScale < 0.01f;
        if (!isPaused)
        {
            previousTimeScale = timeScale;
        }
    }


    // 每秒更新几次窗口，确保UI响应及时
    private void Update()
    {
        if (!Mathf.Approximately(Time.timeScale, timeScale))
        {
            timeScale = Time.timeScale;
            isPaused = Time.timeScale < 0.01f;
            Repaint();
        }
    }
}