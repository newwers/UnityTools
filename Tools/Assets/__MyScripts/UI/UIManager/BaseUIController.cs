using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Z.UI
{
    [RequireComponent(typeof(BaseUIView))]
    [RequireComponent(typeof(UIReferenceComponent))]
    public class BaseUIController : MonoBehaviour
    {
        private BaseUIView m_View;


        /// <summary>
        /// 界面显示状态
        /// </summary>
        private bool isShowState = false;

        public BaseUIView View { get => m_View; set => m_View = value; }
        public bool IsShowState { get => isShowState; set => isShowState = value; }

        /// <summary>
        /// 当创建完界面时,调用一次
        /// </summary>
        /// <param name="args">打开界面时,传递的参数</param>
        public virtual void OnCreated()
        {
            if (View == null)
            {
                View = GetComponent<BaseUIView>();
            }

            RegisterNotification();
        }

        /// <summary>
        /// 在OnCreated后执行,用来进行消息事件的监听
        /// </summary>
        public virtual void RegisterNotification()
        {

        }

        /// <summary>
        /// 当界面显示时调用,如果第一次创建,那么在OnCreated后面调用
        /// </summary>
        /// <param name="args">打开界面时,传递的参数</param>
        public virtual void OnShow()
        {

            IsShowState = true;

            RefreshUI();
        }

        public virtual void RefreshUI()
        {

        }

        /// <summary>
        /// 当界面被关闭隐藏时调用
        /// </summary>
        public virtual void OnHide()
        {

            IsShowState = false;
        }

        /// <summary>
        /// 当界面被销毁时调用
        /// </summary>
        public virtual void OnHideAndDestroy()
        {
            IsShowState = false;
        }


    }
}