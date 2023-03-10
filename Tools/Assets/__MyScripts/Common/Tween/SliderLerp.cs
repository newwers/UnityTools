/********************************************************************
生成日期:	1:11:2020 10:06
类    名: 	SliderLerp
作    者:	zdq
描    述:	滑动条过渡效果组件,使用方式,指定滑动条的前后两张图片即可
*********************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TopGame.UI
{
    [UI.UIWidgetExport]
    public class SliderLerp : MonoBehaviour
    {

        public Image BackSlider;
        public Component FrontSlider;//兼容 Image 类型 和 Scrollbar 类型
        public Image ShieldSlider;

        [Header("插值速度,单位秒")]
        public float LerpSpeed = 1f;

        float m_CurBackValue = 1f;
        float m_CurFrontValue = 1f;
        
        [Header("插值次数,次数越大越平滑")]
        public int LerpCount = 30;


        Coroutine m_LerpCoroutine;

        Image m_Front_Image;
        Scrollbar m_Front_Scrollbar;

        /// <summary>
        /// 血条数值
        /// 0到1
        /// </summary>
        float m_SliderValue = 1f;

        bool m_IsActive = false;

        Image.Type m_BackImageType = Image.Type.Filled;
        float m_MaxWidth = 0;

        [Header("是否开启颜色切换")]
        public bool IsColorChange;

        [Header("第一阶段颜色临界值")]
        public float ChangeValue1 = 0.7f;
        [Header("第二阶段颜色临界值")]
        public float ChangeValue2 = 0.4f;

        [Header("第一阶段颜色")]
        public Color ColorValue1 = new Color(0.27f, 0.81f, 0.27f);
        [Header("第二阶段颜色")]
        public Color ColorValue2 = new Color(0.99f, 0.83f, 0.21f);
        [Header("第三阶段颜色")]
        public Color ColorValue3 = new Color(1f, 0.29f, 0.18f);

        /// <summary>
        /// 设置数值
        /// </summary>
        /// <param name="newValue"></param>
        public void SetValue(float newValue,float oldValue = 1f,bool isSetDefaultValue = false)
        {
            newValue = Mathf.Clamp01(newValue);

            if (m_LerpCoroutine != null)
            {
                StopCoroutine(m_LerpCoroutine);
                m_LerpCoroutine = null;
            }

            if (BackSlider == null || FrontSlider == null)
            {
                Framework.Plugin.Logger.Error("UI组件缺少,不进行过渡;BackSlider:" + BackSlider + ",FrontSlider:" + FrontSlider);
                return;
            }

            if (FrontSlider is Image)
            {
                m_Front_Image = FrontSlider as Image;
            }
            else if (FrontSlider is Scrollbar)
            {
                m_Front_Scrollbar = FrontSlider as Scrollbar;
            }

            m_BackImageType = BackSlider.type;
            if (FrontSlider)
            {
                m_MaxWidth = (FrontSlider.transform as RectTransform).sizeDelta.x;
            }

            m_SliderValue = oldValue;
            if (isSetDefaultValue)
            {
                SetDefaultValue(newValue, oldValue);
            }
            ValueLerp(m_SliderValue, newValue);
            m_SliderValue = newValue;
            m_IsActive = true;
        }
        //------------------------------------------------------
        void SetDefaultValue(float newValue, float oldValue)
        {
            SetFrontSliderValue(oldValue);
            SetBackSliderValue(oldValue);
            m_CurBackValue = oldValue;
            m_CurFrontValue = oldValue;
        }
        //------------------------------------------------------
        void ValueLerp(float oldValue,float newValue)
        {
            WaitForSeconds wait = new WaitForSeconds(LerpSpeed / (float)LerpCount);
            if (!gameObject.activeInHierarchy) return;

            if (oldValue > newValue)
            {
                m_LerpCoroutine = StartCoroutine(SubValue(m_CurBackValue, newValue, wait));
            }
            else if (oldValue < newValue)
            {
                m_LerpCoroutine = StartCoroutine(AddValue(m_CurFrontValue, newValue, wait));
            }
            else
            {
                //数值相等情况,直接赋值
                SetFrontSliderValue(newValue);

                SetBackSliderValue(newValue);

                m_CurBackValue = newValue;
                m_CurFrontValue = newValue;
            }
        }
        //------------------------------------------------------
        IEnumerator AddValue(float oldValue, float newValue, WaitForSeconds wait)
        {
            //先让背景滑动条到最终值,
            SetBackSliderValue(newValue);

            m_CurBackValue = newValue;
            //再过渡前置血条
            for (int i = 1; i <= LerpCount; i++)
            {
                m_CurFrontValue = Mathf.Lerp(oldValue, newValue, (float)i / LerpCount);
                SetFrontSliderValue(m_CurFrontValue);
                yield return wait;
            }
        }
        //------------------------------------------------------
        IEnumerator SubValue(float oldValue, float newValue,WaitForSeconds wait)
        {
            //先让前置血条过渡到最终值
            SetFrontSliderValue(newValue);
            m_CurFrontValue = newValue;
            //再过渡后置血条
            for (int i = 1; i <= LerpCount; i++)
            {
                m_CurBackValue = Mathf.Lerp(oldValue, newValue, (float)i / LerpCount);
                SetBackSliderValue(m_CurBackValue);
                yield return wait;
            }
        }
        //------------------------------------------------------
        void SetBackSliderValue(float value)
        {
            if (BackSlider == null)
            {
                return;
            }
            value = Mathf.Clamp01(value);
            if (m_BackImageType == Image.Type.Filled)
            {
                BackSlider.fillAmount = value;
            }
            else
            {
                SetSlicedValue(m_MaxWidth, value, BackSlider);
            }
        }
        //------------------------------------------------------
        void SetFrontSliderValue(float value)
        {
            if (m_Front_Image)
            {
                m_Front_Image.fillAmount = value;
            }
            if (m_Front_Scrollbar)
            {
                m_Front_Scrollbar.size = value;
                m_Front_Scrollbar.value = 0;//防止血条出现中间过渡的问题
            }
            if (IsColorChange)
            {
                ChangeHpColor(value);
            }
        }
        //------------------------------------------------------
        private void OnDestroy()
        {
            Clear();
            m_LerpCoroutine = null;
            BackSlider = null;
            FrontSlider = null;
            m_Front_Image = null;
            m_Front_Scrollbar = null;
            
        }
        //------------------------------------------------------
        public void Clear()
        {
            if (m_LerpCoroutine != null)
            {
                StopCoroutine(m_LerpCoroutine);
                m_LerpCoroutine = null;
            }

            m_CurBackValue = 1f;
            m_CurFrontValue = 1f;
            m_SliderValue = 1f;
            LerpSpeed = 1f;
            if (m_Front_Image)
            {
                m_Front_Image.fillAmount = m_CurFrontValue;
            }
            if (m_Front_Scrollbar)
            {
                m_Front_Scrollbar.size = m_CurFrontValue;
                m_Front_Scrollbar.value = 0;
            }
            SetBackSliderValue(m_CurBackValue);

            m_IsActive = false;
        }
        //------------------------------------------------------
        /// <summary>
        /// 设置Image类型未Sliced 的进度
        /// 注意锚点在左中的位置,posx = width的一半
        /// 设置轴心点在0,0.5即可实现不需要修改位置
        /// </summary>
        /// <param name="max"></param>
        /// <param name="cur"></param>
        void SetSlicedValue(float max,float cur,Image img,float space = 2f)
        {
            if (max == 0 || img == null)
            {
                return;
            }
            RectTransform rect = img.rectTransform;
            rect.sizeDelta = new Vector2(max * cur - space, rect.sizeDelta.y);
            //rect.anchoredPosition = new Vector2(max * cur / 2f,0);
        }
        //------------------------------------------------------
        public bool GetIsActive()
        {
            return m_IsActive;
        }
        void ChangeHpColor(float value)
        {
            Color color = ColorValue1;
            if (value >= ChangeValue1)
            {
                
            }
            else if (value >= ChangeValue2)
            {
                color = ColorValue2;
            }
            else
            {
                color = ColorValue3;
            }
            if (m_Front_Image)
            {
                m_Front_Image.color = color;
            }
        }
        //------------------------------------------------------
        public void SetShieldValue(float shieldScale,bool isFull)
        {
            if (ShieldSlider == null)
            {
                return;
            }
            if (shieldScale <= 0)
            {
                ShieldSlider.rectTransform.sizeDelta = new Vector2(0, ShieldSlider.rectTransform.sizeDelta.y);
                return;
            }

            //根据比例计算长度
            float width = m_MaxWidth* shieldScale;

            //根据比例,设置护盾长度
            ShieldSlider.rectTransform.sizeDelta = new Vector2(width, ShieldSlider.rectTransform.sizeDelta.y);

            //获取当前血量比例,转化成护盾起始点
            float posX = m_SliderValue * m_MaxWidth;
            if (isFull)//护盾溢出情况,从前面往后设置
            {
                posX = m_MaxWidth - width;
            }
            ShieldSlider.rectTransform.anchoredPosition = new Vector2(posX, ShieldSlider.rectTransform.anchoredPosition.y);
        }
    }
}
