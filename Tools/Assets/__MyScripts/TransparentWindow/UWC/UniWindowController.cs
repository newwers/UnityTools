/*
 * UniWindowController.cs
 * 
 * 作者: Kirurobo http://twitter.com/kirurobo
 * 许可证: MIT
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
#if UNITY_EDITOR

#endif
#if ENABLE_INPUT_SYSTEM
#endif

namespace Kirurobo
{
    /// @cond DOXYGEN_SHOW_INTERNAL_CLASSES

    /// <summary>
    /// 设置可编辑的bool属性
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class EditablePropertyAttribute : UnityEngine.PropertyAttribute { }

    /// <summary>
    /// 将属性设置为只读
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class ReadOnlyAttribute : UnityEngine.PropertyAttribute { }

    /// @endcond


    /// <summary>
    /// 用于 Windows / Mac 的统一窗口控制器
    /// </summary>
    public class UniWindowController : MonoBehaviour
    {
        /// <summary>
        /// 与 UniWinCore.TransparentType 相同
        /// </summary>
        public enum TransparentType : int
        {
            None = 0,
            Alpha = 1,
            ColorKey = 2,
        }

        /// <summary>
        /// 指定点击测试的方法（即切换点击穿透）
        /// </summary>
        public enum HitTestType : int
        {
            None = 0,
            Opacity = 1,
            Raycast = 2,
        }

        /// <summary>
        /// 标识 <see cref="OnStateChanged">OnStateChanged</see> 事件发生时的类型
        /// </summary>
        [Flags]
        public enum WindowStateEventType : int
        {
            None = 0,
            StyleChanged = 1,
            Resized = 2,

            // 以下可能会有规格变更
            TopMostEnabled = 16 + 1 + 8,
            TopMostDisabled = 16 + 1,
            BottomMostEnabled = 32 + 1 + 8,
            BottomMostDisabled = 32 + 1,
            WallpaperModeEnabled = 64 + 1 + 8,
            WallpaperModeDisabled = 64 + 1,
        };

        /// <summary>
        /// 鼠标按钮
        /// </summary>
        [Flags]
        public enum MouseButton : int
        {
            None = 0,
            Left = 1,
            Right = 2,
            Middle = 4,
        }

        /// <summary>
        /// 修饰键
        /// </summary>
        [Flags]
        public enum ModifierKey : int
        {
            None = 0,
            Alt = 1,
            Control = 2,
            Shift = 4,
            Command = 8,
        }

        /// <summary>
        /// 获取当前 UniWindowController 实例
        /// </summary>
        public static UniWindowController current => _current ? _current : FindOrCreateInstance();
        private static UniWindowController _current;

        /// <summary>
        /// 底层类
        /// </summary>
        private UniWinCore _uniWinCore = null;

        /// <summary>
        /// 此窗口是否接收鼠标事件
        /// </summary>
        public bool isClickThrough
        {
            get { return _isClickThrough; }
            set { SetClickThrough(value); }
        }
        private bool _isClickThrough = false;

        /// <summary>
        /// 此窗口是否透明
        /// </summary>
        public bool isTransparent
        {
            get { return _isTransparent; }
            set { SetTransparent(value); }
        }
        [SerializeField, EditableProperty, Tooltip("Check to set transparent on startup")]
        private bool _isTransparent = false;

        /// <summary>
        /// 窗口透明度（0.0 到 1.0）
        /// </summary>
        public float alphaValue
        {
            get { return _alphaValue; }
            set { SetAlphaValue(value); }
        }
        [SerializeField, EditableProperty, Tooltip("Window alpha"), Range(0f, 1f)]
        private float _alphaValue = 1.0f;

        /// <summary>
        /// 此窗口是否置顶
        /// </summary>
        public bool isTopmost
        {
            get { return ((_uniWinCore == null) ? _isTopmost : _isTopmost = _uniWinCore.IsTopmost); }
            set { SetTopmost(value); }
        }
        [SerializeField, EditableProperty, Tooltip("Check to set topmost on startup")]
        private bool _isTopmost = false;

        /// <summary>
        /// 此窗口是否置底
        /// </summary>
        public bool isBottommost
        {
            get { return ((_uniWinCore == null) ? _isBottommost : _isBottommost = _uniWinCore.IsBottommost); }
            set { SetBottommost(value); }
        }
        [SerializeField, EditableProperty, Tooltip("Check to set bottommost on startup")]
        private bool _isBottommost = false;

        /// <summary>
        /// 此窗口是否已最小化
        /// </summary>
        public bool isZoomed
        {
            get { return ((_uniWinCore == null) ? _isZoomed : _isZoomed = _uniWinCore.GetZoomed()); }
            set { SetZoomed(value); }
        }
        [SerializeField, EditableProperty, Tooltip("Check to set zoomed on startup")]
        private bool _isZoomed = false;

        /// <summary>
        /// 窗口是否适配显示器
        /// </summary>
        public bool shouldFitMonitor
        {
            get { return _shouldFitMonitor; }
            set { FitToMonitor(value, _monitorToFit); }
        }
        [SerializeField, EditableProperty, Tooltip("Check to fit the window to the monitor")]
        private bool _shouldFitMonitor = false;

        /// <summary>
        /// 要适配的目标显示器索引（0, 1, ...）
        /// </summary>
        public int monitorToFit
        {
            get { return _monitorToFit; }
            set { FitToMonitor(_shouldFitMonitor, value); }
        }
        private int _monitorToFit = 0;

        /// <summary>
        /// 启用/禁用接受文件拖放
        /// </summary>
        public bool allowDropFiles
        {
            get { return _allowDropFiles; }
            set { SetAllowDrop(value); }
        }
        [SerializeField, EditableProperty, Tooltip("Enable file or folder dropping")]
        private bool _allowDropFiles = false;

        /// <summary>
        /// 是否进行点击穿透自动判定
        /// 不进行的话可以自己修改 isClickThrough
        /// </summary>
        public bool isHitTestEnabled = true;

        /// <summary>
        /// 点击穿透自动判定的方法
        /// </summary>
        [Tooltip("Select the method")]
        public HitTestType hitTestType = HitTestType.Opacity;

        /// <summary>
        /// 点击穿透判定方法为不透明度时使用的阈值
        /// 光标下的Alpha值大于等于此值则判定为命中
        /// </summary>
        [Tooltip("Available on the hit test type is Opacity"), RangeAttribute(0f, 1f)]
        public float opacityThreshold = 0.1f;

        /// <summary>
        /// 点击穿透判定方法为 raycast 时的最远距离
        /// </summary>
        private float raycastMaxDepth = 100.0f;

        /// <summary>
        /// 如果设为true，窗口透明时会自动将相机背景改为纯黑色透明
        /// </summary>
        [Header("Advanced settings")]
        [Tooltip("Change camera background when the window is transparent")]
        public bool autoSwitchCameraBackground = true;

        /// <summary>
        /// 如果设为true，启动时如果是全屏状态会强制解除
        ///
        /// 这是为了在启动对话框中选择了全屏时也能切换到窗口模式
        /// 仅在启动时生效
        /// 在Mac的情况下，强制解除全屏状态后窗口仍会在另一个画面上，似乎效果不太好
        /// </summary>
        [Tooltip("Force windowed on startup")]
        public bool forceWindowed = false;

        /// <summary>
        /// 相机实例
        /// </summary>
        [Tooltip("Main camera is used if None")]
        public Camera currentCamera;

        /// <summary>
        /// 透传方式的指定
        /// </summary>
        [Header("For Windows only")]
        [Tooltip("Select the method. *Only available on Windows")]
        public TransparentType transparentType = TransparentType.Alpha;

        /// <summary>
        /// 透明类型为 ColorKey 时使用的键颜色
        /// </summary>
        [Tooltip("Will be used the next time the window becomes transparent")]
        public Color32 keyColor = new Color32(0x01, 0x00, 0x01, 0x00);

        /// <summary>
        /// 在macOS上，是否允许将窗口放置在菜单栏上方
        /// </summary>
        public bool isFreePositioningEnabled
        {
            get { return ((_uniWinCore == null) ? _isFreePositioningEnabled : _isFreePositioningEnabled = _uniWinCore.IsFreePositioningEnabled); }
            set { SetFreePositioning(value); }
        }
        [Header("For macOS only")]
        [Tooltip("Disable constrainFrameRect() *Only available on macOS")]
        [SerializeField, EditableProperty]
        private bool _isFreePositioningEnabled = false;

        /// <summary>
        /// 鼠标指针是否在不透明像素或对象上
        /// </summary>
        [Header("State")]
        [SerializeField, ReadOnly, Tooltip("Is the mouse pointer on an opaque pixel? (Read only)")]
        private bool onObject = true;

        /// <summary>
        /// 鼠标指针下的像素颜色（只读）
        /// </summary>
        [SerializeField, ReadOnly, Tooltip("Pixel color under the mouse pointer. (Read only)")]
        public Color pickedColor;

        /// <summary>
        /// 获取或设置窗口坐标
        /// </summary>
        public Vector2 windowPosition
        {
            get { return (_uniWinCore != null ? _uniWinCore.GetWindowPosition() : Vector2.zero); }
            set { _uniWinCore?.SetWindowPosition(value); }
        }

        /// <summary>
        /// 获取或设置窗口坐标
        /// </summary>
        public Vector2 windowSize
        {
            get { return (_uniWinCore != null ? _uniWinCore.GetWindowSize() : Vector2.zero); }
            set { _uniWinCore?.SetWindowSize(value); }
        }

        /// <summary>
        /// 获取客户端区域的大小
        /// </summary>
        public Vector2 clientSize
        {
            get { return (_uniWinCore != null ? _uniWinCore.GetClientSize() : Vector2.zero); }
        }

        /// <summary>
        /// 获取或设置鼠标光标坐标
        /// </summary>
        public Vector2 cursorPosition
        {
            get { return UniWinCore.GetCursorPosition(); }
            set { UniWinCore.SetCursorPosition(value); }
        }

        /// <summary>
        /// 初始状态下的窗口位置、大小
        /// </summary>
        private Rect originalWindowRectangle;

        // 为了将相机背景替换为alpha为0的黑色，用于保存原始背景的变量
        private CameraClearFlags originalCameraClearFlags;
        private Color originalCameraBackground;

        /// <summary>
        /// 存储光标下1px颜色的纹理
        /// </summary>
        private Texture2D colorPickerTexture = null;

        /// <summary>
        /// Raycast使用的鼠标事件信息
        /// </summary>
        private PointerEventData pointerEventData;

        /// <summary>
        /// Raycast 时的层遮罩
        /// </summary>
        private int hitTestLayerMask;

        /// <summary>
        /// 窗口样式变化时发生
        /// </summary>
        public event OnStateChangedDelegate OnStateChanged;
        public delegate void OnStateChangedDelegate(WindowStateEventType type);

        public delegate void FilesDelegate(string[] files);

        /// <summary>
        /// 文件或文件夹被拖放后发生
        /// </summary>
        public event FilesDelegate OnDropFiles;

        /// <summary>
        /// 显示器设置或分辨率变化时发生
        /// </summary>
        public event OnMonitorChangedDelegate OnMonitorChanged;
        public delegate void OnMonitorChangedDelegate();


        // 初始化函数
        void Awake()
        {
            // 单例模式：如果已有实例则销毁自身
            if (this != current)
            {
                Destroy(this.gameObject);
                return;
            }
            else
            {
                _current = this;
            }

            // 强制解除全屏。编辑器中不做任何处理
#if !UNITY_EDITOR
            if (forceWindowed && Screen.fullScreen)
            {
                Screen.fullScreen = false;
            }
#endif

            if (!currentCamera)
            {
                // 查找主相机
                currentCamera = Camera.main;

                //// 如果找不到主相机，则用Find查找
                //if (!currentCamera)
                //{
                //    currentCamera = GameObject.FindAnyObjectByType<Camera>();
                //}
            }

            // 保存相机的原始背景
            if (currentCamera)
            {
                originalCameraClearFlags = currentCamera.clearFlags;
                originalCameraBackground = currentCamera.backgroundColor;

            }

            // 鼠标事件信息
            pointerEventData = new PointerEventData(EventSystem.current);

            // 除 Ignore Raycast 以外都有效的遮罩
            hitTestLayerMask = ~LayerMask.GetMask("Ignore Raycast");

            // 准备鼠标下绘制颜色提取用的纹理
            colorPickerTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
#if UNITY_EDITOR
            return;
#endif
            // 创建窗口控制用的实例
            _uniWinCore = new UniWinCore();
        }

        /// <summary>
        /// 适配到指定显示器
        /// </summary>
        private void UpdateMonitorFitting()
        {
            if (!_shouldFitMonitor) return;

            int monitors = UniWinCore.GetMonitorCount();
            int targetMonitorIndex = _monitorToFit;

            if (targetMonitorIndex < 0)
            {
                targetMonitorIndex = 0;
            }
            if (monitors <= targetMonitorIndex)
            {
                targetMonitorIndex = monitors - 1;
            }

            if (targetMonitorIndex >= 0)
            {
                _uniWinCore.FitToMonitor(targetMonitorIndex);
            }
        }

        /// <summary>
        /// 查找现有实例或创建新实例
        /// </summary>
        /// <returns></returns>
        private static UniWindowController FindOrCreateInstance()
        {
            var instance = GameObject.FindAnyObjectByType<UniWindowController>();

            // 暂时禁用自动生成
            // // 如果场景中找不到则新建
            // if (!instance)
            // {
            //     var obj = new GameObject(nameof(UniWindowController));
            //     obj.AddComponent<UniWindowController>();
            // }

            return instance;
        }

        void Start()
        {
            //// 因 New Input System 有问题，用于验证输出
            // #if ENABLE_LEGACY_INPUT_MANAGER
            //             Debug.Log("Use legacy input manager.");
            // #elif ENABLE_INPUT_SYSTEM
            //             Debug.Log("Use new input system.");
            //             Debug.Log("Run In Background " + Mouse.current.canRunInBackground);
            // #else
            //             Debug.Log("Mouse position is not available.");
            // #endif

#if UNITY_EDITOR
            return;
#endif

            // 开始获取鼠标光标正下方颜色的协程
            StartCoroutine(HitTestCoroutine());

            // 获取初始窗口大小和位置

            StoreOriginalWindowRectangle();

            // 适配所选显示器
            OnMonitorChanged += UpdateMonitorFitting;
            UpdateMonitorFitting();
        }

        void OnDestroy()
        {
            if (_uniWinCore != null)
            {
                _uniWinCore.Dispose();
            }

            // 销毁 Instance
            if (this == current)
            {
                _current = null;
            }
        }

        void StoreOriginalWindowRectangle()
        {
            if (_uniWinCore != null)
            {
                var size = _uniWinCore.GetWindowSize();
                var pos = _uniWinCore.GetWindowPosition();
                originalWindowRectangle = new Rect(pos, size);
            }
        }

        // 每帧调用一次
        void Update()
        {
#if UNITY_EDITOR
            return;
#endif

            // 如果未获取到自身窗口，则获取
            if (_uniWinCore == null || !_uniWinCore.IsActive)
            {
                UpdateTargetWindow();
            }
            else
            {
                _uniWinCore.Update();
            }

            // 处理事件
            UpdateEvents();

            // 更新键盘、鼠标操作向下层窗口的穿透状态
            UpdateClickThrough();
        }

        /// <summary>
        /// 检查并处理 UniWinCore 事件
        /// </summary>
        private void UpdateEvents()
        {
            if (_uniWinCore == null) return;

            if (_uniWinCore.ObserveDroppedFiles(out var droppedFiles))
            {
                OnDropFiles?.Invoke(droppedFiles);
            }

            if (_uniWinCore.ObserveMonitorChanged())
            {
                OnMonitorChanged?.Invoke();
            }

            if (_uniWinCore.ObserveWindowStyleChanged(out var type))
            {
                // // 在有显示器适配指定的状态下解除最大化时
                // if (shouldFitMonitor && !uniWinCore.GetZoomed())
                // {
                //     //StartCoroutine("ForceZoomed");    // 延迟强制最大化
                //     //SetZoomed(true);        // 强制最大化 ←不一定有效
                //     //shouldFitMonitor = false;    // 禁用适配
                // }
                if (_shouldFitMonitor) StartCoroutine("ForceZoomed"); // 延迟强制最大化

                OnStateChanged?.Invoke((WindowStateEventType)type);
            }
        }

        IEnumerator ForceZoomed()
        {
            yield return new WaitForSeconds(0.5f);
            if (_shouldFitMonitor && !_uniWinCore.GetZoomed()) SetZoomed(true);
            yield return null;
        }

        /// <summary>
        /// 指定相机。如果有之前的相机则恢复背景
        /// </summary>
        /// <param name="newCamera"></param>
        public void SetCamera(Camera newCamera)
        {
            // 相机变更时，恢复设置
            if (newCamera != currentCamera)
            {
                SetCameraBackground(false);
            }

            currentCamera = newCamera;

            // 保存相机的原始背景
            if (currentCamera)
            {
                originalCameraClearFlags = currentCamera.clearFlags;
                originalCameraBackground = currentCamera.backgroundColor;

                SetCameraBackground(_isTransparent);
            }
        }

        /// <summary>
        /// 将鼠标/触摸操作穿透到下方窗口
        /// </summary>
        /// <param name="isThrough"></param>
        void SetClickThrough(bool isThrough)
        {
            _uniWinCore?.EnableClickThrough(isThrough);
            _isClickThrough = isThrough;
        }

        /// <summary>
        /// 根据像素颜色切换操作接受状态
        /// </summary>
        void UpdateClickThrough()
        {
            // 无自动点击测试则直接返回
            if (!isHitTestEnabled || hitTestType == HitTestType.None) return;

            // 鼠标光标隐藏状态下视为透明像素上
            bool hit = (onObject);

            if (_isClickThrough)
            {
                // 如果之前是点击穿透状态，则仅在命中时恢复
                if (hit)
                {
                    SetClickThrough(false);
                }
            }
            else
            {
                // 如果之前不是点击穿透，则仅在透明且未命中时设为点击穿透
                if (isTransparent && !hit)
                {
                    SetClickThrough(true);
                }
            }
        }

        /// <summary>
        /// 在协程中重复进行光标下颜色或Raycast的点击测试
        /// 因为使用 WaitForEndOfFrame() 所以采用协程
        /// </summary>
        /// <returns></returns>
        private IEnumerator HitTestCoroutine()
        {
            while (Application.isPlaying)
            {
                yield return new WaitForEndOfFrame();

                // Windows情况下，单色透明则点击测试交由OS处理，始终为命中
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
                if (transparentType == TransparentType.ColorKey)
                {
                    onObject = true;
                }
                else
#endif
                    if (hitTestType == HitTestType.Opacity)
                    {
                        HitTestByOpaquePixel();
                    }
                    else if (hitTestType == HitTestType.Raycast)
                    {
                        HitTestByRaycast();
                    }
                    else
                    {
                        // 无点击测试的情况下始终为true
                        onObject = true;
                    }
            }
            yield return null;
        }

        /// <summary>
        /// 将屏幕上的鼠标坐标换算为 Unity 的屏幕坐标系
        /// </summary>
        private Vector2 GetClientCursorPosition()
        {

            // New Input System 中无焦点时无法获取鼠标坐标，因此自行计算
            Vector2 mousePos = UniWinCore.GetCursorPosition();
            Vector2 winPos = windowPosition;
            Rect clientRect = _uniWinCore.GetClientRectangle();
            Vector2 unityPos = new Vector2(
                (mousePos.x - winPos.x - clientRect.x) * Screen.width / clientRect.width,
                (mousePos.y - winPos.y - clientRect.y) * Screen.height / clientRect.height
                );

            //             // 调试用
            //             // 与Unity获取的值比较
            // #if ENABLE_LEGACY_INPUT_MANAGER
            //             Vector2 position = Input.mousePosition;
            // #elif ENABLE_INPUT_SYSTEM
            //             Vector2 position = Mouse.current.position.ReadValue();
            // #endif
            //             if (!position.Equals(unityPos))
            //             {
            //                 Debug.LogWarning("Mouse position diff : " + position + " / " + unityPos);
            //             }

            // 编辑器情况下始终用Unity的功能获取鼠标坐标
            //   因为可能不是单独的Game窗口，或Scale不同，简单计算无法求得
#if UNITY_EDITOR
#if ENABLE_LEGACY_INPUT_MANAGER
            return Input.mousePosition;
#elif UNITY_EDITOR && ENABLE_INPUT_SYSTEM
            return Mouse.current.position.ReadValue();
#else
            return unityPos;
#endif
#else
            return unityPos;
#endif
        }

        /// <summary>
        /// 检查鼠标下是否有像素
        /// </summary>
        private void HitTestByOpaquePixel()
        {
            Vector2 mousePos = GetClientCursorPosition();

            // 检查鼠标坐标
            if (GetOnOpaquePixel(mousePos))
            {
                //Debug.Log("Mouse " + mousePos);
                onObject = true;
                //activeFingerId = -1;    // 解除触摸追踪
                return;
            }
            else
            {
                onObject = false;
            }
        }

        /// <summary>
        /// 返回指定坐标的像素是否透明
        /// </summary>
        /// <param name="mousePos">坐标[px]。必须在绘制范围内。</param>
        /// <returns></returns>
        private bool GetOnOpaquePixel(Vector2 mousePos)
        {
            float w = Screen.width;
            float h = Screen.height;
            //Debug.Log(w + ", " + h);

            // 屏幕外则视为透明
            if (
                mousePos.x < 0 || mousePos.x >= w
                || mousePos.y < 0 || mousePos.y >= h
                )
            {
                return false;
            }

            // 如果不是透明状态，则在范围内视为不透明
            if (!_isTransparent) return true;

            // LayeredWindow则点击穿透交由OS处理，窗口内返回true
            if (transparentType == TransparentType.ColorKey) return true;

            // 根据指定坐标的绘制结果判断
            try   // 在WaitForEndOfFrame时机执行的话，try应该可以去掉
            {
                // Reference http://tsubakit1.hateblo.jp/entry/20131203/1386000440
                colorPickerTexture.ReadPixels(new Rect(mousePos, Vector2.one), 0, 0);
                Color color = colorPickerTexture.GetPixels32()[0];
                pickedColor = color;

                return (color.a >= opacityThreshold);  // α大于等于阈值则视为不透明
            }
            catch (System.Exception ex)
            {
                Debug.LogError(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 检查鼠标下是否有对象
        /// </summary>
        private void HitTestByRaycast()
        {
            Vector2 position = GetClientCursorPosition();

            // // 判断是否在uGUI上
            var raycastResults = new List<RaycastResult>();
            pointerEventData.position = position;
            EventSystem.current.RaycastAll(pointerEventData, raycastResults);
            foreach (var result in raycastResults)
            {
                // 考虑层遮罩（Ignore Raycast 以外则命中）
                if (((1 << result.gameObject.layer) & hitTestLayerMask) > 0)
                {
                    onObject = true;
                    return;
                }
            }
            // 不考虑层直接命中的话用下面的即可
            // // 如果判定在uGUI上，则结束
            // if (EventSystem.current.IsPointerOverGameObject())
            // {
            //     onObject = true;
            //     return;
            // }

            if (currentCamera && currentCamera.isActiveAndEnabled)
            {
                Ray ray = currentCamera.ScreenPointToRay(position);

                // 判断是否在3D对象上
                if (Physics.Raycast(ray, out _, raycastMaxDepth))
                {
                    onObject = true;
                    return;
                }

                // 判断是否在2D对象上
                var rayHit2D = Physics2D.GetRayIntersection(ray);
                Debug.DrawRay(ray.origin, ray.direction, Color.blue, 2f, false);
                if (rayHit2D.collider != null)
                {
                    onObject = true;
                    return;
                }
            }
            else
            {
                // 相机未启用则获取主相机
                currentCamera = Camera.main;
            }

            // 任何都未命中则判断不在对象上
            onObject = false;
        }

        /// <summary>
        /// 如果自己的窗口句柄不确定则重新查找
        /// </summary>
        private void UpdateTargetWindow()
        {
            if (_uniWinCore == null)
            {
                _uniWinCore = new UniWinCore();
            }

            // 如果窗口还未获取到，则进行获取处理
            if (!_uniWinCore.IsActive)
            {
                _uniWinCore.AttachMyWindow();

                // 如果能获取到窗口则设置初始值
                if (_uniWinCore.IsActive)
                {
                    _uniWinCore.SetTransparentType((UniWinCore.TransparentType)transparentType);
                    _uniWinCore.SetKeyColor(keyColor);
                    _uniWinCore.SetAlphaValue(_alphaValue);
                    SetTransparent(_isTransparent);
                    if (_isBottommost)
                    {
                        SetBottommost(_isBottommost);
                    }
                    else
                    {
                        SetTopmost(_isTopmost);
                    }
                    SetZoomed(_isZoomed);
                    SetClickThrough(_isClickThrough);
                    SetAllowDrop(_allowDropFiles);
                    SetFreePositioning(_isFreePositioningEnabled);

                    // 获取窗口时执行与显示器变更同等的处理
                    OnMonitorChanged?.Invoke();
                }
            }
            else
            {
#if UNITY_EDITOR
                // 编辑器中游戏视图会被关闭或停靠，因此如果有变化则更改目标窗口
                // 如果活动窗口与当前目标相同则什么也不做
                _uniWinCore.AttachMyActiveWindow();
#endif
            }
        }

        /// <summary>
        /// 窗口焦点变化时调用
        /// </summary>
        /// <param name="focus"></param>
        private void OnApplicationFocus(bool focus)
        {
#if UNITY_EDITOR
            return;
#endif
            if (focus)
            {
                UpdateTargetWindow();

                // 获得焦点的瞬间，强制关闭点击穿透
                if (_isTransparent && isHitTestEnabled && transparentType != TransparentType.ColorKey)
                {
                    SetClickThrough(false);
                }
            }
        }

        /// <summary>
        /// 窗口变为透明状态时，自动将背景更改为透明单色
        /// </summary>
        /// <param name="transparent"></param>
        void SetCameraBackground(bool transparent)
        {
            // 相机未确定或不进行自动切换的情况下，不做任何处理
            if (!currentCamera || !autoSwitchCameraBackground) return;

            // 要透明则将相机背景更改为透明色
            if (transparent)
            {
                // 在被透明化之前，保存当前相机信息
                if (!isTransparent)
                {
                    originalCameraClearFlags = currentCamera.clearFlags;
                    originalCameraBackground = currentCamera.backgroundColor;
                }

                currentCamera.clearFlags = CameraClearFlags.SolidColor;
                if (transparentType == TransparentType.ColorKey)
                {
                    currentCamera.backgroundColor = keyColor;
                }
                else
                {
                    currentCamera.backgroundColor = Color.clear;
                }
            }
            else
            {
                currentCamera.clearFlags = originalCameraClearFlags;
                currentCamera.backgroundColor = originalCameraBackground;
            }
        }

        /// <summary>
        /// 切换透明化状态
        /// </summary>
        /// <param name="transparent"></param>
        private void SetTransparent(bool transparent)
        {
            SetCameraBackground(transparent);
            _isTransparent = transparent;
#if !UNITY_EDITOR
            if (_uniWinCore != null)
            {
                _uniWinCore.EnableTransparent(transparent);
            }
#endif
            UpdateClickThrough();
        }

        /// <summary>
        /// 更改透明方法
        /// </summary>
        /// <param name="type"></param>
        public void SetTransparentType(TransparentType type)
        {
            if (_uniWinCore != null)
            {
                // 如果正在透明中，则先解除再重新透明
                if (_isTransparent)
                {
                    SetTransparent(false);
                    _uniWinCore.SetTransparentType((UniWinCore.TransparentType)type);
                    transparentType = type;
                    SetTransparent(true);
                }
                else
                {
                    _uniWinCore.SetTransparentType((UniWinCore.TransparentType)type);
                    transparentType = type;
                }
            }
        }

        /// <summary>
        /// 设置窗口透明度
        /// </summary>
        /// <param name="alpha">0.0 to 1.0</param>
        private void SetAlphaValue(float alpha)
        {
            _alphaValue = alpha;
            _uniWinCore?.SetAlphaValue(_alphaValue);
        }

        /// <summary>
        /// 切换置顶
        /// </summary>
        /// <param name="topmost"></param>
        private void SetTopmost(bool topmost)
        {
            //if (_isTopmost == topmost) return;
            if (_uniWinCore == null) return;

            _uniWinCore.EnableTopmost(topmost);
            _isTopmost = _uniWinCore.IsTopmost;
            _isBottommost = _uniWinCore.IsBottommost;
        }

        /// <summary>
        /// 切换常置最底
        /// </summary>
        /// <param name="bottommost"></param>
        private void SetBottommost(bool bottommost)
        {
            if (_uniWinCore == null) return;

            _uniWinCore.EnableBottommost(bottommost);
            _isBottommost = _uniWinCore.IsBottommost;
            _isTopmost = _uniWinCore.IsTopmost;
        }

        /// <summary>
        /// 最大化
        /// </summary>
        /// <param name="zoomed"></param>
        private void SetZoomed(bool zoomed)
        {
            if (_uniWinCore == null) return;

            _uniWinCore.SetZoomed(zoomed);
            _isZoomed = _uniWinCore.GetZoomed();
        }

        private void SetAllowDrop(bool enabled)
        {
            if (_uniWinCore == null) return;

            _uniWinCore.SetAllowDrop(enabled);
            _allowDropFiles = enabled;
        }

        /// <summary>
        /// 在macOS上，允许将窗口放置在包含菜单栏上方的自由位置
        /// </summary>
        /// <param name="enabled"></param>
        private void SetFreePositioning(bool enabled)
        {
            if (_uniWinCore == null) return;

            _uniWinCore.EnableFreePositioning(enabled);
            _isFreePositioningEnabled = _uniWinCore.IsFreePositioningEnabled;
        }

        /// <summary>
        /// 获取连接的显示器数量
        /// </summary>
        /// <returns></returns>
        public static int GetMonitorCount()
        {
            //if (uniWinCore == null) return 0;
            return UniWinCore.GetMonitorCount();
        }

        /// <summary>
        /// 获取显示器位置和大小
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static Rect GetMonitorRect(int index)
        {
            if (UniWinCore.GetMonitorRectangle(index, out Vector2 position, out Vector2 size))
            {
                return new Rect(position, size);
            }
            return Rect.zero;
        }

        /// <summary>
        /// 适配到指定显示器
        /// </summary>
        /// <returns></returns>
        private bool FitToMonitor(bool shouldFit, int monitorIndex)
        {
            if (_uniWinCore == null)
            {
                _shouldFitMonitor = shouldFit;
                _monitorToFit = monitorIndex;
                return false;
            }

            if (shouldFit)
            {
                if (!_shouldFitMonitor)
                {
                    // 之前是不适配的状态
                    _monitorToFit = monitorIndex;
                    _shouldFitMonitor = shouldFit;
                    UpdateMonitorFitting();
                }
                else
                {
                    if (_monitorToFit != monitorIndex)
                    {
                        // 适配目标显示器变化时
                        _monitorToFit = monitorIndex;
                        UpdateMonitorFitting();
                    }
                }
            }
            else
            {
                if (_shouldFitMonitor)
                {
                    // 之前是适配状态，被解除时
                    _monitorToFit = monitorIndex;
                    _shouldFitMonitor = shouldFit;
                    UpdateMonitorFitting();

                    _uniWinCore.SetZoomed(false);
                    //uniWinCore.SetWindowSize(originalWindowRectangle.size);
                    //uniWinCore.SetWindowPosition(originalWindowRectangle.position);
                }
                else
                {
                    // 未适配中则仅更改选择
                    _monitorToFit = monitorIndex;
                }
            }

            return true;
        }

        /// <summary>
        /// 获取鼠标光标位置
        /// </summary>
        /// <returns>光标位置</returns>
        public static Vector2 GetCursorPosition()
        {
            return UniWinCore.GetCursorPosition();
        }

        /// <summary>
        /// 设置鼠标光标位置
        /// </summary>
        /// <param name="position"></param>
        public static void SetCursorPosition(Vector2 position)
        {
            UniWinCore.SetCursorPosition(position);
        }

        /// <summary>
        /// 获取鼠标按钮
        /// </summary>
        /// <returns></returns>
        public static MouseButton GetMouseButtons()
        {
            int buttons = UniWinCore.GetMouseButtons();
            return (MouseButton)buttons;
        }

        /// <summary>
        /// 获取按下的修饰键
        /// </summary>
        /// <returns></returns>
        public static ModifierKey GetModifierKeys()
        {
            int mod = UniWinCore.GetModifierKeys();
            return (ModifierKey)mod;
        }

        /// <summary>
        /// 结束时需要恢复窗口状态的处理
        /// </summary>
        void OnApplicationQuit()
        {
#if UNITY_EDITOR
            return;
#endif
            if (Application.isPlaying)
            {
                if (_uniWinCore != null)
                {
                    // 编辑器下恢复窗口状态
                    // 独立版会看到恢复的样子所以跳过
#if UNITY_EDITOR
                    _uniWinCore.SetWindowSize(originalWindowRectangle.size);
                    _uniWinCore.SetWindowPosition(originalWindowRectangle.position);

                    _uniWinCore.DetachWindow();
#endif
                    _uniWinCore.Dispose();
                }
            }
        }

        /// <summary>
        /// 给自身窗口设置焦点
        /// </summary>
        public void Focus()
        {
            if (_uniWinCore != null)
            {
                //uniWin.SetFocus();
            }
        }


        /// <summary>
        /// 调试专用。用于获取参考信息的函数
        /// </summary>
        /// <returns></returns>
        [Obsolete]
        public int GetDebugInfo()
        {
            if (_uniWinCore != null)
            {
                return UniWinCore.GetDebugInfo();
            }
            return 0;
        }
    }
}
