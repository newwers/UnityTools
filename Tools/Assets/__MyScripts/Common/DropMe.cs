using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 被拖拽的容器脚本,用来接收DragMe.cs脚本的物体
/// </summary>
public class DropMe : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
	public Image containerImage;//容器图片,用来显示高亮颜色的图片
	public Image receivingImage;//接收图片,用来显示物品图标的图片
	private Color normalColor;//正常颜色
	public Color highlightColor = Color.gray;//高亮颜色为灰色
	
	public void OnEnable ()
	{
		if (containerImage != null)
			normalColor = containerImage.color;//设置正常的颜色为容器图片的颜色
	}
	/// <summary>
	/// 当被拖拽结束时触发
	/// </summary>
	/// <param name="data"></param>
	public void OnDrop(PointerEventData data)
	{
		containerImage.color = normalColor;//容器图片的颜色等于正常的颜色
        if (receivingImage == null)//如果接收图片为空返回
			return;
		
		Sprite dropSprite = GetDropSprite (data);//获取拖拽对象上的Image上的sprite组件
        if (dropSprite != null)
        {
            receivingImage.overrideSprite = dropSprite;//因为直接用sprite赋值是不行的,所以用overrideSprite方法
            //todo:通知将拖拽数据添加到数据层中,刷新数据,
            //一种情况是交换物品
            //一种情况是改变物品位置
            //Debug.Log("OnDrop pointerDrag==" + data.pointerDrag.name);//拖拽的对象
            //Debug.Log("OnDrop pointerEnter==" + data.pointerEnter.name);//释放的对象
        }
	}

    /// <summary>
    /// 当鼠标进入对象的时候被调用   继承自IDropHandler接口
    /// </summary>
    /// <param name="data"></param>
	public void OnPointerEnter(PointerEventData data)
	{
		if (containerImage == null)
			return;
		
		Sprite dropSprite = GetDropSprite (data);//获取鼠标点到的容器的sprite
		if (dropSprite != null)
			containerImage.color = highlightColor;//设置容器的颜色为黄色
	}

    /// <summary>
    /// 当鼠标退出的时候被调用  继承自IPointerEnterHandler接口
    /// </summary>
    /// <param name="data"></param>
	public void OnPointerExit(PointerEventData data)
	{
		if (containerImage == null)
			return;
		
		containerImage.color = normalColor;//将颜色改回正常的颜色
	}
	
    /// <summary>
    /// 获取拖拽对象上的Image的sprite
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
	private Sprite GetDropSprite(PointerEventData data)
	{
		var originalObj = data.pointerDrag;//获取拖拽的对象/***************************/
		if (originalObj == null)//为空时返回
			return null;
		
		var dragMe = originalObj.GetComponent<DragMe>();//获取拖拽对象上的DragMe脚本
		if (dragMe == null)//为空时返回
			return null;
		
		var srcImage = originalObj.GetComponent<Image>();//获取拖拽对象上的Image控件
		if (srcImage == null)
			return null;
		
		return srcImage.sprite;//返回拖拽对象上的Image组件中的sprite
	}
}
