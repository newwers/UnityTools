using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Tools
{
    /// <summary>
    /// 工具类
    /// </summary>
    public class Tools
    {
        public static byte[] SerializeObject(object obj)
        {
            if (obj == null)
                return null;
            //内存实例
            MemoryStream ms = new MemoryStream();
            //创建序列化的实例
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(ms, obj);//序列化对象，写入ms流中  
            ms.Position = 0;
            //byte[] bytes = new byte[ms.Length];//这个有错误
            byte[] bytes = ms.GetBuffer();
            ms.Read(bytes, 0, bytes.Length);
            ms.Close();
            return bytes;
        }

        /// <summary>  
        /// 把字节数组反序列化成对象  
        /// </summary>  
        public static T DeserializeObject<T>(byte[] bytes) where T : new()
        {
            object obj = null;
            if (bytes == null)
                return default(T);
            //利用传来的byte[]创建一个内存流
            MemoryStream ms = new MemoryStream(bytes);
            ms.Position = 0;
            BinaryFormatter formatter = new BinaryFormatter();
            obj = formatter.Deserialize(ms);//把内存流反序列成对象  
            ms.Close();
            return (T)obj;
        }

        [MenuItem("Tools/查找物体引用",false,10)]
        public static void FindAssetsReference()
        {
            Dictionary<string, string> guidDics = new Dictionary<string, string>();
            foreach (var item in Selection.objects)
            {
                string path = AssetDatabase.GetAssetPath(item);//获取选中物体路径

                if (string.IsNullOrEmpty(path))
                {
                    continue;
                }

                string guid = AssetDatabase.AssetPathToGUID(path);//通过物体路径得到guid
                if (!guidDics.ContainsKey(guid))
                {
                    guidDics[guid] = item.name;//将选中的物品guid和名字记录选项
                }
            }

            if (guidDics.Count >0)
            {
                List<string> withoutExtensions = new List<string>() { ".prefab",".unity",".mat",".asset"};//设置要搜索的文件后缀
                //var allFiles = Directory.GetFiles(Application.dataPath, "*.*", SearchOption.AllDirectories);
                //List<string> extensionList = new List<string>();
                //foreach (var item in allFiles)
                //{
                //    string extension = Path.GetExtension(item).ToLower();
                //    extensionList.Add(extension);
                //}
                string[] files = Directory.GetFiles(Application.dataPath, "*.*", SearchOption.AllDirectories).Where(s => withoutExtensions.Contains(Path.GetExtension(s).ToLower())).ToArray();//搜索dataPath下的所有文件,符合上面后缀的文件
                for (int i = 0; i < files.Length; i++)
                {
                    string file = files[i];
                    if (i%20==0)//每20个文件刷新一次界面?
                    {
                        bool isCancel = EditorUtility.DisplayCancelableProgressBar("查找相关文件中...", file, (float)i / (float)files.Length);
                        if (isCancel)
                        {
                            break;
                        }
                    }
                    foreach (var item in guidDics)
                    {
                        if (Regex.IsMatch(File.ReadAllText(file),item.Key))//通过正则搜索文件文本中是否含有选中的物体的guid
                        {
                            Debug.Log(string.Format("文件名:{0},路径:{1}",item.Value,file),AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(GetRelativeAssetPath(file)));
                        }
                    }
                }

            }

            EditorUtility.ClearProgressBar();//清空进度条
            Debug.Log("搜索结束");
        }

        /// <summary>
        /// 控制菜单是否可以点击
        /// </summary>
        /// <returns></returns>
        [MenuItem("Tools/查找物体引用",true)]
        public static bool FindAssetsReference_()
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            return (!string.IsNullOrEmpty(path));
        }

        /// <summary>
        /// 将想对路径转换为绝对路径
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        private static string GetRelativeAssetPath(string file)
        {
            return "Assets" + Path.GetFullPath(file).Replace(Path.GetFullPath(Application.dataPath),"").Replace('\\','/');
        }
    }
}

