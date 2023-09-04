using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using static Z.Data.CsvParser;

namespace Z.Data
{
    public class CsvParser 
    {
        public class Cell
        {
            string m_Content;

            public Cell(string content)
            {
                m_Content = content ?? throw new ArgumentNullException(nameof(content));
            }

            //支持配置数据格式

            public T Parse<T>()
            {
                var type = typeof(T);
                //UnityEngine.Debug.Log($"type:{type},name:{type.Name}");
                if (string.IsNullOrWhiteSpace(m_Content))//没填,默认值设置
                {
                    return default(T);
                }
                return (T)Convert.ChangeType(m_Content, type);
            }

            public T[] ParseArray<T>()
            {
                if (string.IsNullOrWhiteSpace(m_Content))//没填,默认值设置
                {
                    return null;
                }
                var array = m_Content.Split('|');
                T[] result = new T[array.Length];
                for (int i = 0; i < result.Length; i++)
                {
                    result[i] = (T)Convert.ChangeType(array[i], typeof(T));
                }
                return result;
            }

            public byte Byte()
            {
                if (byte.TryParse(m_Content,out var value))
                {
                    return value;
                }
                return 0;
            }

            public int Int()
            {
                if (int.TryParse(m_Content, out var value))
                {
                    return value;
                }
                return 0;
            }
        }
        public class Row
        {
            Dictionary<string, Cell> m_vDatas = new Dictionary<string, Cell>();

            public Cell this[string key]
            {
                get 
                {
                    if (m_vDatas.TryGetValue(key,out Cell value))
                    {
                        return value;
                    }
                    return null; 
                }
            }

            public void SetContent(string row,string[] keys)
            {
                //将行解析成字典存储

                var fields = CsvParser.SplitData(row,',');//todo:这边分割时,如果字符串里面存在多个"时,会出现问题,需要优化
                for (int i = 0; i < fields.Count; i++)
                {
                    
                    if (i >= keys.Length || string.IsNullOrWhiteSpace(keys[i]))
                    {
                        continue;
                    }

                    //UnityEngine.Debug.Log($"key:{keys[i]},value:{fields[i]}");

                    m_vDatas[keys[i]] =new Cell(fields[i].Trim());
                }
            }

            
        }

        string m_Content;

        List<Row> m_vRows;

        /// <summary>
        /// 标题行数
        /// </summary>
        int m_nTitleLine = 4;

        public Row this[int index]
        {
            get { return m_vRows[index]; }
            set { m_vRows[index] = value; }
        }

        public void SetContent(string content)
        {
            m_Content = content;

            ParseData();
        }
        //------------------------------------------------------
        private void ParseData()
        {
            if (m_vRows == null)
            {
                m_vRows = new List<Row>();
            }
            m_vRows.Clear();

            //解析内容
            var rows = CsvParser.SplitData(m_Content, '\n');
            var fieldRow = rows[m_nTitleLine-1].Split(',');

            //获取每一行数据
            for (int i = m_nTitleLine; i < rows.Count; i++)//4表示剔除前面几行标题
            {
                if (string.IsNullOrWhiteSpace(rows[i]))
                {
                    continue;
                }
                //将数据转成row格式
                Row row = new Row();

                row.SetContent(rows[i], fieldRow);

                m_vRows.Add(row);
            }
            

        }
        //------------------------------------------------------
        public int RowCount()
        {
            return m_vRows.Count;
        }
        //------------------------------------------------------
        public int GetTitleLine()
        {
            return m_nTitleLine;
        }
        /// <summary>
        /// 根据逗号进行隔开,如果数据是字符串,忽略字符串里面的逗号
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static List<string> SplitData(string data, char separator)
        {
            List<string> dataList = new List<string>();
            bool inQuotes = false;
            string currentData = "";

            foreach (char c in data)
            {
                if (c == '\"')
                {
                    inQuotes = !inQuotes;
                    currentData += c;
                }
                else if (c == separator && !inQuotes)
                {
                    dataList.Add(currentData);
                    currentData = "";
                }
                else
                {
                    currentData += c;
                }
            }

            dataList.Add(currentData);

            return dataList;
        }
    }
}