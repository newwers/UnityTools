using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// ����ק�������ű�,��������DragMe.cs�ű�������
/// </summary>
public class DropMe : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
	public Image containerImage;//����ͼƬ,������ʾ������ɫ��ͼƬ
	public Image receivingImage;//����ͼƬ,������ʾ��Ʒͼ���ͼƬ
	private Color normalColor;//������ɫ
	public Color highlightColor = Color.gray;//������ɫΪ��ɫ
	
	public void OnEnable ()
	{
		if (containerImage != null)
			normalColor = containerImage.color;//������������ɫΪ����ͼƬ����ɫ
	}
	/// <summary>
	/// ������ק����ʱ����
	/// </summary>
	/// <param name="data"></param>
	public void OnDrop(PointerEventData data)
	{
		containerImage.color = normalColor;//����ͼƬ����ɫ������������ɫ
        if (receivingImage == null)//�������ͼƬΪ�շ���
			return;
		
		Sprite dropSprite = GetDropSprite (data);//��ȡ��ק�����ϵ�Image�ϵ�sprite���
        if (dropSprite != null)
        {
            receivingImage.overrideSprite = dropSprite;//��Ϊֱ����sprite��ֵ�ǲ��е�,������overrideSprite����
            //todo:֪ͨ����ק������ӵ����ݲ���,ˢ������,
            //һ������ǽ�����Ʒ
            //һ������Ǹı���Ʒλ��
            //Debug.Log("OnDrop pointerDrag==" + data.pointerDrag.name);//��ק�Ķ���
            //Debug.Log("OnDrop pointerEnter==" + data.pointerEnter.name);//�ͷŵĶ���
        }
	}

    /// <summary>
    /// ������������ʱ�򱻵���   �̳���IDropHandler�ӿ�
    /// </summary>
    /// <param name="data"></param>
	public void OnPointerEnter(PointerEventData data)
	{
		if (containerImage == null)
			return;
		
		Sprite dropSprite = GetDropSprite (data);//��ȡ���㵽��������sprite
		if (dropSprite != null)
			containerImage.color = highlightColor;//������������ɫΪ��ɫ
	}

    /// <summary>
    /// ������˳���ʱ�򱻵���  �̳���IPointerEnterHandler�ӿ�
    /// </summary>
    /// <param name="data"></param>
	public void OnPointerExit(PointerEventData data)
	{
		if (containerImage == null)
			return;
		
		containerImage.color = normalColor;//����ɫ�Ļ���������ɫ
	}
	
    /// <summary>
    /// ��ȡ��ק�����ϵ�Image��sprite
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
	private Sprite GetDropSprite(PointerEventData data)
	{
		var originalObj = data.pointerDrag;//��ȡ��ק�Ķ���/***************************/
		if (originalObj == null)//Ϊ��ʱ����
			return null;
		
		var dragMe = originalObj.GetComponent<DragMe>();//��ȡ��ק�����ϵ�DragMe�ű�
		if (dragMe == null)//Ϊ��ʱ����
			return null;
		
		var srcImage = originalObj.GetComponent<Image>();//��ȡ��ק�����ϵ�Image�ؼ�
		if (srcImage == null)
			return null;
		
		return srcImage.sprite;//������ק�����ϵ�Image����е�sprite
	}
}
