/*
	newwer
*/
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace Tools.FileTool
{
    /// <summary>
    /// 文件工具类,
    /// 功能有:文件检查,文件读取,文件写入
    /// 待实现功能:文件移动,文件重命名,实际需求如果需要可以实现
    /// </summary>
    public static class FileTools
    {

        /// <summary>
        /// 判断文件是否存在
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <returns></returns>
        public static bool ExistFile(string path)
        {
            bool isExist = File.Exists(path);

            if (isExist == true)
            {
                Debug.Log("存在文件：" + path);
            }
            else
            {
                Debug.Log("不存在文件：" + path);
            }

            return isExist;
        }

        /// <summary>
        /// 判断文件夹是否存在
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <returns></returns>
        public static bool ExistDirectory(string path)
        {
            bool isExist = Directory.Exists(path);

            if (isExist == true)
            {
                Debug.Log("存在文件夹：" + path);
            }
            else
            {
                Debug.Log("不存在文件夹：" + path);
            }

            return isExist;
        }


        /// <summary>
        /// 写入文件(如果路径不存在就创建一个),默认UTF-8
        /// 通过StreamWriter的方式实现
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <param name="content">文件内容</param>
        ///  /// <param name="encoding">编码格式,默认UTF-8</param>
        ///   /// <param name="append">覆盖还是追加</param>
        public static void WriteFile(string path, string content, Encoding encoding, bool append)
        {
            try
            {
                //using 语句内执行的对象,会在执行完后自动调用dispose函数进行释放资源
                using (StreamWriter writer = new StreamWriter(path, append, encoding))
                {
                    writer.Write(content);
                }
                Debug.Log("文件写入完成:" + path);
            }
            catch (System.Exception e )
            {

                Debug.Log("文件写入失败:" + path + "," + e);
            }

        }

        /// <summary>
        /// 通过字节进行文件写入
        /// </summary>
        /// <param name="path"></param>
        /// <param name="content"></param>
        public static void WriteFile(string path, byte[] content)
        {
            try
            {
                File.WriteAllBytes(path, content);
                Debug.Log("文件写入完成:" + path);
            }
            catch (System.Exception e)
            {

                Debug.Log("文件写入失败:" + path + "," + e);
            }

        }

        /// <summary>
        /// 文件读取,通过流的方式,默认UTF-8编码格式
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <param name="encoding">编码格式,默认UTF-8</param>
        /// <returns></returns>
        public static string ReadFile(string path, Encoding encoding)
        {
            string content = "";
            try
            {
                //using 语句内执行的对象,会在执行完后自动调用dispose函数进行释放资源
                using (StreamReader reader = new StreamReader(path, encoding))
                {
                    content = reader.ReadToEnd();
                }
                Debug.Log("文件读取完成:" + content);
                return content;
            }
            catch (System.Exception e)
            {
                Debug.Log("文件读取失败:" + path + "," + e);
                throw new IOException();
            }
        }


        public static byte[] ReadFile(string path)
        {
            
            try
            {
                byte[] content = File.ReadAllBytes(path);
                return content;
            }
            catch (System.Exception e)
            {
                Debug.Log("文件读取失败:" + path + "," + e);
                throw new IOException();
            }
        }

        /// <summary>
        /// 将字节数组转为UTF8格式的字符串
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string ByteToString(byte[] bytes)
        {
            return Encoding.UTF8.GetString(bytes);
        }
    }
}



