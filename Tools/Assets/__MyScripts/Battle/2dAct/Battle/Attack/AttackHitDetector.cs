using System.Collections.Generic;
using UnityEngine;

public class AttackHitDetector : BaseMonoSingleClass<AttackHitDetector>
{
    private Dictionary<string, HashSet<GameObject>> attackHitRecords = new Dictionary<string, HashSet<GameObject>>();


    /// <summary>
    /// 开始一次攻击检测
    /// </summary>
    public string StartAttackDetection(ActionData attackData, Vector2 position, bool facingRight, GameObject attacker)
    {
        string attackId = $"{attacker.GetInstanceID()}_{Time.frameCount}_{Random.Range(1000, 9999)}";

        if (!attackHitRecords.ContainsKey(attackId))
        {
            attackHitRecords[attackId] = new HashSet<GameObject>();
        }

        LogManager.Log($"[AttackHitDetector] 开始攻击检测: {attackId}");
        return attackId;
    }

    /// <summary>
    /// 检测指定帧的攻击
    /// </summary>
    public AttackFrameData CheckHitForFrame(string attackId, ActionData attackData, Vector2 position, bool facingRight,
                               float currentAttackTimer, GameObject attacker)
    {
        if (!attackHitRecords.ContainsKey(attackId)) return null;

        var alreadyHit = attackHitRecords[attackId];
        int currentFrameIndex = CalculateCurrentFrameIndex(attackData, currentAttackTimer);

        return CheckHitForFrame(attackData, position, facingRight, currentFrameIndex, alreadyHit, attacker);
    }

    /// <summary>
    /// 结束攻击检测
    /// </summary>
    public void EndAttackDetection(string attackId)
    {
        if (attackHitRecords.ContainsKey(attackId))
        {
            attackHitRecords.Remove(attackId);
            LogManager.Log($"[AttackHitDetector] 结束攻击检测: {attackId}");
        }
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
    private AttackFrameData CheckHitForFrame(ActionData attackData, Vector2 position, bool facingRight,
                                int frameIndex, HashSet<GameObject> alreadyHit, GameObject attacker)
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
            LogManager.LogWarning($"[AttackHitDetector] 第 {frameIndex} 帧没有配置攻击框数据");
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
            if (hit.gameObject != attacker && !alreadyHit.Contains(hit.gameObject))//避免自伤且同一目标只命中一次
            {
                ProcessHit(hit.gameObject, attackData, frameData, attacker);
                alreadyHit.Add(hit.gameObject);
            }
        }

        //LogManager.Log($"[AttackHitDetector] 检测第 {frameIndex} 帧，命中 {hits.Length} 个目标");
        return frameData;
    }

    private int CalculateCurrentFrameIndex(ActionData attackData, float currentAttackTimer)
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

    private void ProcessHit(GameObject target, ActionData attackData, AttackFrameData frameData, GameObject attacker)
    {
        LogManager.Log($"[AttackHitDetector] 第 {frameData.frameIndex} 帧命中: {target.name}");



        // 检查是否是假人
        TestDummy dummy = target.GetComponent<TestDummy>();
        if (dummy != null)
        {
            Vector2 knockbackDirection = (target.transform.position - attacker.transform.position).normalized;
            dummy.TakeDamage(attackData, frameData, knockbackDirection, frameData.knockbackForce);
            return;
        }

        // 检查是否是角色逻辑组件
        CharacterLogic characterLogic = target.GetComponent<CharacterLogic>();
        if (characterLogic != null)
        {
            Vector2 knockbackDirection = (target.transform.position - attacker.transform.position).normalized;

            // 调用角色的受伤逻辑
            characterLogic.OnHurt?.Invoke(attackData, frameData, attacker);
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