using UnityEngine;


/// <summary>
/// Layer管理器，用于统一管理项目中所有脚本对Layer的获取和使用
/// 单例模式，全局只有一个实例
/// </summary>
public class LayerManager : BaseSingleClass<LayerManager>
{
    public int defaultLayer;
    public int uILayer;
    public int groundLayer;
    public int playerLayer;
    public int enemyLayer;
    public int characterLayer;
    public int wallLayer;
    public int projectileLayer;

    public static int DefaultLayer
    {
        get => Instance.defaultLayer;
    }
    public static int UILayer
    {
        get => Instance.uILayer;
    }
    public static int GroundLayer
    {
        get => Instance.groundLayer;
    }
    public static int PlayerLayer
    {
        get => Instance.playerLayer;
    }
    public static int EnemyLayer
    {
        get => Instance.enemyLayer;
    }
    public static int CharacterLayer
    {
        get => Instance.characterLayer;
    }
    public static int WallLayer
    {
        get => Instance.wallLayer;
    }
    public static int ProjectileLayer
    {
        get => Instance.projectileLayer;
    }

    #region 成员变量

    /// <summary>
    /// 是否已初始化
    /// </summary>
    private bool isInitialized = false;

    #endregion

    #region 构造函数

    /// <summary>
    /// 构造函数，初始化权限表
    /// </summary>
    public LayerManager()
    {
        Initialize();
    }

    #endregion

    #region 初始化方法

    /// <summary>
    /// 初始化Layer管理器
    /// </summary>
    private void Initialize()
    {
        if (isInitialized)
            return;

        defaultLayer = LayerMask.NameToLayer("Default");
        enemyLayer = LayerMask.NameToLayer("Enemy");
        playerLayer = LayerMask.NameToLayer("Player");
        characterLayer = LayerMask.NameToLayer("Character");
        groundLayer = LayerMask.NameToLayer("Ground");
        wallLayer = LayerMask.NameToLayer("Wall");
        uILayer = LayerMask.NameToLayer("UI");
        projectileLayer = LayerMask.NameToLayer("Projectile");



        isInitialized = true;
        LogManager.Log("[LayerManager] Layer管理器初始化完成");
    }

    #endregion

}

