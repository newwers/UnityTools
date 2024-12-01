using System.Collections;
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
        void BuilderFile(List<SingleField> fields,string fileName)
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
            FileStream fs = File.OpenWrite(filePath);


            BuilderAutoCode(fileName, fields);
            

            byte[] byteData = System.Text.Encoding.UTF8.GetBytes(m_pStringBuilder.ToString());
            fs.Write(byteData,0, byteData.Length);

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
            
            for (int i = 0;i < fields.Count; i++)
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
            string value = $"{name}Data";

            //字典
            AddString($"Dictionary<{key}, {value}> m_vData = new Dictionary<{key}, {value}>();");

            //获取总数据属性
            AddString($"public Dictionary<{key}, {value}> datas");
            AddString("{");

            m_nTabNum++;
            AddString("get{ return m_vData;}");

            m_nTabNum--;
            AddString("}");

            //获取数据函数
            AddString($"public {value} GetData({key} id)");
            AddString("{");
            m_nTabNum++;

            AddString($"{value} data;");
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
                    AddString($"data.{field.field} = csv[i][\"{field.field}\"].ParseArray<{field.type.Replace("[]","")}>();");
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

            AddString("m_vData.Add(data.id, data);");
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