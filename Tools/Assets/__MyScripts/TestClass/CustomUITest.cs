using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using zdq.UI;

namespace zdq.Test
{
    public class CustomUITest : MonoBehaviour
    {
        public Slider slider;
        public HPCustomUI customUI;

        // Start is called before the first frame update
        void Start()
        {
            slider.onValueChanged.AddListener(OnValueChange);
            Init();
        }

        private void Init()
        {
            if (customUI)
            {
                customUI.m_hp = slider.value;
            }
        }

        private void OnValueChange(float value)
        {
            if (customUI)
            {
                customUI.m_hp = value;
            }
        }

    }
}