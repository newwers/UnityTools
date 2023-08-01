using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Z.Data
{
    public class DataManagerBuilder 
    {
        const string tab = "\t";

        DataConfig m_Configs;
        string m_FileName;
        int m_nTabNum;
        StringBuilder m_pStringBuilder;

        public DataManagerBuilder(DataConfig configs, string name="DataManager_Auto")
        {
            m_Configs = configs;
            m_FileName = name;
            m_nTabNum = 0;
            m_pStringBuilder = new StringBuilder();
        }
        //------------------------------------------------------
        public void Parse()
        {
            if (m_Configs == null)
            {
                return;
            }

            //生成文件
            var fs = BuilderFile(m_FileName);

            BuilderAutoCode(m_FileName);
            



            byte[] byteData = System.Text.Encoding.UTF8.GetBytes(m_pStringBuilder.ToString());
            fs.Write(byteData, 0, byteData.Length);

            fs.Dispose();
            m_pStringBuilder.Length = 0;
        }

        //------------------------------------------------------
        FileStream BuilderFile(string fileName)
        {
            //判断是否存在文件
            var filePath = Path.Combine(Application.dataPath + "/test/DataManager/AutoCode/", fileName + ".cs");//Assets/test/DataManager/CsvBuilder.cs
            //没有则创建,
            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(Application.dataPath + "/test/DataManager/AutoCode/");
            }
            FileStream fs = File.OpenWrite(filePath);

            return fs;
        }
        //------------------------------------------------------
        //------------------------------------------------------
        void BuilderAutoCode(string name)
        {
            AddString("using UnityEngine;");
            AddString("namespace Z.Data");
            AddString("{");

            m_nTabNum++;

            AddString($"public partial class DataManager : ADataManager");
            AddString("{");

            m_nTabNum++;
            
            //根据配置文件生成每个配置的字段
            for (int i = 0; i < m_Configs.vConfigs.Count; i++)
            {
                var cfg = m_Configs.vConfigs[i];
                string cfgName = cfg.data.name.First().ToString().ToUpper() + cfg.data.name.Substring(1);
                AddString($"private CsvData_{cfgName} m_p{cfgName};");
                AddString($"public CsvData_{cfgName} {cfgName}");
                AddString("{");

                m_nTabNum++;
                AddString($"get{{ return m_p{cfgName}; }}");
                AddString($"private set {{m_p{cfgName}=value; }}");

                m_nTabNum--;
                AddString("}");
            }

            //调用解析函数
            AddString("protected override ConfigDataBase Parser(CsvParser csvParser, DataConfig.DataInfo data)");
            AddString("{");
            m_nTabNum++;

            AddString("ConfigDataBase csv = null;");
            AddString("switch(data.guid)");
            AddString("{");
            m_nTabNum++;

            for (int i = 0; i < m_Configs.vConfigs.Count; i++)
            {
                var cfg = m_Configs.vConfigs[i];
                string cfgName = cfg.data.name.First().ToString().ToUpper() + cfg.data.name.Substring(1);

                AddString($"case {cfg.guid}:");
                AddString("{");
                m_nTabNum++;

                AddString($"{cfgName} = new CsvData_{cfgName}();");
                AddString($"csv = {cfgName};");
                AddString("break;");


                m_nTabNum--;
                AddString("}");
            }


            m_nTabNum--;
            AddString("}");

            AddString("if(csv != null)");
            AddString("{");
            m_nTabNum++;

            AddString($"csv.LoadData(data.data.text,csvParser);");
            AddString("m_nLoadCnt++;");


            m_nTabNum--;
            AddString("}");

            AddString("return csv;");


            m_nTabNum--;
            AddString("}");

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
    }
}