using System.Collections.Generic;
using UnityEngine;
namespace Z.Data
{
	public partial class CsvData_Text : ConfigDataBase
	{
		public partial class TextData : DataBase
		{
			public uint        id;//文本ID
			public string        textzhCN;//简体中文
			public string        textzhTW;//繁体中文
			public string        textEN;//英文
			public string        textRU;//俄语
			public string        textesES;//西班牙语西班牙
			public string        textptBR;//葡萄牙语巴西
			public string        textDE;//德语
			public string        textJA;//日语
			public string        textFR;//法语
			public string        textPL;//波兰语
			public string        textKO;//韩语
			public string        textTR;//土耳其语
			public string        textes419;//西班牙语拉丁美洲
			public string        textUK;//乌克兰语
			public string        textIT;//意大利语
			public string        textCS;//捷克语
			public string        textptPT;//葡萄牙语葡萄牙
			public string        textHU;//匈牙利语
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
				data.textzhCN = csv[i]["textzhCN"].Parse<string>();
				data.textzhTW = csv[i]["textzhTW"].Parse<string>();
				data.textEN = csv[i]["textEN"].Parse<string>();
				data.textRU = csv[i]["textRU"].Parse<string>();
				data.textesES = csv[i]["textesES"].Parse<string>();
				data.textptBR = csv[i]["textptBR"].Parse<string>();
				data.textDE = csv[i]["textDE"].Parse<string>();
				data.textJA = csv[i]["textJA"].Parse<string>();
				data.textFR = csv[i]["textFR"].Parse<string>();
				data.textPL = csv[i]["textPL"].Parse<string>();
				data.textKO = csv[i]["textKO"].Parse<string>();
				data.textTR = csv[i]["textTR"].Parse<string>();
				data.textes419 = csv[i]["textes419"].Parse<string>();
				data.textUK = csv[i]["textUK"].Parse<string>();
				data.textIT = csv[i]["textIT"].Parse<string>();
				data.textCS = csv[i]["textCS"].Parse<string>();
				data.textptPT = csv[i]["textptPT"].Parse<string>();
				data.textHU = csv[i]["textHU"].Parse<string>();
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
