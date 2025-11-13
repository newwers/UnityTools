using TMPro;
using UnityEngine;

public class DisplayLogic : IDataSaver
{
    private int currentDisplayIndex;

    private TMP_Dropdown m_screendropdown_tmp_dropdown;

    public DisplayLogic(TMP_Dropdown screendropdown_tmp_dropdown)
    {
        m_screendropdown_tmp_dropdown = screendropdown_tmp_dropdown;
        SetupDisplayDropdown();
        PrintDisplayInfo();
    }


    private void SetupDisplayDropdown()
    {
        m_screendropdown_tmp_dropdown.ClearOptions();

        // 添加所有显示器到下拉菜单
        for (int i = 0; i < Display.displays.Length; i++)
        {
            m_screendropdown_tmp_dropdown.options.Add(new TMP_Dropdown.OptionData($"Display {i + 1}"));
        }
        m_screendropdown_tmp_dropdown.SetValueWithoutNotify(currentDisplayIndex);
        // 添加变更事件
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
            LogManager.LogError($"Invalid display index: {index}");
            return;
        }

        // 更新当前索引
        currentDisplayIndex = index;


        // 移动窗口到目标显示器的中心位置
        MoveWindowToDisplayCenter(index);

        // 更新下拉菜单显示
        m_screendropdown_tmp_dropdown.value = currentDisplayIndex;
        m_screendropdown_tmp_dropdown.RefreshShownValue();


        LogManager.Log($"Switched to Display {currentDisplayIndex + 1}");
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
        LogManager.Log($"切换显示器分辨率 systemWidth:{targetDisplay.systemWidth},systemHeight:{targetDisplay.systemHeight},windowPos:({windowX},{windowY}),screen 获取的分辨率:{Screen.width},{Screen.height}");

        // 应用新的窗口位置
        var displayInfos = new System.Collections.Generic.List<DisplayInfo>();
        Screen.GetDisplayLayout(displayInfos);
        Screen.MoveMainWindowTo(displayInfos[targetDisplayIndex], new Vector2Int(windowX, windowY));//移动窗口
        GameManager.Instance.ResetGameScene();//重置场景

        GameManager.Instance.cameraController.OnChnageDisplay(new Vector2(targetDisplay.systemWidth, targetDisplay.systemHeight));//相机设置
        GameManager.Instance.SetWindowTop(GameManager.Instance.m_IsTop);//刷新窗口置顶

        Debug.Log($"Moved window to Display {targetDisplayIndex + 1} center - Position: ({windowX}, {windowY})");
    }

    void PrintDisplayInfo()
    {
        Debug.Log($"=== 当前连接屏幕数量: {Display.displays.Length} ===");

        // 获取当前屏幕分辨率
        Resolution currentResolution = Screen.currentResolution;
        Debug.Log($"应用程序当前分辨率: {currentResolution.width}x{currentResolution.height} @ {currentResolution.refreshRate}Hz");
        Debug.Log($"应用程序窗口位置: ({Screen.width}x{Screen.height})");

        // 遍历所有显示器
        for (int i = 0; i < Display.displays.Length; i++)
        {
            Display display = Display.displays[i];
            string isActive = display.active ? " (当前)" : "";

            Debug.Log($"屏幕 {i + 1}: {display.renderingWidth}x{display.renderingHeight}{isActive}");
            Debug.Log($"  系统原生分辨率: {display.systemWidth}x{display.systemHeight}");

            // 注意：Unity没有直接获取屏幕物理位置的API
            // 这部分通常需要平台特定代码
        }

        // 判断当前屏幕（简化逻辑）
        DetermineCurrentScreen();
    }

    void DetermineCurrentScreen()
    {
        // 简化逻辑：通过活动状态判断
        for (int i = 0; i < Display.displays.Length; i++)
        {
            if (Display.displays[i].active)
            {
                Debug.Log($"应用程序当前运行在: 屏幕 {i + 1}");
                return;
            }
        }

        // 如果没有明确的活动显示，假设在主屏幕
        Debug.Log("应用程序当前运行在: 主屏幕 (屏幕 1)");
    }

    public void OnSave(SaveData saveData)
    {
        saveData.Set<int>("CurrentDisplayIndex", currentDisplayIndex);
    }

    public void OnLoad(SaveData saveData)
    {
        currentDisplayIndex = saveData.Get<int>("CurrentDisplayIndex");
        m_screendropdown_tmp_dropdown.SetValueWithoutNotify(currentDisplayIndex);//同步设置显示器下拉菜单
    }
}
