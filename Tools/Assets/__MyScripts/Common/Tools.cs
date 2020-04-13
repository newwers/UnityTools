using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
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
    }
}

