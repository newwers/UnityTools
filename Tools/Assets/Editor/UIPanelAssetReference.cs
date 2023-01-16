using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEngine.UI;
using UnityEditor.AnimatedValues;
using UnityEditor.VersionControl;
using System.IO;

public class UIPanelAssetReference : EditorWindow
{
    [MenuItem("zdq/界面资源依赖查找工具(可以通过鼠标拖动窗体大小)")]
    public static void ShowWindow2()
    {
        EditorWindow.GetWindow<UIPanelAssetReference>(false);
    }

    struct ShowItem
    {
        public List<Graphic> graphics;
        public bool isExpand;
        public List<int> instanceGUIDs;
        public List<string> assetsPath;

        public ShowItem(List<Graphic> graphics) : this()
        {
            this.graphics = graphics;
            instanceGUIDs = new List<int>();
            assetsPath = new List<string>();
        }

       

    }

    Vector2 m_ScrollPosition = Vector2.zero;
    Dictionary<string, ShowItem> m_AssetUIDic = new Dictionary<string, ShowItem>();
    AnimBool m_ShowExtraFields;
    string m_LastCopyPath;

    private void OnEnable()
    {
        m_ShowExtraFields = new AnimBool(true);
        m_ShowExtraFields.valueChanged.AddListener(Repaint);
    }

    private void OnGUI()
    {
        m_AssetUIDic.Clear();
        //foreach (var item in m_AssetUIDic)
        //{
        //    item.Value.graphics.Clear();
        //}

        Scene scene = GetPanelPath();


        EditorGUILayout.LabelField("复制到: ", m_LastCopyPath);
        if (GUILayout.Button("设置复制文件夹路径"))
        {
            m_LastCopyPath = EditorUtility.OpenFolderPanel("选择文件夹路径", m_LastCopyPath, "默认名称");
        }

        FillData(scene);
        DrawShowItem();
    }
    //------------------------------------------------------
    void FillData(Scene scene)
    {
        var gameObjects = scene.GetRootGameObjects();
        if (gameObjects != null)
        {
            for (int i = 0; i < gameObjects.Length; i++)
            {
                var rootUI = gameObjects[i];
                //EditorGUILayout.LabelField("路径: ", rootUI.name);
                if (rootUI.name.Equals("UIRoot"))
                {
                    var uis = rootUI.transform.GetComponentsInChildren<Graphic>(true);
                    for (int j = 0; j < uis.Length; j++)
                    {
                        var ui = uis[j];
                        if (Valid(ui))
                        {
                            //记录所有符合条件得UI
                            if (ui is Image)
                            {
                                Image image = (Image)ui;
                                string path = AssetDatabase.GetAssetPath(image.sprite);

                                if (!string.IsNullOrEmpty(path))
                                {
                                    var index = path.LastIndexOf('/');
                                    string key = path.Substring(0, index);
                                    if (!m_AssetUIDic.ContainsKey(key))
                                    {
                                        m_AssetUIDic[key] = new ShowItem(new List<Graphic>());
                                    }
                                    m_AssetUIDic[key].graphics.Add(image);
                                    m_AssetUIDic[key].instanceGUIDs.Add(image.sprite.texture.GetInstanceID());
                                    m_AssetUIDic[key].assetsPath.Add(Path.Combine(Application.dataPath + "/../",path));
                                }
                            }
                            else if (ui is RawImage)
                            {
                                RawImage rawImage = (RawImage)ui;
                                string path = AssetDatabase.GetAssetPath(rawImage.texture);

                                if (!string.IsNullOrEmpty(path))
                                {
                                    var index = path.LastIndexOf('/');
                                    string key = path.Substring(0, index);
                                    if (!m_AssetUIDic.ContainsKey(key))
                                    {
                                        m_AssetUIDic[key] = new ShowItem(new List<Graphic>());
                                    }
                                    m_AssetUIDic[key].graphics.Add(rawImage);
                                    m_AssetUIDic[key].instanceGUIDs.Add(rawImage.texture.GetInstanceID());
                                    m_AssetUIDic[key].assetsPath.Add(Path.Combine(Application.dataPath + "/../", path));
                                }
                            }

                        }
                    }
                    //foreach (Transform ui in rootTrs)//遍历transform只会遍历下一级,而不是所有
                    //{
                    //    EditorGUILayout.LabelField("名称: ", ui.name);
                    //}
                    break;
                }
            }
        }
    }
    //------------------------------------------------------
    void DrawShowItem()
    {
        m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition);
        List<string> keys = ListPool<string>.Get();
        keys.AddRange(m_AssetUIDic.Keys);

        for (int i = 0; i < keys.Count; i++)
        {
            var item = m_AssetUIDic[keys[i]];
            //绘制文件夹
            m_ShowExtraFields.target = EditorGUILayout.ToggleLeft(keys[i], m_ShowExtraFields.target);
            item.isExpand = m_ShowExtraFields.target;
            m_AssetUIDic[keys[i]] = item;

            if (GUILayout.Button("选择所有"))
            {
                Selection.instanceIDs = item.instanceGUIDs.ToArray();
            }
            if (GUILayout.Button("复制"))
            {
                m_LastCopyPath = EditorUtility.OpenFolderPanel("选择文件夹路径", m_LastCopyPath, "");
                if (!string.IsNullOrEmpty(m_LastCopyPath))
                {
                    foreach (var asset in item.assetsPath)
                    {
                        Asset meta = new Asset(asset);
                        File.Copy(asset, m_LastCopyPath + "/" + meta.fullName, true);
                        File.Copy(meta.metaPath, m_LastCopyPath + "/" + meta.fullName + ".meta", true);
                    }
                    ShowNotification(new GUIContent("复制完成"));
                }
            }
            int index = 0;

            if (EditorGUILayout.BeginFadeGroup(m_ShowExtraFields.faded))
            {
                EditorGUI.indentLevel++;
                //绘制所有符合ui
                foreach (var ui in item.graphics)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("名称: ", ui.name);
                    if (GUILayout.Button("UI定位"))
                    {
                        EditorGUIUtility.PingObject(ui);
                    }
                    if (GUILayout.Button("资源定位"))
                    {
                        if (ui is Image)
                        {
                            Image image = (Image)ui;
                            EditorGUIUtility.PingObject(image.sprite.texture);
                        }
                        else if (ui is RawImage)
                        {
                            RawImage rawImage = (RawImage)ui;
                            EditorGUIUtility.PingObject(rawImage.texture);
                        }
                    }

                    EditorGUILayout.LabelField("资源路径: ", item.assetsPath[index]);

                    EditorGUILayout.EndHorizontal();
                }
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndFadeGroup();
        }
        ListPool<string>.Release(keys);

        EditorGUILayout.EndScrollView();
    }
    //------------------------------------------------------
    bool Valid(Graphic graphic)
    {
        bool valid = false;
        if (graphic is Image)
        {
            Image image = (Image)graphic;
            valid = image.sprite != null;
        }
        else if (graphic is RawImage)
        {
            RawImage rawImage = (RawImage)graphic;
            valid = rawImage.texture != null;
        }

        return valid;
    }
    //------------------------------------------------------
    Scene GetPanelPath()
    {
        string panelPath = "无";
        Scene scene;
        if (Selection.count > 0 && AssetDatabase.GetAssetPath(Selection.activeObject).EndsWith(".unity"))
        {
            panelPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            scene = UnityEditor.SceneManagement.EditorSceneManager.GetSceneByPath(panelPath);
        }
        else
        {
            scene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
            if (scene != null && !string.IsNullOrWhiteSpace(scene.name) && !string.IsNullOrWhiteSpace(scene.path))
            {
                panelPath = scene.path;
            }
        }
        EditorGUILayout.LabelField("路径: ", panelPath);
        return scene;
    }
    //------------------------------------------------------
    public void CopyDirIntoDestDirectory(string sourceFileName, string destFileName, bool overwrite)
    {
        if (!Directory.Exists(destFileName))
        {
            Directory.CreateDirectory(destFileName);
        }

        foreach (var file in Directory.GetFiles(sourceFileName))
        {
            File.Copy(file, Path.Combine(destFileName, Path.GetFileName(file)), overwrite);
        }

        foreach (var d in Directory.GetDirectories(sourceFileName))
        {
            CopyDirIntoDestDirectory(d, Path.Combine(destFileName, Path.GetFileName(d)), overwrite);
        }
    }
}
