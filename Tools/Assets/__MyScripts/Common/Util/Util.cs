using UnityEngine;

public static class Util
{
    public static void ApplyShakeEffect(float progress, float maxShakeIntensity, float shakeFrequency, Transform transform, Vector3 originalPosition)
    {
        // 计算当前震动强度（随进度线性增加）
        float shakeIntensity = maxShakeIntensity * progress;

        // 使用Perlin噪声生成平滑的随机震动
        float seed = Time.time * shakeFrequency;
        float xShake = (Mathf.PerlinNoise(seed, 0) - 0.5f) * 2f * shakeIntensity;
        float yShake = (Mathf.PerlinNoise(0, seed) - 0.5f) * 2f * shakeIntensity;

        // 应用震动偏移
        transform.localPosition = originalPosition + new Vector3(xShake, yShake, 0);
    }

    public static void IgnoreCollision(Collider2D selfCollider, Collider2D otherCollider, bool isIgnore)
    {
        Physics2D.IgnoreCollision(selfCollider, otherCollider, isIgnore);
    }

    public static void SpawnCoins(int count, bool isCrit, float coinDropCritMultiplier, Gold prefab, Vector3 pos)
    {
        int gold = count / 100;
        int Silver = count / 10 % 10;
        int Copper = count % 10;

        if (isCrit)
        {
            gold = Mathf.RoundToInt((gold * coinDropCritMultiplier));
            Silver = Mathf.RoundToInt((Silver * coinDropCritMultiplier));
            Copper = Mathf.RoundToInt((Copper * coinDropCritMultiplier));
        }

        int totalCount = gold + Silver + Copper;

        for (int i = 0; i < gold; i++)
        {
            var g = GameManager.Instance.SceneRoot.GetGold(prefab);

            if (g)
            {
                g.transform.position = pos;
                g.Init(EMedalType.Gold);
                GameManager.Instance.OnSpawnGold(g);
            }
            Util.SpawnCoin(i, totalCount, GameManager.Instance.gameDatabase.balloonConfig.spawnGoldForceRange, g.gameObject);
        }

        for (int i = 0; i < Silver; i++)
        {
            var g = GameManager.Instance.SceneRoot.GetGold(prefab);
            if (g)
            {
                g.transform.position = pos;
                g.Init(EMedalType.Silver);
                GameManager.Instance.OnSpawnGold(g);
            }
            Util.SpawnCoin(i + gold, totalCount, GameManager.Instance.gameDatabase.balloonConfig.spawnGoldForceRange, g.gameObject);
        }

        for (int i = 0; i < Copper; i++)
        {
            var g = GameManager.Instance.SceneRoot.GetGold(prefab);
            if (g)
            {
                g.transform.position = pos;
                g.Init(EMedalType.Copper);
                GameManager.Instance.OnSpawnGold(g);
            }
            Util.SpawnCoin(i + gold + Silver, totalCount, GameManager.Instance.gameDatabase.balloonConfig.spawnGoldForceRange, g.gameObject);
        }
    }

    /// <summary>
    /// 生成单个金币并施加爆炸力
    /// </summary>
    public static void SpawnCoin(int index, int spawnCount, Vector2 force, GameObject coin)
    {
        coin.SetActive(true);

        // 获取刚体组件
        Rigidbody2D rb = coin.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogWarning("金币预制体缺少Rigidbody2D组件!");
            return;
        }

        // 计算爆炸方向
        float angle = (index * 360f / spawnCount) + Random.Range(-15f, 15f);
        angle *= Mathf.Deg2Rad;

        // 计算爆炸力
        float forceMagnitude = Random.Range(force.x, force.y);
        Vector2 forceDirection = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

        // 添加向上的偏向力
        forceDirection.y += 1;
        forceDirection.Normalize();

        // 施加力
        rb.AddForce(forceDirection * forceMagnitude, ForceMode2D.Impulse);


    }

    public static void ApplyThrowForce(GameObject obj, Vector2 forceRange)
    {
        Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            Vector2 direction = GetRandomDirectionIn2D60DegreeFan(new Vector2(-15, 0), new Vector2(0, 15f));
            float force = UnityEngine.Random.Range(forceRange.x, forceRange.y); // 可调范围
            rb.AddForce(direction * force, ForceMode2D.Impulse);
        }
    }

    public static void ApplyBalloonMachineThrowForce(GameObject obj, Vector2 forceRange, Vector2 baseDirection)
    {
        Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // 生成-15到15度之间的随机角度（角度范围可根据需要调整）
            float randomAngle = UnityEngine.Random.Range(-15f, 15f);

            // 将角度转换为弧度（Unity的三角函数使用弧度）
            float angleInRadians = randomAngle * Mathf.Deg2Rad;

            // 计算旋转后的方向向量（2D向量旋转公式）
            Vector2 direction = new Vector2(
                baseDirection.x * Mathf.Cos(angleInRadians) - baseDirection.y * Mathf.Sin(angleInRadians),
                baseDirection.x * Mathf.Sin(angleInRadians) + baseDirection.y * Mathf.Cos(angleInRadians)
            );

            // 力的大小在指定范围内随机
            float force = UnityEngine.Random.Range(forceRange.x, forceRange.y);

            // 沿物体up方向施加力
            rb.AddForce(direction * force, ForceMode2D.Impulse);
        }
    }


    public static Vector2 GetRandomDirectionIn2D60DegreeFan(Vector2 leftAngle, Vector2 rightAngle)
    {
        // 将角度范围分为两部分：-30到-10度和10到30度
        float angle;
        if (UnityEngine.Random.value < 0.5f)
        {
            // 生成-30到-10度之间的角度
            angle = UnityEngine.Random.Range(leftAngle.x, leftAngle.y);
        }
        else
        {
            // 生成10到30度之间的角度
            angle = UnityEngine.Random.Range(rightAngle.x, rightAngle.y);
        }

        float angleRad = angle * Mathf.Deg2Rad;

        // 以 Vector2.up 为中心向量进行偏转
        Vector2 direction = new Vector2(Mathf.Sin(angleRad), Mathf.Cos(angleRad));
        return direction.normalized;
    }

}
