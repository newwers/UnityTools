﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System;
using System.IO;
using System.Reflection;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

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

        [MenuItem("zdq/工具window2(可以通过鼠标拖动窗体大小) _F3")]
        public static void ShowWindow2()
        {
            EditorWindow.GetWindow(typeof(ZdqEditorWindow));
        }

        [MenuItem("zdq/清空控制台 &c")]
        public static void ClearConsoleWindow()
        {
            ClearConsole();
        }

        [MenuItem("zdq/RunGame _F5")]
        public static void PlayUnityEditor()
        {
            if (EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = false;
            }
            else
            {
                Scene scene = EditorSceneManager.GetActiveScene();
                if (scene.buildIndex != -1 && scene.isDirty)
                {
                    bool result = EditorUtility.DisplayDialog("提示", "是否保存当前场景?", "保存", "取消");
                    if (result)
                    {
                        EditorSceneManager.SaveScene(scene);
                    }
                }
                
                EditorApplication.isPlaying = true;
            }
        }

        [MenuItem("zdq/Pause _F6")]
        public static void Pause()
        {
            EditorApplication.isPaused = !EditorApplication.isPaused;
        }

        [MenuItem("zdq/Step _F7")]
        public static void Step()
        {
            EditorApplication.Step();
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

            SearchContentReference();

            SelectComponent();

            ImageConvertRawImage();

            RawImageConvertImage();

            //文本框显示鼠标再窗口的位置
            //EditorGUILayout.LabelField("鼠标在窗口的位置:", Event.current.mousePosition.ToString());

            CopyGameObject();

            GameTest();

            SetRectTransformAnchorEqualSize();

            SetRectTransformAnchorToPos();

            if (GUILayout.Button("关闭窗口"))
            {
                //关闭窗口
                this.Close();
            }
        }

        /// <summary>
        /// 让当前选中得RectTrasnform的锚点和Size一样大
        /// </summary>
        void SetRectTransformAnchorEqualSize()
        {
            if (GUILayout.Button("让当前选中得RectTrasnform的锚点和Size一样大"))
            {
                GameObject[] gos = Selection.gameObjects;

                List<RectTransform> rects = new List<RectTransform>();
                foreach (var item in gos)
                {
                    if (item == null)
                    {
                        continue;
                    }
                    rects.Add(item.GetComponent<RectTransform>());
                }

                foreach (var item in rects)
                {
                    if (item)
                    {
                        //怎么求出锚点的坐标?
                        //根据当前屏幕分辨率,计算当前rect大小占屏幕的比例
                        //左下角为min,右上角为max

                        //要求锚点和轴都是(0.5,0.5)
                        if (item.anchorMax != new Vector2(0.5f, 0.5f) || item.anchorMin != new Vector2(0.5f, 0.5f) || item.pivot != new Vector2(0.5f, 0.5f))//todo:如果锚点改变了,怎么计算?
                        {
                            ShowNotification(new GUIContent("要求锚点和轴都是(0.5, 0.5)"));
                            return;
                        }
                        Vector2 rectSize = item.sizeDelta;
                        Vector2 resolution = new Vector2(720, 1280);//Screen.Width 有问题,todo:怎么解决屏幕分辨率的问题?如何取到正确的分辨率

                        float heightRatio = rectSize.y / resolution.y;
                        float heightOffsetRatio = item.localPosition.y / resolution.y;//todo:这边要保证是父物体是全屏,如果不是全屏,能代码解决吗?

                        float minY = 0.5f - (heightRatio / 2f) + heightOffsetRatio;

                        float widthRatio = rectSize.x / resolution.x;
                        float widthOffsetRatio = item.localPosition.x / resolution.x;

                        float minX = 0.5f - (widthRatio / 2f) + widthOffsetRatio;


                        Undo.RecordObject(item, "item");
                        item.anchorMin = new Vector2(minX, minY);
                        item.anchorMax = item.anchorMin + new Vector2(widthRatio, heightRatio);

                        item.offsetMin = Vector2.zero;
                        item.offsetMax = Vector2.zero;
                    }
                }
            }
            
        }

        /// <summary>
        /// 让选择的Recttransform 的anchor到当前的坐标
        /// </summary>
        void SetRectTransformAnchorToPos()
        {
            if (GUILayout.Button("设置锚点跟坐标一样"))
            {
                //查找UI Camera
                //if (m_UICamera == null)
                //{
                //    var cameras = FindObjectsOfType(typeof(Camera)) as Camera[];
                //    foreach (var camera in cameras)
                //    {
                //        if (camera.cullingMask == (1 << LayerMask.NameToLayer("UI")))
                //        {
                //            m_UICamera = camera;
                //            break;
                //        }
                //    }
                //}

                GameObject[] gos = Selection.gameObjects;

                List<RectTransform> rects = new List<RectTransform>();
                foreach (var item in gos)
                {
                    if (item == null)
                    {
                        continue;
                    }
                    rects.Add(item.GetComponent<RectTransform>());
                }

                foreach (var item in rects)
                {
                    if (item)
                    {
                        //要求锚点和轴都是(0.5,0.5)
                        if (item.pivot != new Vector2(0.5f, 0.5f))
                        {
                            ShowNotification(new GUIContent("要求轴是(0.5, 0.5)"));
                            return;
                        }
                        Vector2 rectPos = item.localPosition;
                        Vector2 resolution = new Vector2(720, 1280);//Screen.Width 有问题


                        float heightRatio = 0.5f + (rectPos.y / resolution.y);

                        float minY = heightRatio;

                        float widthRatio = 0.5f + (rectPos.x / resolution.x);

                        float minX = widthRatio;


                        Undo.RecordObject(item, "item");
                        item.anchorMin = new Vector2(minX, minY);
                        item.anchorMax = item.anchorMin;

                        //这边设置完锚点后，坐标应该为0
                        item.anchoredPosition = Vector2.zero;
                    }
                }
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

        int openLine = 50;
        void GameTest()
        {
            if (GUILayout.Button("游戏测试"))
            {
                string path = EditorUtility.OpenFilePanel("选择一个文件", Application.dataPath, "*.*");
                path = path.Replace('/', '\\');
                Debug.Log("path:" + path);
                UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(path, openLine,0);
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


        string m_SearchContent = "";

        string[] m_FileParam = new string[] { "*.asset", "*.prefab", "*.csv", "*.unity" };
        int m_FileParamIndex = 0;
        void SearchContentReference()
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("搜索内容:");

            m_SearchContent = EditorGUILayout.TextField(m_SearchContent);

            m_FileParamIndex = EditorGUILayout.MaskField("搜索文件类型:", m_FileParamIndex, m_FileParam);

            if (GUILayout.Button("搜索"))
            {
                Debug.LogError("m_FileParamIndex:" + m_FileParamIndex + ",二进制:" + Convert.ToString(m_FileParamIndex, 2));
                GetReference(m_SearchContent, m_FileParamIndex);
            }

            EditorGUILayout.EndHorizontal();
        }

        void GetReference(string content, int fileParamIndex)
        {
            if (fileParamIndex == 0)
            {
                Debug.LogError("请选项需要搜索的文件类型");
                return;
            }

            //获取选择的选项
            List<string> options = new List<string>();
            if (fileParamIndex == -1)
            {
                for (int i = 0; i < m_FileParam.Length; i++)
                {
                    options.Add(m_FileParam[i]);
                }
            }
            else
            {
                //解析索引
                string indexs = Convert.ToString(fileParamIndex, 2);
                //从后往前 比如 选择第2,3个 返回的索引是 6 二级制是 110 那么从后往前就是 011 ,0代表未选择,1代表已选择
                int index = 0;
                for (int i = indexs.Length - 1; i >= 0; i--)
                {
                    //Debug.LogError("index: " + indexs[i]);
                    if (indexs[i] == '1')
                    {
                        options.Add(m_FileParam[index]);
                    }
                    index++;
                }
            }

            List<UnityEngine.Object> filelst = new List<UnityEngine.Object>();

            for (int i = 0; i < options.Count; i++)
            {
                Debug.LogError("option index: " + options[i]);
                string[] files = Directory.GetFiles(Application.dataPath, options[i], SearchOption.AllDirectories);

                for (int j = 0; j < files.Length; j++)
                {
                    string filePath = files[j].Replace("\\", "/");
                    EditorUtility.DisplayProgressBar("搜索" + options[i] + "文件中", filePath, (float)j / files.Length);

                    string prefab = File.ReadAllText(filePath);
                    bool isContains = prefab.Contains(content);
                    if (isContains)
                    {
                        filelst.Add(AssetDatabase.LoadMainAssetAtPath(filePath.Replace(Application.dataPath, "Assets")));
                        Debug.LogError("匹配到文件名:" + filePath);
                    }
                }
                EditorUtility.ClearProgressBar();
            }

            if (filelst.Count == 0)
            {
                Debug.LogError("匹配不到引用文件");
            }
            Selection.objects = filelst.ToArray();
        }

        string componentName = "UnityEngine.UI.Text";
        void SelectComponent()
        {
            EditorGUILayout.BeginHorizontal();
            //输入框控件
            componentName = EditorGUILayout.TextField("搜索组件名字：", componentName);

            if (GUILayout.Button("选择所有组件"))
            {
                SelectAllComponent(componentName);
            }

            EditorGUILayout.EndHorizontal();
        }

        void SelectAllComponent(string typeName)
        {
            var selectList = Selection.gameObjects;

            if (selectList.Length <= 0)
            {
                this.ShowNotification(new GUIContent("当前没有选择物体!!"));
                return;
            }
            Type t = null;

            Assembly[] abs = System.AppDomain.CurrentDomain.GetAssemblies();//获取已经加载到此应用程序域的程序集
            foreach (var ab in abs)
            {
                Type[] types = ab.GetTypes();
                foreach (var item in types)
                {
                    if (item.FullName == typeName)
                    {
                        t = item;
                        Debug.LogError(item.FullName);
						break;
                    }
                    //Debug.LogError(item.FullName);
                }
            }


            if (t == null)
            {
                this.ShowNotification(new GUIContent("搜索不到对应类型!!"));
                return;
            }

            List<UnityEngine.GameObject> goList = new List<UnityEngine.GameObject>();
            foreach (var item in selectList)
            {
                UnityEngine.Object[] objs = (UnityEngine.Object[])item.GetComponentsInChildren(t, true);
                foreach (var obj in objs)
                {
                    goList.Add((obj as Component).gameObject);
                }
            }

            Selection.objects = goList.ToArray();
        }

        GameObject m_CopyGo;
        private void CopyGameObject()
        {
            if (GUILayout.Button("复制一个GameObject"))
            {
                if (Selection.gameObjects.Length > 0)
                {
                    m_CopyGo = Selection.gameObjects[0];
                    this.ShowNotification(new GUIContent("复制:" + m_CopyGo.name + ",完成!"));
                }
                else
                {
                    this.ShowNotification(new GUIContent("请选择一个GameObject!!"));
                }
            }
            if (GUILayout.Button("增量黏贴一个GameObject"))
            {
                if (Selection.gameObjects.Length > 0)
                {
                    var pasteGO = Selection.gameObjects[0];
                    Undo.RecordObject(pasteGO, "pasteGO");
                    if (m_CopyGo == null)
                    {
                        this.ShowNotification(new GUIContent("请先复制一个GameObject!!"));
                        return;
                    }
                    var copyComponents = m_CopyGo.GetComponents<Component>();
                    var pasteComponents = pasteGO.GetComponents<Component>();
                    foreach (var copyComponent in copyComponents)
                    {
                        foreach (var pasteComponent in pasteComponents)
                        {
                            if (pasteComponent.GetType() == copyComponent.GetType())
                            {
                                if (UnityEditorInternal.ComponentUtility.CopyComponent(copyComponent))
                                {
                                    UnityEditorInternal.ComponentUtility.PasteComponentValues(pasteComponent);
                                }
                            }
                            else
                            {
                                if (UnityEditorInternal.ComponentUtility.CopyComponent(copyComponent))
                                    UnityEditorInternal.ComponentUtility.PasteComponentAsNew(pasteComponent.gameObject);//todo:是否只复制存在的组件?
                            }
                        }
                    }

                    m_CopyGo = null;
                }
                else
                {
                    this.ShowNotification(new GUIContent("请选择一个GameObject!!"));
                }
            }
            if (GUILayout.Button("不新增组件黏贴一个GameObject"))
            {
                if (Selection.gameObjects.Length > 0)
                {
                    var pasteGO = Selection.gameObjects[0];
                    Undo.RecordObject(pasteGO, "pasteGO");
                    if (m_CopyGo == null)
                    {
                        this.ShowNotification(new GUIContent("请先复制一个GameObject!!"));
                        return;
                    }
                    var copyComponents = m_CopyGo.GetComponents<Component>();
                    var pasteComponents = pasteGO.GetComponents<Component>();
                    foreach (var pasteComponent in pasteComponents)
                    {
                        foreach (var copyComponent in copyComponents)
                        {
                            if (pasteComponent.GetType() == copyComponent.GetType())
                            {
                                if (UnityEditorInternal.ComponentUtility.CopyComponent(copyComponent))
                                {
                                    UnityEditorInternal.ComponentUtility.PasteComponentValues(pasteComponent);
                                }
                                break;
                            }
                        }
                    }

                    m_CopyGo = null;
                }
                else
                {
                    this.ShowNotification(new GUIContent("请选择一个GameObject!!"));
                }
            }
        }
    }
}
