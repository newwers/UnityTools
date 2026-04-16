/*
独立的Steam成就管理器
不依赖于SteamSDKManager
使用SteamAchievementDataSO管理成就
*/
/*
 必须启动 Steam 客户端并登录账号
开发测试时，确保 Steamworks 后台的 "可见性" 设置为 "公开" 或 "好友可见"
运行 Unity 项目前，先通过 Steam 启动一次游戏（确保 App ID 关联正确）

开发时：保留 steam_appid.txt 文件
发布时：移除 steam_appid.txt 文件（Steam客户端会自动提供App ID）

成就在检测的时候有两种方法,一种是用一个变量缓存数据,然后云同步数据,根据数据来判定(这种方式如果成就重置后,有些无法触发检测则完成不了成就,需要重新清空存档重新玩才能触发)
另一个是在每次检测成就的时候,都重新计算,满足就直接解锁(这种即使重置成就,进入旧存档游戏也可以触发,)

 */
using Steamworks;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SteamSDK
{
    public class SteamAchievementManager : MonoSingleton<SteamAchievementManager>
    {
        [Tooltip("统计数据存储的时间间隔（秒）")]
        public float m_statsStoreInterval = 30.0f;

        [Tooltip("成就检查的时间间隔（秒）")]
        public float m_achievementCheckInterval = 1.0f;

        public string PlayerName;
        public string Language;

        [Header("Steam Achievement SOs")]
        public List<SteamAchievementDataSO> achievementSOs = new List<SteamAchievementDataSO>();

        public static Action OnSteamSDKInitAction;


        // 游戏id
        private CGameID m_GameID;

        /// <summary>
        /// 是否请求了Steam统计数据和成就状态
        /// </summary>
        private bool m_bRequestedStats;
        /// <summary>
        /// 是否已经从Steam接收了统计数据和成就状态的回调
        /// </summary>
        private bool m_bStatsValid;

        /// <summary>
        /// 是否在当前帧储存统计数据和成就状态
        /// </summary>
        private bool m_bStoreStats;


        /// <summary>
        /// 自上次存储统计数据以来的时间
        /// </summary>
        private float m_timeSinceLastStoreStats = 0.0f;

        /// <summary>
        /// 自上次检查成就以来的时间
        /// </summary>
        private float m_timeSinceLastAchievementCheck = 0.0f;

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

            // 获取当前玩家昵称
            PlayerName = SteamFriends.GetPersonaName();
            Debug.Log($"Steam SDK 初始化成功 (AppID: {SteamUtils.GetAppID()}), 当前用户: {PlayerName}");

            // 在编辑器中注册播放模式状态变化事件
#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#endif

            // 获取当前玩家Steam ID
            CSteamID steamId = SteamUser.GetSteamID();
            ulong steamId64 = steamId.m_SteamID;
            Debug.Log($"当前玩家Steam ID: {steamId64}");

            //获取玩家语言
            Language = SteamApps.GetCurrentGameLanguage();
            Debug.Log($"当前游戏语言: {Language}");
            //获取玩家steam客户端语言
            Language = SteamUtils.GetSteamUILanguage();
            Debug.Log($"当前steam客户端语言: {Language}");
        }

        private void OnApplicationQuit()
        {
            // 关闭Steam
            if (SteamManager.Initialized)
            {
                SteamAPI.Shutdown();
            }
        }

        // 在编辑器中处理播放模式状态变化的事件
#if UNITY_EDITOR
        private void OnPlayModeStateChanged(UnityEditor.PlayModeStateChange state)
        {
            // 当编辑器从播放模式切换到编辑模式时
            if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode)
            {
                // 重置所有成就SO的intValue和floatValue
                foreach (var achievementSO in achievementSOs)
                {
                    if (achievementSO != null)
                    {
                        achievementSO.AchievementData.IntValue = 0;
                        achievementSO.AchievementData.FloatValue = 0.0f;
                        achievementSO.AchievementData.IsAchieved = false;
                    }
                }
            }
        }
#endif

        private void OnDisable()
        {
            // 在编辑器中取消注册播放模式状态变化事件
#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
#endif
        }


        private void Update()
        {
            if (!SteamManager.Initialized)
                return;

            // if (!m_bRequestedStats)//请求当前玩家的统计数据
            // {
            //     if (!SteamManager.Initialized)
            //     {
            //         m_bRequestedStats = true;
            //         return;
            //     }
            //
            //     // If yes, request our stats
            //     bool bSuccess = SteamUserStats.RequestCurrentStats();
            //
            //     // This function should only return false if we weren't logged in, and we already checked that.
            //     // But handle it being false again anyway, just ask again later.
            //     m_bRequestedStats = bSuccess;
            // }

            // if (!m_bStatsValid)//如果还没有接收到统计数据和成就状态的回调,则不进行成就检查
            //     return;

            // 定期检查所有成就SO的条件
            m_timeSinceLastAchievementCheck += Time.deltaTime;
            if (m_timeSinceLastAchievementCheck >= m_achievementCheckInterval)
            {
                foreach (var achievementSO in achievementSOs)
                {
                    m_bStoreStats |= achievementSO.OnCheck();//如果有任何一个成就SO的条件满足了,则需要在当前帧存储统计数据和成就状态
                }
                m_timeSinceLastAchievementCheck = 0.0f; // 重置计时器
            }

            if (m_bStoreStats)
            {
                // 存储统计数据
                StoreStats();
                m_bStoreStats = false;
                m_timeSinceLastStoreStats = 0.0f; // 重置计时器
            }

            // 定期存储统计数据，即使没有成就达成
            m_timeSinceLastStoreStats += Time.deltaTime;
            if (m_timeSinceLastStoreStats >= m_statsStoreInterval)
            {
                foreach (var achievementSO in achievementSOs)
                {
                    achievementSO.OnStoreStats();
                }

                StoreStats();
                m_timeSinceLastStoreStats = 0.0f;
            }

        }
        #region 回调

        private void OnGameOverlayActivated(GameOverlayActivated_t pCallback)
        {
            if (pCallback.m_bActive != 0)
            {
                Debug.Log("Steam Overlay 被激活");
            }
            else
            {
                Debug.Log("Steam Overlay 已关闭");
            }
        }

        private void OnNumberOfCurrentPlayers(NumberOfCurrentPlayers_t pCallback, bool bIOFailure)
        {
            if (pCallback.m_bSuccess != 1 || bIOFailure)
            {
                Debug.Log(" 获取不到玩家数量!");
            }
            else
            {
                Debug.Log("当前游戏玩家数量: " + pCallback.m_cPlayers);
            }
        }
        /// <summary>
        /// 当用户统计数据接收时调用
        /// </summary>
        private void OnUserStatsReceived(UserStatsReceived_t pCallback)
        {
            if (pCallback.m_nGameID != (ulong)m_GameID)
                return;

            if (pCallback.m_eResult != EResult.k_EResultOK)
                return;

            Debug.Log("Received stats and achievements from Steam");

            m_bStatsValid = true;


            // 通知所有成就SO,刷新成就状态
            foreach (var achievementSO in achievementSOs)
            {
                bool isAchieved;
                if (SteamUserStats.GetAchievement(achievementSO.AchievementData.AchievementId, out isAchieved))
                {
                    achievementSO.OnUserStatsReceived(isAchieved);
                }
            }
        }

        /// <summary>
        /// 当用户统计数据存储时调用
        /// </summary>
        private void OnUserStatsStored(UserStatsStored_t pCallback)
        {
            if (pCallback.m_nGameID != (ulong)m_GameID)
                return;

            if (pCallback.m_eResult == EResult.k_EResultOK)
            {
                Debug.Log("Successfully stored stats and achievements");
            }
            else
            {
                Debug.LogError($"Failed to store stats and achievements: {pCallback.m_eResult}");
            }
        }

        /// <summary>
        /// 当成就存储时调用
        /// </summary>
        private void OnAchievementStored(UserAchievementStored_t pCallback)
        {
            // 这可能会收到其他游戏统计数据的回调，忽略它们
            if ((ulong)m_GameID == pCallback.m_nGameID)
            {
                if (0 == pCallback.m_nMaxProgress)
                {
                    Debug.Log("Achievement '" + pCallback.m_rgchAchievementName + "' unlocked!");
                }
                else
                {
                    Debug.Log("Achievement '" + pCallback.m_rgchAchievementName + "' progress callback, (" + pCallback.m_nCurProgress + "," + pCallback.m_nMaxProgress + ")");
                }
            }
        }


        #endregion

        public void GetNumberOfCurrentPlayers()
        {
            SteamAPICall_t handle = SteamUserStats.GetNumberOfCurrentPlayers();
            m_NumberOfCurrentPlayers.Set(handle);
            Debug.Log("调用获取在线玩家数量函数");
        }

        /// <summary>
        /// 存储统计数据
        /// </summary>
        private void StoreStats()
        {
            if (!SteamManager.Initialized)
                return;

            if (SteamUserStats.StoreStats())
            {
                Debug.Log("Stats stored successfully");
            }
            else
            {
                Debug.LogError("Failed to store stats");
            }
        }

        /// <summary>
        /// 解锁成就
        /// </summary>
        /// <param name="achievementId">成就ID</param>
        public void UnlockAchievement(string achievementId)
        {
            if (!SteamManager.Initialized)
                return;

            // 本地完成成就
            SteamUserStats.SetAchievement(achievementId);

            // 更新配置文件中的成就状态
            GetAchievementById(achievementId)?.OnTriggerAchievement();

            // 存储统计数据
            m_bStoreStats = true;

            Debug.Log($"解锁成就: {achievementId}");
        }

        /// <summary>
        /// 重置成就
        /// </summary>
        /// <param name="achievementId">成就ID</param>
        public void ResetAchievement(string achievementId)
        {
            if (!SteamManager.Initialized)
                return;

            // 清除成就
            SteamUserStats.ClearAchievement(achievementId);

            // 更新配置文件中的成就状态
            GetAchievementById(achievementId)?.OnResetAchievement();

            // 存储统计数据
            m_bStoreStats = true;

            Debug.Log($"重置成就: {achievementId}");
        }

        public void ResetAllAchievement()
        {
            if (!SteamManager.Initialized) return;

            SteamUserStats.ResetAllStats(true);//true表示同时重置统计数据和成就,false表示只重置统计数据
            Debug.Log("重置所有统计数据和成就");

            // 更新配置文件中的所有成就状态
            foreach (var achievementSO in achievementSOs)
            {
                achievementSO.OnResetAchievement();
            }
        }

        /// <summary>
        /// 检查成就是否已解锁
        /// </summary>
        /// <param name="achievementId">成就ID</param>
        /// <returns>是否已解锁</returns>
        public bool IsAchievementUnlocked(string achievementId)
        {
            if (!SteamManager.Initialized)
                return false;

            bool isAchieved;
            if (SteamUserStats.GetAchievement(achievementId, out isAchieved))
            {
                return isAchieved;
            }

            return false;
        }

        /// <summary>
        /// 添加成就SO
        /// </summary>
        /// <param name="achievementSO">成就SO</param>
        public void AddAchievementSO(SteamAchievementDataSO achievementSO)
        {
            if (achievementSO != null && !achievementSOs.Contains(achievementSO))
            {
                achievementSOs.Add(achievementSO);
            }
        }

        /// <summary>
        /// 移除成就SO
        /// </summary>
        /// <param name="achievementSO">成就SO</param>
        public void RemoveAchievementSO(SteamAchievementDataSO achievementSO)
        {
            if (achievementSO != null && achievementSOs.Contains(achievementSO))
            {
                achievementSOs.Remove(achievementSO);
            }
        }

        /// <summary>
        /// 触发成就检查
        /// </summary>
        public void TriggerAchievementCheck(string achievementId)
        {
            foreach (var achievementSO in achievementSOs)
            {
                if (achievementSO.AchievementData.AchievementId.Contains(achievementId))
                {
                    achievementSO.OnCheck();
                }
            }
        }

        public SteamAchievementDataSO GetAchievementById(string id)
        {
            return achievementSOs.Find(achievement => achievement.AchievementData.AchievementId == id);
        }

        #region 添加到愿望单

        /// <summary>
        /// 添加到愿望单按钮点击事件
        /// </summary>
        public void AddGameToWishlist()
        {
            if (!SteamManager.Initialized)
            {
                Debug.LogError("Steam未初始化，请确保Steam客户端正在运行");
                return;
            }

            if (!SteamUser.BLoggedOn())
            {
                Debug.LogError("请先登录Steam账户");
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
    }
}