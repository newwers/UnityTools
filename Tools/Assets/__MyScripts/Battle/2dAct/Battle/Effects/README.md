# 效果系统 (Effect System)

完整的Buff/Debuff效果系统，支持各种战斗效果和属性增强。

## 📁 文件结构

```
Effects/
├── EffectData.cs                    # 效果数据定义（核心）
├── BuffSystem.cs                    # Buff系统实现（核心）
├── StatusEffect.cs                  # 状态效果（占位）
├── EffectDataExamples.cs           # 效果创建示例
├── AttributeEffectsTest.cs         # 测试工具
├── SkillWithAttributeEffects.cs    # 技能示例
├── 属性效果使用指南.md             # 详细使用文档
├── 属性效果系统更新说明.md         # 更新说明
├── 快速参考.md                     # 速查表
└── README.md                        # 本文件
```

## 🎯 主要功能

### 控制效果
- 眩晕、定身、冰冻、沉默、致盲
- 眩晕免疫、无敌、霸体

### 移动效果
- 减速、加速、击退

### 伤害效果
- 伤害增幅、伤害减免、伤害反弹
- 生命偷取、韧性伤害、必定暴击

### 持续效果
- 燃烧、中毒、流血（持续伤害）
- 持续回复、持续能量回复（持续恢复）
- 能量消耗

### 防御效果
- 护盾

### 属性增强（新增）
- **立即回血** - 瞬间恢复生命
- **立即回复能量** - 瞬间恢复能量
- **持续回复能量** - 每秒持续回复能量
- **增加力量** - 提升力量属性
- **增加移速** - 提升移动速度
- **增加攻击** - 提升攻击力
- **增加防御** - 提升防御力
- **增加暴击率** - 提升暴击几率
- **增加暴击伤害** - 提升暴击伤害倍率
- **增加生命上限** - 提升最大生命值
- **增加能量上限** - 提升最大能量值

## 🚀 快速开始

### 基础使用

```csharp
// 获取BuffSystem组件
BuffSystem buffSystem = GetComponent<BuffSystem>();

// 应用一个立即回血效果
var healEffect = EffectDataExamples.CreateInstantHealEffect(50f);
buffSystem.ApplyBuff(healEffect);

// 应用一个增加移速的效果
var speedEffect = EffectDataExamples.CreateSpeedBoostEffect(15f, speedPercent: 30f);
buffSystem.ApplyBuff(speedEffect);
```

### 在Unity Editor中使用

1. 创建效果资源：
   - 右键 > Create > Character System > Effect > Effect Data
   - 选择效果分类（已显示中文名称）
   - 配置参数

2. 在技能或道具中引用效果资源

3. 使用`buffSystem.ApplyBuff(effectData)`应用效果

## 📖 文档指南

### 新手入门
1. 先阅读 `快速参考.md` 了解基本概念
2. 查看 `EffectDataExamples.cs` 学习如何创建效果
3. 运行 `AttributeEffectsTest.cs` 测试各种效果

### 深入学习
1. 阅读 `属性效果使用指南.md` 了解详细用法
2. 参考 `SkillWithAttributeEffects.cs` 学习技能集成
3. 查看 `属性效果系统更新说明.md` 了解系统架构

### 问题排查
1. 检查 `快速参考.md` 的常见问题部分
2. 启用LogManager查看详细日志
3. 使用测试工具验证效果

## 🎮 测试工具

### AttributeEffectsTest
键盘测试工具，支持快速测试各种属性效果。

**使用方法：**
1. 添加到GameObject
2. 设置目标BuffSystem和CharacterAttributes
3. 运行游戏，按数字键测试

**按键映射：**
- `1` - 立即回血
- `2` - 能量效果
- `3` - 增加力量
- `4` - 增加移速
- `5` - 增加攻击
- `6` - 增加防御
- `7` - 增加暴击
- `8` - 增加生命上限
- `9` - 增加能量上限
- `0` - 组合Buff

### SkillWithAttributeEffects
技能示例工具，演示实际游戏场景中的效果使用。

**使用方法：**
1. 添加到GameObject
2. 设置playerAttributes和playerBuffSystem
3. 运行游戏，按QWERT键使用技能

**技能列表：**
- `Q` - 生命祝福（治疗）
- `W` - 战吼（全属性增强）
- `E` - 狂暴状态（高攻低防）
- `R` - 能量冥想（能量恢复）
- `T` - 生命光环（生存增益）

## 💡 设计理念

### 固定值 vs 百分比
大多数属性增强效果同时支持固定值和百分比：
- **固定值** - 直接增减属性绝对值，适合低等级
- **百分比** - 基于当前值的比例增减，适合高等级

### 堆叠机制
效果支持多种堆叠方式：
- **RefreshDuration** - 刷新持续时间
- **AddDuration** - 累加持续时间
- **IncreaseValue** - 增加效果强度

### 自动管理
- 应用效果时自动修改属性
- 移除效果时自动恢复属性
- 周期性效果自动触发
- 上限修改时自动限制当前值

## 🔧 扩展开发

### 添加新效果类型

1. **在EffectData.cs中添加枚举：**
```csharp
[InspectorName("新效果")]
/// <summary>
/// 新效果 - 效果说明
/// 参数: paramName（参数说明）
/// </summary>
NewEffect,
```

2. **在BuffSystem.cs中实现应用逻辑：**
```csharp
case EffectCategory.NewEffect:
    ApplyNewEffect(buff);
    break;
```

3. **在BuffSystem.cs中实现移除逻辑：**
```csharp
case EffectCategory.NewEffect:
    RemoveNewEffect(buff);
    break;
```

4. **在EffectDataExamples.cs中添加创建方法：**
```csharp
public static EffectData CreateNewEffect(float duration, float value)
{
    // 创建逻辑
}
```

## 📊 性能优化建议

1. **使用ScriptableObject** - 在Editor中创建效果资源，避免运行时频繁创建
2. **合理设置堆叠上限** - 避免过多层数影响计算
3. **使用对象池** - 管理临时效果实例
4. **避免每帧操作** - BuffSystem已优化，周期性效果每秒触发

## 🐛 已知限制

1. **攻击增强** - 需要在伤害计算器中手动读取
2. **上限增加** - 不会自动增加当前值
3. **定身效果** - 通过设置moveSpeedMultiplier为0实现
4. **效果移除** - 依赖正确的参数名匹配

## 📞 支持

如有问题，请参考：
1. `快速参考.md` - 常见问题解答
2. `属性效果使用指南.md` - 详细文档
3. LogManager日志输出
4. Unity Console错误信息

## 📝 更新历史

### v2.0 (当前版本)
- ✅ 添加11种新属性效果
- ✅ 所有枚举添加InspectorName
- ✅ 完整的BuffSystem实现
- ✅ 测试工具和技能示例
- ✅ 完整的文档系统

### v1.0
- 基础Buff/Debuff系统
- 控制效果、伤害效果
- 持续效果和护盾系统

## 🎓 相关系统

- **CharacterAttributes** - 角色属性系统
- **DamageCalculator** - 伤害计算系统
- **SkillSystem** - 技能系统
- **CharacterLogic** - 角色逻辑系统

---

**最后更新：** 2024
**版本：** 2.0
**Unity版本：** Unity 6000.0
