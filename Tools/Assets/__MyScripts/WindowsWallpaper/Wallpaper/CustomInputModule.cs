using System;
using UnityEngine.Serialization;

namespace UnityEngine.EventSystems
{
    // Token: 0x0200004E RID: 78
    [AddComponentMenu("Event/Custom Input Module")]
    public class CustomInputModule : PointerInputModule
    {
        // Token: 0x06000346 RID: 838 RVA: 0x000198B8 File Offset: 0x00017AB8
        protected CustomInputModule()
        {
        }

        // Token: 0x1700000B RID: 11
        // (get) Token: 0x06000347 RID: 839 RVA: 0x0001990D File Offset: 0x00017B0D
        [Obsolete("Mode is no longer needed on input module as it handles both mouse and keyboard simultaneously.", false)]
        public CustomInputModule.InputMode inputMode
        {
            get
            {
                return CustomInputModule.InputMode.Mouse;
            }
        }

        // Token: 0x1700000C RID: 12
        // (get) Token: 0x06000348 RID: 840 RVA: 0x00019910 File Offset: 0x00017B10
        // (set) Token: 0x06000349 RID: 841 RVA: 0x00019918 File Offset: 0x00017B18
        [Obsolete("allowActivationOnMobileDevice has been deprecated. Use forceModuleActive instead (UnityUpgradable) -> forceModuleActive")]
        public bool allowActivationOnMobileDevice
        {
            get
            {
                return this.m_ForceModuleActive;
            }
            set
            {
                this.m_ForceModuleActive = value;
            }
        }

        // Token: 0x1700000D RID: 13
        // (get) Token: 0x0600034A RID: 842 RVA: 0x00019921 File Offset: 0x00017B21
        // (set) Token: 0x0600034B RID: 843 RVA: 0x00019929 File Offset: 0x00017B29
        [Obsolete("forceModuleActive has been deprecated. There is no need to force the module awake as StandaloneInputModule works for all platforms")]
        public bool forceModuleActive
        {
            get
            {
                return this.m_ForceModuleActive;
            }
            set
            {
                this.m_ForceModuleActive = value;
            }
        }

        // Token: 0x1700000E RID: 14
        // (get) Token: 0x0600034C RID: 844 RVA: 0x00019932 File Offset: 0x00017B32
        // (set) Token: 0x0600034D RID: 845 RVA: 0x0001993A File Offset: 0x00017B3A
        public float inputActionsPerSecond
        {
            get
            {
                return this.m_InputActionsPerSecond;
            }
            set
            {
                this.m_InputActionsPerSecond = value;
            }
        }

        // Token: 0x1700000F RID: 15
        // (get) Token: 0x0600034E RID: 846 RVA: 0x00019943 File Offset: 0x00017B43
        // (set) Token: 0x0600034F RID: 847 RVA: 0x0001994B File Offset: 0x00017B4B
        public float repeatDelay
        {
            get
            {
                return this.m_RepeatDelay;
            }
            set
            {
                this.m_RepeatDelay = value;
            }
        }

        // Token: 0x17000010 RID: 16
        // (get) Token: 0x06000350 RID: 848 RVA: 0x00019954 File Offset: 0x00017B54
        // (set) Token: 0x06000351 RID: 849 RVA: 0x0001995C File Offset: 0x00017B5C
        public string horizontalAxis
        {
            get
            {
                return this.m_HorizontalAxis;
            }
            set
            {
                this.m_HorizontalAxis = value;
            }
        }

        // Token: 0x17000011 RID: 17
        // (get) Token: 0x06000352 RID: 850 RVA: 0x00019965 File Offset: 0x00017B65
        // (set) Token: 0x06000353 RID: 851 RVA: 0x0001996D File Offset: 0x00017B6D
        public string verticalAxis
        {
            get
            {
                return this.m_VerticalAxis;
            }
            set
            {
                this.m_VerticalAxis = value;
            }
        }

        // Token: 0x17000012 RID: 18
        // (get) Token: 0x06000354 RID: 852 RVA: 0x00019976 File Offset: 0x00017B76
        // (set) Token: 0x06000355 RID: 853 RVA: 0x0001997E File Offset: 0x00017B7E
        public string submitButton
        {
            get
            {
                return this.m_SubmitButton;
            }
            set
            {
                this.m_SubmitButton = value;
            }
        }

        // Token: 0x17000013 RID: 19
        // (get) Token: 0x06000356 RID: 854 RVA: 0x00019987 File Offset: 0x00017B87
        // (set) Token: 0x06000357 RID: 855 RVA: 0x0001998F File Offset: 0x00017B8F
        public string cancelButton
        {
            get
            {
                return this.m_CancelButton;
            }
            set
            {
                this.m_CancelButton = value;
            }
        }

        // Token: 0x06000358 RID: 856 RVA: 0x00019998 File Offset: 0x00017B98
        private bool ShouldIgnoreEventsOnNoFocus()
        {
            return true;
        }

        // Token: 0x06000359 RID: 857 RVA: 0x0001999B File Offset: 0x00017B9B
        public override void UpdateModule()
        {
            //if (!eventSystem.isFocused && ShouldIgnoreEventsOnNoFocus())//取消在无焦点时忽略事件的判断,壁纸模式下需要响应事件
            //{
            //    ReleasePointerDrags();
            //    return;
            //}

            this.m_LastMousePosition = this.m_MousePosition;
            this.m_MousePosition = base.input.mousePosition;
        }

        // Token: 0x0600035A RID: 858 RVA: 0x000199BC File Offset: 0x00017BBC
        private void ReleaseMouse(PointerEventData pointerEvent, GameObject currentOverGo)
        {
            ExecuteEvents.Execute<IPointerUpHandler>(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerUpHandler);
            GameObject eventHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentOverGo);
            if (pointerEvent.pointerClick == eventHandler && pointerEvent.eligibleForClick)
            {
                ExecuteEvents.Execute<IPointerClickHandler>(pointerEvent.pointerClick, pointerEvent, ExecuteEvents.pointerClickHandler);
            }
            if (pointerEvent.pointerDrag != null && pointerEvent.dragging)
            {
                ExecuteEvents.ExecuteHierarchy<IDropHandler>(currentOverGo, pointerEvent, ExecuteEvents.dropHandler);
            }
            pointerEvent.eligibleForClick = false;
            pointerEvent.pointerPress = null;
            pointerEvent.rawPointerPress = null;
            pointerEvent.pointerClick = null;
            if (pointerEvent.pointerDrag != null && pointerEvent.dragging)
            {
                ExecuteEvents.Execute<IEndDragHandler>(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.endDragHandler);
            }
            pointerEvent.dragging = false;
            pointerEvent.pointerDrag = null;
            if (currentOverGo != pointerEvent.pointerEnter)
            {
                base.HandlePointerExitAndEnter(pointerEvent, null);
                base.HandlePointerExitAndEnter(pointerEvent, currentOverGo);
            }
            this.m_InputPointerEvent = pointerEvent;
        }

        // Token: 0x0600035B RID: 859 RVA: 0x00019AA4 File Offset: 0x00017CA4
        public override bool ShouldActivateModule()
        {
            if (!base.ShouldActivateModule())
            {
                return false;
            }
            bool flag = this.m_ForceModuleActive;
            flag |= base.input.GetButtonDown(this.m_SubmitButton);
            flag |= base.input.GetButtonDown(this.m_CancelButton);
            flag |= !Mathf.Approximately(base.input.GetAxisRaw(this.m_HorizontalAxis), 0f);
            flag |= !Mathf.Approximately(base.input.GetAxisRaw(this.m_VerticalAxis), 0f);
            flag |= ((this.m_MousePosition - this.m_LastMousePosition).sqrMagnitude > 0f);
            flag |= base.input.GetMouseButtonDown(0);
            if (base.input.touchCount > 0)
            {
                flag = true;
            }
            return flag;
        }

        // Token: 0x0600035C RID: 860 RVA: 0x00019B70 File Offset: 0x00017D70
        public override void ActivateModule()
        {
            base.ActivateModule();
            this.m_MousePosition = base.input.mousePosition;
            this.m_LastMousePosition = base.input.mousePosition;
            GameObject gameObject = base.eventSystem.currentSelectedGameObject;
            if (gameObject == null)
            {
                gameObject = base.eventSystem.firstSelectedGameObject;
            }
            base.eventSystem.SetSelectedGameObject(gameObject, this.GetBaseEventData());
        }

        // Token: 0x0600035D RID: 861 RVA: 0x00019BD8 File Offset: 0x00017DD8
        public override void DeactivateModule()
        {
            base.DeactivateModule();
            base.ClearSelection();
        }

        // Token: 0x0600035E RID: 862 RVA: 0x00019BE8 File Offset: 0x00017DE8
        public override void Process()
        {
            //if (!eventSystem.isFocused && ShouldIgnoreEventsOnNoFocus())//取消在无焦点时忽略事件的判断,壁纸模式下需要响应事件
            //    return;

            bool flag = this.SendUpdateEventToSelectedObject();
            if (!this.ProcessTouchEvents() && base.input.mousePresent)
            {
                this.ProcessMouseEvent();
            }
            if (base.eventSystem.sendNavigationEvents)
            {
                if (!flag)
                {
                    flag |= this.SendMoveEventToSelectedObject();
                }
                if (!flag)
                {
                    this.SendSubmitEventToSelectedObject();
                }
            }
        }

        // Token: 0x0600035F RID: 863 RVA: 0x00019C3C File Offset: 0x00017E3C
        private bool ProcessTouchEvents()
        {
            for (int i = 0; i < base.input.touchCount; i++)
            {
                Touch touch = base.input.GetTouch(i);
                if (touch.type != (TouchType)1)
                {
                    bool pressed;
                    bool flag;
                    PointerEventData touchPointerEventData = base.GetTouchPointerEventData(touch, out pressed, out flag);
                    this.ProcessTouchPress(touchPointerEventData, pressed, flag);
                    if (!flag)
                    {
                        this.ProcessMove(touchPointerEventData);
                        this.ProcessDrag(touchPointerEventData);
                    }
                    else
                    {
                        base.RemovePointerData(touchPointerEventData);
                    }
                }
            }
            return base.input.touchCount > 0;
        }

        // Token: 0x06000360 RID: 864 RVA: 0x00019CB8 File Offset: 0x00017EB8
        protected void ProcessTouchPress(PointerEventData pointerEvent, bool pressed, bool released)
        {
            GameObject gameObject = pointerEvent.pointerCurrentRaycast.gameObject;
            if (pressed)
            {
                pointerEvent.eligibleForClick = true;
                pointerEvent.delta = Vector2.zero;
                pointerEvent.dragging = false;
                pointerEvent.useDragThreshold = true;
                pointerEvent.pressPosition = pointerEvent.position;
                pointerEvent.pointerPressRaycast = pointerEvent.pointerCurrentRaycast;
                base.DeselectIfSelectionChanged(gameObject, pointerEvent);
                if (pointerEvent.pointerEnter != gameObject)
                {
                    base.HandlePointerExitAndEnter(pointerEvent, gameObject);
                    pointerEvent.pointerEnter = gameObject;
                }
                GameObject gameObject2 = ExecuteEvents.ExecuteHierarchy<IPointerDownHandler>(gameObject, pointerEvent, ExecuteEvents.pointerDownHandler);
                GameObject eventHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(gameObject);
                if (gameObject2 == null)
                {
                    gameObject2 = eventHandler;
                }
                float unscaledTime = Time.unscaledTime;
                if (gameObject2 == pointerEvent.lastPress)
                {
                    if (unscaledTime - pointerEvent.clickTime < 0.3f)
                    {
                        int clickCount = pointerEvent.clickCount + 1;
                        pointerEvent.clickCount = clickCount;
                    }
                    else
                    {
                        pointerEvent.clickCount = 1;
                    }
                    pointerEvent.clickTime = unscaledTime;
                }
                else
                {
                    pointerEvent.clickCount = 1;
                }
                pointerEvent.pointerPress = gameObject2;
                pointerEvent.rawPointerPress = gameObject;
                pointerEvent.pointerClick = eventHandler;
                pointerEvent.clickTime = unscaledTime;
                pointerEvent.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(gameObject);
                if (pointerEvent.pointerDrag != null)
                {
                    ExecuteEvents.Execute<IInitializePotentialDragHandler>(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.initializePotentialDrag);
                }
            }
            if (released)
            {
                ExecuteEvents.Execute<IPointerUpHandler>(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerUpHandler);
                GameObject eventHandler2 = ExecuteEvents.GetEventHandler<IPointerClickHandler>(gameObject);
                if (pointerEvent.pointerClick == eventHandler2 && pointerEvent.eligibleForClick)
                {
                    ExecuteEvents.Execute<IPointerClickHandler>(pointerEvent.pointerClick, pointerEvent, ExecuteEvents.pointerClickHandler);
                }
                if (pointerEvent.pointerDrag != null && pointerEvent.dragging)
                {
                    ExecuteEvents.ExecuteHierarchy<IDropHandler>(gameObject, pointerEvent, ExecuteEvents.dropHandler);
                }
                pointerEvent.eligibleForClick = false;
                pointerEvent.pointerPress = null;
                pointerEvent.rawPointerPress = null;
                pointerEvent.pointerClick = null;
                if (pointerEvent.pointerDrag != null && pointerEvent.dragging)
                {
                    ExecuteEvents.Execute<IEndDragHandler>(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.endDragHandler);
                }
                pointerEvent.dragging = false;
                pointerEvent.pointerDrag = null;
                ExecuteEvents.ExecuteHierarchy<IPointerExitHandler>(pointerEvent.pointerEnter, pointerEvent, ExecuteEvents.pointerExitHandler);
                pointerEvent.pointerEnter = null;
            }
            this.m_InputPointerEvent = pointerEvent;
        }

        // Token: 0x06000361 RID: 865 RVA: 0x00019ED0 File Offset: 0x000180D0
        protected bool SendSubmitEventToSelectedObject()
        {
            if (base.eventSystem.currentSelectedGameObject == null)
            {
                return false;
            }
            BaseEventData baseEventData = this.GetBaseEventData();
            if (base.input.GetButtonDown(this.m_SubmitButton))
            {
                ExecuteEvents.Execute<ISubmitHandler>(base.eventSystem.currentSelectedGameObject, baseEventData, ExecuteEvents.submitHandler);
            }
            if (base.input.GetButtonDown(this.m_CancelButton))
            {
                ExecuteEvents.Execute<ICancelHandler>(base.eventSystem.currentSelectedGameObject, baseEventData, ExecuteEvents.cancelHandler);
            }
            return baseEventData.used;
        }

        // Token: 0x06000362 RID: 866 RVA: 0x00019F54 File Offset: 0x00018154
        private Vector2 GetRawMoveVector()
        {
            Vector2 zero = Vector2.zero;
            zero.x = base.input.GetAxisRaw(this.m_HorizontalAxis);
            zero.y = base.input.GetAxisRaw(this.m_VerticalAxis);
            if (base.input.GetButtonDown(this.m_HorizontalAxis))
            {
                if (zero.x < 0f)
                {
                    zero.x = -1f;
                }
                if (zero.x > 0f)
                {
                    zero.x = 1f;
                }
            }
            if (base.input.GetButtonDown(this.m_VerticalAxis))
            {
                if (zero.y < 0f)
                {
                    zero.y = -1f;
                }
                if (zero.y > 0f)
                {
                    zero.y = 1f;
                }
            }
            return zero;
        }

        // Token: 0x06000363 RID: 867 RVA: 0x0001A024 File Offset: 0x00018224
        protected bool SendMoveEventToSelectedObject()
        {
            float unscaledTime = Time.unscaledTime;
            Vector2 rawMoveVector = this.GetRawMoveVector();
            if (Mathf.Approximately(rawMoveVector.x, 0f) && Mathf.Approximately(rawMoveVector.y, 0f))
            {
                this.m_ConsecutiveMoveCount = 0;
                return false;
            }
            bool flag = Vector2.Dot(rawMoveVector, this.m_LastMoveVector) > 0f;
            if (flag && this.m_ConsecutiveMoveCount == 1)
            {
                if (unscaledTime <= this.m_PrevActionTime + this.m_RepeatDelay)
                {
                    return false;
                }
            }
            else if (unscaledTime <= this.m_PrevActionTime + 1f / this.m_InputActionsPerSecond)
            {
                return false;
            }
            AxisEventData axisEventData = this.GetAxisEventData(rawMoveVector.x, rawMoveVector.y, 0.6f);
            if (axisEventData.moveDir != (MoveDirection)4)
            {
                ExecuteEvents.Execute<IMoveHandler>(base.eventSystem.currentSelectedGameObject, axisEventData, ExecuteEvents.moveHandler);
                if (!flag)
                {
                    this.m_ConsecutiveMoveCount = 0;
                }
                this.m_ConsecutiveMoveCount++;
                this.m_PrevActionTime = unscaledTime;
                this.m_LastMoveVector = rawMoveVector;
            }
            else
            {
                this.m_ConsecutiveMoveCount = 0;
            }
            return axisEventData.used;
        }

        // Token: 0x06000364 RID: 868 RVA: 0x0001A122 File Offset: 0x00018322
        protected void ProcessMouseEvent()
        {
            this.ProcessMouseEvent(0);
        }

        // Token: 0x06000365 RID: 869 RVA: 0x0001A12B File Offset: 0x0001832B
        [Obsolete("This method is no longer checked, overriding it with return true does nothing!")]
        protected virtual bool ForceAutoSelect()
        {
            return false;
        }

        // Token: 0x06000366 RID: 870 RVA: 0x0001A130 File Offset: 0x00018330
        protected void ProcessMouseEvent(int id)
        {
            PointerInputModule.MouseState mousePointerEventData = this.GetMousePointerEventData(id);
            PointerInputModule.MouseButtonEventData eventData = mousePointerEventData.GetButtonState(0).eventData;
            this.m_CurrentFocusedGameObject = eventData.buttonData.pointerCurrentRaycast.gameObject;
            this.ProcessMousePress(eventData);
            this.ProcessMove(eventData.buttonData);
            this.ProcessDrag(eventData.buttonData);
            this.ProcessMousePress(mousePointerEventData.GetButtonState((PointerEventData.InputButton)1).eventData);
            this.ProcessDrag(mousePointerEventData.GetButtonState((PointerEventData.InputButton)1).eventData.buttonData);
            this.ProcessMousePress(mousePointerEventData.GetButtonState((PointerEventData.InputButton)2).eventData);
            this.ProcessDrag(mousePointerEventData.GetButtonState((PointerEventData.InputButton)2).eventData.buttonData);
            if (!Mathf.Approximately(eventData.buttonData.scrollDelta.sqrMagnitude, 0f))
            {
                ExecuteEvents.ExecuteHierarchy<IScrollHandler>(ExecuteEvents.GetEventHandler<IScrollHandler>(eventData.buttonData.pointerCurrentRaycast.gameObject), eventData.buttonData, ExecuteEvents.scrollHandler);
            }
        }

        // Token: 0x06000367 RID: 871 RVA: 0x0001A224 File Offset: 0x00018424
        protected bool SendUpdateEventToSelectedObject()
        {
            if (base.eventSystem.currentSelectedGameObject == null)
            {
                return false;
            }
            BaseEventData baseEventData = this.GetBaseEventData();
            ExecuteEvents.Execute<IUpdateSelectedHandler>(base.eventSystem.currentSelectedGameObject, baseEventData, ExecuteEvents.updateSelectedHandler);
            return baseEventData.used;
        }

        // Token: 0x06000368 RID: 872 RVA: 0x0001A26C File Offset: 0x0001846C
        protected void ProcessMousePress(PointerInputModule.MouseButtonEventData data)
        {
            PointerEventData buttonData = data.buttonData;
            GameObject gameObject = buttonData.pointerCurrentRaycast.gameObject;
            if (data.PressedThisFrame())
            {
                buttonData.eligibleForClick = true;
                buttonData.delta = Vector2.zero;
                buttonData.dragging = false;
                buttonData.useDragThreshold = true;
                buttonData.pressPosition = buttonData.position;
                buttonData.pointerPressRaycast = buttonData.pointerCurrentRaycast;
                base.DeselectIfSelectionChanged(gameObject, buttonData);
                GameObject gameObject2 = ExecuteEvents.ExecuteHierarchy<IPointerDownHandler>(gameObject, buttonData, ExecuteEvents.pointerDownHandler);
                GameObject eventHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(gameObject);
                if (gameObject2 == null)
                {
                    gameObject2 = eventHandler;
                }
                float unscaledTime = Time.unscaledTime;
                if (gameObject2 == buttonData.lastPress)
                {
                    if (unscaledTime - buttonData.clickTime < 0.3f)
                    {
                        PointerEventData pointerEventData = buttonData;
                        int clickCount = pointerEventData.clickCount + 1;
                        pointerEventData.clickCount = clickCount;
                    }
                    else
                    {
                        buttonData.clickCount = 1;
                    }
                    buttonData.clickTime = unscaledTime;
                }
                else
                {
                    buttonData.clickCount = 1;
                }
                buttonData.pointerPress = gameObject2;
                buttonData.rawPointerPress = gameObject;
                buttonData.pointerClick = eventHandler;
                buttonData.clickTime = unscaledTime;
                buttonData.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(gameObject);
                if (buttonData.pointerDrag != null)
                {
                    ExecuteEvents.Execute<IInitializePotentialDragHandler>(buttonData.pointerDrag, buttonData, ExecuteEvents.initializePotentialDrag);
                }
                this.m_InputPointerEvent = buttonData;
            }
            if (data.ReleasedThisFrame())
            {
                this.ReleaseMouse(buttonData, gameObject);
            }
        }

        // Token: 0x06000369 RID: 873 RVA: 0x0001A3AD File Offset: 0x000185AD
        protected GameObject GetCurrentFocusedGameObject()
        {
            return this.m_CurrentFocusedGameObject;
        }

        // Token: 0x0400028F RID: 655
        private float m_PrevActionTime;

        // Token: 0x04000290 RID: 656
        private Vector2 m_LastMoveVector;

        // Token: 0x04000291 RID: 657
        private int m_ConsecutiveMoveCount;

        // Token: 0x04000292 RID: 658
        private Vector2 m_LastMousePosition;

        // Token: 0x04000293 RID: 659
        private Vector2 m_MousePosition;

        // Token: 0x04000294 RID: 660
        private GameObject m_CurrentFocusedGameObject;

        // Token: 0x04000295 RID: 661
        private PointerEventData m_InputPointerEvent;

        // Token: 0x04000296 RID: 662
        [SerializeField]
        private string m_HorizontalAxis = "Horizontal";

        // Token: 0x04000297 RID: 663
        [SerializeField]
        private string m_VerticalAxis = "Vertical";

        // Token: 0x04000298 RID: 664
        [SerializeField]
        private string m_SubmitButton = "Submit";

        // Token: 0x04000299 RID: 665
        [SerializeField]
        private string m_CancelButton = "Cancel";

        // Token: 0x0400029A RID: 666
        [SerializeField]
        private float m_InputActionsPerSecond = 10f;

        // Token: 0x0400029B RID: 667
        [SerializeField]
        private float m_RepeatDelay = 0.5f;

        // Token: 0x0400029C RID: 668
        [SerializeField]
        [FormerlySerializedAs("m_AllowActivationOnMobileDevice")]
        [HideInInspector]
        private bool m_ForceModuleActive;

        // Token: 0x020000DE RID: 222
        [Obsolete("Mode is no longer needed on input module as it handles both mouse and keyboard simultaneously.", false)]
        public enum InputMode
        {
            // Token: 0x040005DC RID: 1500
            Mouse,
            // Token: 0x040005DD RID: 1501
            Buttons
        }
    }
}
