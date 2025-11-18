#if UNITY_EDITOR


using UnityEditor;
#endif
using UnityEngine;

public class AttackHitVisualizer : MonoBehaviour
{
#if UNITY_EDITOR
    [Header("调试可视化")]
    public bool debugVisualize = true;

    private CharacterLogic characterLogic;
    private AttackFrameData currentFrameData;
    private Vector2 currentPosition;
    private bool currentFacingRight;

    private void OnGUI()
    {
        if (debugVisualize && characterLogic != null && characterLogic.currentAttackActionData != null && characterLogic.currentAttackPhase == AttackPhase.Active)
        {
            GUILayout.BeginArea(new Rect(10, 100, 300, 200));
            GUILayout.BeginVertical(EditorStyles.helpBox);

            GUILayout.Label($"攻击检测状态:");
            GUILayout.Label($"当前阶段: {characterLogic.currentAttackPhase}");
            GUILayout.Label($"计时器: {characterLogic.currentAttackTimer:F2}s");
            GUILayout.Label($"攻击中已过: {characterLogic.currentAttackTimer - characterLogic.currentAttackActionData.windUpTime:F2}s");

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }

    public void SetCurrentFrameData(CharacterLogic character, AttackFrameData frameData, Vector2 position, bool facingRight)
    {
        characterLogic = character;
        currentFrameData = frameData;
        currentPosition = position;
        currentFacingRight = facingRight;
    }

    public void ClearFrameData()
    {
        characterLogic = null;
        currentFrameData = null;
    }

    private void Update()
    {
        if (debugVisualize && currentFrameData != null)
        {
            DebugDrawHitBox(currentFrameData, currentPosition, currentFacingRight);
        }
    }

    private void DebugDrawHitBox(AttackFrameData frameData, Vector2 position, bool facingRight)
    {
        float facingMultiplier = facingRight ? 1 : -1;

        switch (frameData.hitboxType)
        {
            case HitboxType.Rectangle:
                DebugDrawRectangle(position, frameData.hitboxSize);
                break;

            case HitboxType.Circle:
                DebugDrawCircle(position, frameData.hitboxRadius);
                break;

            case HitboxType.Capsule:
                DebugDrawCapsule(position, frameData.hitboxSize, frameData.hitboxRadius, facingMultiplier);
                break;

            case HitboxType.Sector:
                DebugDrawSector(position, frameData.hitboxRadius, frameData.hitboxAngle, facingMultiplier);
                break;

            case HitboxType.Line:
                DebugDrawLine(position, frameData.hitboxEndPoint, frameData.hitboxRadius, facingMultiplier);
                break;
        }
    }

    private void DebugDrawRectangle(Vector2 position, Vector2 size)
    {
        Vector2 halfSize = size * 0.5f;
        Vector2 topLeft = position + new Vector2(-halfSize.x, halfSize.y);
        Vector2 topRight = position + new Vector2(halfSize.x, halfSize.y);
        Vector2 bottomLeft = position + new Vector2(-halfSize.x, -halfSize.y);
        Vector2 bottomRight = position + new Vector2(halfSize.x, -halfSize.y);

        Debug.DrawLine(topLeft, topRight, Color.red, 0.1f);
        Debug.DrawLine(topRight, bottomRight, Color.red, 0.1f);
        Debug.DrawLine(bottomRight, bottomLeft, Color.red, 0.1f);
        Debug.DrawLine(bottomLeft, topLeft, Color.red, 0.1f);
    }

    private void DebugDrawCircle(Vector2 position, float radius)
    {
        int segments = 16;
        float angleStep = 360f / segments;

        for (int i = 0; i < segments; i++)
        {
            float angle1 = i * angleStep * Mathf.Deg2Rad;
            float angle2 = (i + 1) * angleStep * Mathf.Deg2Rad;

            Vector2 point1 = position + new Vector2(Mathf.Cos(angle1), Mathf.Sin(angle1)) * radius;
            Vector2 point2 = position + new Vector2(Mathf.Cos(angle2), Mathf.Sin(angle2)) * radius;

            Debug.DrawLine(point1, point2, Color.red, 0.1f);
        }
    }

    private void DebugDrawCapsule(Vector2 position, Vector2 size, float radius, float facingMultiplier)
    {
        Vector2 startPoint = position - new Vector2(size.x * 0.5f * facingMultiplier, 0);
        Vector2 endPoint = position + new Vector2(size.x * 0.5f * facingMultiplier, 0);

        // 绘制两个半圆
        DrawArc(startPoint, radius, 0, 180, 16, Color.red);
        DrawArc(endPoint, radius, 180, 180, 16, Color.red);

        // 绘制连接线
        Debug.DrawLine(startPoint + Vector2.up * radius, endPoint + Vector2.up * radius, Color.red, 0.1f);
        Debug.DrawLine(startPoint + Vector2.down * radius, endPoint + Vector2.down * radius, Color.red, 0.1f);
    }

    private void DebugDrawSector(Vector2 position, float radius, float angle, float facingMultiplier)
    {
        float startAngle = -angle * 0.5f * facingMultiplier;
        float endAngle = angle * 0.5f * facingMultiplier;

        // 绘制弧线
        DrawArc(position, radius, startAngle, angle, 16, Color.red);

        // 绘制扇形边界线
        Vector2 startDir = Quaternion.Euler(0, 0, startAngle) * Vector2.right;
        Vector2 endDir = Quaternion.Euler(0, 0, endAngle) * Vector2.right;
        Debug.DrawLine(position, position + startDir * radius, Color.red, 0.1f);
        Debug.DrawLine(position, position + endDir * radius, Color.red, 0.1f);
    }

    private void DebugDrawLine(Vector2 position, Vector2 endPoint, float width, float facingMultiplier)
    {
        Vector2 worldEndPoint = position + new Vector2(endPoint.x * facingMultiplier, endPoint.y);

        // 绘制线段
        Debug.DrawLine(position, worldEndPoint, Color.red, 0.1f);

        // 绘制线段的宽度表示
        Vector2 direction = (worldEndPoint - position).normalized;
        Vector2 perpendicular = new Vector2(-direction.y, direction.x).normalized * width * 0.5f;

        Debug.DrawLine(position + perpendicular, worldEndPoint + perpendicular, Color.red, 0.1f);
        Debug.DrawLine(position - perpendicular, worldEndPoint - perpendicular, Color.red, 0.1f);
        Debug.DrawLine(position + perpendicular, position - perpendicular, Color.red, 0.1f);
        Debug.DrawLine(worldEndPoint + perpendicular, worldEndPoint - perpendicular, Color.red, 0.1f);
    }

    private void DrawArc(Vector2 center, float radius, float startAngle, float arcAngle, int segments, Color color)
    {
        float angleStep = arcAngle / segments;

        for (int i = 0; i < segments; i++)
        {
            float angle1 = (startAngle + i * angleStep) * Mathf.Deg2Rad;
            float angle2 = (startAngle + (i + 1) * angleStep) * Mathf.Deg2Rad;

            Vector2 point1 = center + new Vector2(Mathf.Cos(angle1), Mathf.Sin(angle1)) * radius;
            Vector2 point2 = center + new Vector2(Mathf.Cos(angle2), Mathf.Sin(angle2)) * radius;

            Debug.DrawLine(point1, point2, color, 0.1f);
        }
    }
#endif
}