/*
	newwer
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Asset.Core.Tools
{

	public class MyTween : MonoBehaviour {

        [Tooltip("位置过渡的起点")]
        public Vector3 From;
        [Tooltip("位置过渡的终点")]
        public Vector3 To;
        [Tooltip("控制移动时,挂载物体的大小")]
        public AnimationCurve Position_curve;
        [Tooltip("控制移动时,挂载物体的大小")]
        public AnimationCurve Scale_curve;
        [Tooltip("过渡的时间")]
        public float Move_time;
        [Tooltip("是否使用动画缩放")]
        public bool isScale;

        WaitForSeconds _waitForSecond;
        float _scaleTime = 0f;
        float _positionTime = 0f;
        Coroutine _scaleCoroutine;
        Coroutine _positionCoroutine;

        private void Awake()
        {
            _waitForSecond = new WaitForSeconds(0.01f);
            _positionCoroutine = StartCoroutine(PosotionTween());
            if (isScale)
            {
                _scaleCoroutine = StartCoroutine(ScaleTween());
            }
        }

        IEnumerator ScaleTween()
        {
            while (true)
            {
                
                if (_scaleTime >= Move_time)
                {
                    _scaleTime = 0f;
                    if (_scaleCoroutine!=null)
                    {
                        StopCoroutine(_scaleCoroutine);
                    }


                    break;//跳出while循环
                }
                float value = Scale_curve.Evaluate(_scaleTime / Move_time);
                transform.localScale = new Vector3(value, value, value);
                _scaleTime += 0.01f;
                
                yield return _waitForSecond;
                
            }
            
        }

        IEnumerator PosotionTween()
        {
            while (true)
            {
                if (_positionTime >= Move_time)
                {
                    _positionTime = 0f;
                    if (_positionCoroutine!=null)
                    {
                        StopCoroutine(_positionCoroutine);
                    }
                    break;
                }

                float value= Position_curve.Evaluate(_positionTime / Move_time);//获取曲线上的值

                Vector3 lerpPos= Vector3.Lerp(From, To, (_positionTime / Move_time)*value);


                transform.localPosition = lerpPos;
                _positionTime += 0.01f;
                yield return _waitForSecond;
            }
        }

        

    }
}
