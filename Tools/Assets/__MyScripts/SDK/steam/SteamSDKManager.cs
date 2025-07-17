/*
 必须启动 Steam 客户端并登录账号
开发测试时，确保 Steamworks 后台的 "可见性" 设置为 "公开" 或 "好友可见"
运行 Unity 项目前，先通过 Steam 启动一次游戏（确保 App ID 关联正确）
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


    private void OnEnable()
    {
        if (SteamManager.Initialized)
        {
            //每次激活或停用 Steam 界面时，它都会向您发送一个回调
            //steam Overlay 默认快捷键：Shift + Tab
            m_GameOverlayActivated = Callback<GameOverlayActivated_t>.Create(OnGameOverlayActivated);

            m_NumberOfCurrentPlayers = CallResult<NumberOfCurrentPlayers_t>.Create(OnNumberOfCurrentPlayers);
        }
    }

    void Start()
    {
        m_IsInitialized = SteamManager.Initialized;
        if (m_IsInitialized == false)
        {
            Debug.LogError("Steam SDK 未初始化.请运行steam!.");
            return;
        }

        // 获取当前玩家昵称
        string playerName = SteamFriends.GetPersonaName();
        Debug.Log($"Steam SDK 初始化成功,当前用户: {playerName}");

        // 获取当前玩家Steam ID
        CSteamID steamId = SteamUser.GetSteamID();
        ulong steamId64 = steamId.m_SteamID;
        Debug.Log($"当前玩家Steam ID: {steamId64}");
    }



    private void Update()
    {
        if (m_IsInitialized == false)
        {
            return;
        }
        // 每帧更新Steam回调
        SteamAPI.RunCallbacks();

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SteamAPICall_t handle = SteamUserStats.GetNumberOfCurrentPlayers();
            m_NumberOfCurrentPlayers.Set(handle);
            Debug.Log("Called GetNumberOfCurrentPlayers()");
        }
#endif
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
