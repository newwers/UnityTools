using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AutoCode
{
    public abstract class AutoCodeBuilderBase 
    {
        const string tab = "\t";

        protected string m_FileName;
        protected string m_FilePath;
        int m_nTabNum;
        StringBuilder m_pStringBuilder;

        public AutoCodeBuilderBase(string name, string filePath)
        {
            m_FileName = name;
            m_nTabNum = 0;
            m_pStringBuilder = new StringBuilder();
            m_FilePath = filePath;
        }

        public virtual void Builder()
        {
            //生成文件
            using (var fs = BuilderFile(m_FileName))
            {
                BuilderAutoCode();

                byte[] byteData = System.Text.Encoding.UTF8.GetBytes(m_pStringBuilder.ToString());
                fs.Write(byteData, 0, byteData.Length);


                m_pStringBuilder.Length = 0;
            }

        }
        //------------------------------------------------------
        protected abstract void BuilderAutoCode();
        //------------------------------------------------------
        protected void AddString(string str)
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
        protected FileStream BuilderFile(string fileName)
        {
            //判断是否存在文件
            var filePath = Path.Combine(m_FilePath, fileName + ".cs");//Assets/test/DataManager/CsvBuilder.cs
            //没有则创建,
            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(m_FilePath);
            }
            FileStream fs = File.OpenWrite(filePath);

            return fs;
        }
        //------------------------------------------------------
        protected void AddTabNum()
        {
            m_nTabNum++;
        }
        //------------------------------------------------------
        //------------------------------------------------------
        protected void SubTabNum()
        {
            m_nTabNum--;
        }
    }
}