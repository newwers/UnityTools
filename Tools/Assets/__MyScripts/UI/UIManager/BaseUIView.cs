using UnityEngine;
using UnityEngine.EventSystems;

namespace Z.UI
{
    /// <summary>
    /// 一个界面的基类
    /// 界面第一次被创建出来的时候
    /// 界面打开时候
    /// 界面关闭隐藏时
    /// 界面被销毁的时候
    /// 判断界面是否打开状态
    /// 每个界面都有一个唯一界面ID
    /// 在awake里面获取所有组件存放到键值对中,这样在使用的时候可以直接通过键值对进行获取UI组件,不用再次拖拽
    /// 缺点是占用一定空间
    /// </summary>
    public class BaseUIView : MonoBehaviour
    {
        /// <summary>
        /// 每个界面都有一个唯一界面ID
        /// </summary>
        private EUIInstanceID m_UIInstanceID;

        private Canvas panelCanvas;
        public UIReferenceComponent ui;

        private RectTransform m_pRectTrans;

        public EUIInstanceID UIInstanceID { get => m_UIInstanceID; set => m_UIInstanceID = value; }

        private void Awake()
        {
        }
        //------------------------------------------------------
        /// <summary>
        /// 通过组件名字获取对应组件
        /// </summary>
        /// <typeparam name="T">要获取的类型</typeparam>
        /// <param name="componentName"></param>
        /// <returns></returns>
        protected T GetComponentByName<T>(string componentName) where T : UIBehaviour
        {
            return ui.GetUI<T>(componentName);
        }


        //------------------------------------------------------
        /// <summary>
        /// 当创建完界面时,调用一次
        /// </summary>
        /// <param name="args">打开界面时,传递的参数</param>
        public virtual void OnCreated(UIScriptable.UIConfig item)
        {
            panelCanvas = GetComponent<Canvas>();
            if (panelCanvas && item != null)
            {
                panelCanvas.sortingOrder = item.order;
            }
        }
        //------------------------------------------------------
        /// <summary>
        /// 当界面显示时调用,如果第一次创建,那么在OnCreated后面调用
        /// </summary>
        public virtual void OnShow()
        {
        }
        //------------------------------------------------------
        /// <summary>
        /// 当界面被关闭隐藏时调用
        /// </summary>
        public virtual void OnHide()
        {

        }
        //------------------------------------------------------
        /// <summary>
        /// 当界面被销毁时调用
        /// </summary>
        public virtual void OnHideAndDestroy()
        {

        }
        //------------------------------------------------------
        public void SetAnchoredPosition(Vector3 poz)
        {
            if (m_pRectTrans == null)
            {
                m_pRectTrans = transform as RectTransform;
            }
            if (m_pRectTrans)
            {
                m_pRectTrans.anchoredPosition = poz;
            }
        }
    }
}