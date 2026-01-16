using UnityEngine;

/// <summary>
/// 召唤效果
/// 负责处理召唤物的创建和初始化
/// </summary>
public class SummonEffect : MonoBehaviour
{
    /// <summary>
    /// 召唤召唤物
    /// </summary>
    /// <param name="summonData">召唤物数据</param>
    /// <param name="summoner">召唤者</param>
    /// <param name="position">召唤位置</param>
    /// <param name="count">召唤数量</param>
    /// <returns>成功召唤的召唤物列表</returns>
    public static SummonController Summon(SummonData summonData, CharacterBase summoner, Vector3 position, int count = 1)
    {
        if (summonData == null || summoner == null)
        {
            Debug.LogError("[SummonEffect] 召唤参数无效");
            return null;
        }
        
        if (count <= 0)
        {
            count = 1;
        }
        
        // 调用SummonManager召唤召唤物
        SummonController summon = SummonManager.Instance.Summon(summonData, position, summoner);
        
        if (summon != null)
        {
            Debug.Log($"[SummonEffect] 成功召唤: {summonData.summonName}");
            
            // 播放召唤特效和音效
            PlaySummonEffects(summonData, position);
        }
        
        return summon;
    }
    
    /// <summary>
    /// 召唤多个召唤物
    /// </summary>
    /// <param name="summonData">召唤物数据</param>
    /// <param name="summoner">召唤者</param>
    /// <param name="position">召唤位置</param>
    /// <param name="count">召唤数量</param>
    /// <returns>成功召唤的召唤物列表</returns>
    public static SummonController[] SummonMultiple(SummonData summonData, CharacterBase summoner, Vector3 position, int count = 1)
    {
        if (summonData == null || summoner == null || count <= 0)
        {
            Debug.LogError("[SummonEffect] 召唤参数无效");
            return new SummonController[0];
        }
        
        SummonController[] summons = new SummonController[count];
        
        for (int i = 0; i < count; i++)
        {
            // 计算每个召唤物的位置偏移，避免重叠
            Vector3 offset = new Vector3(
                Random.Range(-1f, 1f),
                Random.Range(-0.5f, 0.5f),
                0
            );
            
            summons[i] = Summon(summonData, summoner, position + offset, 1);
        }
        
        return summons;
    }
    
    /// <summary>
    /// 播放召唤特效和音效
    /// </summary>
    /// <param name="summonData">召唤物数据</param>
    /// <param name="position">召唤位置</param>
    private static void PlaySummonEffects(SummonData summonData, Vector3 position)
    {
        if (summonData == null)
        {
            return;
        }
        
        // 播放召唤特效
        if (summonData.summonEffectPrefab != null)
        {
            GameObject effect = Instantiate(summonData.summonEffectPrefab, position, Quaternion.identity);
            // 自动销毁特效
            Destroy(effect, 2f);
        }
        
        // 播放召唤音效
        if (summonData.summonSound != null)
        {
            AudioSource.PlayClipAtPoint(summonData.summonSound, position);
        }
    }
    
    /// <summary>
    /// 通过技能数据召唤召唤物
    /// </summary>
    /// <param name="skillData">技能数据</param>
    /// <param name="summoner">召唤者</param>
    /// <param name="position">召唤位置</param>
    /// <returns>成功召唤的召唤物列表</returns>
    public static SummonController SummonBySkill(SkillData skillData, CharacterBase summoner, Vector3 position)
    {
        if (skillData == null || summoner == null)
        {
            Debug.LogError("[SummonEffect] 技能召唤参数无效");
            return null;
        }
        
        // 这里可以通过skillData的名称或其他属性查找对应的SummonData
        // 实际项目中应该有更可靠的映射机制
        
        // 暂时返回null，实际项目中需要实现正确的映射逻辑
        return null;
    }
}
