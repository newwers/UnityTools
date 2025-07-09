using UnityEngine;

public class SceneRoot : MonoBehaviour
{
    public Transform Root; // 场景根节点
    public Transform BalloonRoot;
    public Transform ItemRoot;
    public Transform GoldRoot;
    public Transform GiftRoot;
    public Transform EffectRoot;

    SimpleGameObjectPool<BalloonController> Balloon_NormalPool;
    SimpleGameObjectPool<BalloonController> Balloon_BigPool;
    SimpleGameObjectPool<ItemController> ItemPool;
    SimpleGameObjectPool<Gold> GoldPool;
    SimpleGameObjectPool<GiftBox> GiftPool;
    SimpleGameObjectPool<GameObject> EffectPool;


    public BalloonController GetBalloon(BalloonController prefab)
    {
        if (Balloon_NormalPool == null)
        {
            Balloon_NormalPool = new SimpleGameObjectPool<BalloonController>(prefab, BalloonRoot);
        }
        return Balloon_NormalPool.GetObject();
    }

    public void ReturnBalloon(BalloonController balloonController)
    {
        if (Balloon_NormalPool == null)
        {
            Balloon_NormalPool = new SimpleGameObjectPool<BalloonController>(balloonController, BalloonRoot);
        }

        Balloon_NormalPool.ReturnObject(balloonController);
    }

    public BalloonController GetBigBalloon(BalloonController prefab)
    {
        if (Balloon_BigPool == null)
        {
            Balloon_BigPool = new SimpleGameObjectPool<BalloonController>(prefab, BalloonRoot);
        }
        return Balloon_BigPool.GetObject();
    }

    public void ReturnBigBalloon(BalloonController balloonController)
    {
        if (Balloon_BigPool == null)
        {
            Balloon_BigPool = new SimpleGameObjectPool<BalloonController>(balloonController, BalloonRoot);
        }

        Balloon_BigPool.ReturnObject(balloonController);
    }

    public ItemController GetItem(ItemController prefab)
    {
        if (ItemPool == null)
        {
            ItemPool = new SimpleGameObjectPool<ItemController>(prefab, ItemRoot);
        }
        return ItemPool.GetObject();
    }

    public void ReturnItem(ItemController instance)
    {
        if (ItemPool == null)
        {
            ItemPool = new SimpleGameObjectPool<ItemController>(instance, ItemRoot);
        }

        ItemPool.ReturnObject(instance);
    }

    public Gold GetGold(Gold prefab)
    {
        if (GoldPool == null)
        {
            GoldPool = new SimpleGameObjectPool<Gold>(prefab, GoldRoot);
        }
        return GoldPool.GetObject();
    }

    public void ReturnGold(Gold instance)
    {
        if (GoldPool == null)
        {
            GoldPool = new SimpleGameObjectPool<Gold>(instance, GoldRoot);
        }

        GoldPool.ReturnObject(instance);
    }

    public GiftBox GetGiftBox(GiftBox prefab)
    {
        if (GiftPool == null)
        {
            GiftPool = new SimpleGameObjectPool<GiftBox>(prefab, GiftRoot);
        }
        return GiftPool.GetObject();
    }

    public void ReturnGiftBox(GiftBox instance)
    {
        if (GiftPool == null)
        {
            GiftPool = new SimpleGameObjectPool<GiftBox>(instance, GiftRoot);
        }

        GiftPool.ReturnObject(instance);
    }

    public GameObject GetEffect(GameObject prefab)
    {
        if (EffectPool == null)
        {
            EffectPool = new SimpleGameObjectPool<GameObject>(prefab, EffectRoot);
        }
        return EffectPool.GetObject();
    }

    public void ReturnEffect(GameObject instance)
    {
        if (EffectPool == null)
        {
            EffectPool = new SimpleGameObjectPool<GameObject>(instance, EffectRoot);
        }

        EffectPool.ReturnObject(instance);
    }
}
