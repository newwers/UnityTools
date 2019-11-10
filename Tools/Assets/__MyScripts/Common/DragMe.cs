using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 物品拖拽脚本
/// 原理:通过创建一个新的物体,物体上的图标显示图标为鼠标拖拽的物品图标来实现拖拽
/// </summary>
[RequireComponent(typeof(Image))]//RequireComponent属性可以允许自动添加需要的组件作为一个附属,添加一个Image组件
public class DragMe : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler//继承拖拽接口
{
	public bool dragOnSurfaces = true;//当拖拽表面?定义一个变量用来控制某个代码的功能实现
	
	private Dictionary<int,GameObject> m_DraggingIcons = new Dictionary<int, GameObject>();//拖拽的图标列表
	private Dictionary<int, RectTransform> m_DraggingPlanes = new Dictionary<int, RectTransform>();//拖拽的平台列表

    /// <summary>
    /// 开始拖拽触发事件
    /// </summary>
    /// <param name="eventData"></param>
	public void OnBeginDrag(PointerEventData eventData)
	{
		var canvas = FindInParents<Canvas>(gameObject);//获取物体身上的Canvas控件,如果本身没有去上一级查找,
		if (canvas == null)
			return;
        
        m_DraggingIcons[eventData.pointerId] = new GameObject("icon");//判断左右键：用eventData.pointerID来判断  当左键或者右键点击的时候创建一个游戏物体用来当鼠标拖动的图标

		m_DraggingIcons[eventData.pointerId].transform.SetParent (canvas.transform, false);//设置鼠标左键或者右键点击生成的游戏物体的父物体为canvas,不根据父物体进行旋转和缩放
        m_DraggingIcons[eventData.pointerId].transform.SetAsLastSibling();//移动该变换到此局部变换列表的末尾。 因为越后面的物体渲染越上面,不会被挡住
		
		var image = m_DraggingIcons[eventData.pointerId].AddComponent<Image>();//向创建的游戏对象添加Image组件

        //忽略创建图标的检测
        var group = m_DraggingIcons[eventData.pointerId].AddComponent<CanvasGroup>();//向被创建的icon游戏对象添加CanvasGroup组件
		group.blocksRaycasts = false;//允许(投射)碰撞为false

		image.sprite = GetComponent<Image>().sprite;//创建的icon游戏对象的sprite等于身上Image组件的sprite  就是赋予图片
		image.SetNativeSize();//让生成的游戏对象上面的图片大小变成原本的大小
		
		if (dragOnSurfaces)
			m_DraggingPlanes[eventData.pointerId] = transform as RectTransform;//将自身的transform转化为RectTransform存储?
		else
			m_DraggingPlanes[eventData.pointerId]  = canvas.transform as RectTransform;//将canvas的transform转化为RectTransform存储?
		
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
        if (dragOnSurfaces && eventData.pointerEnter != null && eventData.pointerEnter.transform as RectTransform != null)//​eventData.pointerEnter 表示当前时刻鼠标下的UI对象
			m_DraggingPlanes[eventData.pointerId] = eventData.pointerEnter.transform as RectTransform;//保存点击到的对象的transform组件转化成RectTransform
		
		var rt = m_DraggingIcons[eventData.pointerId].GetComponent<RectTransform>();//获取生成的icon游戏物体身上的RectTransform组件
		Vector3 globalMousePos;//定义存放全局鼠标位置
		if (RectTransformUtility.ScreenPointToWorldPointInRectangle(m_DraggingPlanes[eventData.pointerId], eventData.position, eventData.pressEventCamera, out globalMousePos))//将鼠标点击的屏幕坐标转化为生成的icon游戏物体的RectTransform的世界坐标
		{
			rt.position = globalMousePos;//设置icon物体的RectTransform位置
			rt.rotation = m_DraggingPlanes[eventData.pointerId].rotation;//设置物体的旋转为自身的旋转或者canvas的旋转
		}
	}
    /// <summary>
    /// 结束拖拽触发事件
    /// </summary>
    /// <param name="eventData"></param>
	public void OnEndDrag(PointerEventData eventData)
	{
		if (m_DraggingIcons[eventData.pointerId] != null)//如果根据鼠标左键或者右键点击生成的图标不为空
			Destroy(m_DraggingIcons[eventData.pointerId]);//摧毁生成的icon游戏物体

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
