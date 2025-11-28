using System.Collections.Generic;
using UnityEngine;

public class AttackHitDetector : BaseMonoSingleClass<AttackHitDetector>
{
    // 记录每次攻击命中的目标对象
    private Dictionary<string, HashSet<GameObject>> attackHitRecords = new Dictionary<string, HashSet<GameObject>>();

    // 记录每次攻击关联的DamageInfo（用于在ApplySkillEffectsOnComplete时访问）
    private Dictionary<string, DamageInfo> attackDamageInfos = new Dictionary<string, DamageInfo>();


    /// <summary>
    /// 开始一次攻击检测
    /// 创建并存储DamageInfo，记录攻击方信息
    /// </summary>
    public string StartAttackDetection(ActionData attackData, Vector2 position, bool facingRight, GameObject attacker)
    {
        string attackId = $"{attacker.GetInstanceID()}_{Time.frameCount}_{Random.Range(1000, 9999)}";

        if (!attackHitRecords.ContainsKey(attackId))
        {
            attackHitRecords[attackId] = new HashSet<GameObject>();
        }

        // 为这次攻击创建DamageInfo，记录攻击方
        AttackActionData attackActionData = attackData as AttackActionData;
        if (attackActionData != null)
        {
            DamageInfo damageInfo = new DamageInfo
            {
                attacker = attacker,
                target = null, // 受击方在命中时填充
                skillData = attackActionData.skillData
            };
            attackDamageInfos[attackId] = damageInfo;
        }

        //LogManager.Log($"[AttackHitDetector] 开始攻击检测: {attackId}");
        return attackId;
    }

    /// <summary>
    /// 检测指定帧的攻击
    /// </summary>
    public AttackFrameData CheckHitForFrame(string attackId, AttackActionData attackData, Vector2 position, bool facingRight,
                               float currentAttackTimer, GameObject attacker)
    {
        if (!attackHitRecords.ContainsKey(attackId)) return null;

        var alreadyHit = attackHitRecords[attackId];
        int currentFrameIndex = CalculateCurrentFrameIndex(attackData, currentAttackTimer);

        return CheckHitForFrame(attackData, position, facingRight, currentFrameIndex, alreadyHit, attacker, attackId);
    }

    /// <summary>
    /// 结束攻击检测
    /// 清理攻击记录和DamageInfo
    /// </summary>
    public void EndAttackDetection(string attackId)
    {
        if (attackHitRecords.ContainsKey(attackId))
        {
            attackHitRecords.Remove(attackId);
            //LogManager.Log($"[AttackHitDetector] 结束攻击检测: {attackId}");
        }

        // 清理DamageInfo记录
        if (attackDamageInfos.ContainsKey(attackId))
        {
            attackDamageInfos.Remove(attackId);
        }
    }

    /// <summary>
    /// 获取指定攻击的DamageInfo
    /// 用于在ApplySkillEffectsOnComplete时获取受击方信息
    /// </summary>
    public DamageInfo GetAttackDamageInfo(string attackId)
    {
        if (attackDamageInfos.ContainsKey(attackId))
        {
            return attackDamageInfos[attackId];
        }
        return null;
    }

    /// <summary>
    /// 强制清除所有攻击记录（用于角色死亡等特殊情况）
    /// </summary>
    public void ClearAllAttackRecords()
    {
        attackHitRecords.Clear();
        LogManager.Log("[AttackHitDetector] 清除所有攻击记录");
    }

    #region 核心检测逻辑
    private AttackFrameData CheckHitForFrame(AttackActionData attackData, Vector2 position, bool facingRight,
                                int frameIndex, HashSet<GameObject> alreadyHit, GameObject attacker, string attackId)
    {
        if (attackData == null)
        {
            LogManager.LogWarning("[AttackHitDetector] 当前没有攻击数据");
            return null;
        }

        // 检查帧索引是否在有效范围内(其实不需要检测帧范围,只要用获取攻击帧有没有进行判定即可)
        //if (frameIndex < attackData.ActualWindUpFrames ||
        //    frameIndex > attackData.ActualWindUpFrames + attackData.ActualActiveFrames)
        //{
        //    LogManager.LogWarning($"[AttackHitDetector] 帧索引 {frameIndex} 超出攻击中阶段范围");
        //    return;
        //}
        //LogManager.Log($"检测攻击帧:{frameIndex}");
        // 获取当前帧的攻击框数据
        var frameData = attackData.GetFrameData(frameIndex);
        if (frameData == null)
        {
            //LogManager.LogWarning($"[AttackHitDetector] 第 {frameIndex} 帧没有配置攻击框数据");
            return null;
        }

        if (!frameData.isAttackFrame)
        {
            LogManager.Log($"[AttackHitDetector] 第 {frameIndex} 帧不是攻击帧，跳过检测");
            return null;
        }

        Vector2 detectionPosition = CalculateHitboxPosition(frameData, position, facingRight);


#if UNITY_EDITOR
        var attackHitVisualizer = attacker.GetComponent<AttackHitVisualizer>();//编辑器窗口攻击范围可视化
        if (attackHitVisualizer != null)
        {
            attackHitVisualizer.SetCurrentFrameData(attacker.GetComponent<CharacterLogic>(), frameData, detectionPosition, facingRight);
        }
#endif

        Collider2D[] hits = GetHitColliders(frameData, detectionPosition, facingRight);

        foreach (Collider2D hit in hits)
        {
            if (hit.gameObject == attacker) continue;

            bool alreadyProcessed = alreadyHit.Contains(hit.gameObject);
            if (!alreadyProcessed || frameData.allowIndependentHit)
            {
                // 传递attackId给ProcessHit
                ProcessHit(hit.gameObject, attackData, frameData, attacker, attackId);

                // 仍将目标加入记录列表，确保未标记为独立的后续帧不会重复命中
                alreadyHit.Add(hit.gameObject);
            }
        }

        //LogManager.Log($"[AttackHitDetector] 检测第 {frameIndex} 帧，命中 {hits.Length} 个目标");
        return frameData;
    }

    private int CalculateCurrentFrameIndex(AttackActionData attackData, float currentAttackTimer)
    {
        if (attackData == null) return 0;

        float activeElapsed = currentAttackTimer - attackData.windUpTime;
        activeElapsed = Mathf.Clamp(activeElapsed, 0f, attackData.activeTime);

        int totalActiveFrames = attackData.ActualActiveFrames;
        if (totalActiveFrames <= 0) return 0;

        float phaseProgress = activeElapsed / attackData.activeTime;
        int frameIndex = Mathf.FloorToInt(phaseProgress * totalActiveFrames);//向上取整，确保在activeTime结束时能命中最后一帧
        frameIndex = Mathf.Clamp(frameIndex, 0, totalActiveFrames);//允许等于totalActiveFrames，确保最后一帧能被检测到

        int actualFrameIndex = attackData.ActualWindUpFrames + frameIndex;
        return actualFrameIndex;
    }

    private Vector2 CalculateHitboxPosition(AttackFrameData frameData, Vector2 characterPosition, bool facingRight)
    {
        return characterPosition +
            new Vector2(frameData.hitboxOffset.x * (facingRight ? 1 : -1),
                       frameData.hitboxOffset.y);
    }

    private Collider2D[] GetHitColliders(AttackFrameData frameData, Vector2 position, bool facingRight)
    {
        float facingMultiplier = facingRight ? 1 : -1;

        switch (frameData.hitboxType)
        {
            case HitboxType.Rectangle:
                return Physics2D.OverlapBoxAll(position, frameData.hitboxSize, 0f, frameData.hitLayers);

            case HitboxType.Circle:
                return Physics2D.OverlapCircleAll(position, frameData.hitboxRadius, frameData.hitLayers);

            case HitboxType.Capsule:
                Vector2 capsuleStart = position;
                Vector2 capsuleEnd = position + new Vector2(frameData.hitboxSize.x * facingMultiplier, 0);
                return Physics2D.OverlapCapsuleAll(
                    (capsuleStart + capsuleEnd) * 0.5f,
                    new Vector2(frameData.hitboxSize.x, frameData.hitboxRadius * 2),
                    CapsuleDirection2D.Horizontal,
                    0f,
                    frameData.hitLayers);

            case HitboxType.Sector:
                return GetSectorOverlap(frameData, position, facingRight);

            case HitboxType.Line:
                return GetLineOverlap(frameData, position, facingRight);

            default:
                return Physics2D.OverlapBoxAll(position, frameData.hitboxSize, 0f, frameData.hitLayers);
        }
    }

    /// <summary>
    /// 处理命中目标的逻辑
    /// 使用IDamageable接口统一处理所有可受击对象
    /// 支持玩家、假人、敌人等任何实现了IDamageable接口的对象
    /// </summary>
    /// <param name="target">被命中的目标GameObject</param>
    /// <param name="attackData">攻击动作数据</param>
    /// <param name="frameData">当前帧的攻击数据</param>
    /// <param name="attacker">攻击者GameObject</param>
    /// <param name="attackId">攻击ID，用于获取对应的DamageInfo</param>
    private void ProcessHit(GameObject target, ActionData attackData, AttackFrameData frameData, GameObject attacker, string attackId)
    {
        LogManager.Log($"[AttackHitDetector] 第 {frameData.frameIndex} 帧命中: {target.name}");

        AttackActionData attackActionData = attackData as AttackActionData;
        if (attackActionData == null)
        {
            LogManager.LogWarning("[AttackHitDetector] 无法转换为AttackActionData");
            return;
        }

        IDamageable damageable = target.GetComponent<IDamageable>();
        if (damageable == null)
        {
            damageable = target.GetComponentInParent<IDamageable>();
        }

        if (damageable == null)
        {
            LogManager.LogWarning($"[AttackHitDetector] 目标 {target.name} 没有实现IDamageable接口");
            return;
        }

        if (damageable.IsDead)
        {
            return;
        }

        // 创建或获取伤害信息
        DamageInfo damageInfo = CreateDamageInfo(attackActionData, frameData, attacker);

        // 填充受击方信息
        damageInfo.target = target;

        // 将DamageInfo更新到字典中（用于ApplySkillEffectsOnComplete时访问）
        if (!string.IsNullOrEmpty(attackId) && attackDamageInfos.ContainsKey(attackId))
        {
            attackDamageInfos[attackId].target = target;
        }

        // 仅应用攻击帧级别的效果（命中时）
        ApplyFrameEffects(frameData, target, attacker, true);

        // 应用伤害
        damageable.TakeDamage(damageInfo, attackActionData, frameData, attacker);
    }

    /// <summary>
    /// 创建伤害信息
    /// 计算最终伤害 = 技能基础伤害(baseDamage) + 攻击帧附加伤害(frameData.damage)
    /// </summary>
    public static DamageInfo CreateDamageInfo(AttackActionData attackActionData, AttackFrameData frameData, GameObject attacker)
    {
        var skillData = attackActionData.skillData;

        // 计算最终伤害：技能基础伤害 + 攻击帧附加伤害
        float baseDamage = frameData.damage;

        if (skillData != null)
        {
            // 技能基础伤害 + 攻击帧附加伤害
            if (skillData.baseDamage > 0)
            {
                baseDamage = skillData.baseDamage + frameData.damage;
            }

            bool isGuaranteedCrit = skillData.isGuaranteedCrit;
            float lifeStealPercent = skillData.lifeStealPercent;

            if (skillData.lifeStealChance > 0)
            {
                float roll = Random.Range(0f, 100f);
                if (roll < skillData.lifeStealChance)
                {
                    lifeStealPercent += skillData.lifeStealAmount;
                }
            }

            return new DamageInfo
            {
                baseDamage = baseDamage,
                breakPower = skillData.breakPower,
                isGuaranteedCrit = isGuaranteedCrit,
                lifeStealPercent = lifeStealPercent,
                attacker = attacker,
                target = null,
                skillData = skillData
            };
        }

        // 如果没有技能数据，只使用攻击帧的附加伤害
        return new DamageInfo
        {
            baseDamage = baseDamage,
            breakPower = 0,
            staggerDamage = 0,
            isGuaranteedCrit = false,
            lifeStealPercent = 0,
            attacker = attacker,
            target = null,
            skillData = null
        };
    }

    /// <summary>
    /// 应用攻击帧效果到目标
    /// 从攻击帧数据中提取效果列表并应用到目标的BuffSystem
    /// 用于实现多段攻击中不同段落施加不同效果的功能
    /// 根据EffectTarget决定效果施加给攻击者还是被击中者
    /// </summary>
    /// <param name="frameData">攻击帧数据，包含效果列表</param>
    /// <param name="target">被命中的目标</param>
    /// <param name="attacker">攻击者</param>
    /// <param name="isHit">是否命中目标，用于判断requireHit效果是否生效</param>
    private void ApplyFrameEffects(AttackFrameData frameData, GameObject target, GameObject attacker, bool isHit)
    {
        // 检查攻击帧是否有效果列表
        if (frameData.effects == null || frameData.effects.Count == 0)
        {
            // 没有效果需要应用，直接返回
            return;
        }

        // 遍历并应用攻击帧的所有效果
        foreach (var effect in frameData.effects)
        {
            if (effect != null)
            {
                // 判断效果是否需要命中才能触发
                if (effect.requireHit && !isHit)
                {
                    // 效果需要命中但没有命中，跳过
                    continue;
                }

                // 根据效果目标类型选择施加对象
                GameObject effectReceiver = (effect.effectTarget == EffectTarget.Attacker) ? attacker : target;

                // 尝试获取效果接收者的BuffSystem组件
                var receiverBuffSystem = effectReceiver.GetComponentInParent<BuffSystem>();
                if (receiverBuffSystem == null)
                {
                    LogManager.LogWarning($"[AttackHitDetector] 效果接收者 {effectReceiver.name} 没有BuffSystem组件，无法应用攻击帧效果 {effect.effectName}");
                    continue;
                }

                // 应用Buff效果，由BuffSystem负责处理具体逻辑
                receiverBuffSystem.ApplyBuff(effect, attacker);
                LogManager.Log($"[AttackHitDetector] 应用攻击帧效果: {effect.effectName} 到 {effectReceiver.name} (目标类型: {effect.effectTarget}, 命中状态: {isHit})");
            }
        }
    }

    #endregion

    #region 辅助方法（保持原有实现）
    private Collider2D[] GetSectorOverlap(AttackFrameData frameData, Vector2 position, bool facingRight)
    {
        // 保持原有实现
        List<Collider2D> hits = new List<Collider2D>();
        float facingMultiplier = facingRight ? 1 : -1;

        Collider2D[] circleHits = Physics2D.OverlapCircleAll(position, frameData.hitboxRadius, frameData.hitLayers);

        foreach (Collider2D hit in circleHits)
        {
            Vector2 directionToTarget = (hit.bounds.center - (Vector3)position).normalized;
            Vector2 sectorForward = facingRight ? Vector2.right : Vector2.left;
            float angle = Vector2.Angle(sectorForward, directionToTarget);

            if (angle <= frameData.hitboxAngle * 0.5f)
            {
                hits.Add(hit);
            }
        }

        return hits.ToArray();
    }

    private Collider2D[] GetLineOverlap(AttackFrameData frameData, Vector2 position, bool facingRight)
    {
        // 保持原有实现
        List<Collider2D> hits = new List<Collider2D>();
        float facingMultiplier = facingRight ? 1 : -1;

        Vector2 lineStart = position;
        Vector2 lineEnd = position + new Vector2(
            frameData.hitboxEndPoint.x * facingMultiplier,
            frameData.hitboxEndPoint.y);

        Vector2 lineCenter = (lineStart + lineEnd) * 0.5f;
        float lineLength = Vector2.Distance(lineStart, lineEnd);
        float lineAngle = Mathf.Atan2(lineEnd.y - lineStart.y, lineEnd.x - lineStart.x) * Mathf.Rad2Deg;

        Collider2D[] boxHits = Physics2D.OverlapBoxAll(
            lineCenter,
            new Vector2(lineLength, frameData.hitboxRadius * 2),
            lineAngle,
            frameData.hitLayers);

        foreach (Collider2D hit in boxHits)
        {
            Vector2 closestPoint = GetClosestPointOnLineSegment(lineStart, lineEnd, hit.bounds.center);
            float distance = Vector2.Distance(hit.bounds.center, closestPoint);

            if (distance <= frameData.hitboxRadius)
            {
                hits.Add(hit);
            }
        }

        return hits.ToArray();
    }

    private Vector2 GetClosestPointOnLineSegment(Vector2 lineStart, Vector2 lineEnd, Vector2 point)
    {
        Vector2 lineDirection = lineEnd - lineStart;
        float lineLength = lineDirection.magnitude;
        lineDirection.Normalize();

        float projectionLength = Mathf.Clamp(Vector2.Dot(point - lineStart, lineDirection), 0, lineLength);
        return lineStart + lineDirection * projectionLength;
    }
    #endregion
}