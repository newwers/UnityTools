using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 召唤物管理器
/// 单例模式，负责召唤物的全局管理和对象池
/// </summary>
public class SummonManager : BaseMonoSingleClass<SummonManager>
{
    /// <summary>
    /// 召唤物对象池
    /// Key: 召唤物数据, Value: 可用的召唤物队列
    /// </summary>
    private Dictionary<SummonData, Queue<SummonController>> summonPool = new Dictionary<SummonData, Queue<SummonController>>();
    
    /// <summary>
    /// 活跃召唤物列表
    /// </summary>
    private List<SummonController> activeSummons = new List<SummonController>();
    
    /// <summary>
    /// 从对象池获取或创建召唤物
    /// </summary>
    /// <param name="data">召唤物数据</param>
    /// <returns>召唤物控制器</returns>
    public SummonController GetSummon(SummonData data)
    {
        if (data == null || data.summonPrefab == null)
        {
            Debug.LogError("[SummonManager] 召唤物数据或预制体为空");
            return null;
        }
        
        // 如果对象池不存在该类型的召唤物，创建新的队列
        if (!summonPool.ContainsKey(data))
        {
            summonPool[data] = new Queue<SummonController>();
        }
        
        Queue<SummonController> queue = summonPool[data];
        SummonController summon = null;
        
        // 从队列中获取可用的召唤物
        while (queue.Count > 0)
        {
            var s = queue.Dequeue();
            if (s != null && s.gameObject != null)
            {
                summon = s;
                break;
            }
        }
        
        // 如果没有可用的召唤物，创建新的
        if (summon == null)
        {
            var go = GameObject.Instantiate(data.summonPrefab);
            summon = go.GetComponent<SummonController>();
            
            // 如果预制体没有SummonController组件，添加一个
            if (summon == null)
            {
                summon = go.AddComponent<SummonController>();
            }
        }
        
        // 初始化召唤物
        summon.gameObject.SetActive(false);
        return summon;
    }
    
    /// <summary>
    /// 召唤召唤物
    /// </summary>
    /// <param name="data">召唤物数据</param>
    /// <param name="position">召唤位置</param>
    /// <param name="summoner">召唤者</param>
    /// <returns>召唤物控制器</returns>
    public SummonController Summon(SummonData data, Vector3 position, CharacterBase summoner)
    {
        if (data == null || summoner == null)
        {
            Debug.LogError("[SummonManager] 召唤物数据或召唤者为空");
            return null;
        }
        
        // 获取或创建召唤物
        SummonController summon = GetSummon(data);
        if (summon == null)
        {
            Debug.LogError("[SummonManager] 无法获取召唤物");
            return null;
        }
        
        // 初始化召唤物
        summon.transform.position = position;
        summon.transform.rotation = Quaternion.identity;
        summon.Initialize(data, summoner);
        
        // 激活召唤物
        summon.gameObject.SetActive(true);
        
        // 添加到活跃列表
        activeSummons.Add(summon);
        
        Debug.Log($"[SummonManager] 成功召唤: {data.summonName}");
        return summon;
    }
    
    /// <summary>
    /// 回收召唤物到对象池
    /// </summary>
    /// <param name="summon">要回收的召唤物</param>
    public void ReturnSummon(SummonController summon)
    {
        if (summon == null || summon.SummonData == null)
        {
            Debug.LogError("[SummonManager] 要回收的召唤物或其数据为空");
            return;
        }
        
        // 从活跃列表中移除
        activeSummons.Remove(summon);
        
        // 禁用召唤物
        summon.gameObject.SetActive(false);
        
        // 如果对象池不存在该类型的召唤物，创建新的队列
        if (!summonPool.ContainsKey(summon.SummonData))
        {
            summonPool[summon.SummonData] = new Queue<SummonController>();
        }
        
        // 回收至对象池
        summonPool[summon.SummonData].Enqueue(summon);
        
        Debug.Log($"[SummonManager] 回收召唤物: {summon.SummonData.summonName}");
    }
    
    /// <summary>
    /// 查找召唤者的所有召唤物
    /// </summary>
    /// <param name="summoner">召唤者</param>
    /// <returns>召唤物列表</returns>
    public List<SummonController> FindSummonsBySummoner(CharacterBase summoner)
    {
        List<SummonController> result = new List<SummonController>();
        
        foreach (var summon in activeSummons)
        {
            if (summon != null && summon.Summoner == summoner)
            {
                result.Add(summon);
            }
        }
        
        return result;
    }
    
    /// <summary>
    /// 获取所有活跃召唤物
    /// </summary>
    /// <returns>活跃召唤物列表</returns>
    public List<SummonController> GetActiveSummons()
    {
        return new List<SummonController>(activeSummons);
    }
    
    private void Update()
    {
        // 检查活跃召唤物的生命周期
        for (int i = activeSummons.Count - 1; i >= 0; i--)
        {
            var summon = activeSummons[i];
            if (summon == null || summon.gameObject == null)
            {
                // 移除无效的召唤物
                activeSummons.RemoveAt(i);
                continue;
            }
            
            // 检查生命周期是否结束
            if (summon.IsLifetimeEnded)
            {
                ReturnSummon(summon);
            }
        }
    }
    
    /// <summary>
    /// 清理所有召唤物
    /// </summary>
    public void ClearAllSummons()
    {
        for (int i = activeSummons.Count - 1; i >= 0; i--)
        {
            ReturnSummon(activeSummons[i]);
        }
        
        // 清空对象池
        foreach (var queue in summonPool.Values)
        {
            queue.Clear();
        }
        summonPool.Clear();
        
        Debug.Log("[SummonManager] 已清理所有召唤物");
    }
}