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

            //֧���������ݸ�ʽ

            public T Parse<T>()
            {
                
                var type = typeof(T);
                if (type == typeof(UnityEngine.Vector2))
                {
                    return (T)(object)Vector2();//��ߴ��� vector����ת��
                }
                if (type == typeof(UnityEngine.Vector3))
                {
                    return (T)(object)Vector3();
                }
                if (type == typeof(UnityEngine.Vector4))
                {
                    return (T)(object)Vector4();
                }
                //UnityEngine.Debug.Log($"type:{type},name:{type.Name}");
                if (string.IsNullOrWhiteSpace(m_Content))//û��,Ĭ��ֵ����
                {
                    return default(T);
                }
                return (T)Convert.ChangeType(m_Content, type);
            }

            public T[] ParseArray<T>()
            {
                if (string.IsNullOrWhiteSpace(m_Content))//û��,Ĭ��ֵ����
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

            public UnityEngine.Vector4 Vector4()
            {
                var array = m_Content.Trim('\"').Split(',');//ȥ��ǰ��"��,����,�Ż���
                float x = 0;
                if (array.Length > 0)
                {
                    x = float.Parse(array[0]);
                }
                float y = 0;
                if (array.Length > 1)
                {
                    y = float.Parse(array[1]);
                }

                float z = 0;
                if (array.Length > 2)
                {
                    z = float.Parse(array[2]);
                }

                float w = 0;
                if (array.Length > 3)
                {
                    w = float.Parse(array[3]);
                }


                return new UnityEngine.Vector4(x, y, z,w);
            }

            public UnityEngine.Vector3 Vector3()
            {
                var array = m_Content.Trim('\"').Split(',');
                float x = 0;
                if (array.Length > 0)
                {
                    x = float.Parse(array[0]);
                }
                float y = 0;
                if (array.Length > 1)
                {
                    y =float.Parse(array[1]);
                }

                float z = 0;
                if (array.Length > 2)
                {
                    z = float.Parse(array[2]);
                }


                return new UnityEngine.Vector3(x,y,z);
            }

            public UnityEngine.Vector2 Vector2()
            {
                var array = m_Content.Trim('\"').Split(',');
                float x = 0;
                if (array.Length > 0)
                {
                    x = float.Parse(array[0]);
                }
                float y = 0;
                if (array.Length > 1)
                {
                    y = float.Parse(array[1]);
                }



                return new UnityEngine.Vector2(x, y);
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
                //���н������ֵ�洢

                var fields = CsvParser.SplitData(row,',');//todo:��߷ָ�ʱ,����ַ���������ڶ��"ʱ,���������,��Ҫ�Ż�
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
        /// ��������
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

            //��������
            var rows = CsvParser.SplitData(m_Content.Replace("\r\n", "\n"), '\n');//��csv�ı��з�Ϊ��,�����滻��\r\n,����\n���л����ж�
            var fieldRow = rows[m_nTitleLine-1].Split(',');

            //��ȡÿһ������
            for (int i = m_nTitleLine; i < rows.Count; i++)//4��ʾ�޳�ǰ�漸�б���
            {
                if (string.IsNullOrWhiteSpace(rows[i]))
                {
                    continue;
                }
                //������ת��row��ʽ
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
        /// ���ݶ��Ž��и���,����������ַ���,�����ַ�������Ķ���
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