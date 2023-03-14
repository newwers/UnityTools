/********************************************************************
生成日期:	1:11:2020 10:06
类    名: 	UIUtil
作    者:	zdq
描    述:	UI 通用方法
*********************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Project001.UI
{
    public class UIUtil
    {
        
        //------------------------------------------------------
        public static void SetGraphicColor(Graphic graphic, Color color)
        {
            if (graphic == null) return;
            graphic.color = color;
        }
        
        //------------------------------------------------------
        public static void ReplayPlayableDirector(UnityEngine.Playables.PlayableDirector playable)
        {
            if (playable)
            {
                playable.time = 0;
                playable.Play();
            }
        }
        //------------------------------------------------------
        public static void SetUIColor(Transform root, Color color)
        {
            Graphic[] childs = root.GetComponentsInChildren<Graphic>(true);
            for (int i = 0; i < childs.Length; i++)
            {
                childs[i].color = color;
            }
        }
        //------------------------------------------------------
        public static void SetUIColor(GameObject grid, Color color, string[] excludeNames = null)
        {
            var graphArray = grid.GetComponentsInChildren<Graphic>(true);

            bool isExclude = false;
            foreach (var component in graphArray)
            {
                isExclude = false;
                if (excludeNames != null)
                {
                    for (int j = 0; j < excludeNames.Length; j++)
                    {
                        if (component.name.Contains(excludeNames[j]))
                        {
                            isExclude = true;
                            break;
                        }
                    }
                }

                if (isExclude)
                {
                    continue;
                }
                component.color = color;
            }
        }
        //-----------------------------------------------------
        public static void SetGrayUI(Graphic graphic, bool gray)
        {
            //Material mat = Data.PermanentAssetsUtil.GetAsset<Material>(EPermanentAssetType.GrayMat);
            //if(graphic)
            //{
            //    graphic.material = gray ? mat : null;
            //}
        }
        //-----------------------------------------------------
        public static void SetGrayUI(Transform root, bool gray,Color color)
        {
            //Material mat = Data.PermanentAssetsUtil.GetAsset<Material>(EPermanentAssetType.GrayMat);
            //Graphic[] childs = root.GetComponentsInChildren<Graphic>(true);
            //for (int i = 0; i < childs.Length; i++)
            //{
            //    childs[i].material = gray ? mat : null;
            //    childs[i].color = gray ? color : Color.white;
            //}
        }
        //-----------------------------------------------------
        public static void SetGrayUI(GameObject root, bool gray, Color color)
        {
            //if (root == null)
            //{
            //    return;
            //}
            //Material mat = Data.PermanentAssetsUtil.GetAsset<Material>(EPermanentAssetType.GrayMat);
            //Graphic graphic = root.GetComponent<Graphic>();
            //if (graphic)
            //{
            //    graphic.material = gray ? mat : null;
            //    graphic.color = gray ? color : Color.white;
            //}
        }
        //------------------------------------------------------
        public static void SetLabel(UnityEngine.UI.Text text, uint textId)
        {
            //if (text == null) return;
            //text.text = TopGame.Base.GlobalUtil.ToLocalization((int)textId, "");
        }
        //------------------------------------------------------
        public static void SetLabel(UnityEngine.UI.Text text, string str)
        {
            if (text == null) return;
            text.text = str;
        }
        //------------------------------------------------------
        public static void SetLabel(TMPro.TextMeshProUGUI text, string str)
        {
            if (text == null) return;
            text.text = str;
        }
        //------------------------------------------------------
        public static void SetFillAmount(Image img,float progress)
        {
            if (img)
            {
                img.fillAmount= progress;
            }
        }
        //------------------------------------------------------
        public static void PlayTween(MonoBehaviour tween, bool bRewind)
        {
            //if (tween == null) return;
            //if(tween is DG.Tweening.DOTweenAnimation)
            //{
            //    DG.Tweening.DOTweenAnimation doTween = (DG.Tweening.DOTweenAnimation)tween;
            //    if (bRewind) doTween.DOPlayBackwards();
            //    else doTween.DOPlayForward();
            //    return;
            //}
        }
        
        //------------------------------------------------------
        /// <summary>
        /// 检测改位置是否有UI
        /// </summary>
        /// <param name="screenPos"></param>
        /// <returns></returns>
        public static bool IsPointOverUI(Vector2 screenPos)
        {
            //if (EventSystem.current == null)
            //{
            //    return false;
            //}
            //PointerEventData pointerEventData = new PointerEventData(EventSystem.current);

            //pointerEventData.position = screenPos;

            //List<RaycastResult> results = ListPool<RaycastResult>.Get();
            //EventSystem.current.RaycastAll(pointerEventData, results);
            //int count = results.Count;
            //ListPool < RaycastResult >.Release(results);
            //foreach (var item in results)
            //{
            //    Debug.Log(item.gameObject.name);
            //}

            //return count > 0;
            return false;
        }//------------------------------------------------------
        public static void RebuildLayout(RectTransform transform)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(transform);
        }
        
        //------------------------------------------------------
        public static void SetActive(Component go,bool active)
        {
            if (go)
            {
                go.gameObject.SetActive(active);
            }
        }
        //------------------------------------------------------
        public static void SetActive(GameObject go, bool active)
        {
            if (go)
            {
                go.SetActive(active);
            }
        }
    }
}
