using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

namespace GameUI
{
    /// <summary>
    /// ÆÁÄ»·½ÏòÇÐ»»Âß¼­
    /// </summary>
    public class ScreenOrientationSwitch : MonoBehaviour
    {
        public TMP_Dropdown dropdown;

        private void Awake()
        {
            if (dropdown == null)
            {
                dropdown = GetComponent<TMP_Dropdown>();
            }
            if (dropdown)
            {
                dropdown.onValueChanged.AddListener(OnValueChange);
                dropdown.options.Clear();
                var names = Enum.GetNames(typeof(ScreenOrientation));
                for (int i = 0; i < names.Length; i++)
                {
                    dropdown.options.Add(new TMP_Dropdown.OptionData(names[i]));
                }
                dropdown.value = (int)Screen.orientation;
                dropdown.RefreshShownValue();
            }
        }

        private void OnValueChange(int index)
        {
            Screen.orientation = (ScreenOrientation)index;
        }
    }
}