/********************************************************************
生成日期:	1:11:2020 10:06
类    名: 	UIUtil
作    者:	HappLI
描    述:	UI 通用方法
*********************************************************************/

using Framework.Core;
using Proto3;
using System;
using System.Collections.Generic;
using TopGame.Base;
using TopGame.Core;
using TopGame.Data;
using TopGame.Logic;
using TopGame.Net;
using TopGame.SvrData;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TopGame.UI
{
    [Framework.Plugin.AT.ATExportMono("UI通用工具")]
    public class UIUtil
    {
        //------------------------------------------------------
        [Framework.Plugin.AT.ATMethod]
        public static void SetSerializedUIVisible(UISerialized serializer, bool bVisible)
        {
            if (serializer == null) return;
            if (bVisible) serializer.Visible();
            else serializer.Hidden();
        }
        //------------------------------------------------------
        [Framework.Plugin.AT.ATMethod]
        public static void SetGraphicColor(Graphic graphic, Color color)
        {
            if (graphic == null) return;
            graphic.color = color;
        }
        //------------------------------------------------------
        [Framework.Plugin.AT.ATMethod]
        public static bool OpenUI(EUIType uiType, bool bTips = true)
        {
            if(GameInstance.getInstance().unlockMgr != null)
            {
                if(bTips)
                {
                    if (IsUILockAndTip(uiType))
                    {
                        return false;
                    }
                }
                else
                {
                    if (IsUILocked(uiType))
                    {
                        return false;
                    }
                }
            }
            UIManager.ShowUI(uiType);
            return true;
        }
        //------------------------------------------------------
        public static void ResetDynamicMakerRef()
        {
            ms_nShowDynamicMakerCnt = 0;
        }
        //------------------------------------------------------
        public static void ShowDynamicMakerRef(bool bShow)
        {
            if (bShow)
            {
                ms_nShowDynamicMakerCnt--;
                if (ms_nShowDynamicMakerCnt < 0) ms_nShowDynamicMakerCnt = 0;
            }
            else
            {
                ms_nShowDynamicMakerCnt++;
            }
        }
        //------------------------------------------------------
        //! 是否显示零散的一些ui 小组件，比如 血条、飘字、泡泡对话、建筑标志
        static int ms_nShowDynamicMakerCnt = 0;
        public static bool CanShowDynamicMarker()
        {
            if (ms_nShowDynamicMakerCnt > 0) return false;
            if (!Framework.Module.ModuleManager.startUpGame) return true;
            bool isAnimPathPlaying = GameInstance.getInstance().animPather != null && GameInstance.getInstance().animPather.IsPlaying();
            return Core.CameraController.IsGameMainCamera && !isAnimPathPlaying;
        }
        //------------------------------------------------------
        public static bool ShowUIWithCheckLock(EUIType type)
        {
            if (Framework.Module.ModuleManager.mainFramework == null) return false;
            Core.GameFramework pFramework = Framework.Module.ModuleManager.mainFramework as Core.GameFramework;
            if (pFramework == null || pFramework.unlockMgr == null) return false;
            //             UnLockManager unlockMgr = pFramework.unlockMgr as UnLockManager;
            //             return unlockMgr.ShowUIWithCheckLock(type);
            return true;
        }
        //------------------------------------------------------
        public static bool IsUILockAndTip(EUIType id)
        {
            if (Framework.Module.ModuleManager.mainFramework == null) return false;
            Core.GameFramework pFramework =Framework.Module.ModuleManager.mainFramework as Core.GameFramework;
            if (pFramework == null || pFramework.unlockMgr == null) return false;
            return pFramework.unlockMgr.IsUILockAndTip((uint)id);
        }
        //------------------------------------------------------
        public static bool IsUILocked(EUIType id)
        {
            if (Framework.Module.ModuleManager.mainFramework == null) return false;
            Core.GameFramework pFramework = Framework.Module.ModuleManager.mainFramework as Core.GameFramework;
            if (pFramework == null || pFramework.unlockMgr == null) return false;
            return pFramework.unlockMgr.IsUILocked((uint)id);
        }
        //-----------------------------------------------------
        public static void SetGlhostGraphic(UnityEngine.UI.Graphic graphic, bool gray)
        {
            if (graphic == null) return;
            Material mat = Data.PermanentAssetsUtil.GetAsset<Material>(gray ? EPermanentAssetType.UIGhostGray : EPermanentAssetType.UIGhost);
            graphic.material = mat;
        }
        //-----------------------------------------------------
        public static void SetUIMaterial(UnityEngine.UI.Graphic graphic, bool gray)
        {
            if (graphic == null) return;
            Material mat = Data.PermanentAssetsUtil.GetAsset<Material>(gray ? EPermanentAssetType.UIMaterialGray : EPermanentAssetType.UIMaterial);
            graphic.material = mat;
        }
        //------------------------------------------------------
        [Framework.Plugin.AT.ATMethod]
        public static void ReplayPlayableDirector(UnityEngine.Playables.PlayableDirector playable,bool rebuild = false)
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
            Material mat = Data.PermanentAssetsUtil.GetAsset<Material>(EPermanentAssetType.GrayMat);
            if(graphic)
            {
                graphic.material = gray ? mat : null;
            }
        }
        //-----------------------------------------------------
        public static void SetGrayUI(Transform root, bool gray,Color color)
        {
            Material mat = Data.PermanentAssetsUtil.GetAsset<Material>(EPermanentAssetType.GrayMat);
            Graphic[] childs = root.GetComponentsInChildren<Graphic>(true);
            for (int i = 0; i < childs.Length; i++)
            {
                childs[i].material = gray ? mat : null;
                childs[i].color = gray ? color : Color.white;
            }
        }
        //-----------------------------------------------------
        public static void SetGrayUI(Transform root, bool gray)
        {
            Material mat = Data.PermanentAssetsUtil.GetAsset<Material>(EPermanentAssetType.GrayMat);
            Graphic[] childs = root.GetComponentsInChildren<Graphic>(true);
            for (int i = 0; i < childs.Length; i++)
            {
                childs[i].material = gray ? mat : null;
            }
        }
        //-----------------------------------------------------
        public static void SetGrayUI(GameObject root, bool gray, Color color)
        {
            if (root == null)
            {
                return;
            }
            Material mat = Data.PermanentAssetsUtil.GetAsset<Material>(EPermanentAssetType.GrayMat);
            Graphic graphic = root.GetComponent<Graphic>();
            if (graphic)
            {
                graphic.material = gray ? mat : null;
                graphic.color = gray ? color : Color.white;
            }
        }
        //-----------------------------------------------------
        public static void SetGrayUI(Transform root, bool gray, string fittle)
        {
            Material mat = Data.PermanentAssetsUtil.GetAsset<Material>(EPermanentAssetType.GrayMat);
            Graphic[] childs = root.GetComponentsInChildren<Graphic>(true);
            for (int i = 0; i < childs.Length; i++)
            {
                if (childs[i].name.Contains(fittle))
                {
                    continue;
                }
                childs[i].material = gray ? mat : null;
            }
        }
        //------------------------------------------------------
        public static void SetLabel(UnityEngine.UI.Text text, uint textId)
        {
            if (text == null) return;
            text.text = TopGame.Base.GlobalUtil.ToLocalization((int)textId, "");
        }
        //------------------------------------------------------
        public static void SetLabel(UnityEngine.UI.Text text, string str)
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
        public static void SetFillAmount(Image img,float progress)
        {
            if (img)
            {
                img.fillAmount= progress;
            }
        }
        //------------------------------------------------------
        [Framework.Plugin.AT.ATMethod]
        public static void PlayTween(MonoBehaviour tween, bool bRewind = false,int index = 0)
        {
            if (tween == null) return;
            if(tween is DG.Tweening.DOTweenAnimation)
            {
                DG.Tweening.DOTweenAnimation doTween = (DG.Tweening.DOTweenAnimation)tween;
                if (bRewind) doTween.DOPlayBackwards();
                else doTween.DOPlayForward();
                return;
            }
            if (tween is TopGame.RtgTween.Tweener)
            {
                var tweener = tween as TopGame.RtgTween.Tweener;
                tweener.PlayTween();
                return;
            }
            if (tween is TopGame.RtgTween.TweenerGroup)
            {
                var tweenerGroup = tween as TopGame.RtgTween.TweenerGroup;
                tweenerGroup.Play((short)index);
                return;
            }
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
            ListPool < RaycastResult >.Release(results);
            //foreach (var item in results)
            //{
            //    Debug.Log(item.gameObject.name);
            //}

            return count > 0;
        }//------------------------------------------------------
        [Framework.Plugin.AT.ATMethod]
        public static void RebuildLayout(RectTransform transform)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(transform);
        }
        //-----------------------------------------------------
        public static void ShowCommonTip(TipType type, uint contentID, System.Action confirmAction = null, Action cancelAction = null, Action closeAction = null, uint yesBtnText = 80010015, uint noBtnText = 80010014)
        {
            string content = Core.LocalizationManager.ToLocalization(contentID);
            if (string.IsNullOrEmpty(content))
            {
                Debug.LogError($"找不到多语言:{contentID},请检查配置!");
                return;
            }

            ShowCommonTip(type, content, confirmAction, cancelAction, closeAction, yesBtnText, noBtnText);
        }
        //------------------------------------------------------
        public static void ShowCommonTip(TipType type, string content, System.Action confirmAction = null, Action cancelAction = null, Action closeAction = null, uint yesBtnText = 80010015, uint noBtnText = 80010014)
        {
            if (string.IsNullOrEmpty(content)) return;
            if (GameInstance.getInstance() == null || GameInstance.getInstance().localizationMgr == null || GameInstance.getInstance().uiManager == null) return;

            if (type == TipType.AutoHide)
            {
                CommonAutoHideTipsPanel tipUI = UIKits.CastGetUI<CommonAutoHideTipsPanel>();
                if (tipUI != null) tipUI.SetTextAndShow(content);
            }
            else
            {
                UICommonTip tipUI = UIKits.CastGetUI<UICommonTip>();
                if (tipUI != null)
                {
                    tipUI.ShowTip(type, content, Core.LocalizationManager.ToLocalization(80010041), confirmAction, cancelAction, closeAction, Core.LocalizationManager.ToLocalization(yesBtnText), Core.LocalizationManager.ToLocalization(noBtnText));
                }
            }

        }

        //------------------------------------------------------
        public static void ShowItemTip(uint itemId, Vector3 pos)
        {
            CommonItemTipPanel tipUI = UIKits.CastGetUI<UI.CommonItemTipPanel>();
            if (tipUI != null) tipUI.ShowItemTip(itemId, pos);
        }
        //------------------------------------------------------
        public static void ShowTextTip(uint contentID, Vector3 pos)
        {
            string content = Core.LocalizationManager.ToLocalization(contentID);
            CommonItemTipPanel tipUI = UIKits.CastGetUI<UI.CommonItemTipPanel>();
            if (tipUI != null) tipUI.ShowTextTip(content, pos);
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
            Util.SetActive(go, active);
        }
        //------------------------------------------------------
        public static void PlayPartical(ParticleController particle)
        {
            if (particle)
            {
                particle.Play();
                particle.Resume();
            }
        }
        //------------------------------------------------------
        public static string GetAssetPath(uint key)
        {
            return Core.LocalizationManager.ToLocalization(key);
        }
        //------------------------------------------------------
        public static void UIEffectInstanceCallback(InstanceOperiaon ins)
        {
            if (ins == null || ins.pPoolAble == null)
            {
                return;
            }

            ins.pPoolAble.SetScale(Vector3.one);
        }
        //------------------------------------------------------
        public static string GetItemIconPath(uint configID)
        {
            var cfg = DataManager.getInstance().Item.GetData(configID);
            if (cfg != null)
            {
                return GetAssetPath(cfg.icon);
            }
            return null;
        }
        //------------------------------------------------------
        public static void SetAnchorPosition(RectTransform rect,Vector2 pos)
        {
            if (rect)
            {
                rect.anchoredPosition= pos;
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
        //------------------------------------------------------
        public static void ShowCommonReward(List<ItemData> rewards)
        {
            var reward = ListPool<ItemBaseController.ItemDataSt>.Get();
            if (rewards != null)
            {
                for (int i = 0; i < rewards.Count; i++)
                {
                    var item = rewards[i];
                    ItemBaseController.ItemDataSt data = new ItemBaseController.ItemDataSt();
                    data.configID = (uint)item.ConfigId;
                    data.count = item.Value;
                    reward.Add(data);
                }
            }

            CommonOpenChestPanel.ShowRewardPanel(reward);

            ListPool<ItemBaseController.ItemDataSt>.Release(reward);
        }
    }
}
