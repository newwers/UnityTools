using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UI;
using zdq.UI;

namespace zdq.UIEditor
{


    public class UIToolsEditor : EditorWindow
    {
        static EditorWindow m_Window = null;
        [MenuItem("Tools/UI/UIEditorTools _F12")]
        public static void ShowWindow2()
        {
            EditorWindow window = EditorWindow.GetWindow(typeof(UIToolsEditor));
            window.titleContent = new GUIContent("ui编辑工具面板");
            m_Window = window;
        }

        [MenuItem("Tools/UI/CreatePanelScript")]
        public static void CreatePanelScript()
        {
            string path = EditorUtility.SaveFolderPanel("选择创建路径", "Assets/Scripts/UI","");

            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            var strs = path.Split('/');
            string fileName = strs[strs.Length - 1];

            Tools.FileTool.FileTools.WriteFile( $"{path}/{fileName}.cs", $"using Project001.Core;\r\nusing System.Collections;\r\nusing System.Collections.Generic;\r\nusing UnityEngine;\r\n\r\nnamespace Project001.UI\r\n{{\r\n\r\n\r\n    public class {fileName} : BaseUIController\r\n    {{\r\n        \r\n    }}\r\n}}", System.Text.Encoding.UTF8);



            AssetDatabase.Refresh(ImportAssetOptions.Default);

            EditorUtility.DisplayDialog("title", $"创建路径{path}/{fileName}.cs完成!", "ok");

        }

        [MenuItem("Assets/复制文件路径")]
        public static void CopyPath()
        {
            if (Selection.objects == null || Selection.objects.Length == 0)
            {
                return;
            }
            GUIUtility.systemCopyBuffer = AssetDatabase.GetAssetPath(Selection.objects[0]);
            if (m_Window != null)
            {
                m_Window.ShowNotification(new GUIContent("复制成功:" + GUIUtility.systemCopyBuffer));
            }
        }

        Vector2 m_ScrollPos;
        private void OnGUI()
        {
            m_ScrollPos = EditorGUILayout.BeginScrollView(m_ScrollPos);
            SetSelectGameobjectPos();
            SetSelectGameobjectSize();
            SetUIName();
            CopyGameObject();
            IsAddReferencesToggle();
            EditorGUILayout.BeginHorizontal();
            CreateImage();
            CreateText();
            CreateBtn();
            EditorGUILayout.EndHorizontal();
            ImageConvertImageEX();
            ConvertSelectEmptyImage();
            SetParticleOrder();
            SetParticleMasking();
            EditorGUILayout.EndScrollView();
        }

        Vector3 m_SetSelectPos;
        private void SetSelectGameobjectPos()
        {
            m_SetSelectPos = EditorGUILayout.Vector3Field("设置世界坐标:", m_SetSelectPos);

            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("设置"))
            {
                var gos = Selection.gameObjects;
                if (gos == null || gos.Length == 0)
                {
                    ShowNotification(new GUIContent("请选择一个UI"));
                    return;
                }

                foreach (var item in gos)
                {
                    item.transform.position = m_SetSelectPos;
                }
            }

            if (GUILayout.Button("读取选择的物体世界坐标"))
            {
                var gos = Selection.gameObjects;
                if (gos == null || gos.Length == 0)
                {
                    ShowNotification(new GUIContent("请选择一个UI"));
                    return;
                }

                m_SetSelectPos = gos[0].transform.position;
            }
            EditorGUILayout.EndHorizontal();
        }
        //------------------------------------------------------
        Vector2 m_SetSelectSize;
        private void SetSelectGameobjectSize()
        {
            m_SetSelectSize = EditorGUILayout.Vector2Field("设置大小:", m_SetSelectSize);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("设置"))
            {
                var gos = Selection.gameObjects;
                if (gos == null || gos.Length == 0)
                {
                    ShowNotification(new GUIContent("请选择一个UI"));
                    return;
                }

                RectTransform rect;
                foreach (var item in gos)
                {
                    rect = item.GetComponent<RectTransform>();
                    if (rect)//
                    {
                        //rect.sizeDelta = m_SetSelectSize;//当Anchors不重合的时候，设置sizeDelta就不能正确控制RectTransform的大小
                        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, m_SetSelectSize.x);
                        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, m_SetSelectSize.y);
                    }
                }
            }

            if (GUILayout.Button("读取选择的物体UI大小"))
            {
                var gos = Selection.gameObjects;
                if (gos == null || gos.Length == 0)
                {
                    ShowNotification(new GUIContent("请选择一个UI"));
                    return;
                }

                var rect = gos[0].GetComponent<RectTransform>();
                if (rect == null)
                {
                    ShowNotification(new GUIContent("请选择一个UI"));
                    return;
                }

                m_SetSelectSize  = rect.rect.size;
                ShowNotification(new GUIContent("sizeDelta:" + rect.sizeDelta));
            }
            EditorGUILayout.EndHorizontal();
        }
        //------------------------------------------------------
        string renameText;
        void SetUIName()
        {
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

            if (GUILayout.Button("添加后缀数字", GUILayout.Width(100)))
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

            if (GUILayout.Button("添加父物体名字前缀", GUILayout.Width(100)))
            {
                var selectList = Selection.gameObjects;

                if (selectList.Length <= 0)
                {
                    this.ShowNotification(new GUIContent("当前没有选择物体!!"));
                    return;
                }

                //根据Hierarchy上的顺序进行从小到大的排序
                Undo.RecordObjects(selectList, "selectList3");
                foreach (var item in selectList)
                {
                    if (item.transform.parent == null)
                    {
                        continue;
                    }

                    item.name = item.transform.parent.name + item.name;
                }

            }
            EditorGUILayout.EndHorizontal();
            #endregion
        }
        //------------------------------------------------------
        //------------------------------------------------------
        GameObject m_CopyGo;
        private void CopyGameObject()
        {
            if (GUILayout.Button("复制一个GameObject", GUILayout.Height(50)))
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
            if (GUILayout.Button("黏贴一个GameObject", GUILayout.Height(50)))
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
                                //怎么黏贴?
                                if (UnityEditorInternal.ComponentUtility.CopyComponent(copyComponent))
                                {
                                    UnityEditorInternal.ComponentUtility.PasteComponentValues(pasteComponent);
                                }
                                else
                                {
                                    UnityEditorInternal.ComponentUtility.PasteComponentAsNew(pasteComponent.gameObject);//todo:是否只复制存在的组件?
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
        
        //------------------------------------------------------
        #region CreatUI
        bool m_IsAddReferencesToggle = true;
        void IsAddReferencesToggle()
        {
            m_IsAddReferencesToggle = EditorGUILayout.Toggle(new GUIContent("是否添加控件到引用"),m_IsAddReferencesToggle);
        }

        //------------------------------------------------------
        void CreateImage()
        {
            if (GUILayout.Button("创建Image", new GUILayoutOption[] { GUILayout.Height(30) }))
            {
                if (Selection.activeTransform && Selection.activeTransform.GetComponentInParent<Canvas>())
                {
                    GameObject go = new GameObject(Selection.activeGameObject.name + "Image", typeof(Image));
                    Image img = go.GetComponent<Image>();
                    img.raycastTarget = false;
                    go.transform.SetParent(Selection.activeTransform, false);
                    Selection.activeGameObject = go;
                }
            }
        }
        //------------------------------------------------------
        //------------------------------------------------------
        void CreateText()
        {
            if (GUILayout.Button("创建Text", new GUILayoutOption[] { GUILayout.Height(30) }))
            {
                if (Selection.activeTransform && Selection.activeTransform.GetComponentInParent<Canvas>())
                {
                    GameObject go = new GameObject(Selection.activeGameObject.name + "Text", typeof(Text));
                    Text text = go.GetComponent<Text>();

                    text.raycastTarget = false;
                    text.font = AssetDatabase.LoadAssetAtPath<Font>("Assets/Datas/Fonts/default.ttf");
                    text.supportRichText = false;
                    text.fontSize = 20;
                    text.alignment = TextAnchor.MiddleCenter;
                    text.rectTransform.sizeDelta = new Vector2(100,100);//必要时,可暴露出来给外部面板填写参数
                    text.text = "Hi";
                    //text.rectTransform.anchorMin = Vector2.zero;
                    //text.rectTransform.anchorMax = Vector2.one;
                    //text.rectTransform.sizeDelta = Vector2.zero;

                    go.transform.SetParent(Selection.activeTransform, false);
                    Selection.activeGameObject = go;
                }
            }
        }
        //------------------------------------------------------
        void CreateBtn()
        {
            if (GUILayout.Button("创建Button", new GUILayoutOption[] { GUILayout.Height(30) }))
            {
                if (Selection.activeTransform && Selection.activeTransform.GetComponentInParent<Canvas>())
                {
                    GameObject go = new GameObject("Btn", typeof(EmptyImage),typeof(UIEventListener));
                    GameObject icon = new GameObject("icon", typeof(Image));

                    go.transform.SetParent(Selection.activeTransform, false);
                    var goRect = go.transform as RectTransform;
                    goRect.sizeDelta = new Vector2(220,100);

                    icon.GetComponent<Image>().raycastTarget = false;
                    icon.transform.SetParent(go.transform, false);
                    var iconRect = icon.transform as RectTransform;
                    iconRect.sizeDelta = new Vector2(220, 79);
                    icon.GetComponent<Image>().sprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/DatasRef/UI/Textures/common/buttons/button_general_21.png");
                    //UISerialized ui = FindUISerializedReferences();
                    //AddComponentToUISerialized<Text>(ui, text);//按钮不需要添加到UISerialized中
                    Selection.activeGameObject = go;
                }
            }
        }
        #endregion
        //------------------------------------------------------
#region ReplaceComponent
        //------------------------------------------------------
        void ImageConvertImageEX()
        {
            //if (GUILayout.Button("Image互转ImageEX"))
            //{
            //    var selectList = Selection.gameObjects;

            //    if (selectList.Length <= 0)
            //    {
            //        this.ShowNotification(new GUIContent("当前没有选择物体!!"));
            //        return;
            //    }

            //    Undo.RecordObjects(selectList, "selectList5");

            //    List<Image> images = new List<Image>();
            //    foreach (var item in selectList)
            //    {
            //        var items = item.GetComponentsInChildren<Image>(true);
            //        foreach (var img in items)
            //        {
            //            if (img.GetType() == typeof(Image))
            //            {
            //                images.Add(img);
            //            }
            //        }
            //    }

                //List<ImageEx> imagesEx = new List<ImageEx>();
                //foreach (var item in selectList)
                //{
                //    var items = item.GetComponentsInChildren<ImageEx>(true);
                //    foreach (var img in items)
                //    {
                //        if (img.GetType() == typeof(ImageEx))
                //        {
                //            imagesEx.Add(img);
                //        }
                //    }
                //}




            //    foreach (var item in images)
            //    {
            //        Image image = item;
            //        if (image == null)
            //        {
            //            continue;
            //        }

            //        var ui = FindReferences<Image>(image,out int index);

            //        Texture2D texture = null;
            //        if (image.sprite)
            //        {
            //            texture = image.sprite.texture;
            //        }
            //        string path = "";
            //        if (texture != null)
            //        {
            //            path = AssetDatabase.GetAssetPath(texture.GetInstanceID());
            //        }

            //        var color = image.color;
            //        var material = image.material;
            //        var raycast = image.raycastTarget;
            //        var maskable = image.maskable;

            //        GameObject go = item.gameObject;
            //        DestroyImmediate(image);

            //        ImageEx imageEX = go.AddComponent<ImageEx>();
            //        //image.sprite = Sprite.Create((Texture2D)texture, new Rect(Vector2.zero, texture.texelSize), new Vector2(0.5f,0.5f));
            //        if (string.IsNullOrWhiteSpace(path))
            //        {
            //            imageEX.sprite = null;
            //        }
            //        else
            //        {
            //            imageEX.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            //        }

            //        imageEX.color = color;
            //        imageEX.material = material;
            //        imageEX.raycastTarget = raycast;
            //        imageEX.maskable = maskable;

            //        if (ui != null && index != -1)
            //        {
            //            ui.SetWidget<ImageEx>(index, imageEX);
            //            Debug.Log("ui:" + ui.name + ",设置组件:" + imageEX.name);
            //        }

            //        UnityEditor.EditorUtility.SetDirty(go);

            //        this.ShowNotification(new GUIContent(go.name + " 转换成功!"));
            //    }

            //    foreach (var item in imagesEx)
            //    {
            //        ImageEx imageEx = item;
            //        if (imageEx == null)
            //        {
            //            continue;
            //        }

            //        var ui = FindReferences<ImageEx>(imageEx,out int index);

            //        Texture2D texture = null;
            //        if (imageEx.overrideSprite)
            //        {
            //            texture = imageEx.overrideSprite.texture;
            //        }
            //        string path = "";
            //        if (texture)
            //        {
            //            path = AssetDatabase.GetAssetPath(texture.GetInstanceID());
            //        }

            //        var color = imageEx.color;
            //        var material = imageEx.material;
            //        var raycast = imageEx.raycastTarget;
            //        var maskable = imageEx.maskable;

            //        GameObject go = item.gameObject;
            //        DestroyImmediate(imageEx);

            //        Image image = go.AddComponent<Image>();
            //        //image.sprite = Sprite.Create((Texture2D)texture, new Rect(Vector2.zero, texture.texelSize), new Vector2(0.5f,0.5f));
            //        if (string.IsNullOrWhiteSpace(path))
            //        {
            //            image.sprite = null;
            //        }
            //        else
            //        {
            //            image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            //        }
            //        image.color = color;
            //        image.material = material;
            //        image.raycastTarget = raycast;
            //        image.maskable = maskable;

            //        if (ui != null && index != -1)
            //        {
            //            ui.SetWidget<Image>(index, image);
            //            Debug.Log("ui:" + ui.name + ",设置组件:" + image.name);
            //        }

            //        UnityEditor.EditorUtility.SetDirty(go);

            //        this.ShowNotification(new GUIContent(go.name + " 转换成功!"));
            //    }


            //    AssetDatabase.SaveAssets();
            //    AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            //}

        }
        //------------------------------------------------------
        void ConvertSelectEmptyImage()
        {
            if (GUILayout.Button("选择的转EmptyImage"))
            {
                var selectList = Selection.gameObjects;

                if (selectList.Length <= 0)
                {
                    this.ShowNotification(new GUIContent("当前没有选择物体!!"));
                    return;
                }

                Undo.RecordObjects(selectList, "selectList4");

                List<MaskableGraphic> components = new List<MaskableGraphic>();
                foreach (var item in selectList)
                {
                    var images = item.GetComponent<Image>();
                    var rawImages = item.GetComponent<RawImage>();
                    if (images != null)
                    {
                        components.Add(images);
                    }
                    if (rawImages != null)
                    {
                        components.Add(rawImages);
                    }
                }

                foreach (var item in components)
                {
                    var raycast = item.raycastTarget;
                    GameObject go = item.gameObject;
                    DestroyImmediate(item);
                    EmptyImage rawImage = go.AddComponent<EmptyImage>();
                    rawImage.raycastTarget = raycast;

                    UnityEditor.EditorUtility.SetDirty(go);
                }
                ShowNotification(new GUIContent("替换完成"));
            }
        }
        #endregion
        //------------------------------------------------------
        //------------------------------------------------------
        int m_OrderValue;
        void SetParticleOrder()
        {
            EditorGUILayout.BeginHorizontal();

            m_OrderValue = EditorGUILayout.IntField("层级:", m_OrderValue);
            if (GUILayout.Button("叠加层级"))
            {
                var selectList = Selection.gameObjects;

                if (selectList.Length <= 0)
                {
                    this.ShowNotification(new GUIContent("当前没有选择物体!!"));
                    return;
                }

                List<Renderer> allParticles = new List<Renderer>();
                foreach (var go in selectList)
                {
                    var renders = go.GetComponentsInChildren<Renderer>();
                    foreach (var renderer in renders)
                    {
                        allParticles.Add(renderer);
                    }
                }

                foreach (var renderer in allParticles)
                {
                    renderer.sortingOrder += m_OrderValue;
                }
                this.ShowNotification(new GUIContent("设置层级完成!!"));
            }

            EditorGUILayout.EndHorizontal();
        }
        //------------------------------------------------------
        void SetParticleMasking()
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("设置粒子Masking VisibleInsideMask"))
            {
                var selectList = Selection.gameObjects;

                if (selectList.Length <= 0)
                {
                    this.ShowNotification(new GUIContent("当前没有选择物体!!"));
                    return;
                }

                List<ParticleSystemRenderer> allParticles = new List<ParticleSystemRenderer>();
                foreach (var go in selectList)
                {
                    var renders = go.GetComponentsInChildren<Renderer>();
                    foreach (var renderer in renders)
                    {
                        if (renderer is ParticleSystemRenderer)
                        {
                            allParticles.Add(renderer as ParticleSystemRenderer);
                        }
                    }
                }

                foreach (var renderer in allParticles)
                {
                    renderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
                    EditorUtility.SetDirty(renderer.gameObject);
                }
                this.ShowNotification(new GUIContent("设置完成!!"));
            }

            if (GUILayout.Button("设置粒子 None Masking"))
            {
                var selectList = Selection.gameObjects;

                if (selectList.Length <= 0)
                {
                    this.ShowNotification(new GUIContent("当前没有选择物体!!"));
                    return;
                }

                List<ParticleSystemRenderer> allParticles = new List<ParticleSystemRenderer>();
                foreach (var go in selectList)
                {
                    var renders = go.GetComponentsInChildren<Renderer>();
                    foreach (var renderer in renders)
                    {
                        if (renderer is ParticleSystemRenderer)
                        {
                            allParticles.Add(renderer as ParticleSystemRenderer);
                        }
                    }
                }

                foreach (var renderer in allParticles)
                {
                    renderer.maskInteraction = SpriteMaskInteraction.None;
                    EditorUtility.SetDirty(renderer.gameObject);
                }
                this.ShowNotification(new GUIContent("设置完成!!"));
            }

            EditorGUILayout.EndHorizontal();
        }
    }
}
