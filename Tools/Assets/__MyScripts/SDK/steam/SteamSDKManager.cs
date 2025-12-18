/*
 必须启动 Steam 客户端并登录账号
开发测试时，确保 Steamworks 后台的 "可见性" 设置为 "公开" 或 "好友可见"
运行 Unity 项目前，先通过 Steam 启动一次游戏（确保 App ID 关联正确）

开发时：保留 steam_appid.txt 文件
发布时：移除 steam_appid.txt 文件（Steam客户端会自动提供App ID）

 */
using Steamworks;
using System;
using UnityEngine;

public class SteamSDKManager : BaseMonoSingleClass<SteamSDKManager>
{
    //todo:自定义成就名称
    private enum Achievement : int
    {
        ACH_WIN_ONE_GAME,
        ACH_WIN_100_GAMES,
        ACH_HEAVY_FIRE,
        ACH_TRAVEL_FAR_ACCUM,
        ACH_TRAVEL_FAR_SINGLE,

        ACH_GET_COIN_30,//获得30个硬币
        ACH_GET_COIN_100,
        ACH_GET_COIN_500,
        ACH_GET_COIN_1000,
        ACH_GET_COIN_10000,

        ACH_POP_BALLOON_1,//戳破1个气球
        ACH_POP_BALLOON_20,//戳破20个气球
        ACH_POP_BALLOON_50,
        ACH_POP_BALLOON_100,

        ACH_OPEN_BALLOON_GIFT_BOX_1,//打开1个气球礼盒
        ACH_OPEN_BALLOON_GIFT_BOX_10,
        ACH_OPEN_BALLOON_GIFT_BOX_50,
        ACH_OPEN_BALLOON_GIFT_BOX_100,

        ACH_OPEN_BALLOON_GIFT_BOX_QUALITY_2,//打开2级稀有度气球礼盒
        ACH_OPEN_BALLOON_GIFT_BOX_QUALITY_3,//打开3级稀有度气球礼盒
        ACH_OPEN_BALLOON_GIFT_BOX_QUALITY_4,//打开4级稀有度气球礼盒

        ACH_OPEN_ITEM_GIFT_BOX_1,//打开1个道具礼盒
        ACH_OPEN_ITEM_GIFT_BOX_10,
        ACH_OPEN_ITEM_GIFT_BOX_100,

        ACH_ATTACH_BALLOON_TO_ITEM_FIRST,//首次将气球固定到物品上
        ACH_POP_ITEM_FIRST,//首次绽放物品

        ACH_COLLECT_ONE_ITEM_SET,       // 集齐任意一套物品礼包
        ACH_COLLECT_THREE_ITEM_SETS,    // 集齐3套物品礼包
        ACH_COLLECT_ALL_ITEM_SETS,       // 集齐所有物品礼包

        ACH_UNLOCK_FIRST_BALLOON_PEDIA,//解锁首个气球图鉴
        ACH_UNLOCK_FIRST_LEVEL2_BALLOON_PEDIA,////解锁首款2阶气球图鉴
        ACH_UNLOCK_FIRST_LEVEL3_BALLOON_PEDIA,//解锁首款3阶气球图鉴
        ACH_UNLOCK_10_BASE_BALLOON_PEDIA,//解锁10款基础气球图鉴
        ACH_UNLOCK_30_BASE_BALLOON_PEDIA,//解锁30款基础气球图鉴
        ACH_UNLOCK_ALL_BASE_BALLOON_PEDIA,//解锁所有础气球图鉴
        ACH_UNLOCK_ALL_BALLOON_PEDIA,//解锁全部气球图鉴

        ACH_COLLECT_ONE_BALLOON_SET_BASE,//集齐任意一套气球礼包里的全部基础款式
        ACH_COLLECT_ONE_BALLOON_SET_ALL,//集齐任意一套气球礼包里的全部款式

    };

    private class Achievement_t
    {
        public Achievement m_eAchievementID;
        public string m_strName;
        public string m_strDescription;
        public bool m_bAchieved;

        /// <summary>
        /// 创建一个成就. 你必须同时把数据镜像提交到这个上面 https://partner.steamgames.com/apps/achievements/yourappid
        /// </summary>
        /// <param name="achievement">The "API Name Progress Stat" used to uniquely identify the achievement.</param>
        /// <param name="name">The "Display Name" that will be shown to players in game and on the Steam Community.</param>
        /// <param name="desc">The "Description" that will be shown to players in game and on the Steam Community.</param>
        public Achievement_t(Achievement achievementID, string name, string desc)
        {
            m_eAchievementID = achievementID;
            m_strName = name;
            m_strDescription = desc;
            m_bAchieved = false;
        }
    }

    private Achievement_t[] m_Achievements = new Achievement_t[] {
        new Achievement_t(Achievement.ACH_WIN_ONE_GAME, "Winner", ""),
        new Achievement_t(Achievement.ACH_WIN_100_GAMES, "Champion", ""),
        new Achievement_t(Achievement.ACH_TRAVEL_FAR_ACCUM, "Interstellar", ""),
        new Achievement_t(Achievement.ACH_TRAVEL_FAR_SINGLE, "Orbiter", ""),

        new Achievement_t(Achievement.ACH_GET_COIN_30, "获得30个硬币", ""),
        new Achievement_t(Achievement.ACH_GET_COIN_100, "获得100个硬币", ""),
        new Achievement_t(Achievement.ACH_GET_COIN_500, "获得500个硬币", ""),
        new Achievement_t(Achievement.ACH_GET_COIN_1000, "获得1000个硬币", ""),
        new Achievement_t(Achievement.ACH_GET_COIN_10000, "获得10000个硬币", ""),

        new Achievement_t(Achievement.ACH_POP_BALLOON_1, "戳破1个气球", ""),
        new Achievement_t(Achievement.ACH_POP_BALLOON_20, "戳破20个气球", ""),
        new Achievement_t(Achievement.ACH_POP_BALLOON_50, "戳破50个气球", ""),
        new Achievement_t(Achievement.ACH_POP_BALLOON_100, "戳破100个气球", ""),

        new Achievement_t(Achievement.ACH_OPEN_BALLOON_GIFT_BOX_1, "打开1个气球礼盒", ""),
        new Achievement_t(Achievement.ACH_OPEN_BALLOON_GIFT_BOX_10, "打开10个气球礼盒", ""),
        new Achievement_t(Achievement.ACH_OPEN_BALLOON_GIFT_BOX_50, "打开50个气球礼盒", ""),
        new Achievement_t(Achievement.ACH_OPEN_BALLOON_GIFT_BOX_100, "打开100个气球礼盒", ""),

        new Achievement_t(Achievement.ACH_OPEN_BALLOON_GIFT_BOX_QUALITY_2, "打开2级稀有度气球礼盒", ""),
        new Achievement_t(Achievement.ACH_OPEN_BALLOON_GIFT_BOX_QUALITY_3, "打开3级稀有度气球礼盒", ""),
        new Achievement_t(Achievement.ACH_OPEN_BALLOON_GIFT_BOX_QUALITY_4, "打开4级稀有度气球礼盒", ""),

        new Achievement_t(Achievement.ACH_OPEN_ITEM_GIFT_BOX_1, "打开1个道具礼盒", ""),
        new Achievement_t(Achievement.ACH_OPEN_ITEM_GIFT_BOX_10, "打开10个道具礼盒", ""),
        new Achievement_t(Achievement.ACH_OPEN_ITEM_GIFT_BOX_100, "打开100个道具礼盒", ""),

        new Achievement_t(Achievement.ACH_ATTACH_BALLOON_TO_ITEM_FIRST, "首次将气球固定到物品上", ""),
        new Achievement_t(Achievement.ACH_POP_ITEM_FIRST, "首次戳破物品", ""),

        new Achievement_t(Achievement.ACH_COLLECT_ONE_ITEM_SET, "集齐任意一套物品礼包", ""),
        new Achievement_t(Achievement.ACH_COLLECT_THREE_ITEM_SETS, "集齐3套物品礼包", ""),
        new Achievement_t(Achievement.ACH_COLLECT_ALL_ITEM_SETS, "集齐所有物品礼包", ""),

        new Achievement_t(Achievement.ACH_UNLOCK_FIRST_BALLOON_PEDIA, "解锁首个气球图鉴", ""),
        new Achievement_t(Achievement.ACH_UNLOCK_FIRST_LEVEL2_BALLOON_PEDIA, "解锁首款2阶气球图鉴", ""),
        new Achievement_t(Achievement.ACH_UNLOCK_FIRST_LEVEL3_BALLOON_PEDIA, "解锁首款3阶气球图鉴", ""),
        new Achievement_t(Achievement.ACH_UNLOCK_10_BASE_BALLOON_PEDIA, "解锁10款基础气球图鉴", ""),
        new Achievement_t(Achievement.ACH_UNLOCK_30_BASE_BALLOON_PEDIA, "解锁30款基础气球图鉴", ""),
        new Achievement_t(Achievement.ACH_UNLOCK_ALL_BASE_BALLOON_PEDIA, "解锁所有础气球图鉴", ""),
        new Achievement_t(Achievement.ACH_UNLOCK_ALL_BALLOON_PEDIA, "解锁全部气球图鉴", ""),

        new Achievement_t(Achievement.ACH_COLLECT_ONE_BALLOON_SET_BASE, "集齐任意一套气球礼包里的全部基础款式", ""),
        new Achievement_t(Achievement.ACH_COLLECT_ONE_BALLOON_SET_ALL, "集齐任意一套气球礼包里的全部款式", ""),



    };

    public static Action OnSteamSDKInitAction;

    public string PlayerName;
    public string Language { get; private set; }

    // 游戏id
    private CGameID m_GameID;

    /// <summary>
    /// 是否请求了Steam状态?
    /// </summary>
    private bool m_bRequestedStats;
    /// <summary>
    /// Steam状态是否有效?
    /// </summary>
    private bool m_bStatsValid;

    /// <summary>
    /// 是否在当前帧储存状态?
    /// </summary>
    private bool m_bStoreStats;

    #region 成就统计数据
    // Current Stat details
    private float m_flGameFeetTraveled;//游戏中玩家行走的距离（英尺）
    private float m_ulTickCountGameStart;//游戏开始时的Tick计数
    private double m_flGameDurationSeconds;//游戏持续时间（秒）

    // Persisted Stat details
    private int m_nTotalGamesPlayed;//玩家总共玩过的游戏次数 打开道具礼盒次数来统计
    private int m_nTotalNumWins;//玩家总共赢得的游戏次数  用气球爆炸次数来统计
    private int m_nTotalNumLosses;//玩家总共输掉的游戏次数  打开气球礼盒次数来统计
    private float m_flTotalFeetTraveled;//玩家总共行走的距离（英尺）  用拾取硬币数来统计
    private float m_flMaxFeetTraveled;//玩家在单次游戏中行走的最大距离（英尺）
    private float m_flAverageSpeed;//玩家平均速度（英尺/秒）

    private int m_nOpenBalloonGiftBoxQuality2;//打开2级稀有度气球礼盒次数
    private int m_nOpenBalloonGiftBoxQuality3;//打开3级稀有度气球礼盒次数
    private int m_nOpenBalloonGiftBoxQuality4;//打开4级稀有度气球礼盒次数

    private int m_nAttachBalloonToItemNum;//将气球固定到物品上次数
    private int m_nPopItemNum;//戳破物品次数

    private int m_nCollectedItemSets;    // 已集齐的物品礼包数量
    /// <summary>
    /// 解锁了多少款气球
    /// </summary>
    private int m_nUnlockBalloons;
    /// <summary>
    /// 解锁了多少套气球礼包
    /// </summary>
    private int m_nUnlockGiftPackBalloons;
    /// <summary>
    /// 解锁了多少套最大等级的气球礼包
    /// </summary>
    private int m_nUnlockMaxLevelGiftPackBalloons;

    //升级到2级的气球数量
    private int m_nLevel2Balloons;
    //升级到3级的气球数量
    private int m_nLevel3Balloons;

    #endregion

    //玩家的Steam Overlay激活回调
    protected Callback<GameOverlayActivated_t> m_GameOverlayActivated;
    //获取游戏当前玩家数量的回调
    private CallResult<NumberOfCurrentPlayers_t> m_NumberOfCurrentPlayers;
    protected Callback<UserStatsReceived_t> m_UserStatsReceived;
    protected Callback<UserStatsStored_t> m_UserStatsStored;
    protected Callback<UserAchievementStored_t> m_UserAchievementStored;


    private void OnEnable()
    {
        if (!SteamManager.Initialized)
        {
            return;
        }

        OnSteamSDKInitAction?.Invoke();

        // Cache the GameID for use in the Callbacks
        m_GameID = new CGameID(SteamUtils.GetAppID());

        //每次激活或停用 Steam 界面时，它都会向您发送一个回调
        //steam Overlay 默认快捷键：Shift + Tab
        m_GameOverlayActivated = Callback<GameOverlayActivated_t>.Create(OnGameOverlayActivated);
        m_NumberOfCurrentPlayers = CallResult<NumberOfCurrentPlayers_t>.Create(OnNumberOfCurrentPlayers);
        m_UserStatsReceived = Callback<UserStatsReceived_t>.Create(OnUserStatsReceived);
        m_UserStatsStored = Callback<UserStatsStored_t>.Create(OnUserStatsStored);
        m_UserAchievementStored = Callback<UserAchievementStored_t>.Create(OnAchievementStored);

        // These need to be reset to get the stats upon an Assembly reload in the Editor.
        m_bRequestedStats = false;
        m_bStatsValid = false;

        //SteamUserStats.RequestCurrentStats();//请求当前玩家的统计数据

        // 获取当前玩家昵称
        PlayerName = SteamFriends.GetPersonaName();
        LogManager.Log($"Steam SDK 初始化成功 (AppID: {SteamUtils.GetAppID()}), 当前用户: {PlayerName}");

        // 获取当前玩家Steam ID
        CSteamID steamId = SteamUser.GetSteamID();
        ulong steamId64 = steamId.m_SteamID;
        LogManager.Log($"当前玩家Steam ID: {steamId64}");

        //获取玩家语言
        Language = SteamApps.GetCurrentGameLanguage();
        LogManager.Log($"当前游戏语言: {Language}");
        //获取玩家steam客户端语言
        Language = SteamUtils.GetSteamUILanguage();
        LogManager.Log($"当前steam客户端语言: {Language}");
    }



    private void Update()
    {
        if (!SteamManager.Initialized)
        {
            return;
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SteamAPICall_t handle = SteamUserStats.GetNumberOfCurrentPlayers();
            m_NumberOfCurrentPlayers.Set(handle);
            LogManager.Log("调用获取在线玩家数量函数");
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            //ResetAllAchievement();

        }

        if (!m_bRequestedStats)
        {
            // Is Steam Loaded? if no, can't get stats, done
            if (!SteamManager.Initialized)
            {
                m_bRequestedStats = true;
                return;
            }

            // If yes, request our stats
            bool bSuccess = SteamUserStats.RequestCurrentStats();

            // This function should only return false if we weren't logged in, and we already checked that.
            // But handle it being false again anyway, just ask again later.
            m_bRequestedStats = bSuccess;
        }

        if (!m_bStatsValid)
            return;

        // Get info from sources

        // Evaluate achievements
        foreach (Achievement_t achievement in m_Achievements)
        {
            if (achievement.m_bAchieved)
                continue;

            switch (achievement.m_eAchievementID)
            {
                case Achievement.ACH_WIN_ONE_GAME:
                    if (m_nTotalNumWins != 0)
                    {
                        UnlockAchievement(achievement);
                    }
                    break;
                case Achievement.ACH_WIN_100_GAMES:
                    if (m_nTotalNumWins >= 100)
                    {
                        UnlockAchievement(achievement);
                    }
                    break;
                case Achievement.ACH_TRAVEL_FAR_ACCUM:
                    if (m_flTotalFeetTraveled >= 5280)
                    {
                        UnlockAchievement(achievement);
                    }
                    break;

                case Achievement.ACH_TRAVEL_FAR_SINGLE:
                    if (m_flGameFeetTraveled >= 500)
                    {
                        UnlockAchievement(achievement);
                    }
                    break;
                //硬币成就
                case Achievement.ACH_GET_COIN_30:
                    if (m_flTotalFeetTraveled >= 30)
                    {
                        UnlockAchievement(achievement);
                    }
                    break;
                case Achievement.ACH_GET_COIN_100:
                    if (m_flTotalFeetTraveled >= 100)
                    {
                        UnlockAchievement(achievement);
                    }
                    break;
                case Achievement.ACH_GET_COIN_500:
                    if (m_flTotalFeetTraveled >= 500)
                    {
                        UnlockAchievement(achievement);
                    }
                    break;
                case Achievement.ACH_GET_COIN_1000:
                    if (m_flTotalFeetTraveled >= 1000)
                    {
                        UnlockAchievement(achievement);
                    }
                    break;
                case Achievement.ACH_GET_COIN_10000:
                    if (m_flTotalFeetTraveled >= 10000)
                    {
                        UnlockAchievement(achievement);
                    }
                    break;
                //气球戳破
                case Achievement.ACH_POP_BALLOON_1:
                    if (m_nTotalNumWins >= 1)
                    {
                        UnlockAchievement(achievement);
                    }
                    break;
                case Achievement.ACH_POP_BALLOON_20:
                    if (m_nTotalNumWins >= 20)
                    {
                        UnlockAchievement(achievement);
                    }
                    break;
                case Achievement.ACH_POP_BALLOON_50:
                    if (m_nTotalNumWins >= 50)
                    {
                        UnlockAchievement(achievement);
                    }
                    break;
                case Achievement.ACH_POP_BALLOON_100:
                    if (m_nTotalNumWins >= 100)
                    {
                        UnlockAchievement(achievement);
                    }
                    break;
                //打开气球礼盒
                case Achievement.ACH_OPEN_BALLOON_GIFT_BOX_1:
                    if (m_nTotalNumLosses >= 1)
                    {
                        UnlockAchievement(achievement);
                    }
                    break;
                case Achievement.ACH_OPEN_BALLOON_GIFT_BOX_10:
                    if (m_nTotalNumLosses >= 10)
                    {
                        UnlockAchievement(achievement);
                    }
                    break;
                case Achievement.ACH_OPEN_BALLOON_GIFT_BOX_50:
                    if (m_nTotalNumLosses >= 50)
                    {
                        UnlockAchievement(achievement);
                    }
                    break;
                case Achievement.ACH_OPEN_BALLOON_GIFT_BOX_100:
                    if (m_nTotalNumLosses >= 100)
                    {
                        UnlockAchievement(achievement);
                    }
                    break;
                //打开道具礼盒
                case Achievement.ACH_OPEN_ITEM_GIFT_BOX_1:
                    if (m_nTotalGamesPlayed >= 1)
                    {
                        UnlockAchievement(achievement);
                    }
                    break;
                case Achievement.ACH_OPEN_ITEM_GIFT_BOX_10:
                    if (m_nTotalGamesPlayed >= 10)
                    {
                        UnlockAchievement(achievement);
                    }
                    break;
                case Achievement.ACH_OPEN_ITEM_GIFT_BOX_100:
                    if (m_nTotalGamesPlayed >= 100)
                    {
                        UnlockAchievement(achievement);
                    }
                    break;
                //打开气球礼盒品质成就
                case Achievement.ACH_OPEN_BALLOON_GIFT_BOX_QUALITY_2:
                    if (m_nOpenBalloonGiftBoxQuality2 >= 1)
                    {
                        UnlockAchievement(achievement);
                    }
                    break;
                case Achievement.ACH_OPEN_BALLOON_GIFT_BOX_QUALITY_3:
                    if (m_nOpenBalloonGiftBoxQuality3 >= 1)
                    {
                        UnlockAchievement(achievement);
                    }
                    break;
                case Achievement.ACH_OPEN_BALLOON_GIFT_BOX_QUALITY_4:
                    if (m_nOpenBalloonGiftBoxQuality4 >= 1)
                    {
                        UnlockAchievement(achievement);
                    }
                    break;
                //将气球固定到物品上
                case Achievement.ACH_ATTACH_BALLOON_TO_ITEM_FIRST:
                    if (m_nAttachBalloonToItemNum >= 1)
                    {
                        UnlockAchievement(achievement);
                    }
                    break;
                //戳破物品
                case Achievement.ACH_POP_ITEM_FIRST:
                    if (m_nPopItemNum >= 1)
                    {
                        UnlockAchievement(achievement);
                    }
                    break;
                //物品礼包成就
                case Achievement.ACH_COLLECT_ONE_ITEM_SET:
                    if (m_nCollectedItemSets >= 1)
                    {
                        UnlockAchievement(achievement);
                    }
                    break;
                case Achievement.ACH_COLLECT_THREE_ITEM_SETS:
                    if (m_nCollectedItemSets >= 3)
                    {
                        UnlockAchievement(achievement);
                    }
                    break;
                case Achievement.ACH_COLLECT_ALL_ITEM_SETS:
                    int totalSets = GetTotalItemSetsCount();
                    if (totalSets > 0 && m_nCollectedItemSets >= totalSets)
                    {
                        UnlockAchievement(achievement);
                    }
                    break;
                case Achievement.ACH_UNLOCK_FIRST_BALLOON_PEDIA://解锁首个气球图鉴
                    if (m_nUnlockBalloons >= 1)
                    {
                        UnlockAchievement(achievement);
                    }
                    break;
                case Achievement.ACH_UNLOCK_FIRST_LEVEL2_BALLOON_PEDIA:
                    if (m_nLevel2Balloons >= 1)
                    {
                        //检查是否解锁了2阶气球
                        UnlockAchievement(achievement);
                    }
                    break;
                case Achievement.ACH_UNLOCK_FIRST_LEVEL3_BALLOON_PEDIA:
                    if (m_nLevel3Balloons >= 1)
                    {
                        UnlockAchievement(achievement);
                    }
                    break;
                case Achievement.ACH_UNLOCK_10_BASE_BALLOON_PEDIA:
                    if (m_nUnlockBalloons >= 10)
                    {
                        UnlockAchievement(achievement);
                    }
                    break;
                case Achievement.ACH_UNLOCK_30_BASE_BALLOON_PEDIA:
                    if (m_nUnlockBalloons >= 30)
                    {
                        UnlockAchievement(achievement);
                    }
                    break;
                case Achievement.ACH_UNLOCK_ALL_BASE_BALLOON_PEDIA:
                    //检查是否解锁了所有基础气球
                    int total = GetBalloonMaxCount();
                    if (total > 0 && m_nUnlockBalloons >= total)
                    {
                        UnlockAchievement(achievement);
                    }
                    break;
                case Achievement.ACH_UNLOCK_ALL_BALLOON_PEDIA:
                    //检查是否解锁了所有气球
                    int totalAll = GameManager.Instance.gameDatabase.GetAllBalloonGitfPack().Count;
                    if (totalAll > 0 && m_nUnlockGiftPackBalloons >= totalAll && m_nUnlockMaxLevelGiftPackBalloons >= totalAll)
                    {
                        UnlockAchievement(achievement);
                    }
                    break;
                case Achievement.ACH_COLLECT_ONE_BALLOON_SET_BASE:
                    //集齐任意一套气球礼包里的全部基础款式
                    if (m_nUnlockGiftPackBalloons > 0)
                    {
                        UnlockAchievement(achievement);
                    }
                    break;
                case Achievement.ACH_COLLECT_ONE_BALLOON_SET_ALL:
                    //集齐任意一套气球礼包里的全部款式
                    if (m_nUnlockMaxLevelGiftPackBalloons > 0)
                    {
                        UnlockAchievement(achievement);
                    }
                    break;

            }
        }

        //Store stats in the Steam database if necessary
        if (m_bStoreStats)
        {
            // already set any achievements in UnlockAchievement

            // set stats
            SteamUserStats.SetStat("NumGames", m_nTotalGamesPlayed);
            SteamUserStats.SetStat("NumWins", m_nTotalNumWins);
            SteamUserStats.SetStat("NumLosses", m_nTotalNumLosses);
            SteamUserStats.SetStat("FeetTraveled", m_flTotalFeetTraveled);
            SteamUserStats.SetStat("MaxFeetTraveled", m_flMaxFeetTraveled);

            SteamUserStats.SetStat("OpenBalloonGiftBoxQuality2", m_nOpenBalloonGiftBoxQuality2);
            SteamUserStats.SetStat("OpenBalloonGiftBoxQuality3", m_nOpenBalloonGiftBoxQuality3);
            SteamUserStats.SetStat("OpenBalloonGiftBoxQuality4", m_nOpenBalloonGiftBoxQuality4);

            SteamUserStats.SetStat("AttachBalloonToItemNum", m_nAttachBalloonToItemNum);
            SteamUserStats.SetStat("PopItemNum", m_nPopItemNum);

            SteamUserStats.SetStat("CollectedItemSets", m_nCollectedItemSets);
            SteamUserStats.SetStat("UnlockBalloons", m_nUnlockBalloons);
            SteamUserStats.SetStat("UnlockGiftPackBalloons", m_nUnlockGiftPackBalloons);
            SteamUserStats.SetStat("UnlockMaxLevelGiftPackBalloons", m_nUnlockMaxLevelGiftPackBalloons);

            SteamUserStats.SetStat("Level2Balloons", m_nLevel2Balloons);
            SteamUserStats.SetStat("Level3Balloons", m_nLevel3Balloons);

            // Update average feet / second stat
            SteamUserStats.UpdateAvgRateStat("AverageSpeed", m_flGameFeetTraveled, m_flGameDurationSeconds);
            // The averaged result is calculated for us
            SteamUserStats.GetStat("AverageSpeed", out m_flAverageSpeed);

            bool bSuccess = SteamUserStats.StoreStats();
            // If this failed, we never sent anything to the server, try
            // again later.
            m_bStoreStats = !bSuccess;
        }
    }

    public void OnBalloonUpgrade(int level)
    {
        if (!m_bStatsValid) return;
        if (level == 2)
        {
            m_nLevel2Balloons++;
        }
        else if (level == 3)
        {
            m_nLevel3Balloons++;
        }

        m_bStoreStats = true;
    }

    /// <summary>
    /// 获取气球总数
    /// </summary>
    /// <returns></returns>
    int GetBalloonMaxCount()
    {
        return GameManager.Instance.gameDatabase.GetBalloonMaxCount();
    }

    public void OnBalloonUnlock()
    {
        if (!m_bStatsValid) return;
        //解锁气球数
        var count = GameManager.Instance.GetUnlockedBalloonCollectionCount();
        if (m_nUnlockBalloons != count && count > 0)
        {
            m_nUnlockBalloons = count;
            m_bStoreStats = true;
        }
        //气球礼包解锁套数
        count = GameManager.Instance.gameDatabase.GetBalloonGiftPackUnlockCount();
        if (m_nUnlockGiftPackBalloons != count && count > 0)
        {
            m_nUnlockGiftPackBalloons = count;
            m_bStoreStats = true;
        }

        //最大等级气球礼包解锁数
        count = GameManager.Instance.gameDatabase.GetMaxLevelBalloonGiftPackUnlockCount();
        if (m_nUnlockMaxLevelGiftPackBalloons != count && count > 0)
        {
            m_nUnlockMaxLevelGiftPackBalloons = count;
            m_bStoreStats = true;
        }
    }

    /// <summary>
    /// 当物品集解锁事件
    /// </summary>
    public void OnItemUnlock()
    {
        if (!m_bStatsValid) return;
        var count = GetItemGiftPackUnlockCount();
        if (count > m_nCollectedItemSets)
        {
            m_nCollectedItemSets = count;
            m_bStoreStats = true;
        }
    }

    /// <summary>
    /// 获取道具礼包总数
    /// </summary>
    /// <returns></returns>
    private int GetTotalItemSetsCount()
    {
        return GameManager.Instance.gameDatabase.GetAllItemGitfPack().Count;
    }
    /// <summary>
    /// 获得解锁道具礼包的数量
    /// </summary>
    /// <returns></returns>
    private int GetItemGiftPackUnlockCount()
    {
        return GameManager.Instance.gameDatabase.GetItemGiftPackUnlockCount();
    }

    /// <summary>
    /// 气球拖拽到物体上次数
    /// </summary>
    public void OnAttachBalloonToItem()
    {
        if (!m_bStatsValid)
            return;
        m_nAttachBalloonToItemNum++;


        m_bStoreStats = true;
    }
    /// <summary>
    /// 绽放气球次数
    /// </summary>
    public void OnPopItem()
    {

        if (!m_bStatsValid)
            return;

        m_nPopItemNum++;
        m_bStoreStats = true;
    }

    /// <summary>
    /// 打开道具礼盒事件
    /// </summary>
    public void OnOpenItemGiftBox()
    {
        if (!m_bStatsValid)
            return;

        m_nTotalGamesPlayed++;

        m_bStoreStats = true;
    }

    /// <summary>
    /// 打开气球礼盒事件
    /// </summary>
    public void OnOpenBalloonGiftBox(BalloonGiftBox balloonGiftBox)
    {
        if (!m_bStatsValid)
            return;

        m_nTotalNumLosses++;


        //获取礼盒品质
        switch (balloonGiftBox.GetBalloonData().quality)
        {
            case EQuality.Common:
                break;
            case EQuality.Uncommon:
                m_nOpenBalloonGiftBoxQuality2++;
                break;
            case EQuality.Rare:
                m_nOpenBalloonGiftBoxQuality3++;
                break;
            case EQuality.Epic:
                m_nOpenBalloonGiftBoxQuality4++;
                break;
            case EQuality.Legendary:
                break;
            default:
                break;
        }


        m_bStoreStats = true;
    }

    public void OnBalloonPopEvent()
    {
        if (!m_bStatsValid)
            return;

        m_nTotalNumWins++;

        // We want to update stats the next frame.
        m_bStoreStats = true;

    }

    public void OnGetCoin()
    {
        m_flTotalFeetTraveled++;
        if (m_flTotalFeetTraveled % 10 == 0)//拾取10个记录一次
        {
            // 再下一帧记录
            m_bStoreStats = true;
        }
    }

    private void OnApplicationQuit()
    {
        // 关闭Steam
        if (SteamManager.Initialized)
        {
            SteamAPI.Shutdown();
        }
    }

    #region Callback

    /// <summary>
    /// Steam用户状态接收回调函数
    /// </summary>
    /// <param name="param"></param>
    private void OnUserStatsReceived(UserStatsReceived_t pCallback)
    {
        if (!SteamManager.Initialized)
            return;
        //过滤掉不是当前游戏的回调
        if ((ulong)m_GameID != pCallback.m_nGameID)
        {
            return;
        }


        if (pCallback.m_eResult == EResult.k_EResultOK)
        {
            LogManager.Log("接收 用户状态和成就 从 Steam");

            m_bStatsValid = true;

            // 加载成就,
            foreach (Achievement_t ach in m_Achievements)
            {
                bool ret = SteamUserStats.GetAchievement(ach.m_eAchievementID.ToString(), out ach.m_bAchieved);
                if (ret)
                {
                    ach.m_strName = SteamUserStats.GetAchievementDisplayAttribute(ach.m_eAchievementID.ToString(), "name");
                    ach.m_strDescription = SteamUserStats.GetAchievementDisplayAttribute(ach.m_eAchievementID.ToString(), "desc");
                }
                else
                {
                    LogManager.LogWarning("SteamUserStats.GetAchievement failed for Achievement " + ach.m_eAchievementID + "\nIs it registered in the Steam Partner site?");
                }
            }

            // load stats todo:后面替换自己的成就数据
            SteamUserStats.GetStat("NumGames", out m_nTotalGamesPlayed);
            SteamUserStats.GetStat("NumWins", out m_nTotalNumWins);
            SteamUserStats.GetStat("NumLosses", out m_nTotalNumLosses);
            SteamUserStats.GetStat("FeetTraveled", out m_flTotalFeetTraveled);
            SteamUserStats.GetStat("MaxFeetTraveled", out m_flMaxFeetTraveled);
            SteamUserStats.GetStat("AverageSpeed", out m_flAverageSpeed);

            SteamUserStats.GetStat("OpenBalloonGiftBoxQuality2", out m_nOpenBalloonGiftBoxQuality2);
            SteamUserStats.GetStat("OpenBalloonGiftBoxQuality3", out m_nOpenBalloonGiftBoxQuality3);
            SteamUserStats.GetStat("OpenBalloonGiftBoxQuality4", out m_nOpenBalloonGiftBoxQuality4);

            SteamUserStats.GetStat("AttachBalloonToItemNum", out m_nAttachBalloonToItemNum);
            SteamUserStats.GetStat("PopItemNum", out m_nPopItemNum);

            SteamUserStats.GetStat("CollectedItemSets", out m_nCollectedItemSets);
            SteamUserStats.GetStat("UnlockBalloons", out m_nUnlockBalloons);
            SteamUserStats.GetStat("UnlockGiftPackBalloons", out m_nUnlockGiftPackBalloons);
            SteamUserStats.GetStat("UnlockMaxLevelGiftPackBalloons", out m_nUnlockMaxLevelGiftPackBalloons);

            SteamUserStats.GetStat("Level2Balloons", out m_nLevel2Balloons);
            SteamUserStats.GetStat("Level3Balloons", out m_nLevel3Balloons);
        }
        else
        {
            LogManager.Log("RequestStats - failed, " + pCallback.m_eResult);
        }
    }

    //-----------------------------------------------------------------------------
    // 储存用户状态成就回调函数
    //-----------------------------------------------------------------------------
    private void OnUserStatsStored(UserStatsStored_t pCallback)
    {
        // we may get callbacks for other games' stats arriving, ignore them
        if ((ulong)m_GameID == pCallback.m_nGameID)
        {
            if (EResult.k_EResultOK == pCallback.m_eResult)
            {
                LogManager.Log("StoreStats - success");
            }
            else if (EResult.k_EResultInvalidParam == pCallback.m_eResult)
            {
                // One or more stats we set broke a constraint. They've been reverted,
                // and we should re-iterate the values now to keep in sync.
                LogManager.Log("StoreStats - some failed to validate");
                // Fake up a callback here so that we re-load the values.
                UserStatsReceived_t callback = new UserStatsReceived_t();
                callback.m_eResult = EResult.k_EResultOK;
                callback.m_nGameID = (ulong)m_GameID;
                OnUserStatsReceived(callback);
            }
            else
            {
                LogManager.Log("StoreStats - failed, " + pCallback.m_eResult);
            }
        }
    }

    //-----------------------------------------------------------------------------
    // Purpose: An achievement was stored
    //-----------------------------------------------------------------------------
    private void OnAchievementStored(UserAchievementStored_t pCallback)
    {
        // We may get callbacks for other games' stats arriving, ignore them
        if ((ulong)m_GameID == pCallback.m_nGameID)
        {
            if (0 == pCallback.m_nMaxProgress)
            {
                LogManager.Log("Achievement '" + pCallback.m_rgchAchievementName + "' unlocked!");
            }
            else
            {
                LogManager.Log("Achievement '" + pCallback.m_rgchAchievementName + "' progress callback, (" + pCallback.m_nCurProgress + "," + pCallback.m_nMaxProgress + ")");
            }
        }
    }

    public int GetStat(string name)
    {
        int value;
        if (SteamUserStats.GetStat(name, out value))
        {
            LogManager.Log($"Achievement {name} is achieved: {value}");
        }
        else
        {
            LogManager.LogError("Failed to get achievement state");
        }
        return value;
    }

    public bool GetAchievement(string name = "ACH_WIN_100_GAMES")
    {
        bool isAchieved;
        if (SteamUserStats.GetAchievement(name, out isAchieved))
        {
            LogManager.Log($"Achievement {name} is achieved: {isAchieved}");
        }
        else
        {
            LogManager.LogError("Failed to get achievement state");
        }
        return isAchieved;
    }

    private void OnGameOverlayActivated(GameOverlayActivated_t pCallback)
    {
        if (pCallback.m_bActive != 0)
        {
            LogManager.Log("Steam Overlay 被激活");
        }
        else
        {
            LogManager.Log("Steam Overlay 已关闭");
        }
    }

    private void OnNumberOfCurrentPlayers(NumberOfCurrentPlayers_t pCallback, bool bIOFailure)
    {
        if (pCallback.m_bSuccess != 1 || bIOFailure)
        {
            LogManager.Log(" 获取不到玩家数量!");
        }
        else
        {
            LogManager.Log("当前游戏玩家数量: " + pCallback.m_cPlayers);
        }
    }

    #endregion

    #region 成就


    //-----------------------------------------------------------------------------
    // Purpose: Unlock this achievement
    //-----------------------------------------------------------------------------
    private void UnlockAchievement(Achievement_t achievement)
    {
        achievement.m_bAchieved = true;

        // the icon may change once it's unlocked
        //achievement.m_iIconImage = 0;

        // mark it down
        SteamUserStats.SetAchievement(achievement.m_eAchievementID.ToString());

        // Store stats end of frame
        m_bStoreStats = true;

        LogManager.Log($"解锁成就: {achievement.m_strName} ({achievement.m_eAchievementID}) - {achievement.m_strDescription}");
    }

    /// <summary>
    /// 解锁成就（参数为成就API名称，在Steamworks后台配置）
    /// </summary>
    /// <param name="achievementApiName"></param>
    //public void UnlockAchievement(string achievementApiName)
    //{
    //    if (!SteamManager.Initialized) return;

    //    SteamUserStats.SetAchievement(achievementApiName);//本地完成成就
    //    SteamUserStats.StoreStats(); // 保存统计数据发送到steam服务器
    //}

    //public void SetStateAchievement(string achievementApiName, int value, int max)
    //{
    //    if (!SteamManager.Initialized) return;

    //    if (SteamUserStats.SetStat(achievementApiName, value))//本地完成成就)
    //    {
    //        if (value >= max)//这边设置数量后,需要检测是否完成成就,如果完成了就解锁成就
    //        {
    //            UnlockAchievement(name);
    //        }
    //        SteamUserStats.StoreStats(); // 保存统计数据发送到steam服务器
    //    }

    //}

    //public void SetStateAchievement(string achievementApiName, float value, float max)
    //{
    //    if (!SteamManager.Initialized) return;

    //    if (SteamUserStats.SetStat(achievementApiName, value))//本地完成成就)
    //    {
    //        if (value >= max)//这边设置数量后,需要检测是否完成成就,如果完成了就解锁成就
    //        {
    //            UnlockAchievement(name);
    //        }
    //        SteamUserStats.StoreStats(); // 保存统计数据发送到steam服务器
    //    }
    //}

    // 重置成就
    public void ResetAchievement(string achievementApiName)
    {
        if (!SteamManager.Initialized) return;

        SteamUserStats.ClearAchievement(achievementApiName);
        SteamUserStats.StoreStats();
    }

    public void ResetAllAchievement()
    {
        if (!SteamManager.Initialized) return;

        SteamUserStats.ResetAllStats(true);//true表示同时重置统计数据和成就,false表示只重置统计数据
        LogManager.Log("重置所有统计数据和成就");
    }


    #endregion

#region 添加到愿望单

/// <summary>
/// 添加到愿望单按钮点击事件
/// </summary>
public void AddGameToWishlist()
{
    if (!SteamManager.Initialized)
    {
        LogManager.LogError("Steam未初始化，请确保Steam客户端正在运行");
        return;
    }

    if (!SteamUser.BLoggedOn())
    {
        LogManager.LogError("请先登录Steam账户");
        return;
    }

    // 调用Steamworks API添加到愿望单
    Application.OpenURL($"https://store.steampowered.com/app/{SteamUtils.GetAppID()}/");//https://store.steampowered.com/app/4211860/
}


/// <summary>
/// 打开商店页面
/// </summary>
public void OnOpenStorePageClicked()
{
    if (!SteamManager.Initialized)
    {
        return;
    }

    SteamFriends.ActivateGameOverlayToStore(SteamUtils.GetAppID(), EOverlayToStoreFlag.k_EOverlayToStoreFlag_None);

}

/// <summary>
/// 检查游戏是否已在愿望单中
/// </summary>
public bool CheckWishlistStatus()
{
    if (!SteamManager.Initialized) return false;

    bool isFreeWeekend = SteamApps.BIsSubscribedFromFreeWeekend();
    bool isFamilySharing = SteamApps.BIsSubscribedFromFamilySharing();

    // 检查是否已拥有游戏
    bool isOwned = SteamApps.BIsSubscribedApp(SteamUtils.GetAppID());

    // 检查是否已在愿望单中
    // 注意：Steam API没有直接检查愿望单状态的函数
    // 通常需要通过回调结果来判断
    return isOwned;
}

#endregion


    //-----------------------------------------------------------------------------
    // Purpose: Display the user's stats and achievements
    //-----------------------------------------------------------------------------
    //public void Render()
    //{
    //    if (!SteamManager.Initialized)
    //    {
    //        GUILayout.Label("Steamworks not Initialized");
    //        return;
    //    }

    //    GUILayout.Label("m_ulTickCountGameStart: " + m_ulTickCountGameStart);
    //    GUILayout.Label("m_flGameDurationSeconds: " + m_flGameDurationSeconds);
    //    GUILayout.Label("m_flGameFeetTraveled: " + m_flGameFeetTraveled);
    //    GUILayout.Space(10);
    //    GUILayout.Label("NumGames: " + m_nTotalGamesPlayed);
    //    GUILayout.Label("NumWins: " + m_nTotalNumWins);
    //    GUILayout.Label("NumLosses: " + m_nTotalNumLosses);
    //    GUILayout.Label("FeetTraveled: " + m_flTotalFeetTraveled);
    //    GUILayout.Label("MaxFeetTraveled: " + m_flMaxFeetTraveled);
    //    GUILayout.Label("AverageSpeed: " + m_flAverageSpeed);

    //    GUILayout.BeginArea(new Rect(Screen.width - 300, 0, 300, 800));
    //    foreach (Achievement_t ach in m_Achievements)
    //    {
    //        GUILayout.Label(ach.m_eAchievementID.ToString());
    //        GUILayout.Label(ach.m_strName + " - " + ach.m_strDescription);
    //        GUILayout.Label("Achieved: " + ach.m_bAchieved);
    //        GUILayout.Space(20);
    //    }

    //    // FOR TESTING PURPOSES ONLY!
    //    if (GUILayout.Button("RESET STATS AND ACHIEVEMENTS"))
    //    {
    //        SteamUserStats.ResetAllStats(true);
    //        SteamUserStats.RequestCurrentStats();
    //        //OnGameStateChange(EClientGameState.k_EClientGameActive);
    //    }
    //    GUILayout.EndArea();
    //}

}