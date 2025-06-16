using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class ButtonGroupEvent : UnityEvent<int> { }

public class ButtonGroup : MonoBehaviour
{
    [System.Serializable]
    public class ButtonInfo
    {
        public Button button; // 按钮组件
        public Sprite selectedSprite; // 选中状态的图片
        public Sprite normalSprite; // 默认状态的图片
        public int index; // 按钮索引
    }

    [Header("按钮设置")]
    public List<ButtonInfo> buttons = new List<ButtonInfo>();

    [Header("事件设置")]
    public ButtonGroupEvent OnButtonSelected = new ButtonGroupEvent(); // 当按钮被选中时触发的事件
    public UnityEvent OnSelectionChanged = new UnityEvent(); // 当选择状态改变时触发

    private int _currentSelectedIndex = -1; // 当前选中的按钮索引
    private Dictionary<Button, ButtonInfo> _buttonInfoMap = new Dictionary<Button, ButtonInfo>();

    private void Awake()
    {
        InitializeButtons();
    }

    private void InitializeButtons()
    {
        _buttonInfoMap.Clear();

        // 为每个按钮添加事件监听器
        for (int i = 0; i < buttons.Count; i++)
        {
            var buttonInfo = buttons[i];

            if (buttonInfo.button == null)
            {
                Debug.LogWarning($"按钮组 '{name}' 中存在未分配的按钮对象!");
                continue;
            }

            // 设置按钮索引
            buttonInfo.index = i;
            _buttonInfoMap[buttonInfo.button] = buttonInfo;

            // 添加点击事件
            buttonInfo.button.onClick.AddListener(() => OnButtonClick(buttonInfo.button));

            // 设置初始状态图片
            if (buttonInfo.normalSprite != null)
            {
                buttonInfo.button.GetComponent<Image>().sprite = buttonInfo.normalSprite;
            }
        }

        // 如果没有初始选择，设置第一个为选中状态
        if (_currentSelectedIndex == -1 && buttons.Count > 0)
        {
            SetSelected(0);
        }
    }

    // 处理按钮点击
    private void OnButtonClick(Button clickedButton)
    {
        if (!_buttonInfoMap.ContainsKey(clickedButton))
            return;

        ButtonInfo info = _buttonInfoMap[clickedButton];

        // 如果点击了已经选中的按钮，不重复处理
        if (_currentSelectedIndex == info.index)
            return;

        // 更新选中状态
        SetSelected(info.index);

        // 触发事件
        OnButtonSelected?.Invoke(info.index);
        OnSelectionChanged?.Invoke();
    }

    // 设置选中的按钮
    public void SetSelected(int index)
    {
        // 确保索引在有效范围内
        if (index < 0 || index >= buttons.Count)
        {
            Debug.LogWarning($"试图设置的按钮索引 '{index}' 超出范围 (0-{buttons.Count - 1})");
            return;
        }

        // 更新所有按钮状态
        for (int i = 0; i < buttons.Count; i++)
        {
            ButtonInfo info = buttons[i];

            if (i == index)
            {
                // 设置选中图片
                if (info.selectedSprite != null)
                {
                    info.button.GetComponent<Image>().sprite = info.selectedSprite;
                }

                // 更新选中索引
                _currentSelectedIndex = i;
            }
            else
            {
                // 设置普通图片
                if (info.normalSprite != null)
                {
                    info.button.GetComponent<Image>().sprite = info.normalSprite;
                }
            }
        }
    }

    // 获取当前选中的按钮索引
    public int GetSelectedIndex()
    {
        return _currentSelectedIndex;
    }

    // 添加新按钮到组中
    public void AddButton(Button button, Sprite normalSprite, Sprite selectedSprite)
    {
        ButtonInfo newButton = new ButtonInfo
        {
            button = button,
            normalSprite = normalSprite,
            selectedSprite = selectedSprite,
            index = buttons.Count
        };

        buttons.Add(newButton);
        InitializeButtons(); // 重新初始化
    }

    // 从组中移除按钮
    public void RemoveButton(Button button)
    {
        for (int i = 0; i < buttons.Count; i++)
        {
            if (buttons[i].button == button)
            {
                buttons.RemoveAt(i);
                InitializeButtons(); // 重新初始化
                break;
            }
        }
    }

    // 清除所有按钮
    public void ClearButtons()
    {
        buttons.Clear();
        InitializeButtons();
    }

    // 编辑器方法 - 自动收集所有子对象的按钮
    [ContextMenu("收集子按钮")]
    public void CollectChildButtons()
    {
        buttons.Clear();
        Button[] childButtons = GetComponentsInChildren<Button>(true);

        foreach (Button btn in childButtons)
        {
            buttons.Add(new ButtonInfo
            {
                button = btn,
                index = buttons.Count,
                normalSprite = btn.GetComponent<Image>()?.sprite, // 获取默认图片
            });
        }
    }
}

#if UNITY_EDITOR             
[CustomEditor(typeof(ButtonGroup))]
public class ButtonGroupEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ButtonGroup buttonGroup = (ButtonGroup)target;

        if (GUILayout.Button("收集子按钮"))
        {
            buttonGroup.CollectChildButtons();
        }

        if (GUILayout.Button("清除所有按钮"))
        {
            buttonGroup.ClearButtons();
        }
    }
}
#endif // UNITY_EDITOR