using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 2D图片或者sprite拖拽脚本
/// 原理:直接拖拽物体,而不创建新的
/// </summary>
[RequireComponent(typeof(Image))]//RequireComponent属性可以允许自动添加需要的组件作为一个附属,添加一个Image组件
public class DragImage : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler//继承拖拽接口
{
	private Dictionary<int, RectTransform> m_DraggingIcons = new Dictionary<int, RectTransform>();//拖拽的图标列表
	private Dictionary<int, RectTransform> m_DraggingPlanes = new Dictionary<int, RectTransform>();//记录拖拽的UI所属的Canvas

    RectTransform rectTransform;
    Image m_Image;

    Vector3 offset;
    Vector3 m_BeginDragPosition;

    /// <summary>
    /// 开始拖拽触发事件
    /// </summary>
    /// <param name="eventData"></param>
	public void OnBeginDrag(PointerEventData eventData)
	{
        rectTransform = GetComponent<RectTransform>();

        // 记录拖拽前的位置
        m_BeginDragPosition = rectTransform.position;

        m_DraggingIcons[eventData.pointerId] = rectTransform;//判断左右键：用eventData.pointerID来判断  当左键或者右键点击的时候创建一个游戏物体用来当鼠标拖动的图标
        var canvas = FindInParents<Canvas>(rectTransform.gameObject);
        m_DraggingPlanes[eventData.pointerId] = canvas.transform as RectTransform;

        //拖拽时层级最高,需要保证背景图片,拖拽图片跟容器图片都在同一个子物体底下才行
        //m_DraggingIcons[eventData.pointerId].transform.SetAsLastSibling();//但是有个问题,拖拽的图片会遮挡住容器,导致无法放下
        m_Image = m_DraggingIcons[eventData.pointerId].GetComponent<Image>();
        if (m_Image)
        {
            m_Image.raycastTarget = false;
        }

        //计算拖拽时鼠标点击偏移
        Vector3 globalMousePos;//定义存放全局鼠标位置
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(m_DraggingPlanes[eventData.pointerId], eventData.position, eventData.pressEventCamera, out globalMousePos))//将鼠标点击的屏幕坐标转化为生成的icon游戏物体的RectTransform的世界坐标
        {
            offset = globalMousePos - rectTransform.position;
        }

        SetDraggedPosition(eventData);//设置拖拽的位置
    }

    /// <summary>
    /// 拖拽时不断调用
    /// </summary>
    /// <param name="eventData"></param>
	public void OnDrag(PointerEventData eventData)
	{
		if (m_DraggingIcons[eventData.pointerId] != null)
			SetDraggedPosition(eventData);//设置拖拽位置
	}

	private void SetDraggedPosition(PointerEventData eventData)
	{
		var rt = m_DraggingIcons[eventData.pointerId].GetComponent<RectTransform>();//获取生成的icon游戏物体身上的RectTransform组件
		Vector3 globalMousePos;//定义存放全局鼠标位置
		if (RectTransformUtility.ScreenPointToWorldPointInRectangle(m_DraggingPlanes[eventData.pointerId], eventData.position, eventData.pressEventCamera, out globalMousePos))//将鼠标点击的屏幕坐标转化为生成的icon游戏物体的RectTransform的世界坐标
		{
			rt.position = globalMousePos - offset;//设置icon物体的RectTransform位置
			rt.rotation = m_DraggingPlanes[eventData.pointerId].rotation;//设置物体的旋转为自身的旋转或者canvas的旋转
		}
	}
    /// <summary>
    /// 结束拖拽触发事件
    /// </summary>
    /// <param name="eventData"></param>
	public void OnEndDrag(PointerEventData eventData)
	{
        if (m_Image)
        {
            m_Image.raycastTarget = true;
        }

        //如果成功释放,没事,如何知道释放是否成功
        //不成功释放,图标位置重置回来
        if (eventData.pointerEnter != null)
        {
            var drop = eventData.pointerEnter.GetComponent<DropMe>();
            if (drop)
            {
                //拖拽成功
                drop.DropByDragImage(m_DraggingIcons[eventData.pointerId]);
            }
            else
            {
                //拖拽失败
                m_DraggingIcons[eventData.pointerId].transform.position = m_BeginDragPosition;
            }
        }
        else
        {
            //拖拽失败
            m_DraggingIcons[eventData.pointerId].transform.position = m_BeginDragPosition;
        }
        //Debug.Log("pointerDrag==" + eventData.pointerDrag.name);//拖拽的对象
        //Debug.Log("pointerEnter==" + eventData.pointerEnter.name);//释放的对象
        

        m_DraggingIcons[eventData.pointerId] = null;//设置存放对应的RectTransform列表为空
    }

    /// <summary>
    /// 获取传递的参数身上的组件,如果没有就获取其父物体身上的组件,获取完再获取其父物体的父物体直到获取不到组件为止
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="go"></param>
    /// <returns>返回的是一个泛型</returns>
    static public T FindInParents<T>(GameObject go) where T : Component//where用来判断(筛选)返回的泛型T是在Component这个类里面的
	{
		if (go == null) return null;//如果参数为空,返回空
		var comp = go.GetComponent<T>();//获取当前脚本挂着的游戏对象上的所有组件

		if (comp != null)//如果不为空,返回
			return comp;
		//如果comp为空,也就是comp身上没有组件
		var t = go.transform.parent;//获取传递参数的父物体
		while (t != null && comp == null)//当父物体不为空并且身上的comp为空时
		{
			comp = t.gameObject.GetComponent<T>();//获取到父物体身上的所有组件
			t = t.parent;//让t等于再上一级的父物体,再次循环判断是否t里面有没有东西,没有则结束循环,有就继续获取下去
		}
		return comp;//返回传递参数身上的所有组件
	}
}
