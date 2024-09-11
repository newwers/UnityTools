/*
 管理资源打包前设置包名
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using LitJson;
using Z.FileTool;

namespace Z.Assets
{
    public struct AssetBundleEditorJson
    {
        public string path;
    }

    public class AssetBundleEditor : EditorWindow
    {
        //******窗口参数
        private Vector2 _FilesScrollValue;//当前文件滚动的位置

        //******资源参数
        static List<stru_FileInfo> list_Files;//文件列表

        string assetBundleName;
        string assetBundleVariant = "ab";
        private static AssetBundleEditor m_Window;

        static string m_folderRootName = "AssetBundles";
        AssetBundleEditorJson m_data;
        string m_SelectABPath;

        //int indentation;//缩进等级
        struct stru_FileInfo
        {
            public string fileName;
            public string filePath;//绝对路径
            public string assetPath;//U3D内部路径
            public Type assetType;
        }

        [MenuItem("Tools/AssetBundle工具箱/ab编辑器 _F3")]
        private static void OpenSetAssetBundleNameWindow()
        {

            list_Files = new List<stru_FileInfo>();
            //indentation = 1;
            //EditorUtility.ExtractOggFile
            m_Window = GetWindow<AssetBundleEditor>("设置AssetBundlesName");
            m_Window.position = new Rect(300, 100, 300, 500);
            m_Window.minSize = new Vector2(1000, 800);
            m_Window.Show();


        }

        private void OnEnable()
        {
            Load();
        }


        private void OnGUI()
        {
            if (m_Window != null && EditorApplication.isCompiling)
            {
                m_Window.Close();
                return;
            }

            //设置GUI label参数
            GUI.skin.label.fontSize = 10;
            GUI.skin.label.alignment = TextAnchor.MiddleLeft;
            FilesGUI();
            SetABNameGUI();

            if (GUILayout.Button("移动AB到StreamingAssets"))
            {
                MoveToStreamingAssets();
            }
        }
        void FilesGUI()
        {
            //标题
            GUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(150);
            if (GUILayout.Button("加载所有资源列表", new GUIStyle("Box")))
            {
                LoadAll(false);
            }
            if (GUILayout.Button("加载所有ab资源列表", new GUIStyle("Box")))
            {
                LoadAll(true);
            }
            if (GUILayout.Button("加载上一次路径ab资源列表", new GUIStyle("Box")))
            {
                Load();
            }

            if (GUILayout.Button("保存配置文件", new GUIStyle("Box")))
            {
                Save();
            }
            if (GUILayout.Button("选择打包ab资源路径", new GUIStyle("Box")))
            {
                if (list_Files == null)
                {
                    list_Files = new List<stru_FileInfo>();
                }
                list_Files.Clear();
                bool result = CheckFileSystemInfo();
            }
            EditorGUILayout.EndHorizontal();

            if (list_Files != null)
            {
                GUI.Label(new Rect(5, 35, 100, 20), "选中" + list_Files.Count + "项资源：");
                GUILayout.Space(10);
            }

            _FilesScrollValue = EditorGUILayout.BeginScrollView(_FilesScrollValue, new GUIStyle("Box"), GUILayout.MaxHeight(300));
            AddFileGUIToScroll();
            EditorGUILayout.EndScrollView();

            //GUI.EndGroup();
        }
        /// <summary>
        /// 绘制选中的文件资源到滑块
        /// </summary>
        void AddFileGUIToScroll()
        {
            if (list_Files == null)
            {
                return;
            }
            GUILayout.Space(20);
            foreach (stru_FileInfo file in list_Files)
            {
                // 开启一行
                GUILayout.BeginVertical();
                GUILayout.BeginHorizontal();
                //获取系统中的文件夹图标
                GUIContent content = EditorGUIUtility.ObjectContent(null, file.assetType);
                content.text = file.fileName;
                //以lable展示
                GUILayout.Label(content, GUILayout.Height(20));

                if (GUILayout.Button("设置ab"))
                {
                    AssetImporter importer = AssetImporter.GetAtPath(file.assetPath);
                    if (importer != null)
                    {
                        SetBundleName(file.assetPath);
                        //importer.SaveAndReimport();
                    }
                }
                if (GUILayout.Button("去掉ab"))
                {
                    AssetImporter importer = AssetImporter.GetAtPath(file.assetPath);
                    if (importer != null)
                    {
                        importer.SetAssetBundleNameAndVariant("", "");
                        //importer.SaveAndReimport();
                    }
                }
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
            }
        }
        void SetABNameGUI()
        {
            EditorGUILayout.BeginVertical();
            //设置包名
            //GUILayout.Space(20);
            //EditorGUILayout.BeginHorizontal();
            //assetBundleName = EditorGUILayout.TextField("要设置的包名：", assetBundleName);
            //EditorGUILayout.EndHorizontal();
            //设置AB版本
            EditorGUILayout.BeginHorizontal();
            assetBundleVariant = EditorGUILayout.TextField("资源拓展名：", assetBundleVariant);
            EditorGUILayout.EndHorizontal();

            //确定设置
            GUILayout.Space(20);
            if (GUILayout.Button("设置资源的ab包名"))
            {
                for (int a = 0; a < list_Files.Count; a++)
                {
                    SetBundleName(list_Files[a].assetPath);
                }

                this.ShowNotification(new GUIContent("设置名称完成"));
            }

            if (GUILayout.Button("取消资源的ab包名"))
            {
                for (int a = 0; a < list_Files.Count; a++)
                {
                    AssetImporter importer = AssetImporter.GetAtPath(list_Files[a].assetPath);
                    if (importer != null)
                    {
                        importer.SetAssetBundleNameAndVariant("", "");
                    }
                }

                this.ShowNotification(new GUIContent("取消资源的ab包名完成"));
            }

            if (GUILayout.Button("打包Windows平台ab包"))
            {
                //  DeterministicAssetBundle = 16,//使每个Object具有唯一不变的hash id，可用于增量式发布AssetBoundle
                //  ChunkBasedCompression = 256,//创建AssetBundle时使用LZ4压缩。默认情况是Lzma格式，下载AssetBoundle后立即解压。
                //  Windows平台
                BuildAssetBundles();
                //AssetDatabase.Refresh();
                //string dir = Directory.GetCurrentDirectory();

                //OpenDirectory(m_folderRootName);
            }
            EditorGUILayout.EndVertical();

            //GUI.EndGroup();
        }

        public static AssetBundleManifest BuildAssetBundles()
        {
            string packagePath = m_folderRootName + "/PC";//打包路径在Asset同级下的 AssetBundles/PC
            if (!Directory.Exists(packagePath))
            {
                Directory.CreateDirectory(packagePath);
            }
            var manifest = BuildPipeline.BuildAssetBundles(packagePath, BuildAssetBundleOptions.DeterministicAssetBundle | BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.StandaloneWindows64);

            MoveToStreamingAssets();

            return manifest;
        }

        void OpenDirectory(string dirPath)
        {
            System.Diagnostics.Process.Start("explorer.exe", dirPath);
        }

        static void MoveToStreamingAssets()
        {
            if (!Directory.Exists(m_folderRootName))
            {
                m_Window.ShowNotification(new GUIContent("没有 " + m_folderRootName + " 文件夹,不进行复制!"));
                return;
            }
            //string path = Path.Combine(Directory.GetCurrentDirectory(), m_folderRootName);
            string desPath = Application.streamingAssetsPath + "/" + m_folderRootName;
            if (Directory.Exists(desPath))
            {
                Directory.Delete(desPath,true);
                Debug.Log("delete diretory :" + desPath);
            }
            
            Debug.Log("move: " + m_folderRootName + " to: " + desPath);
            try
            {
                Directory.Move(m_folderRootName, desPath);
            }
            catch (Exception e)
            {

                Debug.Log($"移动文件夹时,出现错误:{e},请检测是否有其他文件打开着,占用文件");
            }
            
            AssetDatabase.Refresh();
        }

        #region 设置选中文件夹下的所有对象的assetBundle的名字
        /// <summary>
        /// 检查文件系统下的信息
        /// </summary>
        private bool CheckFileSystemInfo()
        {
            AssetDatabase.RemoveUnusedAssetBundleNames();//移除无用的AssetBundleName

            string path = EditorUtility.OpenFolderPanel("选择要打ab包的资源文件夹路径", "", "");

            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            CoutineCheck(path);

            m_SelectABPath = path;

            Save();

            return true;
        }
        /// <summary>
        /// 是文件，继续向下
        /// </summary>
        /// <param name="path"></param>
        private static void CoutineCheck(string path,bool isShowAB = false)
        {
            if (string.IsNullOrEmpty(path))
            {
                return;
            }
            DirectoryInfo directory = new DirectoryInfo(path);
            FileSystemInfo[] fileSystemInfos = directory.GetFileSystemInfos();

            foreach (var item in fileSystemInfos)
            {
                if (item.ToString().EndsWith(".meta") || item.ToString().EndsWith(".cs") || item.ToString().EndsWith(".xml"))
                {
                    continue;
                }
                // Debug.Log(item);
                int idx = item.ToString().LastIndexOf(@"\");
                string name = item.ToString().Substring(idx + 1);

                CheckFileOrDirectory(item, path + "/" + name, isShowAB);  //item  文件系统，加相对路径
            }

        }
        /// <summary>
        /// 判断是文件还是文件夹,是文件的话加入列表
        /// </summary>
        /// <param name="fileSystemInfo"></param>
        /// <param name="path"></param>
        private static void CheckFileOrDirectory(FileSystemInfo fileSystemInfo, string path, bool isShowAB = false)
        {
            FileInfo fileInfo = fileSystemInfo as FileInfo;
            if (fileInfo != null)//是文件资源,而不是文件夹
            {
                string assetPath = fileInfo.FullName.Replace("\\", "/").Replace(Application.dataPath, "Assets").ToLower();
                if (isShowAB == false || isShowAB && IsAssetIsAB(assetPath))
                {
                    //Debug.Log(fileInfo.Name);//文件名
                    //Debug.LogWarning(fileInfo.FullName);//路径带文件名
                    //Debug.LogError(fileInfo.DirectoryName);//上级路径
                    stru_FileInfo t_file = new stru_FileInfo();
                    t_file.fileName = fileInfo.Name.ToLower();
                    t_file.filePath = fileInfo.FullName.ToLower();
                    t_file.assetPath = assetPath;//用于下一步获得文件类型
                    t_file.assetType = AssetDatabase.GetMainAssetTypeAtPath(t_file.assetPath);
                    list_Files.Add(t_file);
                }
            }
            else
            {
                CoutineCheck(path, isShowAB);
            }
        }
        /// <summary>
        /// 判断资源是否是有ab标记
        /// </summary>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        static bool IsAssetIsAB(string assetPath)
        {
            var importer = AssetImporter.GetAtPath(assetPath);
            if (importer != null && string.IsNullOrWhiteSpace(importer.assetBundleName) == false)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 设置assetbundle名字
        /// 规则:默认包名取文件上一级的文件夹从Assets开始全称作为包名,后缀自定义
        /// </summary>
        /// <param name="path"></param>
        private void SetBundleName(string path)
        {
            var importer = AssetImporter.GetAtPath(path);
            string[] strs = path.Split('.');
            string[] dictors = strs[0].Split('/');
            string abName = path.Substring(0, path.LastIndexOf('/'));
            Debug.Log("abName:" + abName + ",assetBundleVariant:" + assetBundleVariant);
            importer.SetAssetBundleNameAndVariant(abName, assetBundleVariant);
            if (importer != null)
            {
                if (assetBundleVariant != "")
                {
                    importer.assetBundleVariant = assetBundleVariant;
                }
                if (assetBundleName != "")
                {
                    importer.assetBundleName = abName;
                }
            }
            else
                Debug.Log("importer是空的");
        }

        #endregion

        #region Save

        void Save()
        {
            if (m_data.path == m_SelectABPath)
            {
                return;
            }
            m_data.path = m_SelectABPath;
            var json = JsonMapper.ToJson(m_data);
            FileTools.WriteFile(Application.dataPath + "/Editor/AssetBundleEditorData.json", json, System.Text.Encoding.UTF8);
            ShowNotification(new GUIContent("保存完成"));
        }

        void Load()
        {
            var json = FileTools.ReadFile(Application.dataPath + "/Editor/AssetBundleEditorData.json", System.Text.Encoding.UTF8);
            if (json != null)
            {
                m_data = JsonMapper.ToObject<AssetBundleEditorJson>(json);
                ShowNotification(new GUIContent("加载完成"));
                if (list_Files == null)
                {
                    list_Files = new List<stru_FileInfo>();
                }
                list_Files.Clear();
                CoutineCheck(m_data.path);
            }
            else
            {
                m_data = new AssetBundleEditorJson();
            }
        }

        void LoadAll(bool isShowAB)
        {
            if (list_Files == null)
            {
                list_Files = new List<stru_FileInfo>();
            }
            list_Files.Clear();
            CoutineCheck(Application.dataPath, isShowAB);
        }

        #endregion
    }
}


