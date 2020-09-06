using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System;

namespace TopGame
{
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

        [MenuItem("zdq/清空控制台 &c")]
        public static void ClearConsoleWindow()
        {
            ClearConsole();
        }

        //输入文字到内容
        private string renameText;

        private Vector2Int size = new Vector2Int(50, 50);

        private void OnFocus()
        {
            m_TimeScale = Time.timeScale;
            Debug.Log("zdq OnFocus:");
        }


        private void OnGUI()
        {
            //绘制窗体
            #region 设置名字


            EditorGUILayout.BeginHorizontal();
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

            #region 修改属性

            SetTextSize();
            SetButtonSize();

            #endregion

            TimeScale();

            ImageConvertRawImage();

            RawImageConvertImage();

            //文本框显示鼠标再窗口的位置
            //EditorGUILayout.LabelField("鼠标在窗口的位置:", Event.current.mousePosition.ToString());

            GameTest();

            if (GUILayout.Button("关闭窗口"))
            {
                //关闭窗口
                this.Close();
            }
        }

        void SetButtonSize()
        {
            EditorGUILayout.BeginHorizontal();

            //修改选中物体的属性
            if (GUILayout.Button("设置Button组件的Size", GUILayout.Width(200)))
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
                    RectTransform btnrec = item.GetComponent<RectTransform>();
                    btnrec.sizeDelta = size;
                }

            }
            EditorGUILayout.EndHorizontal();
        }

        void SetTextSize()
        {
            EditorGUILayout.BeginHorizontal();

            size = EditorGUILayout.Vector2IntField("要修改的Text组件大小:", size);

            //修改选中物体的属性
            if (GUILayout.Button("设置Text组件的Size", GUILayout.Width(200)))
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
                    Text text = item.GetComponent<Text>();
                    text.rectTransform.sizeDelta = size;
                    text.raycastTarget = false;
                    text.alignment = TextAnchor.MiddleCenter;
                    text.color = Color.black;
                    text.supportRichText = false;
                    text.horizontalOverflow = HorizontalWrapMode.Overflow;

                    //text.resizeTextForBestFit = true;
                }

            }
            EditorGUILayout.EndHorizontal();
        }

        private static System.Reflection.MethodInfo clearMethod = null;
        /// <summary>
        /// 清空log信息
        /// </summary>
        public static void ClearConsole()
        {
            if (clearMethod == null)
            {
                Type log = typeof(UnityEditor.EditorWindow).Assembly.GetType("UnityEditor.LogEntries");
                clearMethod = log.GetMethod("Clear");
            }
            clearMethod.Invoke(null, null);

            //unity 2017前的清空console 代码
            //var logEntries = System.Type.GetType("UnityEditorInternal.LogEntries,UnityEditor.dll");
            //var clearMethod = logEntries.GetMethod("Clear", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            //clearMethod.Invoke(null, null);

            //新版本的清空console 代码
            //Assembly assembly = Assembly.GetAssembly(typeof(SceneView));
            //Type logEntries = assembly.GetType("UnityEditor.LogEntries");
            //MethodInfo clearConsoleMethod = logEntries.GetMethod("Clear");
            //clearConsoleMethod.Invoke(new object(), null);
        }

        float m_TimeScale;

        /// <summary>
        /// 时间缩放管理
        /// </summary>
        void TimeScale()
        {
            EditorGUILayout.BeginHorizontal();

            m_TimeScale = EditorGUILayout.FloatField("时间缩放", m_TimeScale);

            //修改选中物体的属性
            if (GUILayout.Button("设置时间缩放", GUILayout.Width(200)))
            {
                Time.timeScale = m_TimeScale;
                Debug.Log("设置倍速:" + m_TimeScale);
            }
            EditorGUILayout.EndHorizontal();
        }

        void GameTest()
        {
            if (GUILayout.Button("游戏测试"))
            {
                //UI.UIBase pUI = GameInstance.getInstance().uiManager.GetUI((ushort)UI.EUIType.DefeatPanel);
                //if (pUI != null)
                //{
                //    pUI.Show();
                //}
            }
        }

        void ImageConvertRawImage()
        {
            if (GUILayout.Button("Image转RawImage"))
            {
                var selectList = Selection.gameObjects;

                if (selectList.Length <= 0)
                {
                    this.ShowNotification(new GUIContent("当前没有选择物体!!"));
                    return;
                }

                Undo.RecordObjects(selectList, "selectList4");

                foreach (var item in selectList)
                {
                    Image image = item.GetComponent<Image>();
                    if (image == null)
                    {
                        continue;
                    }
                    var sprite = image.sprite;
                    var color = image.color;
                    var material = image.material;
                    var raycast = image.raycastTarget;
                    var maskable = image.maskable;
                    //var type = image.type;

                    DestroyImmediate(image);

                    RawImage  rawImage = item.AddComponent<RawImage>();
                    rawImage.texture = sprite.texture;
                    rawImage.color = color;
                    rawImage.material = material;
                    rawImage.raycastTarget = raycast;
                    rawImage.maskable = maskable;

                    UnityEditor.EditorUtility.SetDirty(item);
                }
            }

            
        }

        void RawImageConvertImage()
        {
            if (GUILayout.Button("RawImage转Image"))
            {
                var selectList = Selection.gameObjects;

                if (selectList.Length <= 0)
                {
                    this.ShowNotification(new GUIContent("当前没有选择物体!!"));
                    return;
                }

                Undo.RecordObjects(selectList, "selectList5");

                foreach (var item in selectList)
                {
                    RawImage rawImage = item.GetComponent<RawImage>();
                    if (rawImage == null)
                    {
                        continue;
                    }
                    
                    var texture = rawImage.texture;
                    var path = AssetDatabase.GetAssetPath(texture.GetInstanceID());

                    var color = rawImage.color;
                    var material = rawImage.material;
                    var raycast = rawImage.raycastTarget;
                    var maskable = rawImage.maskable;

                    DestroyImmediate(rawImage);

                    Image image = item.AddComponent<Image>();
                    //image.sprite = Sprite.Create((Texture2D)texture, new Rect(Vector2.zero, texture.texelSize), new Vector2(0.5f,0.5f));
                    image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                    image.color = color;
                    image.material = material;
                    image.raycastTarget = raycast;
                    image.maskable = maskable;

                    UnityEditor.EditorUtility.SetDirty(item);
                }
            }

        }

    }
}
