using UnityEngine;
namespace Z.Data
{
	public partial class DataManager : ADataManager
	{
		private CsvData_SystemConfig m_pSystemConfig;
		public CsvData_SystemConfig SystemConfig
		{
			get{ return m_pSystemConfig; }
			private set {m_pSystemConfig=value; }
		}
		protected override ConfigDataBase Parser(CsvParser csvParser, DataConfig.DataInfo data)
		{
			ConfigDataBase csv = null;
			switch(data.guid)
			{
				case 55504:
				{
					SystemConfig = new CsvData_SystemConfig();
					csv = SystemConfig;
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
