using System;
using System.Collections;
using System.Collections.Generic;

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
                var fields = row.Split(',');
                for (int i = 0; i < fields.Length; i++)
                {
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
            var rows = m_Content.Split('\n');
            var fieldRow = rows[m_nTitleLine-1].Split(',');

            //获取每一行数据
            for (int i = m_nTitleLine; i < rows.Length; i++)//4表示剔除前面几行标题
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
    }
}