using TMPro;
using UnityEngine;
//如果游戏一开始就根据有几个显示器就设置几个display,那么是否支持跨屏幕拖拽功能了?
//切换完屏幕后,置顶功能没有了,需要重新置顶
//切换屏幕后,需要考虑分辨率不同,刷新画面大小问题
//切换屏幕本质是修改窗口位置

public class DisplayTest : MonoBehaviour
{
    public TMP_Dropdown m_screendropdown_tmp_dropdown;
    private int currentDisplayIndex;

    void Start()
    {
        SetupDisplayDropdown();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            OnDisplayChanged(0);
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            OnDisplayChanged(1);
        }
    }

    private void SetupDisplayDropdown()
    {
        m_screendropdown_tmp_dropdown.ClearOptions();
        print("显示器数量:" + Display.displays.Length);

        for (int i = 0; i < Display.displays.Length; i++)
        {
            m_screendropdown_tmp_dropdown.options.Add(new TMP_Dropdown.OptionData($"Display {i + 1}"));
        }

        m_screendropdown_tmp_dropdown.onValueChanged.AddListener(OnDisplayChanged);
    }

    private void OnDisplayChanged(int index)
    {
        ChangeDisplay(index);
    }

    public void ChangeDisplay(int index)
    {
        if (index < 0 || index >= Display.displays.Length)
        {
            Debug.LogError($"Invalid display index: {index}");
            return;
        }

        currentDisplayIndex = index;

        // 移动窗口到目标显示器的中心位置
        MoveWindowToDisplayCenter(index);

        m_screendropdown_tmp_dropdown.value = currentDisplayIndex;
        m_screendropdown_tmp_dropdown.RefreshShownValue();

        Debug.Log($"Switched to Display {currentDisplayIndex + 1}");

    }

    private void MoveWindowToDisplayCenter(int targetDisplayIndex)
    {
        // 获取目标显示器的信息
        Display targetDisplay = Display.displays[targetDisplayIndex];

        // 计算目标显示器的中心位置
        int targetCenterX = targetDisplay.systemWidth / 2;
        int targetCenterY = targetDisplay.systemHeight / 2;

        // 获取目标显示器在虚拟桌面中的位置
        // 注意：Unity的Display类不直接提供显示器在虚拟桌面中的位置信息
        // 我们需要使用系统API来获取准确的显示器布局信息

        // 设置窗口位置到目标显示器的中心
        // 减去窗口宽度和高度的一半，使窗口中心对准显示器中心
        int windowX = targetDisplay.systemWidth + targetCenterX - (Screen.width / 2);
        int windowY = targetDisplay.systemHeight + targetCenterY - (Screen.height / 2);
        LogManager.Log($"systemWidth:{targetDisplay.systemWidth},systemHeight:{targetDisplay.systemHeight},windowPos:({windowX},{windowY})");

        // 应用新的窗口位置
        var displayInfos = new System.Collections.Generic.List<DisplayInfo>();
        Screen.GetDisplayLayout(displayInfos);
        Screen.MoveMainWindowTo(displayInfos[targetDisplayIndex], new Vector2Int(windowX, windowY));

        Debug.Log($"Moved window to Display {targetDisplayIndex + 1} center - Position: ({windowX}, {windowY})");
    }

}