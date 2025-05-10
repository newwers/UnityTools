/********************************************************************
生成日期:	1:11:2020 10:06
类    名: 	UIUtil
作    者:	zdq
描    述:	UI 通用方法
*********************************************************************/

using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace Z.UI
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
        public static void ReplayPlayableDirector(UnityEngine.Playables.PlayableDirector playable, bool rebuild = false)
        {
            if (playable)
            {
                if (rebuild)
                {
                    playable.RebuildGraph();
                }

                playable.time = 0;
                playable.Play();
            }
        }
        public static void ReplayPlayableDirectorIndex(UnityEngine.Playables.PlayableDirector playable, UnityEngine.Playables.DirectorWrapMode wrapMode, bool rebuild = false, int index = 0)
        {
            if (playable == null) return;
            var rankTimelineAsset = playable.playableAsset as UnityEngine.Timeline.TimelineAsset;
            if (rankTimelineAsset == null || rankTimelineAsset.rootTrackCount < index)
            {
                Debug.LogError("taskTimelineAsset 数据有误,请检查timeline资源是否轨道组数有问题");
                return;
            }

            for (int i = 0; i < rankTimelineAsset.rootTrackCount; i++)
            {
                rankTimelineAsset.GetRootTrack(i).muted = i != index;
            }

            playable.extrapolationMode = wrapMode;
            ReplayPlayableDirector(playable, rebuild);
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
        public static void SetGrayUI(Transform root, bool gray, Color color)
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
        public static void SetGrayUI(Transform root, bool gray)
        {
            //Material mat = Data.PermanentAssetsUtil.GetAsset<Material>(EPermanentAssetType.GrayMat);
            //Graphic[] childs = root.GetComponentsInChildren<Graphic>(true);
            //for (int i = 0; i < childs.Length; i++)
            //{
            //    childs[i].material = gray ? mat : null;
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
        //-----------------------------------------------------
        public static void SetGrayUI(Transform root, bool gray, string fittle)
        {
            //Material mat = Data.PermanentAssetsUtil.GetAsset<Material>(EPermanentAssetType.GrayMat);
            //Graphic[] childs = root.GetComponentsInChildren<Graphic>(true);
            //for (int i = 0; i < childs.Length; i++)
            //{
            //    if (childs[i].name.Contains(fittle))
            //    {
            //        continue;
            //    }
            //    childs[i].material = gray ? mat : null;
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
        public static void SetPositionTo(Transform transform, Vector3 toPos)
        {
            if (transform == null) return;
            transform.position = toPos;
        }
        //------------------------------------------------------
        public static void SetFillAmount(Image img, float progress)
        {
            if (img)
            {
                img.fillAmount = progress;
            }
        }
        //------------------------------------------------------
        public static void PlayTween(MonoBehaviour tween, bool bRewind = false, int index = 0)
        {
            if (tween == null) return;
            //if(tween is DG.Tweening.DOTweenAnimation)
            //{
            //    DG.Tweening.DOTweenAnimation doTween = (DG.Tweening.DOTweenAnimation)tween;
            //    if (bRewind) doTween.DOPlayBackwards();
            //    else doTween.DOPlayForward();
            //    return;
            //}
            //if (tween is TopGame.RtgTween.Tweener)
            //{
            //    var tweener = tween as TopGame.RtgTween.Tweener;
            //    tweener.PlayTween();
            //    return;
            //}
            //if (tween is TopGame.RtgTween.TweenerGroup)
            //{
            //    var tweenerGroup = tween as TopGame.RtgTween.TweenerGroup;
            //    tweenerGroup.Play((short)index);
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
            if (EventSystem.current == null)
            {
                return false;
            }
            PointerEventData pointerEventData = new PointerEventData(EventSystem.current);

            pointerEventData.position = screenPos;

            List<RaycastResult> results = ListPool<RaycastResult>.Get();
            EventSystem.current.RaycastAll(pointerEventData, results);
            int count = results.Count;
            ListPool<RaycastResult>.Release(results);
            //foreach (var item in results)
            //{
            //    Debug.Log(item.gameObject.name);
            //}

            return count > 0;
        }
        //------------------------------------------------------
        // 检查指针是否在UI元素上
        public static bool IsPointerOverUI()
        {
            // 通过EventSystem获取当前指针下的事件目标
            PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
            pointerEventData.position = Input.mousePosition;

            // 模拟一个列表来存储RaycastResult
            List<RaycastResult> results = new List<RaycastResult>();

            // 执行UI射线检测
            EventSystem.current.RaycastAll(pointerEventData, results);

            // 如果结果列表不为空，则表示指针在UI元素上
            return results.Count > 0;
        }

        //------------------------------------------------------
        public static void RebuildLayout(RectTransform transform)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(transform);
        }
        //------------------------------------------------------
        public static void SetActive(Component go, bool active)
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

        //------------------------------------------------------
        public static void SetAnchorPosition(RectTransform rect, Vector2 pos)
        {
            if (rect)
            {
                rect.anchoredPosition = pos;
            }
        }
        //------------------------------------------------------
        public static void SetAnchorPosition(Graphic rect, Vector2 pos)
        {
            if (rect)
            {
                rect.rectTransform.anchoredPosition = pos;
            }
        }

        public static void SetEnable(Behaviour component, bool bEnable)
        {
            if (component)
            {
                component.enabled = bEnable;
            }
        }

        public static void SetEnable(Renderer component, bool bEnable)
        {
            if (component)
            {
                component.enabled = bEnable;
            }
        }

        public static bool IsInView(Camera camera, Renderer renderer)
        {
            if (!camera || !renderer)
            {
                Debug.Log("无法正常计算是否在视野中");
                return false;
            }

            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);

            // 判断物体是否在摄像机视野范围内
            if (GeometryUtility.TestPlanesAABB(planes, renderer.bounds))
            {
                Debug.Log("物体在摄像机视野范围内");
                return true;
            }
            else
            {
                Debug.Log("物体不在摄像机视野范围内");
                return false;
            }
        }

        public static bool IsInView(Camera camera, GameObject go)
        {
            if (!camera || !go)
            {
                Debug.Log("无法正常计算是否在视野中");
                return false;
            }
            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);

            // 判断物体是否在摄像机视野范围内
            if (GeometryUtility.TestPlanesAABB(planes, go.GetComponent<Renderer>().bounds))
            {
                Debug.Log("物体在摄像机视野范围内");
                return true;
            }
            else
            {
                Debug.Log("物体不在摄像机视野范围内");
                return false;
            }
        }

        public static T FindComponentInParents<T>(Transform transform) where T : Component
        {
            if (transform == null) return null;

            T component = transform.GetComponent<T>();

            if (component != null)
            {
                return component;
            }
            else if (transform.parent != null)
            {
                return FindComponentInParents<T>(transform.parent);
            }
            else
            {
                return null;
            }
        }

        public static T DeepCopy<T>(T obj)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, obj);
                stream.Seek(0, SeekOrigin.Begin);
                return (T)formatter.Deserialize(stream);
            }
        }

        public static float GetRatio(Vector2 size)
        {
            Vector2 OriginScreenSize = size;

            float radio = (OriginScreenSize.x / OriginScreenSize.y) / (Screen.width / (float)Screen.height);
            //Debug.Log("radio:" + radio);
            return radio;
        }


        public static void SetImage(UnityEngine.UI.Image img, string path)
        {
            if (img)
            {
                img.sprite = ResourceLoadManager.Instance.ResourceLoad<Sprite>(path);
            }
        }
    }
}
