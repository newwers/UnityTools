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
        return proj;
    }

    // 回收投掷物
    public void ReturnProjectile(ProjectileController proj)
    {
        if (proj == null || proj.data == null) return;

        proj.gameObject.SetActive(false);

        if (!pool.ContainsKey(proj.data))
        {
            pool[proj.data] = new Queue<ProjectileController>();
        }
        pool[proj.data].Enqueue(proj);
    }
}
