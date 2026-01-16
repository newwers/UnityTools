using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Senses
{
    public class VisionSense : BaseSense
    {
        [Header("视觉系统配置")]
        [Tooltip("视野距离")]
        public float viewDistance = 10f;
        [Tooltip("视野角度 (度)")]
        public float viewAngle = 90f;
        [Tooltip("视野高度限制")]
        public float heightLimit = 2f;
        [Tooltip("射线检测高度偏移")]
        public float heightOffset = 0f;
        [Tooltip("目标层级")]
        public LayerMask targetLayers;
        [Tooltip("障碍物层级")]
        public LayerMask obstacleLayers;
        [Tooltip("是否忽略静态物体")]
        public bool ignoreStaticObjects = true;
        [Tooltip("是否启用视锥体剔除")]
        public bool enableFrustumCulling = true;

        private List<GameObject> visibleTargets = new List<GameObject>();

        // 位置追踪变量
        private Vector3 previousPosition; // 上一帧的位置
        private Vector3 currentFacingDirection = Vector3.right; // 当前角色朝向，默认为右侧

        public override void UpdateDetection()
        {
            if (!isEnabled)
                return;

            // 计算角色朝向
            CalculateFacingDirection();

            visibleTargets.Clear();
            DetectVisibleTargets();
        }

        /// <summary>
        /// 通过比较当前位置和上一帧位置计算角色朝向
        /// 由于检测有0.1秒间隔,因此AI如果转向后,会有短暂的朝向延迟,这让AI的行为更自然一些
        /// </summary>
        private void CalculateFacingDirection()
        {
            // 获取当前位置
            Vector3 currentPosition = transform.position;

            // 计算位置差
            Vector3 positionDelta = currentPosition - previousPosition;

            // 如果位置有变化，更新朝向
            if (positionDelta.magnitude > 0.01f) // 使用小阈值避免微小移动导致的朝向变化
            {
                // 只考虑X轴方向的移动来确定朝向（2D游戏）
                if (Mathf.Abs(positionDelta.x) > 0.01f)
                {
                    currentFacingDirection = new Vector3(Mathf.Sign(positionDelta.x), 0, 0).normalized;
                }
            }

            // 更新上一帧位置
            previousPosition = currentPosition;
        }

        private void DetectVisibleTargets()
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, viewDistance, targetLayers);

            foreach (Collider2D collider in colliders)
            {
                GameObject target = collider.gameObject;

                if (!IsValidTarget(target))
                    continue;

                if (ignoreStaticObjects && target.isStatic)
                    continue;

                if (IsInViewCone(target) && !IsObstructed(target))
                {
                    visibleTargets.Add(target);
                    float distance = Vector3.Distance(transform.position, target.transform.position);
                    float intensity = Mathf.Clamp01(1.0f - (distance / viewDistance));
                    TriggerSenseEvent(new SenseEvent(SenseType.Vision, target, target.transform.position, intensity, "VisionDetected"));
                }
            }
        }

        /// <summary>
        /// 检查目标是否在角色的视野锥体内
        /// </summary>
        /// <param name="target">要检查的目标游戏对象</param>
        /// <returns>如果目标在视野锥体内返回true，否则返回false</returns>
        private bool IsInViewCone(GameObject target)
        {
            // 计算从角色到目标的方向向量
            Vector3 directionToTarget = (target.transform.position - transform.position).normalized;

            // 计算角色朝向与目标方向之间的夹角
            // 使用currentFacingDirection（基于角色移动计算出的朝向）作为参考方向
            float angleToTarget = Vector3.Angle(currentFacingDirection, directionToTarget);

            // 检查夹角是否在视野范围内（视野角的一半，因为视野是对称的）
            if (angleToTarget > viewAngle / 2)
                return false;

            // 检查目标高度是否在允许范围内
            float heightDifference = Mathf.Abs(target.transform.position.y - transform.position.y);
            if (heightDifference > heightLimit)
                return false;

            // 检查目标距离是否在视野距离范围内
            float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);
            if (distanceToTarget > viewDistance)
                return false;

            // 目标在视野锥体内
            return true;
        }

        private bool IsObstructed(GameObject target)
        {
            Vector3 directionToTarget = target.transform.position - transform.position;
            float distanceToTarget = directionToTarget.magnitude;

            Vector3 rayStart = transform.position + new Vector3(0, heightOffset, 0);
            RaycastHit2D hit = Physics2D.Raycast(rayStart, directionToTarget.normalized, distanceToTarget, obstacleLayers);

            if (hit.collider != null)
            {
                return hit.collider.gameObject != target;
            }

            return false;
        }
#if UNITY_EDITOR

        private void OnDrawGizmosSelected()
        {
            DrawGizmos();
        }

        public override void DrawGizmos()
        {
            if (!isEnabled)
                return;

            Gizmos.color = new Color(1, 1, 0, 0.3f);
            DrawViewCone();

            Gizmos.color = Color.green;
            int index = 1;
            foreach (GameObject target in visibleTargets)
            {
                if (target != null)
                {
                    Gizmos.DrawLine(transform.position + new Vector3(0, heightOffset, 0), target.transform.position);
                    Vector3 midPoint = (transform.position + new Vector3(0, heightOffset, 0) + target.transform.position) / 2;
                    Handles.Label(midPoint, "VisionSense的视觉目标" + index);
                    index++;
                }
            }
        }
#endif

        private void DrawViewCone()
        {
            float halfAngle = viewAngle / 2;
            // 使用计算出的朝向currentFacingDirection作为参考方向
            Vector3 forward = currentFacingDirection;

            Vector3 leftRay = Quaternion.Euler(0, 0, halfAngle) * forward * viewDistance;
            Vector3 rightRay = Quaternion.Euler(0, 0, -halfAngle) * forward * viewDistance;

            Vector3 rayStart = transform.position + new Vector3(0, heightOffset, 0);

            Gizmos.DrawLine(rayStart, rayStart + leftRay);
            Gizmos.DrawLine(rayStart, rayStart + rightRay);

            Vector3 current = leftRay;
            int segments = 16;
            float angleStep = viewAngle / segments;

            for (int i = 0; i <= segments; i++)
            {
                Vector3 next = Quaternion.Euler(0, 0, -angleStep * i) * forward * viewDistance;
                Gizmos.DrawLine(rayStart + current, rayStart + next);
                current = next;
            }

            Gizmos.DrawWireSphere(rayStart, viewDistance);
        }


        public List<GameObject> GetVisibleTargets()
        {
            return new List<GameObject>(visibleTargets);
        }

        public GameObject GetClosestVisibleTarget()
        {
            GameObject closestTarget = null;
            float closestDistance = Mathf.Infinity;

            foreach (GameObject target in visibleTargets)
            {
                if (target == null)
                    continue;

                float distance = Vector3.Distance(transform.position, target.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestTarget = target;
                }
            }

            return closestTarget;
        }

        public bool IsTargetVisible(GameObject target)
        {
            return visibleTargets.Contains(target);
        }
    }
}
