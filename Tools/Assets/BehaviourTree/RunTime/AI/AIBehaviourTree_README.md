# 行为树实现 UpdateAI 功能说明

## 概述

使用行为树替代原有的 `UpdateAI` 状态机逻辑，通过组合不同的节点来实现敌人AI行为。

## 创建的节点列表

### 条件节点 (Decorator/Composite)
| 节点名称 | 功能说明 | 返回状态 |
|---------|---------|---------|
| CheckDead | 检查敌人是否死亡 | Success=死亡, Failure=存活 |
| CheckStunned | 检查敌人是否眩晕 | Success=眩晕中, Failure=正常 |
| CheckTarget | 检查是否有目标 | Success=有目标, Failure=无目标 |
| CheckInAttackRange | 检查是否在攻击范围内 | Success=在范围内, Failure=不在范围内 |

### 动作节点 (Action)
| 节点名称 | 功能说明 | 返回状态 |
|---------|---------|---------|
| ActionIdle | 执行空闲状态 | Running=进行中, Success=完成 |
| ActionPatrol | 执行巡逻行为 | Running=进行中, Success=到达目标 |
| ActionChase | 执行追击行为 | Running=进行中, Success=进入攻击范围, Failure=丢失目标 |
| ActionAttack | 执行攻击行为 | Running=进行中, Failure=目标超出范围 |
| ActionRetreat | 执行撤退行为 | Running=进行中 |
| ActionDodge | 执行闪避行为 | Running=闪避中, Success=闪避完成 |

## 行为树结构设计

### 主行为树结构

```
Root
└── Selector (优先级选择)
    ├── Sequence (死亡检查)
    │   └── CheckDead
    ├── Sequence (眩晕检查)
    │   └── CheckStunned
    ├── Sequence (攻击分支)
    │   ├── CheckTarget
    │   ├── CheckInAttackRange
    │   └── ActionAttack
    ├── Sequence (追击分支)
    │   ├── CheckTarget
    │   └── ActionChase
    ├── ActionPatrol (巡逻)
    └── ActionIdle (空闲)
```

### 设计说明

1. **优先级顺序**: 死亡检查 > 眩晕检查 > 攻击 > 追击 > 巡逻 > 空闲
2. **Selector 节点**: 按顺序执行子节点，找到第一个返回 Success 的节点执行
3. **Sequence 节点**: 按顺序执行所有子节点，全部成功才返回 Success

## 使用方法

1. 打开行为树编辑器 (`行为树/行为树编辑器`)
2. 创建新的行为树文件
3. 按照上述结构添加节点并连接

### 推荐配置示例

```
Root
└── Selector
    ├── Sequence (死亡)
    │   └── CheckDead
    ├── Sequence (眩晕)
    │   └── CheckStunned
    ├── Sequence (攻击)
    │   ├── CheckTarget
    │   ├── CheckInAttackRange
    │   └── ActionAttack
    ├── Sequence (追击)
    │   ├── CheckTarget
    │   └── ActionChase
    ├── ActionPatrol
    └── ActionIdle
```

## 与原有 UpdateAI 的对应关系

| 原有状态 | 行为树实现 |
|---------|-----------|
| Idle | ActionIdle |
| Patrol | ActionPatrol |
| Chase | ActionChase |
| Attacking | ActionAttack |
| Retreat | ActionRetreat |
| Dodging | ActionDodge |
| Stunned | CheckStunned + 无动作 |
| Death | CheckDead + 无动作 |

## 配置到敌人控制器

在 `EnemyConfigData` 中设置行为树引用：

```csharp
// 在 EnemyAIController 中自动初始化
behaviorTree = configData.behaviorTree.Clone();
behaviorTree.Blackboard.SetValue("controller", this);
behaviorTree.Blackboard.SetValue("config", configData);
```

## 扩展建议

1. **添加更多条件节点**: CheckHealthPercent (检查血量百分比)
2. **添加组合节点**: 添加随机选择器实现概率行为
3. **添加装饰节点**: 添加 Repeat (重复执行)、Inverter (反转结果)
4. **优化性能**: 对于频繁检查的节点添加缓存机制