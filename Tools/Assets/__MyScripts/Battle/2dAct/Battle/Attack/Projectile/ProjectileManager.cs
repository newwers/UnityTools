using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 投掷物管理器（单例）
/// 提供获取、发射与回收投掷物的功能
/// 管理基于对象池的投掷物复用
/// </summary>
public class ProjectileManager : BaseMonoSingleClass<ProjectileManager>
{
    private Dictionary<ProjectileData, Queue<ProjectileController>> pool = new Dictionary<ProjectileData, Queue<ProjectileController>>();
    private List<ProjectileController> activeProjectiles = new List<ProjectileController>();

    // 获取投掷物实例（从对象池或新建）
    public ProjectileController GetProjectile(ProjectileData data)
    {
        if (data == null || data.prefab == null) return null;

        if (!pool.ContainsKey(data))
        {
            pool[data] = new Queue<ProjectileController>();
        }

        Queue<ProjectileController> q = pool[data];
        ProjectileController proj = null;
        while (q.Count > 0)
        {
            var p = q.Dequeue();
            if (p != null)
            {
                proj = p;
                break;
            }
        }

        if (proj == null)
        {
            var go = GameObject.Instantiate(data.prefab);
            proj = go.GetComponent<ProjectileController>();
            if (proj == null)
            {
                proj = go.AddComponent<ProjectileController>();
            }
        }

        proj.gameObject.SetActive(false);
        return proj;
    }

    // 发射投掷物
    public ProjectileController LaunchProjectile(ProjectileData data, Vector2 position, Vector2 direction, CharacterBase owner)
    {
        if (data == null) return null;

        ProjectileController proj = GetProjectile(data);
        if (proj == null) return null;

        proj.transform.position = position;
        proj.transform.rotation = Quaternion.identity;
        proj.data = data;

        proj.Launch(direction, owner, data);
        activeProjectiles.Add(proj);
        return proj;
    }

    // 回收投掷物
    public void ReturnProjectile(ProjectileController proj)
    {
        if (proj == null || proj.data == null) return;

        activeProjectiles.Remove(proj);
        proj.gameObject.SetActive(false);

        if (!pool.ContainsKey(proj.data))
        {
            pool[proj.data] = new Queue<ProjectileController>();
        }
        pool[proj.data].Enqueue(proj);
    }

    /// <summary>
    /// 检测角色身前是否有投掷物
    /// </summary>
    /// <param name="character">检测目标</param>
    /// <param name="detectionRange">检测距离</param>
    /// <returns></returns>
    public bool HasProjectileInFront(CharacterBase character, float detectionRange = 2f)
    {
        if (character == null) return false;

        Vector2 characterPosition = character.transform.position;

        foreach (ProjectileController projectile in activeProjectiles)
        {
            if (projectile == null || !projectile.gameObject.activeInHierarchy) continue;

            Vector2 projectilePosition = projectile.transform.position;
            float distance = Vector2.Distance(characterPosition, projectilePosition);

            // 检查距离是否在检测范围内
            if (distance > detectionRange) continue;

            bool isInFront = false;

            // 检查投掷物是否在角色身前
            if (character.isFacingRight)
            {
                // 角色朝右，检查投掷物是否在角色右侧
                if (projectilePosition.x > characterPosition.x)
                {
                    isInFront = true;
                }
            }
            else
            {
                // 角色朝左，检查投掷物是否在角色左侧
                if (projectilePosition.x < characterPosition.x)
                {
                    isInFront = true;
                }
            }
            if (isInFront)
            {
                Rigidbody2D projectileRb = projectile.rb;//检测投掷物刚体速度方向是否朝向角色
                if (projectileRb != null)
                {
                    Vector2 velocity = projectileRb.linearVelocity;
                    bool isMovingTowards = (character.isFacingRight && velocity.x < 0) || (!character.isFacingRight && velocity.x > 0);
                    if (isMovingTowards)
                    {
                        return true;
                    }
                }
            }

        }

        return false;
    }
}
