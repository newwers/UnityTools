using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace Z.Data
{
    /// <summary>
    /// 配置基类
    /// </summary>
    public class ConfigDataBase 
    {
        public string strFilePath;

        public virtual bool LoadBinary(BinaryReader reader)
        {
            return true;
        }

        public virtual bool LoadJson(string strJson)
        {
            return true;
        }

        public virtual bool LoadData(string csvContent, CsvParser csv)
        {
            return true;
        }

        protected virtual void OnLoadCompleted()
        {
        }

        protected virtual void OnAddData(DataBase baseData)
        {
        }

        public virtual void Save(string filename = null)
        {
        }

        public virtual void ClearData()
        {
            OnClearData();
        }

        protected virtual void OnClearData()
        {
        }

        protected string ReadString(BinaryReader reader)
        {
            ushort num = reader.ReadUInt16();
            if (num <= 0)
            {
                return "";
            }

            return Encoding.UTF8.GetString(reader.ReadBytes(num));
        }

        protected void WriterString(BinaryWriter writer, string strValue)
        {
            if (string.IsNullOrEmpty(strValue))
            {
                writer.Write((ushort)0);
                return;
            }

            writer.Write((ushort)strValue.Length);
            byte[] bytes = Encoding.UTF8.GetBytes(strValue);
            writer.Write(bytes);
        }

        public virtual void Destroy()
        {
        }
    }
    
    /// <summary>
    /// 配置数据结构基类
    /// </summary>
    public class DataBase
    {

    }
}