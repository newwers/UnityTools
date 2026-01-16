using UnityEngine;

/// <summary>
/// 召唤系统测试脚本
/// 用于测试召唤系统的基本功能
/// </summary>
public class SummonSystemTest : MonoBehaviour
{
    [Header("测试配置")]
    [Tooltip("测试用召唤物数据")]
    public SummonData testSummonData;

    [Tooltip("测试用召唤者")]
    public CharacterBase testSummoner;

    [Tooltip("召唤位置偏移")]
    public Vector3 summonOffset = new Vector3(2f, 0f, 0f);

    [Tooltip("测试召唤数量")]
    public int testSummonCount = 1;

    [Tooltip("是否自动测试")]
    public bool autoTest = true;

    [Tooltip("自动测试延迟（秒）")]
    public float autoTestDelay = 3f;

    private float testTimer = 0f;
    private bool testExecuted = false;

    private void Update()
    {
        if (autoTest && !testExecuted)
        {
            testTimer += Time.deltaTime;
            if (testTimer >= autoTestDelay)
            {
                ExecuteTest();
                testExecuted = true;
            }
        }
    }

    /// <summary>
    /// 执行测试
    /// </summary>
    [ContextMenu("Execute Test")]
    public void ExecuteTest()
    {
        Debug.Log("[SummonSystemTest] 开始测试召唤系统");

        if (testSummonData == null)
        {
            Debug.LogError("[SummonSystemTest] 测试失败：未配置召唤物数据");
            return;
        }

        if (testSummoner == null)
        {
            Debug.LogError("[SummonSystemTest] 测试失败：未配置召唤者");
            return;
        }

        // 计算召唤位置
        Vector3 summonPosition = testSummoner.transform.position + summonOffset;

        // 测试SummonManager单例是否可用
        if (SummonManager.Instance == null)
        {
            Debug.LogError("[SummonSystemTest] 测试失败：SummonManager单例未初始化");
            return;
        }

        Debug.Log($"[SummonSystemTest] 测试参数：");
        Debug.Log($"  召唤物：{testSummonData.summonName}");
        Debug.Log($"  召唤者：{testSummoner.name}");
        Debug.Log($"  召唤位置：{summonPosition}");
        Debug.Log($"  召唤数量：{testSummonCount}");

        // 测试召唤功能
        SummonController summon = SummonManager.Instance.Summon(testSummonData, summonPosition, testSummoner);

        if (summon != null)
        {
            Debug.Log("[SummonSystemTest] 测试成功：召唤物创建成功");

            // 测试召唤物属性
            Debug.Log($"  召唤物名称：{summon.gameObject.name}");
            Debug.Log($"  召唤物生命值：{summon.PlayerAttributes.characterAtttibute.currentHealth}/{summon.PlayerAttributes.characterAtttibute.maxHealth}");
            Debug.Log($"  召唤物攻击力：{summon.PlayerAttributes.characterAtttibute.baseAttackDamage}");
            Debug.Log($"  召唤物防御力：{summon.PlayerAttributes.characterAtttibute.baseDefense}");

            // 测试召唤物状态
            Debug.Log($"  召唤物是否存活：{!summon.IsDead}");
            Debug.Log($"  召唤物是否在活跃列表中：{SummonManager.Instance.GetActiveSummons().Contains(summon)}");

            // 延迟测试回收功能
            StartCoroutine(TestSummonReturn(summon, 5f));
        }
        else
        {
            Debug.LogError("[SummonSystemTest] 测试失败：无法创建召唤物");
        }
    }

    /// <summary>
    /// 测试召唤物回收
    /// </summary>
    /// <param name="summon">要测试的召唤物</param>
    /// <param name="delay">延迟时间（秒）</param>
    private System.Collections.IEnumerator TestSummonReturn(SummonController summon, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (summon != null)
        {
            Debug.Log("[SummonSystemTest] 测试回收召唤物");
            SummonManager.Instance.ReturnSummon(summon);

            // 检查召唤物是否已从活跃列表中移除
            yield return null; // 下一帧检查

            bool isStillActive = SummonManager.Instance.GetActiveSummons().Contains(summon);
            Debug.Log($"  召唤物是否已从活跃列表移除：{!isStillActive}");
            Debug.Log($"  召唤物是否已禁用：{!summon.gameObject.activeSelf}");

            if (!isStillActive && !summon.gameObject.activeSelf)
            {
                Debug.Log("[SummonSystemTest] 测试成功：召唤物回收成功");
            }
            else
            {
                Debug.LogError("[SummonSystemTest] 测试失败：召唤物回收失败");
            }
        }

        // 测试完成
        Debug.Log("[SummonSystemTest] 所有测试完成");
    }

    /// <summary>
    /// 测试对象池功能
    /// </summary>
    [ContextMenu("Test Object Pool")]
    public void TestObjectPool()
    {
        if (testSummonData == null || testSummoner == null)
        {
            Debug.LogError("[SummonSystemTest] 测试对象池失败：参数无效");
            return;
        }

        Debug.Log("[SummonSystemTest] 开始测试对象池");

        Vector3 summonPosition = testSummoner.transform.position + summonOffset;

        // 第一次召唤
        SummonController summon1 = SummonManager.Instance.Summon(testSummonData, summonPosition, testSummoner);
        GameObject instance1 = summon1 != null ? summon1.gameObject : null;
        Debug.Log($"  第一次召唤：{instance1?.name}");

        // 回收召唤物
        if (summon1 != null)
        {
            SummonManager.Instance.ReturnSummon(summon1);
        }

        // 第二次召唤，应该使用对象池中的实例
        SummonController summon2 = SummonManager.Instance.Summon(testSummonData, summonPosition, testSummoner);
        GameObject instance2 = summon2 != null ? summon2.gameObject : null;
        Debug.Log($"  第二次召唤：{instance2?.name}");

        // 检查是否是同一个实例
        if (instance1 != null && instance2 != null && instance1 == instance2)
        {
            Debug.Log("[SummonSystemTest] 测试成功：对象池正常工作，两次召唤使用了同一个实例");
        }
        else
        {
            Debug.LogWarning("[SummonSystemTest] 测试警告：两次召唤使用了不同的实例，可能是对象池未正确工作");
        }

        // 清理测试数据
        if (summon2 != null)
        {
            SummonManager.Instance.ReturnSummon(summon2);
        }

        Debug.Log("[SummonSystemTest] 对象池测试完成");
    }

    /// <summary>
    /// 测试召唤物生命周期
    /// </summary>
    [ContextMenu("Test Lifetime")]
    public void TestLifetime()
    {
        if (testSummonData == null || testSummoner == null)
        {
            Debug.LogError("[SummonSystemTest] 测试生命周期失败：参数无效");
            return;
        }

        Debug.Log("[SummonSystemTest] 开始测试召唤物生命周期");

        // 保存原始生命周期
        float originalLifetime = testSummonData.lifetime;

        // 设置短生命周期用于测试
        testSummonData.lifetime = 3f;

        Vector3 summonPosition = testSummoner.transform.position + summonOffset;
        SummonController summon = SummonManager.Instance.Summon(testSummonData, summonPosition, testSummoner);

        if (summon != null)
        {
            Debug.Log($"  召唤物生命周期设置：{testSummonData.lifetime}秒");
            Debug.Log("  等待召唤物生命周期结束...");

            // 延迟检查召唤物是否自动回收
            StartCoroutine(CheckLifetimeEnd(summon, testSummonData.lifetime + 1f));
        }

        // 恢复原始生命周期
        testSummonData.lifetime = originalLifetime;
    }

    /// <summary>
    /// 检查召唤物生命周期是否结束
    /// </summary>
    /// <param name="summon">要检查的召唤物</param>
    /// <param name="delay">延迟时间（秒）</param>
    private System.Collections.IEnumerator CheckLifetimeEnd(SummonController summon, float delay)
    {
        yield return new WaitForSeconds(delay);

        bool isStillActive = SummonManager.Instance.GetActiveSummons().Contains(summon);
        Debug.Log($"  召唤物是否已自动回收：{!isStillActive}");

        if (!isStillActive)
        {
            Debug.Log("[SummonSystemTest] 测试成功：召唤物生命周期正常结束");
        }
        else
        {
            Debug.LogError("[SummonSystemTest] 测试失败：召唤物生命周期未正常结束");

            // 手动清理
            if (summon != null)
            {
                SummonManager.Instance.ReturnSummon(summon);
            }
        }

        Debug.Log("[SummonSystemTest] 生命周期测试完成");
    }
}
