using System.Collections.Generic;
namespace Z.Data
{
	public partial class CsvData_Text : ConfigDataBase
	{
		public partial class TextData : DataBase
		{
			public uint        id;//文本ID
			public string        textCN;//中文文本
			public string        textEN;//英文文本
		}
		Dictionary<uint, TextData> m_vData = new Dictionary<uint, TextData>();
		public Dictionary<uint, TextData> datas
		{
			get{ return m_vData;}
		}
		public TextData GetData(uint id)
		{
			TextData data;
			if(m_vData.TryGetValue(id, out data))
				return data;
			return null;
		}
		public override bool LoadData(string strContext,CsvParser csv = null)
		{
			if(csv == null) csv = new CsvParser();
			csv.SetContent(strContext);
			ClearData();
			int rawCount = csv.RowCount();
			for(int i = 0; i < rawCount; i++)
			{
				TextData data = new TextData();
				data.id = csv[i]["id"].Parse<uint>();
				data.textCN = csv[i]["textCN"].Parse<string>();
				data.textEN = csv[i]["textEN"].Parse<string>();
				m_vData.Add(data.id, data);
				OnAddData(data);
			}
			OnLoadCompleted();
			return true;
		}
		public override void ClearData()
		{
			m_vData.Clear();
			base.ClearData();
		}
	}
}
