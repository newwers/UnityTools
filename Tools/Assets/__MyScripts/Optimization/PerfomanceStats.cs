using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;

public class PerfomanceStats : MonoBehaviour
{

    // Frame time stats
    private List<float> samples = new List<float>();
    private int totalSamples = 250;

    private string totalMemoryText;
    private string gpuMemoryText;
    private string fpsText;
    private string resolutionText; // 新增变量来存储分辨率信息

    public Text m_FPS;
    public Text m_Memory;
    public Text m_GPUMemory;

    void Update()
    {
        // sample frametime
        samples.Insert(0, Time.deltaTime); // add sample at the start
        if (samples.Count >= totalSamples)
        {
            samples.RemoveAt(totalSamples - 1);
        }
        UpdateFrametime();

        long totalMem = Profiler.GetTotalAllocatedMemoryLong();
        totalMemoryText = string.Format("{0}MB", ((float)totalMem / 1000000).ToString("##.00"));
        long gpuMem = Profiler.GetAllocatedMemoryForGraphicsDriver();
        gpuMemoryText = string.Format("{0}MB", ((float)gpuMem / 1000000).ToString("##.00"));

        // 获取当前分辨率信息
        resolutionText = string.Format("{0}x{1}", Screen.width, Screen.height);

        //m_FPS.text = fpsText;
        //m_Memory.text = totalMemoryText;
        //m_GPUMemory.text = gpuMemoryText;
    }

    void UpdateFrametime()
    {
        float avgFrametime = 0f;
        float sampleDivision = 1f / samples.Count;
        for (var i = 0; i < samples.Count; i++)
        {
            avgFrametime += samples[i] * sampleDivision;
        }

        fpsText = (1f / avgFrametime).ToString("###.00");
    }

    private void OnGUI()
    {
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.red;
        style.fontSize = 40;
        GUI.TextArea(new Rect(10, 10, 200, 30), fpsText, style);
        GUI.TextArea(new Rect(10, 80, 200, 30), totalMemoryText, style);
        GUI.TextArea(new Rect(10, 150, 200, 30), gpuMemoryText, style);
        // 显示分辨率信息
        GUI.TextArea(new Rect(10, 220, 200, 30), resolutionText, style);
    }
}