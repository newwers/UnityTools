/*
 使用规则:
csv 第一行 中文 name
第二行 字段类型 type int int[] float vector2,3,4(这个类型是自己处理的,其他类型用Convert.ChangeType 进行转换)
第三行 特殊字段 special ,预留扩展 目前有个 group字段
第四行 字段名称 field 
默认第一个字段作为id
用Dictionary<id,value>进行储存,如果是group 那么格式为 Dictionary<id,List<value>>
group类型可以存在同id,多数据格式
其中value 是以 CsvData_csv名称 进行命名,作为一行csv数据格式
 
 */
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Z.Data
{
    public class CsvBuilder
    {
        class SingleField
        {
            public string name;
            public string type;
            public string special;
            public string field;

            public override string ToString()
            {
                return $"name:{name},type:{type},special:{special},field:{field}";
            }
        }

        const string tab = "\t";
        const string specialField_group = "group";

        string m_Content;
        string m_FileName;
        int m_nTabNum;
        StringBuilder m_pStringBuilder;
        private string m_FilePath;

        public CsvBuilder(string str, string name, string filePath)
        {
            m_Content = str;
            m_FileName = name.First().ToString().ToUpper() + name.Substring(1);
            m_nTabNum = 0;
            m_pStringBuilder = new StringBuilder();
            m_FilePath = filePath;
        }

        public void Parse()
        {
            if (m_Content == null) { return; }

            var rows = m_Content.Split('\n');
            Debug.Log("rows:" + rows.Length);
            var fields = new List<SingleField>();
            int cellCount = rows[0].Split(',').Length;
            for (int i = 0; i < cellCount; i++)
            {
                fields.Add(new SingleField());
            }
            for (int i = 0; i < rows.Length; i++)
            {
                BuildRow(rows[i], fields, i);
            }

            for (int i = 0; i < fields.Count; i++)//这边获取到表里面,每个字段类型和名称,根据类型和名称生成代码
            {
                Debug.Log(fields[i].ToString());
            }

            BuilderFile(fields, m_FileName);
        }
        //------------------------------------------------------
        void BuilderFile(List<SingleField> fields, string fileName)
        {
            //判断是否存在文件
            var filePath = Path.Combine(m_FilePath, "CsvData_" + fileName + ".cs");
            //var filePath = Path.Combine(Application.dataPath + "/DataManager/AutoCode/", fileName + ".cs");
            Debug.Log("filePath:" + filePath);
            //没有则创建,
            //if (!Directory.Exists(filePath))
            //{
            //    //Directory.CreateDirectory(Application.dataPath + "/DataManager/AutoCode/");//如何修改创建文件夹
            //}
            FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);

            m_pStringBuilder.Clear();
            BuilderAutoCode(fileName, fields);


            byte[] byteData = System.Text.Encoding.UTF8.GetBytes(m_pStringBuilder.ToString());
            fs.Write(byteData, 0, byteData.Length);

            fs.Dispose();
            m_pStringBuilder.Length = 0;
        }
        //------------------------------------------------------
        void BuilderAutoCode(string name, List<SingleField> fields)
        {
            AddString("using System.Collections.Generic;");
            AddString("using UnityEngine;");
            AddString("namespace Z.Data");
            AddString("{");

            m_nTabNum++;

            AddString($"public partial class CsvData_{name} : ConfigDataBase");
            AddString("{");

            m_nTabNum++;
            AddString($"public partial class {name}Data : DataBase");
            AddString("{");

            //生成配置字段对应数据类型
            m_nTabNum++;

            for (int i = 0; i < fields.Count; i++)
            {
                var field = fields[i];
                if (string.IsNullOrWhiteSpace(field.type) || string.IsNullOrWhiteSpace(field.field))
                {
                    continue;
                }
                AddString($"public {field.type}        {field.field};//{field.name}");
            }


            m_nTabNum--;
            AddString("}");

            //生成配置类
            string key = fields[0].type;
            string id = fields[0].field;
            string value = $"{name}Data";
            bool isGroup = fields[0].special == specialField_group;//同组id

            //字典
            if (isGroup)
            {
                AddString($"Dictionary<{key}, List<{value}>> m_vData = new Dictionary<{key}, List<{value}>>();");
            }
            else
            {
                AddString($"Dictionary<{key}, {value}> m_vData = new Dictionary<{key}, {value}>();");
            }


            //获取总数据属性
            if (isGroup)
            {
                AddString($"public Dictionary<{key}, List<{value}>> datas");
            }
            else
            {
                AddString($"public Dictionary<{key}, {value}> datas");
            }

            AddString("{");

            m_nTabNum++;
            AddString("get{ return m_vData;}");

            m_nTabNum--;
            AddString("}");

            //获取数据函数
            if (isGroup)
            {
                AddString($"public List<{value}> GetData({key} id)");
            }
            else
            {
                AddString($"public {value} GetData({key} id)");
            }

            AddString("{");
            m_nTabNum++;

            if (isGroup)
            {
                AddString($"List<{value}> data;");
            }
            else
            {
                AddString($"{value} data;");
            }

            AddString("if(m_vData.TryGetValue(id, out data))");
            m_nTabNum++;
            AddString("return data;");

            m_nTabNum--;
            AddString("return null;");

            m_nTabNum--;
            AddString("}");

            //加载数据函数
            AddString($"public override bool LoadData(string strContext,CsvParser csv = null)");
            AddString("{");
            m_nTabNum++;

            AddString("if(csv == null) csv = new CsvParser();");
            AddString("csv.SetContent(strContext);");
            AddString("ClearData();");

            //AddString("int titleLine = csv.GetTitleLine();");
            AddString("int rawCount = csv.RowCount();");
            AddString("for(int i = 0; i < rawCount; i++)");
            AddString("{");
            m_nTabNum++;

            AddString($"{value} data = new {value}();");

            for (int i = 0; i < fields.Count; i++)
            {
                var field = fields[i];
                if (string.IsNullOrWhiteSpace(field.type) || string.IsNullOrWhiteSpace(field.field))
                {
                    continue;
                }
                if (field.type.Contains("[]"))//数组解析
                {
                    AddString($"data.{field.field} = csv[i][\"{field.field}\"].ParseArray<{field.type.Replace("[]", "")}>();");
                }
                //else if (field.type.Contains("Vector3"))
                //{
                //    AddString($"data.{field.field} = csv[i][\"{field.field}\"].Parse<{field.type}>();");
                //}
                else
                {
                    AddString($"data.{field.field} = csv[i][\"{field.field}\"].Parse<{field.type}>();");
                }
            }

            //添加数据
            if (isGroup)
            {
                AddString($"if (m_vData.TryGetValue(data.{id},out var list))");
                AddString("{");
                m_nTabNum++;

                AddString("list.Add(data);");

                m_nTabNum--;
                AddString("}");

                AddString("else");
                AddString("{");
                m_nTabNum++;

                AddString($"m_vData[data.{id}] = new List<{value}> {{data}};");

                m_nTabNum--;
                AddString("}");
            }
            else
            {
                AddString($"m_vData.Add(data.{id}, data);");
            }

            AddString("OnAddData(data);");

            m_nTabNum--;
            AddString("}");

            AddString("OnLoadCompleted();");

            AddString($"return true;");



            m_nTabNum--;
            AddString("}");

            //清理数据函数
            AddString($"public override void ClearData()");
            AddString("{");

            m_nTabNum++;
            AddString("m_vData.Clear();");
            AddString("base.ClearData();");

            m_nTabNum--;
            AddString("}");

            //结束,添加括号

            m_nTabNum--;
            AddString("}");

            m_nTabNum--;
            AddString("}");
        }
        //------------------------------------------------------
        void AddString(string str)
        {
            if (m_pStringBuilder == null)
            {
                return;
            }

            for (int i = 0; i < m_nTabNum; i++)
            {
                m_pStringBuilder.Append(tab);
            }

            m_pStringBuilder.AppendLine(str);
        }
        //------------------------------------------------------
        void BuildRow(string row, List<SingleField> fields, int index)
        {
            if (string.IsNullOrWhiteSpace(row))
            {
                return;
            }

            //Debug.Log(row);
            var cells = row.Split(',');
            switch (index)
            {
                case 0://名字
                    for (int i = 0; i < cells.Length; i++)
                    {
                        if (i < fields.Count)
                        {
                            fields[i].name = cells[i].Trim();
                        }
                    }
                    break;
                case 1://类型
                    for (int i = 0; i < cells.Length; i++)
                    {
                        if (i < fields.Count)
                        {
                            fields[i].type = cells[i].Trim();
                        }
                    }
                    break;
                case 2://特殊字符
                    for (int i = 0; i < cells.Length; i++)
                    {
                        if (i < fields.Count)
                        {
                            fields[i].special = cells[i].Trim();
                        }
                    }
                    break;
                case 3://字段名
                    for (int i = 0; i < cells.Length; i++)
                    {
                        if (i < fields.Count)
                        {
                            fields[i].field = cells[i].Trim();
                        }
                    }
                    break;
                default:
                    break;
            }
        }
    }
}