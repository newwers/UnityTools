using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Z.UI
{
    [AddComponentMenu("UI/List View", 38)]
    [SelectionBase]
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public class ListView : UIBehaviour, IInitializePotentialDragHandler, IBeginDragHandler, IEndDragHandler, IDragHandler, IScrollHandler, ICanvasElement, ILayoutGroup
    {
        #region 定义结构

        [Serializable]
        public class ScrollRectEvent : UnityEvent<Vector2> { }

        [Serializable]
        public class ScrollRenderEvent : UnityEvent<int, Z.UI.UIReferenceComponent> { }

        public enum MovementType
        {
            Unrestricted, // Unrestricted movement -- can scroll forever
            Elastic, // Restricted but flexible -- can go past the edges, but springs back in place
            Clamped, // Restricted movement where it's not possible to go past the edges
            SizeLimit,
        }
        public enum Corner
        {
            Upper = 0,
            Lower = 1,
        }

        public enum ScrollbarVisibility
        {
            Permanent,
            AutoHide,
            AutoHideAndExpandViewport,
        }

        /// <summary>
        /// 滚动方向
        /// </summary>
        public enum MotionType
        {
            /// <summary>
            /// 横向
            /// </summary>
            Horizontal = 0,

            /// <summary>
            /// 竖向
            /// </summary>
            Vertical = 1
        }

        /// <summary>
        /// 自动吸附滑动
        /// </summary>
        [Serializable]
        public struct AttachSnap
        {
            /// <summary>
            /// 速度阈值
            /// </summary>
            public float VelocityThreshold;
            /// <summary>
            /// 
            /// </summary>
            public float Duration;
        }

        /// <summary>
        /// 自动滚动状态
        /// </summary>
        private class AutoScrollState
        {
            public bool Enable;
            public float Duration;
            public float StartTime;
            public Vector2 EndScrollPosition;
            public Action Callback;
        }

        /// <summary>
        /// 单个渲染信息
        /// </summary>
        private class ItemInfo
        {
            public RectTransform transform;
            public UIReferenceComponent serialized;
            public int virtualIndex;
            public int actualIndex;
            public bool active;
            public bool renderable;
            public ItemInfo(RectTransform trans)
            {
                transform = trans;
                if (transform)
                {
                    serialized = transform.GetComponent<UIReferenceComponent>();
                    if (serialized == null) serialized = transform.gameObject.AddComponent<UIReferenceComponent>();
                }
                trans.anchorMin = Vector2.up;
                trans.anchorMax = Vector2.up;
                trans.pivot = Vector2.one * 0.5f;
                trans.gameObject.SetActive(false);
                trans.localPosition = Vector3.zero;
                virtualIndex = int.MaxValue;
                actualIndex = int.MaxValue;
                active = false;
            }

            Vector3 m_Position;
            public Vector3 position
            {
                set
                {
                    //Log("actualIndex:" + actualIndex + ",before pos:" + m_Position + ",set pos:" + value);
                    m_Position = value;
                    if (transform != null)
                        transform.anchoredPosition = m_Position;
                }
            }
            public float scale
            {
                set
                {
                    if (transform != null)
                        transform.localScale = Vector3.one * value;
                }
            }

            public void UpdateActive(bool bOutsideUnActive)
            {
                if (bOutsideUnActive)
                {
                    if (transform.gameObject.activeSelf != active)
                        transform.gameObject.SetActive(active);
                }
                else
                {
                    if (!transform.gameObject.activeSelf)
                        transform.gameObject.SetActive(true);
                    if (!active)
                        transform.anchoredPosition = new Vector3(10000, 10000, 10000);//这边设置故意跳过了使用属性 position 的设置方式
                    else
                        transform.anchoredPosition = m_Position;
                }
            }

            public int CalcActualIndex(int virIdx, uint len)
            {
                int actual = 0;
                if (len == 0)
                    return actual;

                if (virIdx < 0)
                {
                    actual = (int)((len - 1) + (virIdx + 1) % len);
                }
                else if (virIdx > len - 1)
                {
                    actual = (int)(virIdx % len);
                }
                else
                {
                    actual = virIdx;
                }
                return actual;
            }

            public void UpdateIndex(int virIdx, uint len)
            {
                virtualIndex = virIdx;
                actualIndex = CalcActualIndex(virIdx, len);
#if UNITY_EDITOR
                transform.gameObject.name = actualIndex.ToString();
#endif
            }

            public void Destroy()
            {
                if (transform != null)
                    UnityEngine.Object.Destroy(transform.gameObject);
                serialized = null;
            }
        }
        #endregion

        #region 属性

        struct EnterScale
        {
            public ItemInfo item;
            public float time;
        }

        [SerializeField]
        private RectTransform m_ViewPort;
        public RectTransform viewport { get { return m_ViewPort; } set { m_ViewPort = value; SetDirtyCaching(); } }

        [SerializeField]
        private RectTransform m_Content;
        public RectTransform content { get { return m_Content; } set { m_Content = value; } }

        [SerializeField]
        private RectTransform m_Prefab;
        public RectTransform prefab { get { return m_Prefab; } set { SetPrefab(value); } }

        [SerializeField]
        private int m_nInitMaxCount = 0;

        [SerializeField]
        private bool m_OutsideUnActive = true;

        [SerializeField]
        private uint m_NumItems;
        public uint numItems { get { return m_NumItems; } set { SetNumItems(value); } }

        [SerializeField]
        private MotionType m_MotionType = MotionType.Vertical;
        public MotionType motionType { get { return m_MotionType; } set { m_MotionType = value; } }

        [SerializeField]
        private MovementType m_MovementType = MovementType.Elastic;
        public MovementType movementType { get { return m_MovementType; } set { m_MovementType = value; } }

        [SerializeField]
        private Corner m_StartCorner = Corner.Upper;
        public Corner startCorner { get { return m_StartCorner; } set { m_StartCorner = value; } }

        [SerializeField]
        private float m_Elasticity = 0.1f; // Only used for MovementType.Elastic
        public float elasticity { get { return m_Elasticity; } set { m_Elasticity = value; } }

        [SerializeField]
        private bool m_Inertia = true;
        public bool inertia { get { return m_Inertia; } set { m_Inertia = value; } }

        [SerializeField]
        private float m_DecelerationRate = 0.135f; // Only used when inertia is enabled
        public float decelerationRate { get { return m_DecelerationRate; } set { m_DecelerationRate = value; } }

        [SerializeField]
        private float m_ScrollSensitivity = 1.0f;
        public float scrollSensitivity { get { return m_ScrollSensitivity; } set { m_ScrollSensitivity = value; } }

        [SerializeField]
        private Vector2 m_MinSize = new Vector2(0, 0);
        public Vector2 minSize { get { return m_MinSize; } set { m_MinSize = value; } }

        [SerializeField]
        private List<Vector2> m_itemOffsets = new List<Vector2>();
        public List<Vector2> itemOffsets { get { return m_itemOffsets; } set { m_itemOffsets = value; } }

        [SerializeField]
        private RectOffset m_Margin;
        public RectOffset margin { get { return m_Margin; } set { m_Margin = value; } }

        [SerializeField]
        private uint m_FixedCount = 1;
        public uint fixedCount { get { return m_FixedCount; } set { m_FixedCount = value; } }

        [SerializeField]
        private bool m_bAutoFit = false;
        private bool m_bDirtyFixedCount = false;
        private Vector2 m_AutoSpace = Vector2.zero;

        [SerializeField]
        private bool m_bCoroutinesRefresh = true;
        private List<ItemInfo> m_vCoroutineItems = null;

        [SerializeField]
        private Vector2 m_ItemSize;
        public Vector2 itemSize { get { return m_ItemSize; } set { m_ItemSize = value; } }

        [SerializeField]
        private Vector2 m_Space;
        public Vector2 space { get { return m_Space; } set { m_Space = value; } }

        [SerializeField]
        private bool m_Loop;
        public bool loop { get { return m_Loop; } set { SetLoop(value); } }

        [SerializeField]
        private bool m_AutoAttach;
        public bool autoAttach { get { return m_AutoAttach; } set { m_AutoAttach = value; } }

        [SerializeField]
        private AttachSnap m_AttachSnap = new AttachSnap() { VelocityThreshold = 0.5f, Duration = 0.3f };
        public AttachSnap attachSnap { get { return m_AttachSnap; } set { m_AttachSnap = value; } }

        [SerializeField]
        private bool m_ScaleCurve;
        public bool scaleCurve { get { return m_ScaleCurve; } set { m_ScaleCurve = value; } }

        [SerializeField]
        private bool m_EnterTween;
        public bool enterTween { get { return m_EnterTween; } set { m_EnterTween = value; } }

        [SerializeField]
        private AnimationCurve m_ScaleAnimation = new AnimationCurve(new Keyframe[] { new Keyframe(0, 1), new Keyframe(1, 0.5f) });
        public AnimationCurve scaleAnimation { get { return m_ScaleAnimation; } set { m_ScaleAnimation = value; } }

        private byte m_bUpdateList = 1; //2 forceupdate
        private bool m_bFirstUpdate = true;
        public bool FirstUpdate { get { return m_bFirstUpdate; } }

        private bool m_bTriggerEnterScale = true;
        [SerializeField]
        private bool m_EnterScale;
        public bool enterScale { get { return m_EnterScale; } set { m_EnterScale = value; } }

        //[SerializeField]
        //private FMODUnity.EventReference fmodEventRef;

        [SerializeField]
        private float m_EnterScaleDelay;
        public float enterScaleDelay { get { return m_EnterScaleDelay; } set { m_EnterScaleDelay = value; } }

        [SerializeField]
        private AnimationCurve m_EnterScaleAnimation = new AnimationCurve(new Keyframe[] { new Keyframe(0, 1), new Keyframe(1, 0.5f) });
        public AnimationCurve enterScaleAnimation { get { return m_EnterScaleAnimation; } set { m_EnterScaleAnimation = value; } }

        private List<EnterScale> m_vEnterScale = null;

        private WaitForEndOfFrame m_WaitForEndOfFrame = new WaitForEndOfFrame();

        [SerializeField]
        private Scrollbar m_HorizontalScrollbar;
        public Scrollbar horizontalScrollbar
        {
            get
            {
                return m_HorizontalScrollbar;
            }
            set
            {
                if (m_HorizontalScrollbar)
                    m_HorizontalScrollbar.onValueChanged.RemoveListener(SetHorizontalNormalizedPosition);
                m_HorizontalScrollbar = value;
                if (m_HorizontalScrollbar)
                    m_HorizontalScrollbar.onValueChanged.AddListener(SetHorizontalNormalizedPosition);
                SetDirtyCaching();
            }
        }

        [SerializeField]
        private Scrollbar m_VerticalScrollbar;
        public Scrollbar verticalScrollbar
        {
            get
            {
                return m_VerticalScrollbar;
            }
            set
            {
                if (m_VerticalScrollbar)
                    m_VerticalScrollbar.onValueChanged.RemoveListener(SetVerticalNormalizedPosition);
                m_VerticalScrollbar = value;
                if (m_VerticalScrollbar)
                    m_VerticalScrollbar.onValueChanged.AddListener(SetVerticalNormalizedPosition);
                SetDirtyCaching();
            }
        }

        [SerializeField]
        private ScrollbarVisibility m_HorizontalScrollbarVisibility;
        public ScrollbarVisibility horizontalScrollbarVisibility { get { return m_HorizontalScrollbarVisibility; } set { m_HorizontalScrollbarVisibility = value; SetDirtyCaching(); } }

        [SerializeField]
        private ScrollbarVisibility m_VerticalScrollbarVisibility;
        public ScrollbarVisibility verticalScrollbarVisibility { get { return m_VerticalScrollbarVisibility; } set { m_VerticalScrollbarVisibility = value; SetDirtyCaching(); } }

        [SerializeField]
        private float m_HorizontalScrollbarSpacing;
        public float horizontalScrollbarSpacing { get { return m_HorizontalScrollbarSpacing; } set { m_HorizontalScrollbarSpacing = value; SetDirty(); } }

        [SerializeField]
        private float m_VerticalScrollbarSpacing;
        public float verticalScrollbarSpacing { get { return m_VerticalScrollbarSpacing; } set { m_VerticalScrollbarSpacing = value; SetDirty(); } }

        [SerializeField]
        private ScrollRectEvent m_OnValueChanged = new ScrollRectEvent();
        public ScrollRectEvent onValueChanged { get { return m_OnValueChanged; } set { m_OnValueChanged = value; } }

        [SerializeField]
        private ScrollRenderEvent m_OnItemRender = new ScrollRenderEvent();
        public ScrollRenderEvent onItemRender { get { return m_OnItemRender; } set { m_OnItemRender = value; } }
        [SerializeField]
        private ScrollRenderEvent m_OnItemInit = new ScrollRenderEvent();
        public ScrollRenderEvent onItemInit { get { return m_OnItemInit; } set { m_OnItemInit = value; } }

        public Action OnInitCompletdAction;

        private bool m_IsInited = false;


        #endregion



        #region 滑动重构
        // The offset from handle position to mouse down position
        private Vector2 m_PointerStartLocalCursor = Vector2.zero;
        private Vector2 m_ContentStartPosition = Vector2.zero;

        private Bounds m_ContentBounds;
        private Bounds m_ViewBounds;

        private Vector2 m_Velocity;
        public Vector2 velocity { get { return m_Velocity; } set { m_Velocity = value; } }

        private bool m_Dragging;
        public bool isDragging { get { return m_Dragging; } }

        private Vector2 m_PrevPosition = Vector2.zero;
        private Bounds m_PrevContentBounds;
        private Bounds m_PrevViewBounds;
        [NonSerialized]
        private bool m_HasRebuiltLayout = false;

        private bool m_HSliderExpand;
        private bool m_VSliderExpand;
        private float m_HSliderHeight;
        private float m_VSliderWidth;

        [NonSerialized]
        private RectTransform m_Rect;
        private RectTransform rectTransform
        {
            get
            {
                if (m_Rect == null)
                    m_Rect = GetComponent<RectTransform>();
                return m_Rect;
            }
        }

        private RectTransform m_HorizontalScrollbarRect;
        private RectTransform m_VerticalScrollbarRect;

        private DrivenRectTransformTracker m_Tracker;
        #endregion

        #region 虚拟列表数据

        private bool m_HasInit;//在点击运行后是否初始化
        private int m_TotalLine;//总行或列数
        private int m_MaxInit;//最大实例化数量
        private int m_StartIdx;//开始序号
        private int m_EndIdx;//结束序号
        private int m_CurLine = -1;//当前行
        private List<ItemInfo> m_VirtualItems = new List<ItemInfo>();

        readonly AutoScrollState autoScrollState = new AutoScrollState();
        readonly Dictionary<int, ItemInfo> tmpContains = new Dictionary<int, ItemInfo>();
        readonly Queue<ItemInfo> tmpPool = new Queue<ItemInfo>();

        public int StartIdx
        {
            get
            {
                return m_StartIdx;
            }
        }
        public int EndIdx
        {
            get
            {
                return m_EndIdx;
            }
        }
        #endregion

        #region 列表重载

        protected ListView() { }

        void ICanvasElement.Rebuild(CanvasUpdate executing)
        {
            if (executing == CanvasUpdate.Prelayout)
            {
                UpdateCachedData();
            }

            if (executing == CanvasUpdate.PostLayout)
            {
                UpdateBounds();
                UpdateScrollbars(Vector2.zero);
                UpdatePrevData();

                m_HasRebuiltLayout = true;
            }
        }

        void ICanvasElement.LayoutComplete()
        {
            //初始化RebuildList
            if (IsActive() && !m_HasInit && m_ViewPort.rect.size.x > 0 && m_ViewPort.rect.size.y > 0)
            {
                StartCoroutine(RebuildList());
            }
        }

        void ICanvasElement.GraphicUpdateComplete() { }

        void UpdateCachedData()
        {
            if (m_ViewPort == null)
                return;

            Transform transform = this.transform;
            m_HorizontalScrollbarRect = m_HorizontalScrollbar == null ? null : m_HorizontalScrollbar.transform as RectTransform;
            m_VerticalScrollbarRect = m_VerticalScrollbar == null ? null : m_VerticalScrollbar.transform as RectTransform;

            // These are true if either the elements are children, or they don't exist at all.
            bool viewIsChild = (m_ViewPort.parent == transform);
            bool hScrollbarIsChild = (!m_HorizontalScrollbarRect || m_HorizontalScrollbarRect.parent == transform);
            bool vScrollbarIsChild = (!m_VerticalScrollbarRect || m_VerticalScrollbarRect.parent == transform);
            bool allAreChildren = (viewIsChild && hScrollbarIsChild && vScrollbarIsChild);

            m_HSliderExpand = allAreChildren && m_HorizontalScrollbarRect && horizontalScrollbarVisibility == ScrollbarVisibility.AutoHideAndExpandViewport;
            m_VSliderExpand = allAreChildren && m_VerticalScrollbarRect && verticalScrollbarVisibility == ScrollbarVisibility.AutoHideAndExpandViewport;
            m_HSliderHeight = (m_HorizontalScrollbarRect == null ? 0 : m_HorizontalScrollbarRect.rect.height);
            m_VSliderWidth = (m_VerticalScrollbarRect == null ? 0 : m_VerticalScrollbarRect.rect.width);
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            if (m_HorizontalScrollbar)
                m_HorizontalScrollbar.onValueChanged.AddListener(SetHorizontalNormalizedPosition);
            if (m_VerticalScrollbar)
                m_VerticalScrollbar.onValueChanged.AddListener(SetVerticalNormalizedPosition);

            if (m_ViewPort == null)
            {
                var view = transform.Find("Viewport");
                if (view)
                    m_ViewPort = view as RectTransform;
                else
                    m_ViewPort = rectTransform;
            }
            CheckCoroutinesRefreshList();

            CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild(this);
        }

        protected override void OnDisable()
        {
            CanvasUpdateRegistry.UnRegisterCanvasElementForRebuild(this);

            if (m_HorizontalScrollbar)
                m_HorizontalScrollbar.onValueChanged.RemoveListener(SetHorizontalNormalizedPosition);
            if (m_VerticalScrollbar)
                m_VerticalScrollbar.onValueChanged.RemoveListener(SetVerticalNormalizedPosition);

            m_HasRebuiltLayout = false;
            m_Tracker.Clear();
            m_Velocity = Vector2.zero;
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
            base.OnDisable();
        }

        public override bool IsActive()
        {
            return base.IsActive() && m_Content != null;
        }

        private void EnsureLayoutHasRebuilt()
        {
            if (!m_HasRebuiltLayout && !CanvasUpdateRegistry.IsRebuildingLayout())
                Canvas.ForceUpdateCanvases();
        }

        public void StopMovement()
        {
            m_Velocity = Vector2.zero;
        }

        void IScrollHandler.OnScroll(PointerEventData data)
        {
            if (!IsActive())
                return;

            EnsureLayoutHasRebuilt();
            UpdateBounds();

            Vector2 delta = data.scrollDelta;
            // Down is positive for scroll events, while in UI system up is positive.
            delta.y *= -1;
            if (m_MotionType == MotionType.Vertical)
            {
                if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                    delta.y = delta.x;
                delta.x = 0;
                if (m_MovementType == MovementType.SizeLimit)
                {
                    if (m_Content.sizeDelta.y <= this.GetViewSize())
                        delta.y = 0;
                }
            }
            if (m_MotionType == MotionType.Horizontal)
            {
                if (Mathf.Abs(delta.y) > Mathf.Abs(delta.x))
                    delta.x = delta.y;
                delta.y = 0;
                if (m_MovementType == MovementType.SizeLimit)
                {
                    if (m_Content.sizeDelta.x <= this.GetViewSize())
                        delta.x = 0;
                }
            }

            if (delta.sqrMagnitude > 0)
            {
                Vector2 position = m_Content.anchoredPosition;
                position += delta * m_ScrollSensitivity;
                if (m_MovementType == MovementType.Clamped || m_MovementType == MovementType.SizeLimit)
                {
                    position += CalculateOffset(position - m_Content.anchoredPosition);
                }

                SetContentAnchoredPosition(position);
                UpdateLoopContentSize();
            }

            UpdateBounds();
        }

        void OnInitializePotentialDragItem(GameObject go, BaseEventData eventData)
        {
            OnInitializePotentialDrag(eventData as PointerEventData);
        }
        public void OnInitializePotentialDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            m_Velocity = Vector2.zero;
        }

        void OnBeginDragItem(GameObject go, BaseEventData eventData)
        {
            OnBeginDrag(eventData as PointerEventData);
        }
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            if (!IsActive())
                return;

            UpdateBounds();

            m_PointerStartLocalCursor = Vector2.zero;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(m_ViewPort, eventData.position, eventData.pressEventCamera, out m_PointerStartLocalCursor);
            m_ContentStartPosition = m_Content.anchoredPosition;
            m_Dragging = true;
        }

        void OnEndDragItem(GameObject go, BaseEventData eventData)
        {
            OnEndDrag(eventData as PointerEventData);
        }
        public void OnEndDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            m_Dragging = false;
            UpdateLoopContentSize();
        }

        void OnDragItem(GameObject go, BaseEventData eventData)
        {
            OnDrag(eventData as PointerEventData);
        }
        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            if (!IsActive())
                return;

            Vector2 localCursor;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(m_ViewPort, eventData.position, eventData.pressEventCamera, out localCursor))
                return;

            UpdateBounds();

            var pointerDelta = localCursor - m_PointerStartLocalCursor;
            Vector2 position = m_ContentStartPosition + pointerDelta;

            // Offset to get content into place in the view.
            Vector2 offset = CalculateOffset(position - m_Content.anchoredPosition);
            position += offset;
            if (m_MovementType == MovementType.Elastic)
            {
                if (offset.x != 0)
                    position.x = position.x - RubberDelta(offset.x, m_ViewBounds.size.x);
                if (offset.y != 0)
                    position.y = position.y - RubberDelta(offset.y, m_ViewBounds.size.y);
            }

            SetContentAnchoredPosition(position);
            //UpdateLoopContentSize();是否要在拖拽得时候更新content大小?
        }

        protected void SetContentAnchoredPosition(Vector2 position)
        {
            if (m_MotionType != MotionType.Horizontal)
                position.x = m_Content.anchoredPosition.x;
            if (m_MotionType != MotionType.Vertical)
                position.y = m_Content.anchoredPosition.y;

            if (position != m_Content.anchoredPosition)
            {
                m_Content.anchoredPosition = position;
                UpdateBounds();
            }
        }

        protected void LateUpdate()
        {
            if (!m_Content)
                return;

            EnsureLayoutHasRebuilt();
            UpdateScrollbarVisibility();
            UpdateBounds();
            bool bForceRender = false;
            float deltaTime = Time.unscaledDeltaTime;
            Vector2 offset = CalculateOffset(Vector2.zero);
            if (autoScrollState.Enable)//设置自动滚动
            {
                float interp = 0;
                if (autoScrollState.Duration > 0)
                {
                    var alpha = Mathf.Clamp01((Time.unscaledTime - autoScrollState.StartTime) / Mathf.Max(autoScrollState.Duration, float.Epsilon));
                    interp = EaseInOutCubic(0, 1, alpha);
                }
                else
                {
                    interp = 1;
                    m_Content.anchoredPosition = autoScrollState.EndScrollPosition;
                    //SetContentAnchoredPosition(autoScrollState.EndScrollPosition);
                }
                var position = Vector2.Lerp(m_Content.anchoredPosition, autoScrollState.EndScrollPosition, interp);
                SetContentAnchoredPosition(position);
                if (Vector2.Distance(m_Content.anchoredPosition, autoScrollState.EndScrollPosition) <= 1)
                {
                    SetContentAnchoredPosition(autoScrollState.EndScrollPosition);
                    autoScrollState.Enable = false;
                    bForceRender = true;
                    UpdateLoopContentSize();
                    if (autoScrollState.Callback != null)
                    {
                        autoScrollState.Callback();
                        autoScrollState.Callback = null;
                    }
                }
            }
            else if (!m_Dragging && (offset != Vector2.zero || m_Velocity != Vector2.zero) && m_NumItems > 0)
            {
                Vector2 position = m_Content.anchoredPosition;
                if (m_MovementType == MovementType.Elastic && offset != Vector2.zero)
                {
                    var curIndex = GetLoopIndex();
                    var attachIndex = GetAutoAttachIndex();
                    if (m_AutoAttach && curIndex != attachIndex && m_Velocity.magnitude < m_AttachSnap.VelocityThreshold)//自动吸附时滚动
                    {
                        ScrollTo(attachIndex, m_AttachSnap.Duration);
                    }
                    else
                    {
                        var speed = m_Velocity;
                        position = Vector2.SmoothDamp(m_Content.anchoredPosition, m_Content.anchoredPosition + offset, ref speed,
                            m_Elasticity, Mathf.Infinity, deltaTime);
                        //Debug.Log($"m_Velocity:{m_Velocity},speed:{speed}");
                        m_Velocity = speed;
                    }
                }
                else if (m_Inertia)
                {
                    m_Velocity.x *= Mathf.Pow(m_DecelerationRate, deltaTime);
                    m_Velocity.y *= Mathf.Pow(m_DecelerationRate, deltaTime);
                    //Debug.Log($"m_Velocity:{m_Velocity},sqrMagnitude:{m_Velocity.sqrMagnitude}");
                    if (m_Velocity.sqrMagnitude < 10)
                        m_Velocity = Vector2.zero;
                    position += m_Velocity * deltaTime;

                    if (m_AutoAttach && m_Velocity.magnitude < m_AttachSnap.VelocityThreshold)
                    {
                        //var curIndex = GetLoopIndex();
                        //var attachIndex = GetAutoAttachIndex();
                        //if (curIndex != attachIndex)
                        {
                            //中部滚动中，滚动速度小于预设值自动吸附
                            ScrollTo(GetAutoAttachIndex(), m_AttachSnap.Duration);
                        }
                    }
                }
                else
                {
                    m_Velocity = Vector2.zero;
                }

                if (m_Velocity.magnitude > 0.1f)
                {
                    if (m_MovementType == MovementType.Clamped || m_MovementType == MovementType.SizeLimit)
                    {
                        offset = CalculateOffset(position - m_Content.anchoredPosition);
                        position += offset;
                    }
                    SetContentAnchoredPosition(position);
                    //UpdateLoopContentSize();
                }
            }

            if (m_Dragging && m_Inertia)
            {
                Vector3 newVelocity = (m_Content.anchoredPosition - m_PrevPosition) / deltaTime;
                m_Velocity = Vector3.Lerp(m_Velocity, newVelocity, deltaTime * 10);
            }
            bool bUpdateBound = false;
            if (m_ViewBounds != m_PrevViewBounds || m_ContentBounds != m_PrevContentBounds || m_Content.anchoredPosition != m_PrevPosition)
            {
                bUpdateBound = true;
                if (m_bUpdateList <= 0) m_bUpdateList = 1;
            }

            if (m_bUpdateList != 0 || bForceRender)
                UpdatePosition(m_bUpdateList > 1 || bForceRender);//这边的刷新,忽略携程缩放动画的检测,否则会导致重复添加,播放多次动画

            if (bUpdateBound)
            {
                UpdateScrollbars(offset);
                m_OnValueChanged.Invoke(normalizedPosition);
                UpdatePrevData();

                if (m_ScaleCurve)
                {
                    float viewSize = GetViewSize();
                    int count = m_VirtualItems.Count;
                    for (int i = 0; i < count; i++)
                    {
                        var item = m_VirtualItems[i];
                        if (item.active)
                        {
                            Bounds bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(m_ViewPort, item.transform);
                            var ratio = Mathf.Abs(m_MotionType == MotionType.Horizontal ? bounds.center.x : bounds.center.y) / viewSize;
                            item.scale = m_ScaleAnimation.Evaluate(ratio);
                        }
                    }
                }
            }

            if (m_EnterScale)
            {
                if (m_vEnterScale != null)
                {
                    float viewSize = GetViewSize();
                    float time = Time.time;
                    EnterScale enter;
                    float curveTime = m_EnterScaleAnimation.keys[m_EnterScaleAnimation.keys.Length - 1].time;
                    for (int i = 0; i < m_vEnterScale.Count;)
                    {
                        enter = m_vEnterScale[i];
                        if (enter.item.active)
                        {
                            if (m_ScaleCurve)
                            {
                                float t = enter.time - time;
                                Bounds bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(m_ViewPort, enter.item.transform);
                                var ratio = Mathf.Abs(m_MotionType == MotionType.Horizontal ? bounds.center.x : bounds.center.y) / viewSize;
                                enter.item.scale = m_ScaleAnimation.Evaluate(ratio) * m_EnterScaleAnimation.Evaluate(curveTime - t);

                                if (t < 0)
                                {
                                    m_vEnterScale.RemoveAt(i);
                                    continue;
                                }
                            }
                            else
                            {
                                float t = enter.time - time;
                                float scale = m_EnterScaleAnimation.Evaluate(curveTime - t);
                                enter.item.scale = scale;

                                if (t < 0)
                                {
                                    m_vEnterScale.RemoveAt(i);
                                    continue;
                                }
                            }
                        }
                        ++i;
                    }
                }
            }
#if UNITY_EDITOR
            GetFixedCount();
#endif
        }

        #endregion

        #region 列表数据更新
        private void UpdatePrevData()
        {
            if (m_Content == null)
                m_PrevPosition = Vector2.zero;
            else
                m_PrevPosition = m_Content.anchoredPosition;
            m_PrevViewBounds = m_ViewBounds;
            m_PrevContentBounds = m_ContentBounds;
        }

        private void UpdateScrollbars(Vector2 offset)
        {
            if (m_HorizontalScrollbar)
            {
                if (m_ContentBounds.size.x > 0)
                    m_HorizontalScrollbar.size = Mathf.Clamp01((m_ViewBounds.size.x - Mathf.Abs(offset.x)) / m_ContentBounds.size.x);
                else
                    m_HorizontalScrollbar.size = 1;

                m_HorizontalScrollbar.value = horizontalNormalizedPosition;
            }

            if (m_VerticalScrollbar)
            {
                if (m_ContentBounds.size.y > 0)
                    m_VerticalScrollbar.size = Mathf.Clamp01((m_ViewBounds.size.y - Mathf.Abs(offset.y)) / m_ContentBounds.size.y);
                else
                    m_VerticalScrollbar.size = 1;

                m_VerticalScrollbar.value = verticalNormalizedPosition;
            }
        }

        public Vector2 normalizedPosition
        {
            get
            {
                return new Vector2(horizontalNormalizedPosition, verticalNormalizedPosition);
            }
            set
            {
                SetNormalizedPosition(value.x, 0);
                SetNormalizedPosition(value.y, 1);
            }
        }

        public float horizontalNormalizedPosition
        {
            get
            {
                UpdateBounds();
                if (m_ContentBounds.size.x <= m_ViewBounds.size.x)
                    return (m_ViewBounds.min.x > m_ContentBounds.min.x) ? 1 : 0;
                return (m_ViewBounds.min.x - m_ContentBounds.min.x) / (m_ContentBounds.size.x - m_ViewBounds.size.x);
            }
            set
            {
                SetNormalizedPosition(value, 0);
            }
        }

        public float verticalNormalizedPosition
        {
            get
            {
                UpdateBounds();
                if (m_ContentBounds.size.y <= m_ViewBounds.size.y)
                    return (m_ViewBounds.min.y > m_ContentBounds.min.y) ? 1 : 0;
                ;
                return (m_ViewBounds.min.y - m_ContentBounds.min.y) / (m_ContentBounds.size.y - m_ViewBounds.size.y);
            }
            set
            {
                SetNormalizedPosition(value, 1);
            }
        }

        private void SetHorizontalNormalizedPosition(float value) { SetNormalizedPosition(value, 0); }
        private void SetVerticalNormalizedPosition(float value) { SetNormalizedPosition(value, 1); }

        private void SetNormalizedPosition(float value, int axis)
        {
            EnsureLayoutHasRebuilt();
            UpdateBounds();
            // How much the content is larger than the view.
            float hiddenLength = m_ContentBounds.size[axis] - m_ViewBounds.size[axis];
            // Where the position of the lower left corner of the content bounds should be, in the space of the view.
            float contentBoundsMinPosition = m_ViewBounds.min[axis] - value * hiddenLength;
            // The new content localPosition, in the space of the view.
            float newLocalPosition = m_Content.localPosition[axis] + contentBoundsMinPosition - m_ContentBounds.min[axis];

            Vector3 localPosition = m_Content.localPosition;
            if (Mathf.Abs(localPosition[axis] - newLocalPosition) > 0.01f)
            {
                localPosition[axis] = newLocalPosition;
                m_Content.localPosition = localPosition;
                m_Velocity[axis] = 0;
                UpdateBounds();
            }
        }

        private static float RubberDelta(float overStretching, float viewSize)
        {
            return (1 - (1 / ((Mathf.Abs(overStretching) * 0.55f / viewSize) + 1))) * viewSize * Mathf.Sign(overStretching);
        }

        protected override void OnRectTransformDimensionsChange()
        {
            SetDirty();
        }

        private bool hScrollingNeeded
        {
            get
            {
                if (Application.isPlaying)
                    return m_ContentBounds.size.x > m_ViewBounds.size.x + 0.01f;
                return true;
            }
        }
        private bool vScrollingNeeded
        {
            get
            {
                if (Application.isPlaying)
                    return m_ContentBounds.size.y > m_ViewBounds.size.y + 0.01f;
                return true;
            }
        }

        public virtual void SetLayoutHorizontal()
        {
            m_Tracker.Clear();

            if (m_HSliderExpand || m_VSliderExpand)
            {
                m_Tracker.Add(this, m_ViewPort,
                    DrivenTransformProperties.Anchors |
                    DrivenTransformProperties.SizeDelta |
                    DrivenTransformProperties.AnchoredPosition);

                // Make view full size to see if content fits.
                m_ViewPort.anchorMin = Vector2.zero;
                m_ViewPort.anchorMax = Vector2.one;
                m_ViewPort.sizeDelta = Vector2.zero;
                m_ViewPort.anchoredPosition = Vector2.zero;

                // Recalculate content layout with this size to see if it fits when there are no scrollbars.
                LayoutRebuilder.ForceRebuildLayoutImmediate(content);
                m_ViewBounds = new Bounds(m_ViewPort.rect.center, m_ViewPort.rect.size);
                m_ContentBounds = GetBounds();
            }

            // If it doesn't fit vertically, enable vertical scrollbar and shrink view horizontally to make room for it.
            if (m_VSliderExpand && vScrollingNeeded)
            {
                m_ViewPort.sizeDelta = new Vector2(-(m_VSliderWidth + m_VerticalScrollbarSpacing), m_ViewPort.sizeDelta.y);

                // Recalculate content layout with this size to see if it fits vertically
                // when there is a vertical scrollbar (which may reflowed the content to make it taller).
                LayoutRebuilder.ForceRebuildLayoutImmediate(content);
                m_ViewBounds = new Bounds(m_ViewPort.rect.center, m_ViewPort.rect.size);
                m_ContentBounds = GetBounds();
            }

            // If it doesn't fit horizontally, enable horizontal scrollbar and shrink view vertically to make room for it.
            if (m_HSliderExpand && hScrollingNeeded)
            {
                m_ViewPort.sizeDelta = new Vector2(m_ViewPort.sizeDelta.x, -(m_HSliderHeight + m_HorizontalScrollbarSpacing));
                m_ViewBounds = new Bounds(m_ViewPort.rect.center, m_ViewPort.rect.size);
                m_ContentBounds = GetBounds();
            }

            // If the vertical slider didn't kick in the first time, and the horizontal one did,
            // we need to check again if the vertical slider now needs to kick in.
            // If it doesn't fit vertically, enable vertical scrollbar and shrink view horizontally to make room for it.
            if (m_VSliderExpand && vScrollingNeeded && m_ViewPort.sizeDelta.x == 0 && m_ViewPort.sizeDelta.y < 0)
            {
                m_ViewPort.sizeDelta = new Vector2(-(m_VSliderWidth + m_VerticalScrollbarSpacing), m_ViewPort.sizeDelta.y);
            }
        }

        public virtual void SetLayoutVertical()
        {
            UpdateScrollbarLayout();
            m_ViewBounds = new Bounds(m_ViewPort.rect.center, m_ViewPort.rect.size);
            m_ContentBounds = GetBounds();
        }

        void UpdateScrollbarVisibility()
        {
            if (m_VerticalScrollbar && m_VerticalScrollbarVisibility != ScrollbarVisibility.Permanent && m_VerticalScrollbar.gameObject.activeSelf != vScrollingNeeded)
                m_VerticalScrollbar.gameObject.SetActive(vScrollingNeeded);

            if (m_HorizontalScrollbar && m_HorizontalScrollbarVisibility != ScrollbarVisibility.Permanent && m_HorizontalScrollbar.gameObject.activeSelf != hScrollingNeeded)
                m_HorizontalScrollbar.gameObject.SetActive(hScrollingNeeded);
        }

        void UpdateScrollbarLayout()
        {
            if (m_VSliderExpand && m_HorizontalScrollbar)
            {
                m_Tracker.Add(this, m_HorizontalScrollbarRect,
                    DrivenTransformProperties.AnchorMinX |
                    DrivenTransformProperties.AnchorMaxX |
                    DrivenTransformProperties.SizeDeltaX |
                    DrivenTransformProperties.AnchoredPositionX);
                m_HorizontalScrollbarRect.anchorMin = new Vector2(0, m_HorizontalScrollbarRect.anchorMin.y);
                m_HorizontalScrollbarRect.anchorMax = new Vector2(1, m_HorizontalScrollbarRect.anchorMax.y);
                m_HorizontalScrollbarRect.anchoredPosition = new Vector2(0, m_HorizontalScrollbarRect.anchoredPosition.y);
                if (vScrollingNeeded)
                    m_HorizontalScrollbarRect.sizeDelta = new Vector2(-(m_VSliderWidth + m_VerticalScrollbarSpacing), m_HorizontalScrollbarRect.sizeDelta.y);
                else
                    m_HorizontalScrollbarRect.sizeDelta = new Vector2(0, m_HorizontalScrollbarRect.sizeDelta.y);
            }

            if (m_HSliderExpand && m_VerticalScrollbar)
            {
                m_Tracker.Add(this, m_VerticalScrollbarRect,
                    DrivenTransformProperties.AnchorMinY |
                    DrivenTransformProperties.AnchorMaxY |
                    DrivenTransformProperties.SizeDeltaY |
                    DrivenTransformProperties.AnchoredPositionY);
                m_VerticalScrollbarRect.anchorMin = new Vector2(m_VerticalScrollbarRect.anchorMin.x, 0);
                m_VerticalScrollbarRect.anchorMax = new Vector2(m_VerticalScrollbarRect.anchorMax.x, 1);
                m_VerticalScrollbarRect.anchoredPosition = new Vector2(m_VerticalScrollbarRect.anchoredPosition.x, 0);
                if (hScrollingNeeded)
                    m_VerticalScrollbarRect.sizeDelta = new Vector2(m_VerticalScrollbarRect.sizeDelta.x, -(m_HSliderHeight + m_HorizontalScrollbarSpacing));
                else
                    m_VerticalScrollbarRect.sizeDelta = new Vector2(m_VerticalScrollbarRect.sizeDelta.x, 0);
            }
        }

        private void UpdateBounds()
        {
            if (m_ViewPort == null)
                return;

            m_ViewBounds = new Bounds(m_ViewPort.rect.center, m_ViewPort.rect.size);
            m_ContentBounds = GetBounds();

            if (m_Content == null)
                return;

            // Make sure content bounds are at least as large as view by adding padding if not.
            // One might think at first that if the content is smaller than the view, scrolling should be allowed.
            // However, that's not how scroll views normally work.
            // Scrolling is *only* possible when content is *larger* than view.
            // We use the pivot of the content rect to decide in which directions the content bounds should be expanded.
            // E.g. if pivot is at top, bounds are expanded downwards.
            // This also works nicely when ContentSizeFitter is used on the content.
            Vector3 contentSize = m_ContentBounds.size;
            Vector3 contentPos = m_ContentBounds.center;
            Vector3 excess = m_ViewBounds.size - contentSize;
            if (excess.x > 0)
            {
                contentPos.x -= excess.x * (m_Content.pivot.x - 0.5f);
                contentSize.x = m_ViewBounds.size.x;
            }
            if (excess.y > 0)
            {
                contentPos.y -= excess.y * (m_Content.pivot.y - 0.5f);
                contentSize.y = m_ViewBounds.size.y;
            }

            m_ContentBounds.size = contentSize;
            m_ContentBounds.center = contentPos;
        }

        private readonly Vector3[] m_Corners = new Vector3[4];
        private Bounds GetBounds()
        {
            if (m_Content == null)
                return new Bounds();

            var vMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            var vMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            var toLocal = m_ViewPort.worldToLocalMatrix;
            m_Content.GetWorldCorners(m_Corners);
            for (int j = 0; j < 4; j++)
            {
                Vector3 v = toLocal.MultiplyPoint3x4(m_Corners[j]);
                vMin = Vector3.Min(v, vMin);
                vMax = Vector3.Max(v, vMax);
            }

            var bounds = new Bounds(vMin, Vector3.zero);
            bounds.Encapsulate(vMax);
            return bounds;
        }

        private Vector2 CalculateOffset(Vector2 delta)
        {
            Vector2 offset = Vector2.zero;
            if (m_MovementType == MovementType.Unrestricted)
                return offset;

            Vector2 min = m_ContentBounds.min;
            Vector2 max = m_ContentBounds.max;

            if (m_MotionType == MotionType.Horizontal)
            {
                min.x += delta.x;
                max.x += delta.x;
                if (min.x > m_ViewBounds.min.x)
                    offset.x = m_ViewBounds.min.x - min.x;
                else if (max.x < m_ViewBounds.max.x)
                    offset.x = m_ViewBounds.max.x - max.x;
            }

            if (m_MotionType == MotionType.Vertical)
            {
                min.y += delta.y;
                max.y += delta.y;
                if (max.y < m_ViewBounds.max.y)
                    offset.y = m_ViewBounds.max.y - max.y;
                else if (min.y > m_ViewBounds.min.y)
                    offset.y = m_ViewBounds.min.y - min.y;
            }

            return offset;
        }

        protected void SetDirty()
        {
            if (!IsActive())
                return;

            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
        }

        protected void SetDirtyCaching()
        {
            if (!IsActive())
                return;

            CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild(this);
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
        }


#if UNITY_EDITOR
        protected override void OnValidate()
        {
            SetDirtyCaching();
        }
#endif

        #endregion

        #region 无限、虚拟列表重构
        private uint GetFixedCount()
        {
            if (m_bAutoFit)
            {
                uint preCnt = m_FixedCount;
                if (m_MotionType == MotionType.Horizontal)
                {
                    m_AutoSpace[1] = 0;
                    float height = m_ViewPort.rect.height - m_Margin.top - m_Margin.bottom;
                    float cell = GetCellSize(1);
                    m_FixedCount = (uint)Mathf.FloorToInt(height / cell);
                    if (m_FixedCount > 1)
                        m_AutoSpace[1] = ((height - m_FixedCount * cell) / (float)(m_FixedCount - 1)) * 0.5f;
                }
                else
                {
                    m_AutoSpace[0] = 0;
                    float width = m_ViewPort.rect.width - m_Margin.left - m_Margin.right;
                    float cell = GetCellSize(0);
                    m_FixedCount = (uint)Mathf.FloorToInt(width / cell);
                    if (m_FixedCount > 1)
                        m_AutoSpace[0] = ((width - m_FixedCount * cell) / (float)(m_FixedCount - 1)) * 0.5f;
                }
                if (preCnt != m_FixedCount)
                {
                    UdateFit();
                    m_bDirtyFixedCount = true;
#if UNITY_EDITOR
                    m_bUpdateList = 1;
                    UpdateContent((int)m_FixedCount);
#endif
                }
            }
            return m_FixedCount;
        }

        void UdateFit()
        {
            if (m_MotionType == MotionType.Horizontal)
            {
                m_AutoSpace[0] = 0;
                if (numItems > 1 && m_bAutoFit && m_FixedCount >= 1)
                {
                    uint num = (numItems - 1) / m_FixedCount;//这边计算当超过第二行开始加上自动高度适配
                    if (num > 1)
                    {
                        float width = m_ViewPort.rect.width - m_Margin.left - m_Margin.right;
                        float realWidth = num * GetCellSize(0);
                        if (width > realWidth)
                            m_AutoSpace[0] = ((width - realWidth) / (float)(num - 1)) * 0.5f;

                    }
                }
            }
            else
            {
                m_AutoSpace[1] = 0;
                if (numItems > 1 && m_bAutoFit && m_FixedCount >= 1)
                {
                    uint num = (numItems - 1) / m_FixedCount;//这边计算当超过第二行开始加上自动高度适配
                    if (num > 1)
                    {
                        float height = m_ViewPort.rect.height - m_Margin.top - m_Margin.bottom;
                        float realHeight = num * GetCellSize(1);
                        if (height > realHeight)
                            m_AutoSpace[1] = ((height - realHeight) / (float)(num - 1)) * 0.5f;
                    }

                }
            }
            //Log("m_AutoSpace:" + m_AutoSpace);
        }

        private float GetViewSize()
        {
            return m_MotionType == MotionType.Vertical ? m_ViewPort.rect.height : m_ViewPort.rect.width;
        }

        private float GetSpace(int axis = -1)
        {
            if (axis == -1)
            {
                axis = m_MotionType == MotionType.Horizontal ? 0 : 1;
            }
            return m_Space[axis] + m_AutoSpace[axis];

        }

        private float GetCellSize(int axis = -1)
        {
            if (axis == -1)
            {
                axis = m_MotionType == MotionType.Horizontal ? 0 : 1;
            }
            return m_ItemSize[axis] + GetSpace(axis);
        }

        private Vector2 GetItemOffset(int index)
        {
            if (m_itemOffsets == null || m_itemOffsets.Count <= 0) return Vector2.zero;
            index = index % m_itemOffsets.Count;
            if (index >= 0 && index < m_itemOffsets.Count)
                return m_itemOffsets[index];
            return Vector2.zero;
        }

        private void UpdatePosition(bool forceRender = false)
        {
            m_bUpdateList = 0;
            int index = GetLoopIndex();
            if (index == m_CurLine && !forceRender)
                return;
            m_CurLine = index;

            int fixedCnt = (int)GetFixedCount();

            m_StartIdx = m_CurLine * fixedCnt;
            m_EndIdx = m_StartIdx + m_MaxInit;

            if (!m_Loop)
            {
                if (m_EndIdx >= m_NumItems)
                    m_EndIdx = (int)m_NumItems;
            }

            tmpContains.Clear();
            tmpPool.Clear();

            int num = m_VirtualItems.Count;
            if (num <= 0)
                return;
            ItemInfo item;
            //Log("m_StartIdx:" + m_StartIdx + ",m_EndIdx:" + m_EndIdx + ",m_bDirtyFixedCount:" + m_bDirtyFixedCount);
            for (int i = 0; i < num; i++)
            {
                item = m_VirtualItems[i];
                if (m_bDirtyFixedCount || item.virtualIndex < m_StartIdx || item.virtualIndex >= m_EndIdx)
                {
                    tmpPool.Enqueue(item);
                    if (m_bDirtyFixedCount || item.virtualIndex >= m_EndIdx)
                    {
                        item.active = false;
                    }
                    //Log("不渲染index:" + item.virtualIndex);
                }
                else
                {
                    tmpContains.Add(item.virtualIndex, item);
                    item.active = true;
                    //Log("渲染index:" + item.virtualIndex);
                }
                item.renderable = false;
            }
            m_bDirtyFixedCount = false;
            //Debug.Log(string.Format("##渲染{0}\t({1}---{2})\t{3},{4}", m_CurLine, m_StartIdx, m_EndIdx, tmpPool.Count, tmpContains.Count));

            #region 渲染

            int line = m_CurLine;
            for (int i = m_StartIdx; i < m_EndIdx;)
            {
                for (int j = 0; j < fixedCnt; j++)
                {
                    int virIdx = i + j;
                    if (virIdx >= m_EndIdx)
                        break;

                    if (!tmpContains.TryGetValue(virIdx, out item))
                    {
                        item = tmpPool.Dequeue();
                        if (m_MotionType == MotionType.Vertical)
                        {
                            Vector2 offset = new Vector2((j + 0.5f) * itemSize.x + j * GetSpace(0),
                                                -(line + 0.5f) * itemSize.y - line * GetSpace(1)) + GetItemOffset(item.CalcActualIndex(virIdx, m_NumItems));
                            //Log("j = " + j + " ,GetSpace(1):" + GetSpace(1) + " ,line:" + line + ",itemSize.y:" + itemSize.y + ",ItemOffset:" + GetItemOffset(item.CalcActualIndex(virIdx, m_NumItems)));
                            if (!m_Loop)
                                item.position = new Vector2(margin.left, -margin.top) + offset;
                            else
                                item.position = new Vector2(margin.left, 0) + offset;

                            //Log("anchorPos = " + " margin.left:" + margin.left + " + (-margin.top):" + (-margin.top) + " offset:" + offset);
                        }
                        else
                        {
                            Vector2 offset = new Vector2((line + 0.5f) * itemSize.x + line * GetSpace(0),
                                                -(j + 0.5f) * itemSize.y - j * GetSpace(1)) + GetItemOffset(item.CalcActualIndex(virIdx, m_NumItems));
                            if (!m_Loop)
                                item.position = new Vector2(margin.left, margin.top) + offset;
                            else
                                item.position = new Vector2(0, margin.top) + offset;
                        }


                        item.UpdateIndex(virIdx, m_NumItems);
                        switch (m_StartCorner)
                        {
                            case Corner.Upper:
                                item.transform.gameObject.name = item.actualIndex.ToString();
                                break;
                            case Corner.Lower:
                                item.transform.gameObject.name = (m_NumItems - item.actualIndex).ToString();
                                break;
                        }
                        item.active = true;
                        item.renderable = true;
                    }
                    else
                    {
                        item.UpdateIndex(virIdx, m_NumItems);
                        switch (m_StartCorner)
                        {
                            case Corner.Upper:
                                item.transform.gameObject.name = item.actualIndex.ToString();
                                break;
                            case Corner.Lower:
                                item.transform.gameObject.name = (m_NumItems - item.actualIndex).ToString();
                                break;
                        }
                        item.active = true;
                        item.renderable = forceRender;
                    }
                }
                line++;
                i += fixedCnt;
            }

            #endregion

            //    if (m_vCoroutineItems != null) m_vCoroutineItems.Clear();
            for (int i = 0; i < num; i++)
            {
                item = m_VirtualItems[i];
                item.UpdateActive(m_OutsideUnActive);

                // Render在Active后
                if (item.renderable)
                {
                    if (m_bCoroutinesRefresh)
                    {
                        m_OnItemInit.Invoke(item.actualIndex, item.serialized);
                        if (m_vCoroutineItems == null) m_vCoroutineItems = new List<ItemInfo>();
                        if (m_vCoroutineItems.IndexOf(item) < 0) m_vCoroutineItems.Add(item);
                    }
                    else
                    {
                        if (m_bTriggerEnterScale)
                        {
                            bool bTween = false;
                            if (m_EnterTween)
                            {
                                //if (item.serialized != null && item.serialized.Widgets != null)
                                //{
                                //    for (int j = 0; j < item.serialized.Widgets.Length; ++j)
                                //    {
                                //        RtgTween.Tweener tweener = item.serialized.Widgets[j].widget as RtgTween.Tweener;
                                //        if (tweener)
                                //        {
                                //            tweener.ForcePlayTween(i * m_EnterScaleDelay);
                                //            TopGame.Core.AudioManager.PlayEvent(fmodEventRef, 0.5f + i * m_EnterScaleDelay);
                                //            bTween = true;
                                //        }
                                //    }
                                //}
                            }

                            if (!bTween && m_EnterScale)
                            {
                                EnterScale enterS = new EnterScale();
                                enterS.item = item;
                                item.scale = 1;
                                enterS.time = i * m_EnterScaleDelay;
                                AddEnterScale(enterS);
                            }
                        }
                        m_OnItemRender.Invoke(item.actualIndex, item.serialized);
                    }

                }
                item.renderable = false;
            }

            if (m_IsInited == false && m_bCoroutinesRefresh == false)//如果是单帧刷新,就标记初始化完成
            {
                m_IsInited = true;
                OnInitCompletdAction?.Invoke();
            }

            CheckCoroutinesRefreshList();

            m_bTriggerEnterScale = false;
        }
        void CheckCoroutinesRefreshList()
        {
            if (m_vCoroutineItems != null && m_vCoroutineItems.Count > 0 && isActiveAndEnabled)
            {
                StartCoroutine(CoroutinesRefreshList());
            }
        }
        private IEnumerator CoroutinesRefreshList()
        {
            bool bTriggerEnterScale = m_bTriggerEnterScale;
            int index = 0;
            while (m_vCoroutineItems.Count > 0)
            {
                ItemInfo item = m_vCoroutineItems[0];
                m_vCoroutineItems.RemoveAt(0);
                if (bTriggerEnterScale)
                {
                    bool bTween = false;
                    if (m_EnterTween)
                    {
                        //if (item.serialized != null && item.serialized.Widgets != null)
                        //{
                        //    for (int j = 0; j < item.serialized.Widgets.Length; ++j)
                        //    {
                        //        RtgTween.Tweener tweener = item.serialized.Widgets[j].widget as RtgTween.Tweener;
                        //        if (tweener)
                        //        {
                        //            tweener.ForcePlayTween(index * m_EnterScaleDelay);
                        //            TopGame.Core.AudioManager.PlayEvent(fmodEventRef, 0.5f + index * m_EnterScaleDelay);
                        //            bTween = true;
                        //        }
                        //    }
                        //}
                    }

                    if (!bTween && m_EnterScale)
                    {
                        EnterScale enterS = new EnterScale();
                        enterS.item = item;
                        item.scale = 1;
                        enterS.time = index * m_EnterScaleDelay;
                        AddEnterScale(enterS);
                    }
                }
                m_OnItemRender.Invoke(item.actualIndex, item.serialized);
                index++;
            }

            if (m_vCoroutineItems.Count > 0)
                yield return CoroutinesRefreshList();

            if (m_IsInited == false)
            {
                m_IsInited = true;//标记初始化完成
                OnInitCompletdAction?.Invoke();
            }
        }

        void AddEnterScale(EnterScale enterS)
        {
            if (m_vEnterScale == null) m_vEnterScale = new List<EnterScale>();
            int findIndex = -1;
            for (int j = 0; j < m_vEnterScale.Count; ++j)
            {
                if (m_vEnterScale[j].item == enterS.item)
                {
                    findIndex = j;
                    break;
                }
            }
            float time = enterS.time;
            enterS.time = Time.time + 0.2f + time;
            if (findIndex < 0)
            {
                m_vEnterScale.Add(enterS);
                //TopGame.Core.AudioManager.PlayEvent(fmodEventRef, 0.5f + time);
            }
            else m_vEnterScale[findIndex] = enterS;
        }

        private Vector2 CalculateClosestPosition(int target, bool isLimit = false)
        {
            Vector2 offset = Vector2.zero;
            if (m_Loop)
            {
                if (m_MotionType == MotionType.Horizontal)
                {
                    int index = target != int.MaxValue
                        ? GetClosestLoopIndex(target)
                        : GetLoopIndex(-GetCellSize(0) * 0.5f);
                    offset.x = -(itemSize.x * index + (index - 1) * GetSpace(0));
                    offset.y = m_Content.anchoredPosition.y;
                }
                else
                {
                    int index = target != int.MaxValue
                        ? GetClosestLoopIndex(target)
                        : GetLoopIndex(GetCellSize(1) * 0.5f);
                    offset.y = itemSize.y * index + (index - 1) * GetSpace(1);
                    offset.x = m_Content.anchoredPosition.x;
                }
            }
            else
            {
                int index = target;
                if (m_MotionType == MotionType.Horizontal)
                {
                    if (index == 0)
                    {
                        offset.x = 0;
                    }
                    else
                    {
                        offset.x = -(margin.left + itemSize.x * index + (index - 1) * GetSpace(0));
                    }
                    offset.y = m_Content.anchoredPosition.y;
                }
                else
                {
                    if (index == 0)
                    {
                        offset.y = 0;
                    }
                    else
                    {
                        offset.y = margin.top + itemSize.y * index + (index - 1) * GetSpace(1);
                        if (isLimit)
                        {
                            offset.y = Mathf.Min(m_Content.rect.height - m_ViewPort.rect.height, offset.y);
                        }
                    }

                    offset.x = m_Content.anchoredPosition.x;
                }
            }
            return offset;
        }

        private int GetClosestLoopIndex(int index)
        {
            Vector2 offset = Vector2.zero;

            if (index == 0)
                return 0;

            var diff = GetLoopPosition(index, (int)m_NumItems) - GetLoopPosition(m_CurLine, (int)m_NumItems);
            if (Mathf.Abs(diff) > m_NumItems / 2)
                diff = (int)Mathf.Sign(-diff) * ((int)m_NumItems - Mathf.Abs(diff));

            return diff + m_CurLine;
        }

        public void InitItem()
        {
            if (m_VirtualItems == null) return;
            for (int i = 0; i < m_VirtualItems.Count; ++i)
            {
                if (m_OnItemInit != null) m_OnItemInit.Invoke(i, m_VirtualItems[i].serialized);
            }
        }

        public void BackTop(float time, Action endCallback = null)
        {
            ScrollTo(0, time, 0, endCallback);
        }

        public void ScrollTo(int index, float duration, float offset = 0, Action endCallback = null, bool isLimit = false)
        {
            velocity = Vector2.zero;
            if (fixedCount > 0) index = Mathf.FloorToInt(index / (int)fixedCount);

            autoScrollState.Enable = true;
            autoScrollState.Duration = duration;
            autoScrollState.StartTime = Time.unscaledTime;
            if (motionType == MotionType.Horizontal)
                autoScrollState.EndScrollPosition = CalculateClosestPosition(index, isLimit) + new Vector2(offset, 0);
            else
                autoScrollState.EndScrollPosition = CalculateClosestPosition(index, isLimit) + new Vector2(0, offset);
            autoScrollState.Callback = endCallback;
        }

        public void ScrollTo(float value)
        {
            //实现从0到1的滚动
            if (m_MotionType == MotionType.Horizontal)
            {
                float x = Mathf.Lerp(m_ViewBounds.min.x, m_ViewBounds.max.x, value);
                SetContentAnchoredPosition(new Vector2(x, m_Content.anchoredPosition.y));
            }
            else
            {
                float y = Mathf.Lerp(m_ViewBounds.min.y, m_ViewBounds.max.y, value);
                SetContentAnchoredPosition(new Vector2(m_Content.anchoredPosition.x, y));
            }
        }

        private int GetLoopPosition(int index, int length)
        {
            if (length == 0)
            {
                return 0;
            }
            if (index < 0)
            {
                index = (length - 1) + (index + 1) % length;
            }
            else if (index > length - 1)
            {
                index = index % length;
            }
            return index;
        }

        private void SetPrefab(RectTransform rt)
        {
            //清理掉现在所有的预制
            //创建新的
            int num = m_VirtualItems.Count;
            for (int i = 0; i < num; i++)
            {
                if (m_VirtualItems.Count > 0)
                {
                    var item = m_VirtualItems[0];
                    m_VirtualItems.RemoveAt(0);
                    item.Destroy();
                }
            }

            if (m_ItemSize.sqrMagnitude <= 0)
            {
                m_ItemSize = Vector2.zero;
            }

            m_Prefab = rt;
            if (rt == null)
                return;

            rt.gameObject.SetActive(false);
            if (m_ItemSize.sqrMagnitude <= 0)
            {
                Vector2 size = rt.sizeDelta;
                m_ItemSize.x = m_ItemSize.x > 0 ? m_ItemSize.x : size.x;
                m_ItemSize.y = m_ItemSize.y > 0 ? m_ItemSize.y : size.y;
            }
        }
        private void SetNumItems(uint num)
        {
            m_NumItems = num;
            // if(m_EnterScaleDelay>0)m_EnterScaleDelay = Mathf.Max(0.03f, m_EnterScaleDelay);

            if (!Application.isPlaying || !m_HasInit || !m_Content || !m_Prefab || !m_ViewPort)
                return;
            int fixedCount = (int)GetFixedCount();
            if (fixedCount <= 0) return;
            int count = m_VirtualItems.Count;
            int init = 0;
            if (m_Loop)
                init = m_MaxInit;
            else
                init = Mathf.Min((int)num, m_MaxInit);
            if (count < init)
            {
                for (int i = count; i < init; i++)
                {
                    RectTransform item = CreateItem();
                    m_VirtualItems.Add(new ItemInfo(item));
                }
            }

            m_TotalLine = Mathf.CeilToInt(num * 1.0f / fixedCount);
            UdateFit();
            UpdateContent(fixedCount);
            UpdateLoopContentSize();
            RefreshList();
        }

        void UpdateContent(int fixedCount)
        {
            if (m_MotionType == MotionType.Horizontal)
            {
                float x = margin.left + margin.right + m_ItemSize.x * m_TotalLine +
                          GetSpace(0) * Mathf.Clamp(m_TotalLine, 0, m_TotalLine - 1);
                float y = margin.top + margin.bottom + m_ItemSize.y * fixedCount +
                          GetSpace(1) * Mathf.Clamp(fixedCount, 0, fixedCount - 1);

                m_Content.sizeDelta = new Vector2(x, m_ViewPort.rect.height > y ? m_ViewPort.rect.height : y);
            }
            else
            {
                float x = margin.left + margin.right + m_ItemSize.x * fixedCount +
                          GetSpace(0) * Mathf.Clamp(fixedCount, 0, fixedCount - 1);
                float y = margin.top + margin.bottom + m_ItemSize.y * m_TotalLine +
                          GetSpace(1) * Mathf.Clamp(m_TotalLine, 0, m_TotalLine - 1);
                m_Content.sizeDelta = new Vector2(m_ViewPort.rect.width > x ? m_ViewPort.rect.width : x, y);
            }
            if (m_MinSize.x > 0)
            {
                Vector2 size = m_Content.sizeDelta;
                size.x = Mathf.Max(size.x, m_MinSize.x);
                m_Content.sizeDelta = size;
            }
            if (m_MinSize.y > 0)
            {
                Vector2 size = m_Content.sizeDelta;
                size.y = Mathf.Max(size.y, m_MinSize.y);
                m_Content.sizeDelta = size;
            }
        }
        //------------------------------------------------------
        private void UpdateLoopContentSize()
        {
            if (m_Loop == false)
            {
                return;
            }
            var pos = m_Content.anchoredPosition;
            float viewSize = GetViewSize();
            var contentSize = m_Content.sizeDelta;


            if (m_MotionType == MotionType.Horizontal)
            {
                //:横屏循环边界扩展
                if (pos.x > 0)//x小于0说明到达左边界
                {
                    //往下移动一个循环
                    m_TotalLine *= 2;//每次扩展在当前基础上扩大2倍
                    UpdateContent((int)GetFixedCount());
                    SetContentAnchoredPosition(pos - new Vector2(contentSize.x, 0));
                    //print($"抵达上边界 pos:{pos},viewSize:{viewSize},contentSize:{contentSize}");
                    autoScrollState.Enable = false;//扩展时停止自动滚动
                }
                else if ((Mathf.Abs(pos.x) + viewSize) > contentSize.x)//抵达右边界
                {
                    m_TotalLine *= 2;
                    UpdateContent((int)GetFixedCount());
                    //print($"抵达下边界 pos:{pos},viewSize:{viewSize},contentSize:{contentSize}");
                    autoScrollState.Enable = false;
                }
            }
            else//垂直
            {
                //上边界判断
                if (pos.y <= viewSize)
                {
                    //往下移动一个循环
                    m_TotalLine *= 2;//每次扩展在当前基础上扩大2倍
                    UpdateContent((int)GetFixedCount());
                    SetContentAnchoredPosition(pos + new Vector2(0, contentSize.y));
                    //print($"抵达上边界 pos:{pos},viewSize:{viewSize},contentSize:{contentSize}");
                    autoScrollState.Enable = false;//扩展时停止自动滚动
                }
                else if ((pos.y + viewSize) > contentSize.y)//抵达下边界
                {
                    m_TotalLine *= 2;
                    UpdateContent((int)GetFixedCount());
                    //print($"抵达下边界 pos:{pos},viewSize:{viewSize},contentSize:{contentSize}");
                    autoScrollState.Enable = false;
                }
            }

        }
        //------------------------------------------------------

        private void SetLoop(bool isLoop)
        {
            if (isLoop)
            {
                int count = m_VirtualItems.Count;
                if (count < m_MaxInit)
                {
                    for (int i = count; i < m_MaxInit; i++)
                    {
                        RectTransform item = CreateItem();
                        m_VirtualItems.Add(new ItemInfo(item));
                    }
                }
            }
            m_Loop = isLoop;
            RefreshList();
        }

        private IEnumerator RebuildList()
        {
            if (!Application.isPlaying || !m_HasInit || !m_Content || !m_Prefab || !m_ViewPort)
                yield return null;
            yield return m_WaitForEndOfFrame;

            m_Content.anchorMin = new Vector2(0, 1);
            m_Content.anchorMax = new Vector2(0, 1);
            m_Content.pivot = new Vector2(0, 1);

            SetPrefab(m_Prefab);

            m_MaxInit = Mathf.CeilToInt(GetViewSize() / GetCellSize()) + 1;
            m_MaxInit *= (int)GetFixedCount();
            if (m_MaxInit < m_nInitMaxCount) m_MaxInit = m_nInitMaxCount;

            m_HasInit = true;
            SetNumItems(m_NumItems);
        }

        public void ResetList()
        {
            StartCoroutine(RebuildList());
        }

        public void RefreshList()
        {
            m_bTriggerEnterScale = true;
            m_IsInited = false;
            m_bUpdateList = 2;
            if (m_bFirstUpdate)
            {
                UpdatePosition(true);
                m_bFirstUpdate = false;
            }
            //   UpdatePosition(true);
            //    StartCoroutine(InnerRefreshList());
        }

        IEnumerator InnerRefreshList()
        {
            yield return m_WaitForEndOfFrame;
            m_bTriggerEnterScale = true;
            m_IsInited = false;
            m_bUpdateList = 2;
            if (m_bFirstUpdate)
            {
                UpdatePosition(true);
                m_bFirstUpdate = false;
            }
            //   UpdatePosition(true);
        }

        public void RefreshIndex(int index)
        {
            if (index < 0) return;
            for (int i = 0; i < m_VirtualItems.Count; ++i)
            {
                if (m_VirtualItems[i].actualIndex == index)
                {
                    if ((!m_OutsideUnActive || m_VirtualItems[i].active) && !m_VirtualItems[i].renderable)
                        m_OnItemRender.Invoke(m_VirtualItems[i].actualIndex, m_VirtualItems[i].serialized);
                    return;
                }
            }
        }

        public void RefreshIndexByVirtualIndex(int index)
        {
            if (index < 0) return;
            for (int i = 0; i < m_VirtualItems.Count; ++i)
            {
                if (m_VirtualItems[i].virtualIndex == index)
                {
                    if ((!m_OutsideUnActive || m_VirtualItems[i].active) && !m_VirtualItems[i].renderable)
                        m_OnItemRender.Invoke(m_VirtualItems[i].virtualIndex, m_VirtualItems[i].serialized);
                    return;
                }
            }
        }


        public void RefreshBehindIndex(int index)
        {
            for (int i = 0; i < m_VirtualItems.Count; ++i)
            {
                if (m_VirtualItems[i].actualIndex >= index)
                {
                    if ((!m_OutsideUnActive || m_VirtualItems[i].active) && !m_VirtualItems[i].renderable)
                        m_OnItemRender.Invoke(m_VirtualItems[i].actualIndex, m_VirtualItems[i].serialized);
                }
            }
        }

        private int GetLoopIndex(float offset = 0)
        {
            int index = 0;
            float pos = m_MotionType == MotionType.Horizontal
                ? m_Content.anchoredPosition.x
                : m_Content.anchoredPosition.y;
            pos = Mathf.Round(pos) + offset;
            if (m_Loop)
            {
                if (m_MotionType == MotionType.Horizontal)
                {
                    index = Mathf.FloorToInt(-pos / GetCellSize(0));
                }
                else
                {
                    index = Mathf.FloorToInt(pos / GetCellSize(1));
                }
            }
            else
            {
                if (m_MotionType == MotionType.Horizontal)
                {
                    if (pos >= 0)
                        index = 0;
                    else
                        index = Mathf.FloorToInt(Mathf.Abs(pos + m_Margin.left) / GetCellSize(0));
                }
                else
                {
                    if (pos < 0)
                        index = 0;
                    else
                        index = Mathf.FloorToInt(Mathf.Abs(pos - m_Margin.top) / GetCellSize(1));
                }
            }

            return index;
        }

        private RectTransform CreateItem()
        {
            if (m_Prefab)
            {
                GameObject obj = GameObject.Instantiate(m_Prefab.gameObject) as GameObject;
                obj.transform.SetParent(m_Content);
                obj.transform.localScale = Vector3.one;

                RectTransform rectTransfrom = obj.transform as RectTransform;
                rectTransfrom.sizeDelta = m_ItemSize;
                return rectTransfrom;
            }
            return null;
        }

        private float EaseInOutCubic(float start, float end, float value)//滑动曲线
        {
            value /= 0.5f;
            end -= start;
            if (value < 1f)
            {
                return end * 0.5f * value * value * value + start;
            }
            value -= 2f;
            return end * 0.5f * (value * value * value + 2f) + start;
        }

        public Transform GetItemByIndex(int index)
        {
            if (index < 0)
            {
                return null;
            }

            for (int i = 0; i < m_VirtualItems.Count; ++i)
            {
                if (m_VirtualItems[i].virtualIndex == index)
                {
                    return m_VirtualItems[i].transform;
                }
            }

            //找不到列表子物体情况,滑动到指定位置再次查找
            ScrollTo(index, 0, 0);
            LateUpdate();
            RefreshIndex(index);//强制刷新当前格子数据

            for (int i = 0; i < m_VirtualItems.Count; ++i)
            {
                if (m_VirtualItems[i].virtualIndex == index)
                {
                    return m_VirtualItems[i].transform;
                }
            }


            return null;
        }
        //------------------------------------------------------
        /// <summary>
        /// 获取当前listview的平均索引
        /// 根据当前加载的item index进行平均值计算
        /// </summary>
        /// <returns></returns>
        public int GetCurIndex()
        {
            int index = -1;

            int count = 0;

            for (int i = 0; i < m_VirtualItems.Count; ++i)
            {
                if (m_VirtualItems[i].virtualIndex == int.MaxValue || m_VirtualItems[i].active == false)//过滤掉未计算索引的格子
                {
                    continue;
                }
                index += m_VirtualItems[i].virtualIndex;
                count++;
            }

            if (count == 0)
            {

                return 0;
            }

            return index / count - 1;
        }
        //------------------------------------------------------
        /// <summary>
        /// 通过content位置计算当前索引
        /// </summary>
        /// <returns></returns>
        public int GetCurIndexByContentPos()
        {
            if (content == null)
            {
                return 0;
            }

            Vector2 pos = content.anchoredPosition;
            if (motionType == MotionType.Vertical)
            {
                return (int)pos.y / ((int)itemSize.y + (int)space.y);
            }
            return (int)Mathf.Abs(pos.x) / ((int)itemSize.x + (int)space.x);
        }
        //------------------------------------------------------
        int GetAutoAttachIndex()
        {

            //根据当前滚动,当惯性力小于一定水平时,才开始吸附
            //然后获取当前滚动得index,和偏移得坐标,坐标超过一半才吸附到下一个过去,否则吸附到当前
            int index = 0;
            if (content == null)
            {
                return index;
            }

            Vector2 pos = content.anchoredPosition;

            var itemSizeAndSpace = itemSize + space;

            if (motionType == MotionType.Vertical)
            {
                int curIndex = (int)pos.y / (int)itemSizeAndSpace.y;
                var value = pos.y % (itemSizeAndSpace.y);
                float per = value / itemSizeAndSpace.y;
                bool isNext = per > 0.5f;
                //Debug.Log($"curIndex:{curIndex},per:{per},isNext:{isNext},");

                if (isNext)
                {
                    curIndex++;
                }
                index = curIndex;
            }
            else
            {
                int curIndex = (int)Mathf.Abs(pos.x) / (int)itemSizeAndSpace.x;
                var value = Mathf.Abs(pos.x) % (itemSizeAndSpace.x);
                float per = value / itemSizeAndSpace.x;
                bool isNext = per > 0.5f;
                if (isNext)
                {
                    curIndex++;
                }
                index = curIndex;
            }

            return index;
        }
        //------------------------------------------------------

        public UIReferenceComponent GetItem(int actualIndex)
        {
            if (actualIndex < 0)
            {
                return null;
            }
            for (int i = 0; i < m_VirtualItems.Count; ++i)
            {
                if (m_VirtualItems[i].actualIndex == actualIndex)
                    return m_VirtualItems[i].serialized;
            }

            return null;
        }
        //------------------------------------------------------
        public int GetIndexByItem(GameObject go)
        {
            for (int i = 0; i < m_VirtualItems.Count; i++)
            {
                if (m_VirtualItems[i].transform.gameObject == go)
                {
                    return m_VirtualItems[i].actualIndex;
                }
            }
            return -1;
        }
        //------------------------------------------------------
        public bool GetIsLoadCompleted()
        {
            return m_IsInited && (m_vCoroutineItems == null || m_vCoroutineItems.Count <= 0);
        }
        //------------------------------------------------------
        void Log(string t)
        {
            Debug.Log(t);
        }
        #endregion
    }

}