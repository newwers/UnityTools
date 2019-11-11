using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ZdqEditorWindow : EditorWindow
{

    [MenuItem("zdq/工具window1(固定大小)")]
    public static void ShowWindow1()
    {
        //创建窗口
        Rect wr = new Rect(0, 0, 500, 500);
        ZdqEditorWindow window = (ZdqEditorWindow)EditorWindow.GetWindowWithRect(typeof(ZdqEditorWindow), wr, true, "窗口标题");
        window.Show();

    }

    [MenuItem("zdq/工具window2(可以通过鼠标拖动窗体大小)")]
    public static void ShowWindow2()
    {
        EditorWindow.GetWindow(typeof(ZdqEditorWindow));
    }

    //输入文字到内容
    private string renameText;

    private Vector2Int size;

    bool m_IsShowTestToggle = false;
    private void OnGUI()
    {
        //绘制窗体

        //设置选中物品名字
        SetSelectItemsName();
        //设置UILanguageSprite的Size
        SetUILanguageSpriteSize();
        //一键创建lua模块文件夹和文件
        CreateLuaModuleFolder();

        
        

        //文本框显示鼠标再窗口的位置
        //EditorGUILayout.LabelField("鼠标在窗口的位置:", Event.current.mousePosition.ToString());

        if (GUILayout.Button("关闭窗口"))
        {
            //关闭窗口
            this.Close();
        }

        m_IsShowTestToggle = GUILayout.Toggle(m_IsShowTestToggle, "是否显示所有样式");
        if (m_IsShowTestToggle == true)
        {
            TestGUIStyle();
        }
    }
    /// <summary>
    /// 设置选中物品名字
    /// </summary>
    void SetSelectItemsName()
    {
        #region 设置名字


        EditorGUILayout.BeginHorizontal(new GUIStyle("HelpBox"));
        //输入框控件
        renameText = EditorGUILayout.TextField("重命名名字：", renameText);

        if (GUILayout.Button("设置名字", GUILayout.Width(100)))
        {
            //打开通知栏
            //this.ShowNotification(new GUIContent("this is a notification"));
            //关闭通知栏
            //this.RemoveNotification();
            var selectList = Selection.gameObjects;

            if (selectList.Length <= 0)
            {
                this.ShowNotification(new GUIContent("当前没有选择物体!!"));
                return;
            }
            Undo.RecordObjects(selectList, "selectList1");
            foreach (var item in selectList)
            {
                item.name = renameText;
            }

        }

        if (GUILayout.Button("选中物体添加后缀数字", GUILayout.Width(100)))
        {
            var selectList = Selection.gameObjects;

            if (selectList.Length <= 0)
            {
                this.ShowNotification(new GUIContent("当前没有选择物体!!"));
                return;
            }

            //根据Hierarchy上的顺序进行从小到大的排序
            List<GameObject> list = new List<GameObject>(selectList);
            list.Sort((a, b) => a.transform.GetSiblingIndex().CompareTo(b.transform.GetSiblingIndex()));
            foreach (var item in list)
            {
                Debug.Log(item.name);
                Debug.Log("顺序:" + item.transform.GetSiblingIndex());
            }
            Undo.RecordObjects(selectList, "selectList2");
            for (int i = 1; i <= list.Count; i++)
            {
                string name = list[i - 1].name;
                name = name.Split(' ')[0];
                list[i - 1].name = name + i.ToString();
            }

        }
        EditorGUILayout.EndHorizontal();
        #endregion
    }
    /// <summary>
    /// 设置UILanguageSprite的Size
    /// </summary>
    void SetUILanguageSpriteSize()
    {
        #region 修改属性


        EditorGUILayout.BeginHorizontal(new GUIStyle("HelpBox"));

        size = EditorGUILayout.Vector2IntField("要修改的坐标:", size);

        //修改选中物体的属性
        if (GUILayout.Button("设置UILanguageSprite的Size", GUILayout.Width(200)))
        {
            var selectList = Selection.gameObjects;

            if (selectList.Length <= 0)
            {
                this.ShowNotification(new GUIContent("当前没有选择物体!!"));
                return;
            }
            Undo.RecordObjects(selectList, "selectList3");
            foreach (var item in selectList)
            {
                //var sprite = item.GetComponent<UILanguageSprite>();
                //sprite.width = size.x;
                //sprite.height = size.y;
            }

        }
        EditorGUILayout.EndHorizontal();

        #endregion
    }


    /// <summary>
    /// 一键创建lua模块文件夹和文件
    /// </summary>
    void CreateLuaModuleFolder()
    {
        GUIStyle style = new GUIStyle("HelpBox");
        EditorGUILayout.BeginHorizontal(style);
        string path = Application.dataPath + "/[Resources]/Lua/Module";



        EditorGUILayout.EndHorizontal();
    }
    Vector2 scrollPosition = new Vector2(0, 0);
    GUIStyle textStyle = new GUIStyle("HeaderLabel");
    string search = "";
    void TestGUIStyle()
    {
        if (textStyle == null)
        {
            textStyle = new GUIStyle("HeaderLabel");
            textStyle.fontSize = 20;
        }

        GUILayout.BeginHorizontal("HelpBox");
        GUILayout.Label("点击示例，可以将其名字复制下来", textStyle);
        GUILayout.FlexibleSpace();
        GUILayout.Label("Search:");
        search = EditorGUILayout.TextField(search);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal("PopupCurveSwatchBackground");
        GUILayout.Label("示例", textStyle, GUILayout.Width(300));
        GUILayout.Label("名字", textStyle, GUILayout.Width(300));
        GUILayout.EndHorizontal();


        scrollPosition = GUILayout.BeginScrollView(scrollPosition);

        foreach (var style in GUI.skin.customStyles)
        {
            if (style.name.ToLower().Contains(search.ToLower()))
            {
                GUILayout.Space(15);
                GUILayout.BeginHorizontal("PopupCurveSwatchBackground");
                if (GUILayout.Button(style.name, style, GUILayout.Width(300)))
                {
                    EditorGUIUtility.systemCopyBuffer = style.name;
                    Debug.LogError(style.name);
                }
                EditorGUILayout.SelectableLabel(style.name, GUILayout.Width(300));
                GUILayout.EndHorizontal();
            }
        }

        GUILayout.EndScrollView();
    }


}
