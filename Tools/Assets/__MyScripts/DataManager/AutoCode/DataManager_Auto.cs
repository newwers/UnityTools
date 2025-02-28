using UnityEngine;
namespace Z.Data
{
	public partial class DataManager : ADataManager
	{
		private CsvData_Text m_pText;
		public CsvData_Text Text
		{
			get{ return m_pText; }
			private set {m_pText=value; }
		}
		protected override ConfigDataBase Parser(CsvParser csvParser, DataConfig.DataInfo data)
		{
			ConfigDataBase csv = null;
			switch(data.guid)
			{
				case "":
				{
					Text = new CsvData_Text();
					csv = Text;
					break;
				}
			}
			if(csv != null)
			{
				csv.LoadData(data.data.text,csvParser);
				m_nLoadCnt++;
			}
			return csv;
		}
	}
}
