
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// UI事件监听
/// </summary>
public class UIEventListener : UnityEngine.EventSystems.EventTrigger
{
    public delegate void VoidDelegate();
    public VoidDelegate onBeginDrag;
    public VoidDelegate onCancel;
    public VoidDelegate onDeselect;
    public VoidDelegate onDrag;
    public VoidDelegate onDrop;
    public VoidDelegate onEndDrag;
    public VoidDelegate onInitializePotentialDrag;
    public VoidDelegate onMove;
    public VoidDelegate onPointerClick;
    public VoidDelegate onPointerDown;
    public VoidDelegate onPointerEnter;
    public VoidDelegate onPointerExit;
    public VoidDelegate onPointerUp;
    public VoidDelegate onScroll;
    public VoidDelegate onSelect;
    public VoidDelegate onSubmit;
    public VoidDelegate onUpdateSelected;

    /// <summary>
    /// 游戏物体绑定的参数
    /// </summary>
    public System.Object Parameter;

    static public UIEventListener Get(GameObject go)
    {
        UIEventListener listener = go.GetComponent<UIEventListener>();
        if (listener == null)
            listener = go.AddComponent<UIEventListener>();

        return listener;
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        if (onBeginDrag != null)
            onBeginDrag();
    }

    public override void OnCancel(BaseEventData eventData)
    {
        if (onCancel != null)
            onCancel();
    }

    public override void OnDeselect(BaseEventData eventData)
    {
        if (onDeselect != null)
            onDeselect();
    }

    public override void OnDrag(PointerEventData eventData)
    {
        if (onDrag != null)
            onDrag();
    }

    public override void OnDrop(PointerEventData eventData)
    {
        if (onDrop != null)
            onDrop();
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        if (onEndDrag != null)
            onEndDrag();
    }

    public override void OnInitializePotentialDrag(PointerEventData eventData)
    {
        if (onInitializePotentialDrag != null)
            onInitializePotentialDrag();
    }

    public override void OnMove(AxisEventData eventData)
    {
        if (onMove != null)
            onMove();
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        if (onPointerClick != null)
            onPointerClick();
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        if (onPointerDown != null)
            onPointerDown();
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        if (onPointerEnter != null)
            onPointerEnter();
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        if (onPointerExit != null)
            onPointerExit();
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        if (onPointerUp != null)
            onPointerUp();
    }

    public override void OnScroll(PointerEventData eventData)
    {
        if (onScroll != null)
            onScroll();
    }

    public override void OnSelect(BaseEventData eventData)
    {
        if (onSelect != null)
            onSelect();
    }

    public override void OnSubmit(BaseEventData eventData)
    {
        if (onSubmit != null)
            onSubmit();
    }

    public override void OnUpdateSelected(BaseEventData eventData)
    {
        if (onUpdateSelected != null)
            onUpdateSelected();
    }
}