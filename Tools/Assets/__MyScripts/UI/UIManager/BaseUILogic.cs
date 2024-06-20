using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Z.UI
{
    public class BaseUILogic : MonoBehaviour
    {
        public BaseUIView View;
        public BaseUIController Controller;


        public virtual void OnShow(object args)
        {

        }

        /// <summary>
        /// 当界面被关闭隐藏时调用
        /// </summary>
        public virtual void OnHide()
        {

        }

        /// <summary>
        /// 当界面被销毁时调用
        /// </summary>
        public virtual void OnHideAndDestroy()
        {

        }
    }
}