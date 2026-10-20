using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 枚举桌面上可见的顶层窗口，在窗口顶部/底部创建实体 BoxCollider2D 作为地板，
/// 左右两侧创建触发器 BoxCollider2D（厚度参数可调），
/// 定时轮询同步窗口的移动、关闭、新开。
/// </summary>
public class WindowColliderManager : BaseMonoSingleClass<WindowColliderManager>
{
    [Header("开关")]
    [Tooltip("是否启用窗口碰撞器")]
    public bool enableWindowColliders = true;
    [Tooltip("编辑器模式下是否运行（Game视图坐标与屏幕坐标不对应，默认关闭）")]
    public bool runInEditor = false;

    [Tooltip("是否忽略最大化和全屏窗口")]
    public bool ignoreMaximizedOrFullscreenWindows = false;

    [Header("轮询")]
    [Tooltip("窗口枚举轮询间隔（秒）")]
    public float pollInterval = 0.5f;
    [Tooltip("相机移动/缩放触发的最快重算间隔（秒）")]
    public float minRefreshInterval = 0.1f;

    [Header("过滤")]
    [Tooltip("忽略宽度小于该值(物理像素)的窗口")]
    public float minWindowWidth = 60f;

    [Header("碰撞器设置")]
    [Tooltip("窗口碰撞器所在层级名称（需与动物碰撞矩阵放行）")]
    public string colliderLayerName = "Ground";
    [Tooltip("边条碰撞器厚度（世界单位）：顶/底碰撞器的高度与左右触发器的宽度共用此参数")]
    public float edgeThickness = 0.3f;
    [Tooltip("平台效果器只对该层生效（动物可站上窗口边缘并从下方穿过）")]
    public string animalLayerName = "Animal";

    private readonly Dictionary<IntPtr, WindowColliderParts> activeColliders = new Dictionary<IntPtr, WindowColliderParts>();
    private float pollTimer;
    private int lastWindowCount = -1;
    private int colliderLayer = -1;
    private bool initialized;

    private Camera mainCamera;
    private Vector3 lastCameraPos;
    private float lastCameraOrthoSize;
    private Transform colliderRoot;

    /// <summary>
    /// 由 GameManager 显式触发创建并注入主相机
    /// </summary>
    public void Initialize(Camera mainCamera)
    {
        this.mainCamera = mainCamera;
    }

    private void Start()
    {
#if UNITY_EDITOR
        if (!runInEditor) return;
#endif
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        // 场景根节点，保持恒等变换，使 BoxCollider2D.offset 可直接使用世界坐标
        colliderRoot = new GameObject("WindowColliderRoot").transform;

        if (!TryResolveColliderLayer())
        {
            enabled = false;
            return;
        }

        InjectIgnoreLayer();

        initialized = true;
        lastCameraPos = mainCamera != null ? mainCamera.transform.position : Vector3.zero;
        lastCameraOrthoSize = mainCamera != null ? mainCamera.orthographicSize : 0f;

        // 立即执行一次
        RefreshColliders();
    }

    private bool TryResolveColliderLayer()
    {
        colliderLayer = LayerMask.NameToLayer(colliderLayerName);
        if (colliderLayer < 0)
        {
            LogManager.LogError($"[WindowColliderManager] 找不到层级 {colliderLayerName}，窗口碰撞器功能已禁用");
            return false;
        }
        return true;
    }

    /// <summary>
    /// 将碰撞器层级注入 TranspareWindows.ignoreLayers，确保鼠标在该区域仍可点击穿透到真实窗口
    /// </summary>
    private void InjectIgnoreLayer()
    {
        TranspareWindows transpareWindows = FindFirstObjectByType<TranspareWindows>();
        if (transpareWindows == null)
        {
            LogManager.LogWarning("[WindowColliderManager] 未找到 TranspareWindows，跳过鼠标穿透层级注入");
            return;
        }

        int layerMask = 1 << colliderLayer;
        if ((transpareWindows.ignoreLayers.value & layerMask) == 0)
        {
            transpareWindows.ignoreLayers.value |= layerMask;
            LogManager.Log($"[WindowColliderManager] 已将层级 {colliderLayerName} 注入鼠标穿透忽略列表");
        }
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (!runInEditor) return;
#endif
        if (!initialized) return;

        if (!enableWindowColliders)
        {
            if (activeColliders.Count > 0)
            {
                ClearAllColliders();
            }
            return;
        }

        // 相机被拖动/缩放时提前触发重算，避免碰撞器错位（限频）
        if (mainCamera != null &&
            (mainCamera.transform.position != lastCameraPos ||
             mainCamera.orthographicSize != lastCameraOrthoSize))
        {
            lastCameraPos = mainCamera.transform.position;
            lastCameraOrthoSize = mainCamera.orthographicSize;
            pollTimer = Mathf.Max(pollTimer, pollInterval - minRefreshInterval);
        }

        pollTimer += Time.unscaledDeltaTime;
        if (pollTimer < pollInterval) return;
        pollTimer = 0f;

        RefreshColliders();
    }

    private void RefreshColliders()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null) return;
        }

        List<WindowInfo> windows = WindowEnumerator.GetVisibleWindows(WindowEnumerator.GetSelfWindowHandle());

        if (windows.Count != lastWindowCount)
        {
            lastWindowCount = windows.Count;
            LogManager.Log($"[WindowColliderManager] 当前可见窗口数量: {windows.Count}");
        }

        // 移除已消失或不再符合条件的窗口碰撞器
        List<IntPtr> removedHandles = null;
        foreach (KeyValuePair<IntPtr, WindowColliderParts> kvp in activeColliders)
        {
            bool keep = false;
            for (int i = 0; i < windows.Count; i++)
            {
                if (windows[i].hWnd == kvp.Key)
                {
                    keep = IsWindowAllowed(windows[i]);
                    break;
                }
            }
            if (!keep)
            {
                (removedHandles ??= new List<IntPtr>()).Add(kvp.Key);
            }
        }
        if (removedHandles != null)
        {
            foreach (IntPtr hwnd in removedHandles)
            {
                if (activeColliders[hwnd] != null && activeColliders[hwnd].gameObject != null)
                {
                    Destroy(activeColliders[hwnd].gameObject);
                }
                activeColliders.Remove(hwnd);
            }
        }

        // 创建/更新碰撞器
        for (int i = 0; i < windows.Count; i++)
        {
            WindowInfo window = windows[i];
            if (!IsWindowAllowed(window)) continue;

            if (activeColliders.TryGetValue(window.hWnd, out WindowColliderParts parts))
            {
                if (parts == null || parts.gameObject == null)
                {
                    parts = CreateColliderParts(window);
                    activeColliders[window.hWnd] = parts;
                }
            }
            else
            {
                parts = CreateColliderParts(window);
                activeColliders[window.hWnd] = parts;
            }

            UpdateCollider(parts, window);
        }
    }

    /// <summary>
    /// 窗口是否允许生成碰撞器（宽度过滤、最大化/全屏开关）
    /// </summary>
    private bool IsWindowAllowed(WindowInfo window)
    {
        if (window.screenRect.width < minWindowWidth) return false;
        if (ignoreMaximizedOrFullscreenWindows && window.isFullscreen) return false;
        return true;
    }

    /// <summary>
    /// 单个窗口的碰撞器组：顶/底实体碰撞器（带平台效果器）+ 左右触发器
    /// </summary>
    private class WindowColliderParts
    {
        public GameObject gameObject;
        public BoxCollider2D top;
        public BoxCollider2D bottom;
        public BoxCollider2D leftTrigger;
        public BoxCollider2D rightTrigger;
    }

    private WindowColliderParts CreateColliderParts(WindowInfo window)
    {
        string objectName = string.IsNullOrEmpty(window.title)
            ? $"WindowCollider_{window.hWnd}"
            : $"WindowCollider_{window.title}";
        GameObject colliderObj = new GameObject(objectName);
        colliderObj.layer = colliderLayer;
        colliderObj.transform.SetParent(colliderRoot, false);

        WindowColliderParts parts = new WindowColliderParts
        {
            gameObject = colliderObj,
            top = CreateChildCollider(colliderObj, "Top", isTrigger: false, useByEffect: true),
            bottom = CreateChildCollider(colliderObj, "Bottom", isTrigger: false, useByEffect: true),
            leftTrigger = CreateChildCollider(colliderObj, "Left", isTrigger: true, useByEffect: false),
            rightTrigger = CreateChildCollider(colliderObj, "Right", isTrigger: true, useByEffect: false)
        };
        return parts;
    }

    /// <summary>
    /// 创建单个边条子物体（子物体位置固定在原点，用 collider.offset 表达世界坐标）
    /// </summary>
    private BoxCollider2D CreateChildCollider(GameObject parent, string childName, bool isTrigger, bool useByEffect)
    {
        GameObject child = new GameObject(childName);
        child.layer = colliderLayer;
        child.transform.SetParent(parent.transform, false);

        BoxCollider2D box = child.AddComponent<BoxCollider2D>();
        box.isTrigger = isTrigger;
        box.usedByEffector = useByEffect;

        if (!isTrigger)
        {
            // 平台效果器：仅对 Animal 层生效，动物可站上表面、从下方穿过
            int animalLayer = LayerMask.NameToLayer(animalLayerName);
            if (animalLayer >= 0)
            {
                PlatformEffector2D effector = child.AddComponent<PlatformEffector2D>();
                effector.colliderMask = 1 << animalLayer;
                effector.surfaceArc = 135f;
            }
            else
            {
                LogManager.LogError($"[WindowColliderManager] 找不到层级 {animalLayerName}，PlatformEffector2D 未生效");
            }
        }
        return box;
    }

    private void UpdateCollider(WindowColliderParts parts, WindowInfo window)
    {
        if (parts.gameObject == null) return;

        Rect worldRect = ScreenRectToWorldRect(window.screenRect);

        // GameObject 固定在原点，用 offset 表达世界坐标
        float thickness = Mathf.Max(0.01f, edgeThickness);
        float centerX = worldRect.center.x;
        float centerY = worldRect.center.y;
        float width = worldRect.width;
        float height = worldRect.height;

        // 顶部（实体地板）：贴窗口顶边向内延伸
        parts.top.offset = new Vector2(centerX, worldRect.yMax - thickness * 0.5f);
        parts.top.size = new Vector2(width, thickness);
        // 底部（实体地板）：贴窗口底边向内延伸
        parts.bottom.offset = new Vector2(centerX, worldRect.yMin + thickness * 0.5f);
        parts.bottom.size = new Vector2(width, thickness);
        // 左侧（触发器）：贴窗口左边向内延伸
        parts.leftTrigger.offset = new Vector2(worldRect.xMin + thickness * 0.5f, centerY);
        parts.leftTrigger.size = new Vector2(thickness, height);
        // 右侧（触发器）：贴窗口右边向内延伸
        parts.rightTrigger.offset = new Vector2(worldRect.xMax - thickness * 0.5f, centerY);
        parts.rightTrigger.size = new Vector2(thickness, height);
    }

    /// <summary>
    /// Windows 屏幕矩形（物理像素，Y向下）→ 世界坐标矩形
    /// </summary>
    private Rect ScreenRectToWorldRect(RectInt screenRect)
    {
        // 物理像素 → Unity Screen 坐标比例（全屏/壁纸模式=1；DPI缩放时自动校正）
        float scaleX = Screen.width / (float)Display.main.systemWidth;
        float scaleY = Screen.height / (float)Display.main.systemHeight;
        // 正交相机：z 取相机位置的负值，使转换结果落在 z=0 平面
        float camZ = -mainCamera.transform.position.z;

        // Windows Y轴向下 → Unity Y轴向上；先转成 Screen 坐标（原点左下）再转世界坐标
        Vector3 screenTopLeft = new Vector3(
            screenRect.x * scaleX,
            Screen.height - (screenRect.y + screenRect.height) * scaleY,
            camZ);
        Vector3 screenBottomRight = new Vector3(
            (screenRect.x + screenRect.width) * scaleX,
            Screen.height - screenRect.y * scaleY,
            camZ);

        Vector3 worldTopLeft = mainCamera.ScreenToWorldPoint(screenTopLeft);
        Vector3 worldBottomRight = mainCamera.ScreenToWorldPoint(screenBottomRight);

        float minX = Mathf.Min(worldTopLeft.x, worldBottomRight.x);
        float maxX = Mathf.Max(worldTopLeft.x, worldBottomRight.x);
        float minY = Mathf.Min(worldTopLeft.y, worldBottomRight.y);
        float maxY = Mathf.Max(worldTopLeft.y, worldBottomRight.y);

        return Rect.MinMaxRect(minX, minY, maxX, maxY);
    }

    private void ClearAllColliders()
    {
        foreach (KeyValuePair<IntPtr, WindowColliderParts> kvp in activeColliders)
        {
            if (kvp.Value != null && kvp.Value.gameObject != null)
            {
                Destroy(kvp.Value.gameObject);
            }
        }
        activeColliders.Clear();
        lastWindowCount = -1;
    }
}
