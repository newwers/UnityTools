/********************************************************************
生成日期:	1:11:2020 10:06
类    名: 	EventTriggerListener
作    者:	HappLI
描    述:	UI 公共监听
*********************************************************************/
using System;
using Framework.Plugin.AT;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using TopGame.Data;
using TopGame.Core;
using Framework.Core;
using FMODUnity;
#if UNITY_EDITOR
using UnityEditor;
using System.Reflection;
#endif
namespace TopGame.UI
{
    [UIWidgetExport]
    public class EventTriggerListener : EventTrigger
    {
        public UIParamArgv paramArgvs = new UIParamArgv();
        UIPenetrate m_Parentrate = null;

        //按钮缩放
        public enum ButtonAnimation
        {
            None,
            Scale,
            ScaleHightlight,
            /// <summary>
            /// 缩放并且替换图标
            /// </summary>
            ScaleAndPicture,
        }

        public ButtonAnimation animationType = ButtonAnimation.Scale;
        public float scaleDelta = 0.9f;
        public float scaleDeltaTime = 0.05f;
        public Transform scalerTarget = null;
        public Graphic hightGraphic;
        public Color highlightColor = new Color(0.63f, 0.63f, 0.63f, 1);
        public Sprite hightlightSprite;

        Vector3 m_originalScale = Vector3.one;
        Color m_originalColor = Color.white;

        //是否播放默认点击音效
        public bool isPlayCommonClickSound = true;
        public int PlayClickSoundId = 0;
        

        //[SerializeField]
        public FMODUnity.StudioEventEmitter pAudioEvent;

        public delegate void VoidDelegate(GameObject go, params VariablePoolAble[] param);
        [NonSerialized]public VoidDelegate onClick;
        [NonSerialized]public VoidDelegate onDown;
        [NonSerialized]public VoidDelegate onEnter;
        [NonSerialized]public VoidDelegate onExit;
        [NonSerialized]public VoidDelegate onUp;
        [NonSerialized]public VoidDelegate onSelect;
        [NonSerialized]public VoidDelegate onUpdateSelect;
        [NonSerialized]public VoidDelegate onDrag;
        [NonSerialized]public VoidDelegate onDrop;
        [NonSerialized]public VoidDelegate onDeselect;
        [NonSerialized]public VoidDelegate onScroll;
        [NonSerialized]public VoidDelegate onMove;
        [NonSerialized]public VoidDelegate onInitializePotentialDrag;
        [NonSerialized]public VoidDelegate onBeginDrag;
        [NonSerialized]public VoidDelegate onEndDrag;
        [NonSerialized]public VoidDelegate onSubmit;
        [NonSerialized]public VoidDelegate onCancel;
		
		public delegate void VoidEventDelegate(GameObject go, BaseEventData evtData, params VariablePoolAble[] param);
        [NonSerialized]public VoidEventDelegate onClickEvent;
        [NonSerialized]public VoidEventDelegate onDownEvent;
        [NonSerialized]public VoidEventDelegate onEnterEvent;
        [NonSerialized]public VoidEventDelegate onExitEvent;
        [NonSerialized]public VoidEventDelegate onUpEvent;
        [NonSerialized]public VoidEventDelegate onSelectEvent;
        [NonSerialized]public VoidEventDelegate onUpdateSelectEvent;
        [NonSerialized]public VoidEventDelegate onDragEvent;
        [NonSerialized]public VoidEventDelegate onDropEvent;
        [NonSerialized]public VoidEventDelegate onDeselectEvent;
        [NonSerialized]public VoidEventDelegate onScrollEvent;
        [NonSerialized]public VoidEventDelegate onMoveEvent;
        [NonSerialized]public VoidEventDelegate onInitializePotentialDragEvent;
        [NonSerialized]public VoidEventDelegate onBeginDragEvent;
        [NonSerialized]public VoidEventDelegate onEndDragEvent;
        [NonSerialized]public VoidEventDelegate onSubmitEvent;
        [NonSerialized]public VoidEventDelegate onCancelEvent;
        
        public bool IsCommonItem = false;

        public UserActionData userActionData;

        public BtnUnLockData btnUnLockData;

        [NonSerialized]
        public UIRuntimeParamArgvs param = new UIRuntimeParamArgvs();

        private Framework.Plugin.Guide.GuideGuid m_GuideGuid = null;
        private int m_nListIndex = -1;

        public VariablePoolAble param1
        {
            get { return param.param1; }
            set { param.param1 = value; }
        }

        public VariablePoolAble param2
        {
            get { return param.param2; }
            set { param.param2 = value; }
        }
        public VariablePoolAble param3
        {
            get { return param.param3; }
            set { param.param3 = value; }
        }

        public VariablePoolAble param4
        {
            get { return param.param4; }
            set { param.param4 = value; }
        }

        public int listIndex
        {
            get { return m_nListIndex; }
            set { m_nListIndex = value; }
        }
        public ADParamArgv adParam;
        bool CanFire(Vector2 Delta)
        {
            float sensitivity = 0.1f;
            if (Data.GlobalSetting.Instance != null) sensitivity = Data.GlobalSetting.Instance.fTouchUISensitivity;
            if (Delta.sqrMagnitude >= sensitivity* sensitivity) return false;
            return true;
        }
        //------------------------------------------------------
        public void SetParentrate(UIPenetrate parentrate)
        {
            m_Parentrate = parentrate;
        }
        //------------------------------------------------------
        void CheckParam()
        {
            param.FillParam(paramArgvs);
        }
        //------------------------------------------------------
        GameObject GetTriggerObject()
        {
            if (paramArgvs.agentTrigger) return paramArgvs.agentTrigger;
            return this.gameObject;
        }
        //------------------------------------------------------
        bool OnUIWidgetTrigger(BaseEventData eventData, Base.EUIEventType eventType)
        {
            if (Framework.Module.ModuleManager.mainFramework == null) return false;
            GameFramework gameFramework = Framework.Module.ModuleManager.mainFramework as GameFramework;
            if (gameFramework == null) return false;
            return gameFramework.OnUIWidgetTrigger(this, eventData, eventType, m_GuideGuid ? m_GuideGuid.Guid : -1, m_nListIndex, param1, param1, param2, param3, param4);
        }
        //------------------------------------------------------
        public override void OnPointerClick(PointerEventData eventData)
        {
            if (eventData != null && eventData.dragging && !CanFire(eventData.position - eventData.pressPosition))
                return;

            if (pAudioEvent != null)
                pAudioEvent.Play();
            else
                RuntimeManager.PlayOneShot("event:/Common_UI/Click");

            if (OnUIWidgetTrigger(eventData, Base.EUIEventType.onClick))
                return;

            if (onClickEvent != null) onClickEvent(GetTriggerObject(), eventData, param.param1, param.param2, param3, param4);
            OnATEvent(GetTriggerObject(), Base.EUIEventType.onClick, param);

            if (onClick != null) onClick(GetTriggerObject(), param.param1, param.param2, param3, param4);

            if (m_Parentrate != null) m_Parentrate.OnPointerClick(eventData);


        }
        public override void OnPointerDown(PointerEventData eventData)
        {
            DoPressAction();

            if (OnUIWidgetTrigger(eventData, Base.EUIEventType.onDown))
                return;
            if (onDownEvent != null) onDownEvent(GetTriggerObject(), eventData, param.param1, param.param2, param3, param4);
            OnATEvent(GetTriggerObject(), Base.EUIEventType.onDown, param);
            if (onDown != null) onDown(GetTriggerObject(), param.param1, param.param2, param3, param4);

            if (m_Parentrate != null) m_Parentrate.OnPointerDown(eventData);
        }
        public override void OnPointerEnter(PointerEventData eventData)
        {
            if (OnUIWidgetTrigger(eventData, Base.EUIEventType.onEnter))
                return;
            if (onEnterEvent != null) onEnterEvent(GetTriggerObject(), eventData, param.param1, param.param2, param3, param4);
            //OnATEvent(GetTriggerObject(), Base.EATEventType.onEnter, param);
            if (onEnter != null) onEnter(GetTriggerObject(), param.param1, param.param2, param3, param4);
            if (m_Parentrate != null) m_Parentrate.OnPointerEnter(eventData);
        }
        public override void OnPointerExit(PointerEventData eventData)
        {
            if (OnUIWidgetTrigger(eventData, Base.EUIEventType.onExit))
                return;
            if (onExitEvent != null) onExitEvent(GetTriggerObject(), eventData, param.param1, param.param2, param3, param4);
            //OnATEvent(GetTriggerObject(), Base.EATEventType.onExit, param);
            if (onExit != null) onExit(GetTriggerObject(), param.param1, param.param2, param3, param4);
            if (m_Parentrate != null) m_Parentrate.OnPointerExit(eventData);
        }
        public override void OnPointerUp(PointerEventData eventData)
        {
            DoUpAction();
            if (OnUIWidgetTrigger(eventData, Base.EUIEventType.onUp))
                return;
            if (onUpEvent != null) onUpEvent(GetTriggerObject(), eventData, param.param1, param.param2, param3, param4);
            OnATEvent(GetTriggerObject(), Base.EUIEventType.onUp, param);
            if (onUp != null) onUp(GetTriggerObject(), param.param1, param.param2, param3, param4);
            if (m_Parentrate != null) m_Parentrate.OnPointerUp(eventData);
        }
        public override void OnSelect(BaseEventData eventData)
        {
            if (OnUIWidgetTrigger(eventData, Base.EUIEventType.onSelect))
                return;
            if (onSelectEvent != null) onSelectEvent(GetTriggerObject(), eventData, param.param1, param.param2, param3, param4);
            OnATEvent(GetTriggerObject(), Base.EUIEventType.onSelect, param);
            if (onSelect != null) onSelect(GetTriggerObject(), param.param1, param.param2, param3, param4);
            if (m_Parentrate != null) m_Parentrate.OnSelect(eventData);
        }
        public override void OnUpdateSelected(BaseEventData eventData)
        {
            if (OnUIWidgetTrigger(eventData, Base.EUIEventType.onUpdateSelect))
                return;
            if (onUpdateSelectEvent != null) onUpdateSelectEvent(GetTriggerObject(), eventData, param.param1, param.param2, param3, param4);
            OnATEvent(GetTriggerObject(), Base.EUIEventType.onUpdateSelect, param);
            if (onUpdateSelect != null) onUpdateSelect(GetTriggerObject(), param.param1, param.param2, param3, param4);
            if (m_Parentrate != null) m_Parentrate.OnUpdateSelected(eventData);
        }

        public override void OnDrag(PointerEventData eventData)
        {
            if (OnUIWidgetTrigger(eventData, Base.EUIEventType.onDrag))
                return;
            if (onDragEvent != null) onDragEvent(GetTriggerObject(), eventData, param.param1, param.param2, param3, param4);
            OnATEvent(GetTriggerObject(), Base.EUIEventType.onDrag, param);
            if (onDrag != null) onDrag(GetTriggerObject(), param.param1, param.param2, param3, param4);
            if (m_Parentrate != null) m_Parentrate.OnDrag(eventData);
        }

        public override void OnDrop(PointerEventData eventData)
        {
            if (OnUIWidgetTrigger(eventData, Base.EUIEventType.onDrop))
                return;
            if (onDropEvent != null) onDropEvent(GetTriggerObject(), eventData, param.param1, param.param2, param3, param4);
            OnATEvent(GetTriggerObject(), Base.EUIEventType.onDrop, param);
            if (onDrop != null) onDrop(GetTriggerObject(), param.param1, param.param2, param3, param4);
            if (m_Parentrate != null) m_Parentrate.OnDrop(eventData);
        }

        public override void OnDeselect(BaseEventData eventData)
        {
            if (OnUIWidgetTrigger(eventData, Base.EUIEventType.onDeselect))
                return;
            if (onDeselectEvent != null) onDeselectEvent(GetTriggerObject(), eventData, param.param1, param.param2, param3, param4);
            OnATEvent(GetTriggerObject(), Base.EUIEventType.onDeselect, param);
            if (onDeselect != null) onDeselect(GetTriggerObject(), param.param1, param.param2, param3, param4);
            if (m_Parentrate != null) m_Parentrate.OnDeselect(eventData);
        }

        public override void OnScroll(PointerEventData eventData)
        {
            if (OnUIWidgetTrigger(eventData, Base.EUIEventType.onScroll))
                return;
            if (onScrollEvent != null) onScrollEvent(GetTriggerObject(), eventData, param.param1, param.param2, param3, param4);
            OnATEvent(GetTriggerObject(), Base.EUIEventType.onScroll, param);
            if (onScroll != null) onScroll(GetTriggerObject(), param.param1, param.param2, param3, param4);
            if (m_Parentrate != null) m_Parentrate.OnScroll(eventData);
        }

        public override void OnMove(AxisEventData eventData)
        {
            if (OnUIWidgetTrigger(eventData, Base.EUIEventType.onMove))
                return;
            if (onMoveEvent != null) onMoveEvent(GetTriggerObject(), eventData, param.param1, param.param2, param3, param4);
            if (onMove != null) onMove(GetTriggerObject(), param.param1, param.param2, param3, param4);
            if (m_Parentrate != null) m_Parentrate.OnMove(eventData);
        }

        public override void OnInitializePotentialDrag(PointerEventData eventData)
        {
            if (onInitializePotentialDragEvent != null) onInitializePotentialDragEvent(GetTriggerObject(), eventData, param.param1, param.param2, param3, param4);
            if (onInitializePotentialDrag != null) onInitializePotentialDrag(GetTriggerObject(), param.param1, param.param2, param3, param4);
        }

        public override void OnBeginDrag(PointerEventData eventData)
        {
            if (OnUIWidgetTrigger(eventData, Base.EUIEventType.onBeginDrag))
                return;
            if (onBeginDragEvent != null) onBeginDragEvent(GetTriggerObject(), eventData, param.param1, param.param2, param3, param4);
            OnATEvent(GetTriggerObject(), Base.EUIEventType.onBeginDrag, param);
            if (onBeginDrag != null) onBeginDrag(GetTriggerObject(), param.param1, param.param2, param3, param4);
            if (m_Parentrate != null) m_Parentrate.OnBeginDrag(eventData);
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            if (OnUIWidgetTrigger(eventData, Base.EUIEventType.onEndDrag))
                return;
            if (onEndDragEvent != null) onEndDragEvent(GetTriggerObject(), eventData, param.param1, param.param2, param3, param4);
            OnATEvent(GetTriggerObject(), Base.EUIEventType.onEndDrag, param);
            if (onEndDrag != null) onEndDrag(GetTriggerObject(), param.param1, param.param2, param3, param4);
            if (m_Parentrate != null) m_Parentrate.OnEndDrag(eventData);
        }

        public override void OnSubmit(BaseEventData eventData)
        {
            if (OnUIWidgetTrigger(eventData, Base.EUIEventType.onSubmit))
                return;
            if (onSubmitEvent != null) onSubmitEvent(GetTriggerObject(), eventData, param.param1, param.param2, param3, param4);
            OnATEvent(GetTriggerObject(), Base.EUIEventType.onSubmit, param);
            if (onSubmit != null) onSubmit(GetTriggerObject(), param.param1, param.param2, param3, param4);
            if (m_Parentrate != null) m_Parentrate.OnSubmit(eventData);
        }

        public override void OnCancel(BaseEventData eventData)
        {
            if (OnUIWidgetTrigger(eventData, Base.EUIEventType.onCancel))
                return;
            if (onCancelEvent != null) onCancelEvent(GetTriggerObject(), eventData, param.param1, param.param2, param3, param4);
            OnATEvent(GetTriggerObject(), Base.EUIEventType.onCancel, param);
            if (onCancel != null) onCancel(GetTriggerObject(), param.param1, param.param2, param3, param4);
            if (m_Parentrate != null) m_Parentrate.OnCancel(eventData);
        }

        public static EventTriggerListener Get(GameObject go)
        {
            if (go == null) return null;
            EventTriggerListener listener = go.GetComponent<EventTriggerListener>();
            if (listener == null) listener = go.AddComponent<EventTriggerListener>();
            return listener;
        }

        public void SetGuideGuid(Framework.Plugin.Guide.GuideGuid guideGuid)
        {
            m_GuideGuid = guideGuid;
        }

        bool OnATEvent(GameObject pTrans, Base.EUIEventType evtType, UIRuntimeParamArgvs param)
        {
            int index = IndexListItem();
            if(index>=0)
                return AgentTreeManager.getInstance().ExecuteEvent((ushort)evtType, pTrans,  new Variable1() { intVal = index });
            else
                return AgentTreeManager.getInstance().ExecuteEvent((ushort)evtType, pTrans, param);
        }

        public int IndexListItem()
        {
            if (paramArgvs.listBehavour == null) return -1;
            if (paramArgvs.listBehavour is ListView)
            {
                ListView pGrid = paramArgvs.listBehavour as ListView;
                return pGrid.GetIndexByItem(this.gameObject);
            }
            return -1;
        }

        public void RefreshScale()
        {
            m_originalScale = GetScaler().localScale;
        }

        public void ReplaceScale(Vector3 scale)
        {
            m_originalScale = scale;
        }

        public void ReplaceHightColor(Color color)
        {
            m_originalColor = color;
        }

        private void Awake()
        {
            CheckParam();
            m_originalScale = GetScaler().localScale;
            if (hightGraphic)
            {
                m_originalColor = hightGraphic.color;
            }
            m_nListIndex = -1;
            m_GuideGuid = GetComponent<Framework.Plugin.Guide.GuideGuid>();
            if(m_GuideGuid == null)
            {
                IGuideScroll scroll = GetComponentInParent<IGuideScroll>();
                if(scroll != null)
                {
                    m_GuideGuid = scroll.GetComponent<Framework.Plugin.Guide.GuideGuid>();
                    if (m_GuideGuid)//这边为了能获取到正确得index,要在 scroll 得生成格子上添加EventTriggerListener进行点击触发,而不是在子物体底下加,不然找不到正确index
                        m_nListIndex = scroll.GetIndexByItem(this.gameObject);//从0开始得索引,
                }
            }
        }

        private void OnDisable()
        {
            if (animationType == ButtonAnimation.ScaleAndPicture && hightGraphic is Image)
            {
                (hightGraphic as Image).overrideSprite = null;
            }
        }

        #region 按钮点击表现
        //------------------------------------------------------
        public virtual Transform GetScaler()
        {
            if (scalerTarget) return scalerTarget;
            return this.transform;
        }
        //------------------------------------------------------
        string GetSymboleName()
        {
            return GetScaler().name;
        }
        //------------------------------------------------------
        public void DoPressAction()
        {
            if(animationType == ButtonAnimation.None)return;
            if (UICommonScaler.IsIngoreSet(GetSymboleName())) return;
               
            UICommonScaler.AddScaler(this.GetScaler(), m_originalScale, 1, scaleDelta, scaleDeltaTime);
            if (animationType == ButtonAnimation.ScaleHightlight && hightGraphic)
            {
                UICommonScaler.AddHightLight(this.hightGraphic, m_originalColor, m_originalColor, highlightColor, scaleDeltaTime);
            }
            if (animationType == ButtonAnimation.ScaleAndPicture && hightGraphic is Image)
            {
                (hightGraphic as Image).overrideSprite = hightlightSprite;
            }
        }
        //------------------------------------------------------
        public void DoUpAction()
        {
            if (animationType == ButtonAnimation.None) return;
            if (UICommonScaler.IsIngoreSet(GetSymboleName())) return;

            UICommonScaler.AddScaler(this.GetScaler(), m_originalScale, scaleDelta, 1, scaleDeltaTime);
            if (animationType == ButtonAnimation.ScaleHightlight && hightGraphic)
            {
                UICommonScaler.AddHightLight(this.hightGraphic, m_originalColor, highlightColor, m_originalColor, scaleDeltaTime);
            }
            if (animationType == ButtonAnimation.ScaleAndPicture && hightGraphic is Image)
            {
                (hightGraphic as Image).overrideSprite = null;
            }
        }
        //------------------------------------------------------
        private void OnDestroy()
        {
            UICommonScaler.OnDestroy(this.transform);
        }
        #endregion
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(EventTriggerListener))]
    [CanEditMultipleObjects]
    public class EventTriggerListenerEditor : UnityEditor.Editor
    {
        private void OnEnable()
        {
            EventTriggerListener evt = target as EventTriggerListener;
            if (evt.btnUnLockData.listener == null)
                evt.btnUnLockData.listener = evt.GetComponent<Logic.UnLockListener>();
            if (evt.paramArgvs.listBehavour == null)
            {
                evt.paramArgvs.listBehavour = evt.GetComponentInParent<ListView>();
                if (evt.paramArgvs.listBehavour && evt.paramArgvs.agentTrigger == null)
                    evt.paramArgvs.agentTrigger = evt.paramArgvs.listBehavour.gameObject;
            }
        }
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EventTriggerListener evt = target as EventTriggerListener;

            GUILayout.Label("参数:");
            EditorGUI.indentLevel++;
            float labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 80;
            {
                evt.paramArgvs.agentTrigger = EditorGUILayout.ObjectField("回调GO", evt.paramArgvs.agentTrigger, typeof(UnityEngine.GameObject), true) as GameObject;
                if(evt.paramArgvs.agentTrigger)
                {
                    evt.paramArgvs.listBehavour = evt.paramArgvs.agentTrigger.GetComponent<ListView>();
                }
                GUILayout.BeginHorizontal();
                evt.paramArgvs.intParam = EditorGUILayout.IntField("Int", evt.paramArgvs.intParam);
                evt.paramArgvs.SetFlag(EParamArgvFlag.Int, EditorGUILayout.Toggle(evt.paramArgvs.IsFlag(EParamArgvFlag.Int)));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                evt.paramArgvs.floatParam = EditorGUILayout.FloatField("Float", evt.paramArgvs.floatParam);
                evt.paramArgvs.SetFlag(EParamArgvFlag.Float, EditorGUILayout.Toggle(evt.paramArgvs.IsFlag(EParamArgvFlag.Float)));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                evt.paramArgvs.strParam = EditorGUILayout.TextField("String", evt.paramArgvs.strParam);
                evt.paramArgvs.SetFlag(EParamArgvFlag.String, EditorGUILayout.Toggle(evt.paramArgvs.IsFlag(EParamArgvFlag.String)));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                evt.paramArgvs.objParam = EditorGUILayout.ObjectField("GO", evt.paramArgvs.objParam, typeof(UnityEngine.Object), true);
                evt.paramArgvs.SetFlag(EParamArgvFlag.Obj, EditorGUILayout.Toggle(evt.paramArgvs.IsFlag(EParamArgvFlag.Obj)));
                GUILayout.EndHorizontal();

                //GUILayout.BeginHorizontal();
                //evt.paramArgvs.objParam = EditorGUILayout.TextField("FMODEvent", evt.paramArgvs.strParam );
                //evt.paramArgvs.SetFlag(EParamArgvFlag.Obj, EditorGUILayout.Toggle(evt.paramArgvs.IsFlag(EParamArgvFlag.Obj)));
                //GUILayout.EndHorizontal();


            }
            EditorGUI.indentLevel--;
            EditorGUIUtility.labelWidth = labelWidth;

            FieldInfo[] fields = evt.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
            for (int i = 0; i < fields.Length; ++i)
            {
                if (fields[i].IsNotSerialized) continue;
                if (fields[i].Name.CompareTo("paramArgvs") == 0) continue;
                if(serializedObject == null || fields[i]==null) continue; 
                EditorGUILayout.PropertyField(serializedObject.FindProperty(fields[i].Name), true);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}