/*
 必须启动 Steam 客户端并登录账号
开发测试时，确保 Steamworks 后台的 "可见性" 设置为 "公开" 或 "好友可见"
运行 Unity 项目前，先通过 Steam 启动一次游戏（确保 App ID 关联正确）

开发时：保留 steam_appid.txt 文件
发布时：移除 steam_appid.txt 文件（Steam客户端会自动提供App ID）

 */
using Steamworks;
using UnityEngine;

public class SteamSDKManager : BaseMonoSingleClass<SteamSDKManager>
{
    //玩家的Steam Overlay激活回调
    protected Callback<GameOverlayActivated_t> m_GameOverlayActivated;
    //获取游戏当前玩家数量的回调
    private CallResult<NumberOfCurrentPlayers_t> m_NumberOfCurrentPlayers;

    private bool m_IsInitialized;

    protected override void Awake()
    {
        base.Awake();
        // 初始化Steam SDK

        m_IsInitialized = SteamAPI.Init();
        if (!m_IsInitialized)
        {
            Debug.LogError($"Steam SDK 初始化失败！请检查Steam客户端是否运行以及AppID {SteamUtils.GetAppID()} 是否正确");
            return;
        }
    }


    private void OnEnable()
    {
        if (m_IsInitialized)
        {
            //每次激活或停用 Steam 界面时，它都会向您发送一个回调
            //steam Overlay 默认快捷键：Shift + Tab
            m_GameOverlayActivated = Callback<GameOverlayActivated_t>.Create(OnGameOverlayActivated);
            m_NumberOfCurrentPlayers = CallResult<NumberOfCurrentPlayers_t>.Create(OnNumberOfCurrentPlayers);
        }
    }

    void Start()
    {
        // 获取当前玩家昵称
        string playerName = SteamFriends.GetPersonaName();
        Debug.Log($"Steam SDK 初始化成功 (AppID: {SteamUtils.GetAppID()}), 当前用户: {playerName}");

        // 获取当前玩家Steam ID
        CSteamID steamId = SteamUser.GetSteamID();
        ulong steamId64 = steamId.m_SteamID;
        Debug.Log($"当前玩家Steam ID: {steamId64}");

        // 初始化回调（确保OnEnable可能未执行时也能初始化回调）
        if (m_GameOverlayActivated == null)
        {
            m_GameOverlayActivated = Callback<GameOverlayActivated_t>.Create(OnGameOverlayActivated);
        }
        if (m_NumberOfCurrentPlayers == null)
        {
            m_NumberOfCurrentPlayers = CallResult<NumberOfCurrentPlayers_t>.Create(OnNumberOfCurrentPlayers);
        }
    }



    private void Update()
    {
        if (!m_IsInitialized)
        {
            return;
        }
        // 每帧更新Steam回调
        SteamAPI.RunCallbacks();


        if (Input.GetKeyDown(KeyCode.Space))
        {
            SteamAPICall_t handle = SteamUserStats.GetNumberOfCurrentPlayers();
            m_NumberOfCurrentPlayers.Set(handle);
            Debug.Log("Called GetNumberOfCurrentPlayers()");
        }
    }

    private void OnApplicationQuit()
    {
        // 关闭Steam
        if (m_IsInitialized)
        {
            SteamAPI.Shutdown();
        }
    }

    #region Callback



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

    #endregion

    #region 成就




    // 解锁成就（参数为成就API名称，在Steamworks后台配置）
    public void UnlockAchievement(string achievementApiName)
    {
        if (!m_IsInitialized) return;

        SteamUserStats.SetAchievement(achievementApiName);
        SteamUserStats.StoreStats(); // 保存统计数据
    }

    // 重置成就
    public void ResetAchievement(string achievementApiName)
    {
        if (!m_IsInitialized) return;

        SteamUserStats.ClearAchievement(achievementApiName);
        SteamUserStats.StoreStats();
    }
    #endregion

    #region 联机


    // 发送P2P消息
    public void SendP2PMessage(CSteamID targetId, byte[] data)
    {
        if (!m_IsInitialized) return;

        SteamNetworking.SendP2PPacket(
            targetId,
            data,
            (uint)data.Length,
            EP2PSend.k_EP2PSendReliable
        );
    }

    // 接收P2P消息
    private void OnUpdate()
    {
        if (!m_IsInitialized) return;

        uint packetSize;
        while (SteamNetworking.IsP2PPacketAvailable(out packetSize))
        {
            byte[] data = new byte[packetSize];
            CSteamID senderId;

            if (SteamNetworking.ReadP2PPacket(data, packetSize, out packetSize, out senderId))
            {
                // 处理接收到的消息
                Debug.Log($"收到来自 {senderId.m_SteamID} 的消息");
            }
        }
    }

    #endregion
}