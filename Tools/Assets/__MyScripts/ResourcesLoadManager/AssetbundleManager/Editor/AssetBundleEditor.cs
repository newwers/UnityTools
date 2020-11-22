/*
 管理资源打包前设置包名
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;

namespace newwer
{
    public class AssetBundleEditor : EditorWindow
    {
        //******窗口参数
        private Vector2 _FilesScrollValue;//当前文件滚动的位置

        //******资源参数
        static List<stru_FileInfo> list_Files;//文件列表

        string assetBundleName;
        string assetBundleVariant = "ab";
        private static AssetBundleEditor m_Window;

        string m_folderRootName = "AssetBundles";

        //int indentation;//缩进等级
        struct stru_FileInfo
        {
            public string fileName;
            public string filePath;//绝对路径
            public string assetPath;//U3D内部路径
            public Type assetType;
        }

        [MenuItem("Assets/AssetBundle工具箱/设置AB包名 _F3")]
        private static void OpenSetAssetBundleNameWindow()
        {

            list_Files = new List<stru_FileInfo>();
            //indentation = 1;
            //EditorUtility.ExtractOggFile
            CheckFileSystemInfo();
            m_Window = GetWindow<AssetBundleEditor>("设置AssetBundlesName");
            m_Window.position = new Rect(300, 100, 300, 500);
            m_Window.minSize = new Vector2(300, 500);
            m_Window.Show();


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
            if (GUILayout.Button("刷新资源", new GUIStyle("Box")))
            {
                if (list_Files == null)
                {
                    return;
                }
                list_Files.Clear();
                CheckFileSystemInfo();
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
                //获取系统中的文件夹图标
                GUIContent content = EditorGUIUtility.ObjectContent(null, file.assetType);
                content.text = file.fileName;
                //以lable展示
                GUILayout.Label(content, GUILayout.Height(20));

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
            if (GUILayout.Button("确定"))
            {
                for (int a = 0; a < list_Files.Count; a++)
                {
                    SetBundleName(list_Files[a].assetPath);
                }
            }

            if (GUILayout.Button("打包Windows平台ab包"))
            {
                //  DeterministicAssetBundle = 16,//使每个Object具有唯一不变的hash id，可用于增量式发布AssetBoundle
                //  ChunkBasedCompression = 256,//创建AssetBundle时使用LZ4压缩。默认情况是Lzma格式，下载AssetBoundle后立即解压。
                //  Windows平台
                string packagePath = m_folderRootName  + "/PC";
                if (!Directory.Exists(packagePath))
                {
                    Directory.CreateDirectory(packagePath);
                }
                BuildPipeline.BuildAssetBundles(packagePath, BuildAssetBundleOptions.DeterministicAssetBundle | BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.StandaloneWindows64);
                //AssetDatabase.Refresh();
                //string dir = Directory.GetCurrentDirectory();

                OpenDirectory(m_folderRootName);
            }
            EditorGUILayout.EndVertical();

            //GUI.EndGroup();
        }

        void OpenDirectory(string dirPath)
        {
            System.Diagnostics.Process.Start("explorer.exe", dirPath);
        }

        void MoveToStreamingAssets()
        {
            if (!Directory.Exists(m_folderRootName))
            {
                ShowNotification(new GUIContent("没有 " + m_folderRootName + " 文件夹,不进行复制!"));
                return;
            }
            //string path = Path.Combine(Directory.GetCurrentDirectory(), m_folderRootName);
            string desPath = Application.streamingAssetsPath + "/" + m_folderRootName;
            if (Directory.Exists(desPath))
            {
                Directory.Delete(desPath);
            }
            
            Debug.Log("move: " + m_folderRootName + " to: " + desPath);
            Directory.Move(m_folderRootName, desPath);
            AssetDatabase.Refresh();
        }

        #region 设置选中文件夹下的所有对象的assetBundle的名字
        /// <summary>
        /// 检查文件系统下的信息
        /// </summary>
        private static void CheckFileSystemInfo()
        {
            AssetDatabase.RemoveUnusedAssetBundleNames();//移除无用的AssetBundleName

            string path = EditorUtility.OpenFolderPanel("选择要打包的文件夹路径", "", "");

            CoutineCheck(path);
        }
        /// <summary>
        /// 是文件，继续向下
        /// </summary>
        /// <param name="path"></param>
        private static void CoutineCheck(string path)
        {
            DirectoryInfo directory = new DirectoryInfo(path);
            FileSystemInfo[] fileSystemInfos = directory.GetFileSystemInfos();

            foreach (var item in fileSystemInfos)
            {
                if (item.ToString().EndsWith(".meta") || item.ToString().EndsWith(".cs"))
                {
                    continue;
                }
                // Debug.Log(item);
                int idx = item.ToString().LastIndexOf(@"\");
                string name = item.ToString().Substring(idx + 1);

                CheckFileOrDirectory(item, path + "/" + name);  //item  文件系统，加相对路径
            }

        }
        /// <summary>
        /// 判断是文件还是文件夹,是文件的话加入列表
        /// </summary>
        /// <param name="fileSystemInfo"></param>
        /// <param name="path"></param>
        private static void CheckFileOrDirectory(FileSystemInfo fileSystemInfo, string path)
        {
            FileInfo fileInfo = fileSystemInfo as FileInfo;
            if (fileInfo != null)
            {
                //Debug.Log(fileInfo.Name);//文件名
                //Debug.LogWarning(fileInfo.FullName);//路径带文件名
                //Debug.LogError(fileInfo.DirectoryName);//上级路径
                stru_FileInfo t_file = new stru_FileInfo();
                t_file.fileName = fileInfo.Name.ToLower();
                t_file.filePath = fileInfo.FullName.ToLower();
                t_file.assetPath = fileInfo.FullName.Replace("\\", "/").Replace(Application.dataPath, "Assets").ToLower();//用于下一步获得文件类型
                t_file.assetType = AssetDatabase.GetMainAssetTypeAtPath(t_file.assetPath);
                list_Files.Add(t_file);
            }
            else
            {
                CoutineCheck(path);
            }
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
    }
}


