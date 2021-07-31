using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Test
{
    /// <summary>
    /// 测试鼠标穿透
    /// </summary>
    public class TestInputPrnetrate : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            UIEventListener.Get(gameObject).onPointerClick = OnClick;
        }

        private void OnClick()
        {
            print("click " + name);
        }
    }
}