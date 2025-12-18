using UnityEngine;

public enum PatrolAreaType
{
    [InspectorName("不巡逻")]
    None,       // 不巡逻
    [InspectorName("圆形巡逻")]
    Circle,     // 圆形巡逻
    [InspectorName("矩形巡逻")]
    Rectangle   // 矩形巡逻
}

[System.Serializable]
public class PatrolArea
{
    public PatrolAreaType patrolType = PatrolAreaType.None;

    [Header("圆形巡逻区域")]
    public float circleRadius = 5f;

    [Header("矩形巡逻区域")]
    public Vector2 rectangleSize = new Vector2(10f, 5f);

    public Vector3 GetRandomPointInArea(Vector3 center)
    {
        switch (patrolType)
        {
            case PatrolAreaType.Circle:
                return GetRandomPointInCircle(center);
            case PatrolAreaType.Rectangle:
                return GetRandomPointInRectangle(center);
            default:
                return center;
        }
    }

    public bool IsInArea(Vector3 position, Vector3 center)
    {
        switch (patrolType)
        {
            case PatrolAreaType.Circle:
                return Vector3.Distance(position, center) <= circleRadius;
            case PatrolAreaType.Rectangle:
                Vector3 offset = position - center;
                return Mathf.Abs(offset.x) <= rectangleSize.x * 0.5f &&
                       Mathf.Abs(offset.y) <= rectangleSize.y * 0.5f;
            default:
                return false;
        }
    }

    private Vector3 GetRandomPointInCircle(Vector3 center)
    {
        Vector2 randomPoint = Random.insideUnitCircle * circleRadius;
        return center + new Vector3(randomPoint.x, randomPoint.y, 0);
    }

    private Vector3 GetRandomPointInRectangle(Vector3 center)
    {
        float randomX = Random.Range(-rectangleSize.x * 0.5f, rectangleSize.x * 0.5f);
        float randomY = Random.Range(-rectangleSize.y * 0.5f, rectangleSize.y * 0.5f);
        return center + new Vector3(randomX, randomY, 0);
    }

    public void DrawGizmos(Vector3 center, Color color)
    {
        Gizmos.color = color;
        switch (patrolType)
        {
            case PatrolAreaType.Circle:
                DrawCircleGizmo(center);
                break;
            case PatrolAreaType.Rectangle:
                DrawRectangleGizmo(center);
                break;
        }
    }

    private void DrawCircleGizmo(Vector3 center)
    {
        const int segments = 32;
        Vector3 prevPoint = center + new Vector3(circleRadius, 0, 0);

        for (int i = 1; i <= segments; i++)
        {
            float angle = (i / (float)segments) * Mathf.PI * 2f;
            Vector3 newPoint = center + new Vector3(
                Mathf.Cos(angle) * circleRadius,
                Mathf.Sin(angle) * circleRadius,
                0
            );
            Gizmos.DrawLine(prevPoint, newPoint);
            prevPoint = newPoint;
        }
    }

    private void DrawRectangleGizmo(Vector3 center)
    {
        Vector3 halfSize = new Vector3(rectangleSize.x * 0.5f, rectangleSize.y * 0.5f, 0);
        Vector3 topLeft = center + new Vector3(-halfSize.x, halfSize.y, 0);
        Vector3 topRight = center + new Vector3(halfSize.x, halfSize.y, 0);
        Vector3 bottomLeft = center + new Vector3(-halfSize.x, -halfSize.y, 0);
        Vector3 bottomRight = center + new Vector3(halfSize.x, -halfSize.y, 0);

        Gizmos.DrawLine(topLeft, topRight);
        Gizmos.DrawLine(topRight, bottomRight);
        Gizmos.DrawLine(bottomRight, bottomLeft);
        Gizmos.DrawLine(bottomLeft, topLeft);
    }
}
