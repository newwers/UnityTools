=========================================
游戏难度系统使用说明
=========================================

本游戏难度系统允许设置三种难度级别: 普通、困难、地狱
不同难度下怪物的属性和AI行为会有所不同。

-----------------------------------------
系统特点
-----------------------------------------

1. 普通难度: 标准的游戏体验,敌人使用基础属性和普通AI
2. 困难难度: 敌人属性全面提升(生命、攻击、速度等),但AI策略不变  
3. 地狱难度: 敌人属性大幅提升,并且会使用增强的AI策略

-----------------------------------------
快速开始
-----------------------------------------

步骤1: 创建难度设置资产
- Project窗口 → 右键 → Create → Game → Difficulty Settings
- 命名为 Game Difficulty Settings
- 建议放置在 /Assets/Scripts/Core/ 目录

步骤2: 配置难度倍率
选中创建的资产,在Inspector中配置三个难度的倍率值

步骤3: 创建难度管理器
- 场景中创建空GameObject,命名为 GameDifficultyManager
- 添加 GameDifficultyManager 组件
- 将 Game Difficulty Settings 资产拖拽到对应字段
- 选择初始难度

步骤4: 使用代码切换难度
GameDifficultyManager.Instance.SetDifficulty(GameDifficulty.Normal);
GameDifficultyManager.Instance.SetDifficulty(GameDifficulty.Hard);
GameDifficultyManager.Instance.SetDifficulty(GameDifficulty.Hell);

-----------------------------------------
难度配置参数说明
-----------------------------------------

普通难度:
- 所有倍率为 1.0 (标准值)
- 不使用增强AI

困难难度:
- 生命值倍率: 1.5
- 攻击力倍率: 1.3
- 移动速度倍率: 1.2
- 攻击速度倍率: 0.85 (值越小攻击越快)
- 检测范围倍率: 1.3
- AI决策速度倍率: 1.2
- 不使用增强AI

地狱难度:
- 生命值倍率: 2.0
- 攻击力倍率: 1.8
- 移动速度倍率: 1.5
- 攻击速度倍率: 0.7
- 检测范围倍率: 1.5
- AI决策速度倍率: 1.5
- 使用增强AI: true

-----------------------------------------
地狱AI增强特性
-----------------------------------------

地狱难度下,敌人使用 HellAIStrategy,拥有以下能力:

1. 位置预判 - 根据玩家速度预判移动位置
2. 组合攻击 - 连续使用多次攻击
3. 智能闪避 - 主动闪避玩家攻击
4. 反击判定 - 在合适时机反击
5. 侧翼识别 - 检测并应对玩家侧面进攻
6. 优化攻击选择 - 根据距离选择最佳攻击

-----------------------------------------
注意事项
-----------------------------------------

1. GameDifficultyManager 使用单例模式,场景切换时保持存在
2. 难度修改只对新生成的敌人有效
3. 已存在的敌人属性不会因难度修改而改变
4. 必须在场景中配置 GameDifficultyManager
5. 同一场景中只能有一个 GameDifficultyManager

-----------------------------------------
文件清单
-----------------------------------------

新创建的文件:
- /Assets/Scripts/Core/GameDifficulty.cs
- /Assets/Scripts/Core/GameDifficultySettings.cs
- /Assets/Scripts/Core/GameDifficultyManager.cs
- /Assets/Scripts/Character/Enemy/AIStrategy/HellAIStrategy.cs
- /Assets/Scripts/Battle/UI/GameDifficultyUI.cs

修改的文件:
- /Assets/Scripts/Character/Enemy/EnemyAIController.cs
- /Assets/Scripts/Character/Enemy/AIStrategy/BaseAIStrategy.cs

-----------------------------------------
扩展示例
-----------------------------------------

监听难度变化事件:

private void OnEnable()
{
    if (GameDifficultyManager.Instance != null)
    {
        GameDifficultyManager.Instance.OnDifficultyChanged += OnDifficultyChanged;
    }
}

private void OnDifficultyChanged(GameDifficulty newDifficulty)
{
    Debug.Log("难度已改变为: " + newDifficulty);
}

获取经验和掉落倍率:

float exp = baseExp * GameDifficultyManager.Instance.GetExpMultiplier();
float loot = baseLoot * GameDifficultyManager.Instance.GetLootMultiplier();

=========================================
