using System.Collections.Generic;
namespace Z.Data
{
	public partial class CsvData_SystemConfig : ConfigDataBase
	{
		public partial class SystemConfigData : DataBase
		{
			public byte        id;//id
			public uint[]        initialHero;//初始英雄
			public uint[]        initialTeam;//初始上阵
			public uint[]        initialItem;//初始道具
			public uint[]        initialItemValue;//初始道具数量
			public uint        distancePoint;//跑酷距离分数/米
			public byte        rebornTimes;//复活次数上限
			public uint        rebornCost;//复活消耗道具
			public uint        costNum;//复活消耗道具数量
			public uint        diamondRebornCost;//钻石复活需要消耗数量
			public uint        rebornHPRecover;//复活时恢复HP比例（万分比）
			public uint        rebornCountdown;//复活倒计时（秒）
			public byte        adRebornTimes;//广告复活次数上限
			public int        runSkillTime;//技能选择时间限制
			public int        runPetTime;//宠物选择时间限制
			public int        runPetLimit;//宠物选择数量上限
			public uint        shopRefreshTime;//每日商店刷新间隔
			public uint        goldShopRefreshTime;//金币商店刷新间隔
			public uint        bagItemLimit;//背包上限显示
			public uint        fastHangUpNum;//每日快速挂机次数
			public uint        videoHangUpNum;//每日看视频挂机次数
			public uint        fastHangUpCost;//每日快速挂机花费体力
			public uint        fastHangUpRewardTime;//快速挂机奖励时间（秒）
			public uint        hangUpTimeMax;//玩家最长可挂机时间（秒）
			public uint        hangUpTimeMin;//玩家最短挂机时间（秒）
			public uint        hangUpItemInterval;//挂机道具奖励触发间隔（秒）
			public uint        hangUpTipsTime;//挂机时间触发红点时间（秒）
			public uint[]        hangUpGoldShow;//玩家挂机金币堆大小表现（秒）
			public uint        hangUpDefaultTime;//玩家初始赠送挂机时间（秒）
			public uint        actionPointLimit;//玩家体力上限
			public uint        actionPointRecover;//玩家回复1点体力需要的时间（秒）
			public uint        actionPointBuyLimit;//玩家每日钻石购买体力次数
			public uint        actionPointGetNum;//玩家每次钻石购买体力可获得的体力值
			public uint[]        actionPointCost;//玩家购买体力需要消耗的钻石数量
			public uint        actionPointAdLimit;//玩家广告购买体力次数
			public uint        actionPointAdGetNum;//玩家广告购买体力获得的体力值
			public uint        petHatchPosionLimit1;//1号宠物孵化位开启等级
			public uint        petHatchPosionLimit2;//2号宠物孵化位开启等级
			public uint        petHatchPosionLimit3;//3号宠物孵化位开启等级
			public uint        petHatchSpeedUpCost;//加速孵化宠物多少秒需要花费1钻
			public uint        petLevelLimit;//宠物不允许超过玩家的等级数
			public uint[]        commonEquip;//玩家通用装备（衣服，项链，戒指，裤子，鞋子）
			public uint[]        defaultPet;//玩家初始拥有宠物
			public uint        dayShopFreeTime;//每日商店第1格每日免费次数
			public uint        dayShopADTime;//每日商店第1格每日视频次数
			public uint        GoldShopFreeTime;//金币商店第1格每日免费次数
			public uint        GoldShopADTime;//金币商店第1格每日视频次数
			public uint        seasonDay;//排行榜每个赛季的天数（天）
			public uint[]        goldMultipleID;//受到章节系数影响的局内金币道具id
			public uint        OpenSystemUnock;//是否开启功能解锁功能（不开启则默认全解锁）
			public uint        OpenAD;//是否开启广告功能
			public uint        changeNameCost;//修改昵称需要花费的钻石
		}
		Dictionary<byte, SystemConfigData> m_vData = new Dictionary<byte, SystemConfigData>();
		public Dictionary<byte, SystemConfigData> datas
		{
			get{ return m_vData;}
		}
		public SystemConfigData GetData(byte id)
		{
			SystemConfigData data;
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
				SystemConfigData data = new SystemConfigData();
				data.id = csv[i]["id"].Parse<byte>();
				data.initialHero = csv[i]["initialHero"].ParseArray<uint>();
				data.initialTeam = csv[i]["initialTeam"].ParseArray<uint>();
				data.initialItem = csv[i]["initialItem"].ParseArray<uint>();
				data.initialItemValue = csv[i]["initialItemValue"].ParseArray<uint>();
				data.distancePoint = csv[i]["distancePoint"].Parse<uint>();
				data.rebornTimes = csv[i]["rebornTimes"].Parse<byte>();
				data.rebornCost = csv[i]["rebornCost"].Parse<uint>();
				data.costNum = csv[i]["costNum"].Parse<uint>();
				data.diamondRebornCost = csv[i]["diamondRebornCost"].Parse<uint>();
				data.rebornHPRecover = csv[i]["rebornHPRecover"].Parse<uint>();
				data.rebornCountdown = csv[i]["rebornCountdown"].Parse<uint>();
				data.adRebornTimes = csv[i]["adRebornTimes"].Parse<byte>();
				data.runSkillTime = csv[i]["runSkillTime"].Parse<int>();
				data.runPetTime = csv[i]["runPetTime"].Parse<int>();
				data.runPetLimit = csv[i]["runPetLimit"].Parse<int>();
				data.shopRefreshTime = csv[i]["shopRefreshTime"].Parse<uint>();
				data.goldShopRefreshTime = csv[i]["goldShopRefreshTime"].Parse<uint>();
				data.bagItemLimit = csv[i]["bagItemLimit"].Parse<uint>();
				data.fastHangUpNum = csv[i]["fastHangUpNum"].Parse<uint>();
				data.videoHangUpNum = csv[i]["videoHangUpNum"].Parse<uint>();
				data.fastHangUpCost = csv[i]["fastHangUpCost"].Parse<uint>();
				data.fastHangUpRewardTime = csv[i]["fastHangUpRewardTime"].Parse<uint>();
				data.hangUpTimeMax = csv[i]["hangUpTimeMax"].Parse<uint>();
				data.hangUpTimeMin = csv[i]["hangUpTimeMin"].Parse<uint>();
				data.hangUpItemInterval = csv[i]["hangUpItemInterval"].Parse<uint>();
				data.hangUpTipsTime = csv[i]["hangUpTipsTime"].Parse<uint>();
				data.hangUpGoldShow = csv[i]["hangUpGoldShow"].ParseArray<uint>();
				data.hangUpDefaultTime = csv[i]["hangUpDefaultTime"].Parse<uint>();
				data.actionPointLimit = csv[i]["actionPointLimit"].Parse<uint>();
				data.actionPointRecover = csv[i]["actionPointRecover"].Parse<uint>();
				data.actionPointBuyLimit = csv[i]["actionPointBuyLimit"].Parse<uint>();
				data.actionPointGetNum = csv[i]["actionPointGetNum"].Parse<uint>();
				data.actionPointCost = csv[i]["actionPointCost"].ParseArray<uint>();
				data.actionPointAdLimit = csv[i]["actionPointAdLimit"].Parse<uint>();
				data.actionPointAdGetNum = csv[i]["actionPointAdGetNum"].Parse<uint>();
				data.petHatchPosionLimit1 = csv[i]["petHatchPosionLimit1"].Parse<uint>();
				data.petHatchPosionLimit2 = csv[i]["petHatchPosionLimit2"].Parse<uint>();
				data.petHatchPosionLimit3 = csv[i]["petHatchPosionLimit3"].Parse<uint>();
				data.petHatchSpeedUpCost = csv[i]["petHatchSpeedUpCost"].Parse<uint>();
				data.petLevelLimit = csv[i]["petLevelLimit"].Parse<uint>();
				data.commonEquip = csv[i]["commonEquip"].ParseArray<uint>();
				data.defaultPet = csv[i]["defaultPet"].ParseArray<uint>();
				data.dayShopFreeTime = csv[i]["dayShopFreeTime"].Parse<uint>();
				data.dayShopADTime = csv[i]["dayShopADTime"].Parse<uint>();
				data.GoldShopFreeTime = csv[i]["GoldShopFreeTime"].Parse<uint>();
				data.GoldShopADTime = csv[i]["GoldShopADTime"].Parse<uint>();
				data.seasonDay = csv[i]["seasonDay"].Parse<uint>();
				data.goldMultipleID = csv[i]["goldMultipleID"].ParseArray<uint>();
				data.OpenSystemUnock = csv[i]["OpenSystemUnock"].Parse<uint>();
				data.OpenAD = csv[i]["OpenAD"].Parse<uint>();
				data.changeNameCost = csv[i]["changeNameCost"].Parse<uint>();
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
