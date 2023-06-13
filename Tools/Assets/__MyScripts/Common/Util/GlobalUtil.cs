/********************************************************************
生成日期:	17:9:2019   16:19
类    名: 	Util
作    者:	HappLI
描    述:	通用工具集
*********************************************************************/
using Framework.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace TopGame.Base
{
    public static class GlobalUtil
    {
        public static float PROGRESS_END_SNAP = 0.99f;
        //-----------------------------------------------------
        public static System.Text.StringBuilder stringBuilder
        {
            get
            {
                return BaseUtil.stringBuilder;
            }
        }
        //-----------------------------------------------------
        public static string ToLocalization(int locationId, string strDefault = null)
        {
            return Core.ALocalizationManager.ToLocalization(locationId, strDefault);
        }
        //------------------------------------------------------
        public static string SetNum(long left, long right, bool isShowRed = true)
        {
            string leftShort = GetNumString(left, 10000, 10000000, false);
            string rightShort = GetNumString(right, 10000, 10000000);
            if (isShowRed && left < right)
            {
                leftShort = Framework.Core.BaseUtil.stringBuilder.Append("<color=#FF8C8C>").Append(leftShort).Append("</color>").ToString();
            }

            return Framework.Core.BaseUtil.stringBuilder.Append(leftShort).Append("/").Append(rightShort).ToString();
        }
        //-----------------------------------------------------
        public static string GetShortNum(long amount)
        {
            if (amount > 1000000000) return Framework.Core.BaseUtil.stringBuilder.Append(Mathf.FloorToInt(amount / 1000000000)).Append("B").ToString();
            if (amount > 1000000) return Framework.Core.BaseUtil.stringBuilder.Append(Mathf.FloorToInt(amount / 1000000)).Append("M").ToString();
            if (amount > 1000) return Framework.Core.BaseUtil.stringBuilder.Append(Mathf.FloorToInt(amount / 1000)).Append("K").ToString();
            return amount.ToString();
        }
        //------------------------------------------------------
        public static string GetNumString(long money, long k, long m, bool isFloor = true)
        {
            if (money >= m)
            {
                if (isFloor)
                {
                    return Framework.Core.BaseUtil.stringBuilder.Append(Mathf.FloorToInt(money / 1000000)).Append("M").ToString();
                }
                return Framework.Core.BaseUtil.stringBuilder.Append(Mathf.CeilToInt(money / 1000000)).Append("M").ToString();
            }
            else if (money >= k)
            {
                if (isFloor)
                {
                    return Framework.Core.BaseUtil.stringBuilder.Append(Mathf.FloorToInt(money / 1000)).Append("K").ToString();
                }
                return Framework.Core.BaseUtil.stringBuilder.Append(Mathf.CeilToInt(money / 1000)).Append("K").ToString();
            }
            return money.ToString();
        }
        //------------------------------------------------------
        public static string GetNumFormat(long money, long k=100000, long m=10000000)
        {
            if (money >= m*10)
            {
                return Framework.Core.BaseUtil.stringBuilder.Append((money / 1000000)).Append("M").ToString();
            }
            else if (money >= m)
            {
                return Framework.Core.BaseUtil.stringBuilder.Append((money / 1000000f).ToString("0.#")).Append("M").ToString();
            }
            else if (money >= k*10)
            {
                return Framework.Core.BaseUtil.stringBuilder.Append((money / 1000)).Append("K").ToString();
            }
            else if (money >= k)
            {
                return Framework.Core.BaseUtil.stringBuilder.Append((money / 1000f).ToString("0.#")).Append("K").ToString();
            }
            return money.ToString();
        }
        //-----------------------------------------------------
        public static Vector3 Bezier2(float t, Vector3 p1, Vector3 p2)
        {
            return BaseUtil.Bezier2(t, p1, p2);
        }
        //-----------------------------------------------------
        public static Vector3 Bezier3(float t, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            return BaseUtil.Bezier3(t, p1, p2, p3);
        }
        //-----------------------------------------------------
        public static Vector3 Bezier4(float t, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
        {
            return BaseUtil.Bezier4(t, p1, p2, p3, p4);
        }
        //------------------------------------------------------
        public static Vector3 RayHitPos(Vector3 pos, Vector3 dir, float floorY = 0)
        {
            return BaseUtil.RayHitPos(pos, dir, floorY);
        }
        //------------------------------------------------------
        public static Vector3 RayHitPos(Ray ray, float floorY = 0)
        {
            return BaseUtil.RayHitPos(ray, floorY);
        }
        //------------------------------------------------------
        public static Vector3 ProjectLinePos(ref float factor, Vector3 linePos, Vector3 lineDir, Vector3 point, bool bProjectFacor = true)
        {
            return BaseUtil.ProjectLinePos(ref factor, linePos, lineDir, point, bProjectFacor);
        }
        //------------------------------------------------------
        public static Vector3 ProjectPointOnPlane(Vector3 planeNormal, Vector3 planePoint, Vector3 point)
        {
            return BaseUtil.ProjectPointOnPlane(planeNormal, planePoint, point);
        }
        //------------------------------------------------------
        public static Vector3 ProjectPointOnPlane(ref float distance, Vector3 planeNormal, Vector3 planePoint, Vector3 point)
        {
            return BaseUtil.ProjectPointOnPlane(ref distance, planeNormal, planePoint, point);
        }
        //------------------------------------------------------
        public static float PointDistancePlane(Vector3 planeNormal, Vector3 planePoint, Vector3 point)
        {
            return BaseUtil.PointDistancePlane(planeNormal, planePoint, point);
        }
        //------------------------------------------------------
        public static bool LinePlaneIntersection(out Vector3 intersection, Vector3 linePoint, Vector3 lineVec, Vector3 planeNormal, Vector3 planePoint)
        {
            return BaseUtil.LinePlaneIntersection(out intersection, linePoint, lineVec, planeNormal, planePoint);
        }
        //------------------------------------------------------
        public static bool LineLineIntersection(out Vector3 intersection, Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2)
        {
            return BaseUtil.LineLineIntersection(out intersection, linePoint1, lineVec1, linePoint2, lineVec2);
        }
        //-----------------------------------------------------
        static public bool rayHitTriangle(Ray ray, Vector3 v0, Vector3 v1, Vector3 v2)
        {
            Vector3 hit = Vector3.zero;
            Vector3 edge1 = v1 - v0;
            Vector3 edge2 = v2 - v0;

            Vector3 p = Vector3.Cross(ray.direction, edge2);

            float det = Vector3.Dot(edge1, p);

            Vector3 t;
            if (det > 0f) t = ray.origin - v0;
            else
            {
                t = v0 - ray.origin;
                det = -det;
            }

            if (det < 0.0001f) return false;

            hit.y = Vector3.Dot(t, p);
            if (hit.y < 0f || hit.y > det) return false;

            Vector3 q = Vector3.Cross(t, edge1);
            hit.z = Vector3.Dot(ray.direction, q);
            if (hit.z < 0f || (hit.z + hit.y) > det) return false;

            hit.x = Vector3.Dot(edge2, q);
            return true;
        }
        //-----------------------------------------------------
        static public bool rayHitTriangle(Ray ray, Vector3 v0, Vector3 v1, Vector3 v2, out Vector3 hit)
        {
            hit = Vector3.zero;

            Vector3 normal = Vector3.Cross(v1 - v0, v2 - v0);
            normal.Normalize();

            float num = Vector3.Dot(ray.direction, normal);
            float num2 = -Vector3.Dot(ray.origin, normal) + Vector3.Dot(normal, v0);

            if (Mathf.Approximately(num, 0f))
                return false;

            if (!rayHitTriangle(ray, v0, v1, v2)) return false;

            hit = ray.origin + ray.direction * num2 / num;

            return true;
        }
        //-----------------------------------------------------
        static public bool rayHitPlaneLimitBounds(Ray ray, Plane plane, Bounds bounds, out Vector3 hitPos)
        {
            hitPos = Vector3.zero;

            float enter = 0f;

            if (plane.Raycast(ray, out enter) && bounds.IntersectRay(ray))
            {
                hitPos = ray.origin + ray.direction.normalized * enter;
                return true;
            }
            return false;
        }
        //-----------------------------------------------------
        public static void ExternPolygon(System.Collections.Generic.List<Vector3> polygon, float fExtern, bool bCheckConvex = true)
        {
            Framework.Base.PolygonUtil.ExternPolygon(polygon, fExtern, bCheckConvex);
        }
        //-----------------------------------------------------
        public static bool IsPointInPolygon(Vector3 point, List<Vector3> polygon, float fExtern =0, bool bCheckConvex = true)
        {
            if (polygon == null || polygon.Count < 3) return false;
            if(Mathf.Abs(fExtern)>0.001f)
            {
                ExternPolygon(polygon, fExtern);
            }
            return Framework.Base.PolygonUtil.ContainsConvexPolygonPoint(polygon, point, bCheckConvex);
        }
        //-----------------------------------------------------
        public static bool IsPolygonInPolygon(List<Vector3> checkPly, List<Vector3> polygon, bool bCheckConvex = true)
        {
            return Framework.Base.PolygonUtil.ContainsPolygonInPolygon(checkPly, polygon, bCheckConvex);
        }
        //-----------------------------------------------------
        static public Vector3 EulersAngleToDirection(Vector3 eulerAngle)
        {
            return BaseUtil.EulersAngleToDirection(eulerAngle);
        }
        //-----------------------------------------------------
        static public Vector3 DirectionToEulersAngle(Vector3 dir)
        {
            return BaseUtil.DirectionToEulersAngle(dir);
        }
        //-----------------------------------------------------
        public static bool Equal(Vector3 v0, Vector3 v1, float failover = 0.01f)
        {
            return BaseUtil.Equal(v0, v1, failover);
        }
        //-----------------------------------------------------
        public static void SetActive(UnityEngine.GameObject pObj, bool bActive)
        {
            if (pObj == null) return;
            pObj.SetActive(bActive);
        }
        //-----------------------------------------------------
        public static void ResetGameObject(GameObject gameObject, EResetType type = EResetType.Local)
        {
            if (gameObject == null) return;
            ResetGameObject(gameObject.transform, type);
        }
        //-----------------------------------------------------
        public static void ResetGameObject(Transform pTrans, EResetType type = EResetType.Local)
        {
            if (pTrans == null) return;
            if (type == EResetType.World || type == EResetType.All)
            {
                pTrans.position = Vector3.zero;
                pTrans.rotation = Quaternion.identity;
                pTrans.eulerAngles = Vector3.zero;
            }

            if (type == EResetType.Local || type == EResetType.All)
            {
                pTrans.localPosition = Vector3.zero;
                pTrans.localRotation = Quaternion.identity;
                pTrans.localEulerAngles = Vector3.zero;
            }
        }
        //-----------------------------------------------------
        public static void Desytroy(UnityEngine.Object pObj)
        {
            if (pObj == null) return;
#if UNITY_EDITOR
            if (UnityEditor.EditorApplication.isPlaying)
                UnityEngine.Object.Destroy(pObj);
            else
                UnityEngine.Object.DestroyImmediate(pObj);
#else
            UnityEngine.Object.Destroy(pObj);
#endif
        }
        //------------------------------------------------------
        public static void DesytroyDelay(UnityEngine.Object pObj, float fDelay)
        {
            if (pObj == null) return;
            UnityEngine.Object.Destroy(pObj, fDelay);
        }
        //------------------------------------------------------
        public static void DestroyImmediate(UnityEngine.Object pObj)
        {
            if (pObj == null) return;
#if UNITY_EDITOR
            UnityEngine.Object.DestroyImmediate(pObj);
#else
            UnityEngine.Object.Destroy(pObj);
#endif
        }
        //------------------------------------------------------
        public static void DestroyChilds(UnityEngine.GameObject pObj)
        {
            if (pObj == null) return;
            UnityEngine.Transform pTransform = pObj.transform;
            int count = pTransform.childCount;
            for (int i = 0; i < count; ++i)
            {
                Desytroy(pTransform.GetChild(i).gameObject);
            }
            pTransform.DetachChildren();
        }
        //------------------------------------------------------
        public static void DestroyChildsDelay(UnityEngine.GameObject pObj, float fDelay)
        {
            if (pObj == null) return;
            UnityEngine.Transform pTransform = pObj.transform;
            int count = pTransform.childCount;
            for (int i = 0; i < count; ++i)
            {
                DesytroyDelay(pTransform.GetChild(i).gameObject, fDelay);
            }
            pTransform.DetachChildren();
        }
        //------------------------------------------------------
        public static void DestroyChildsImmediate(UnityEngine.GameObject pObj)
        {
            if (pObj == null) return;
#if UNITY_EDITOR
            UnityEngine.Transform pTransform = pObj.transform;
            int count = pTransform.childCount;
            while (pTransform.childCount > 0)
            {
                DestroyImmediate(pTransform.GetChild(0).gameObject);
            }
#else
            UnityEngine.Transform pTransform = pObj.transform;
            int count = pTransform.childCount;
            for(int i = 0; i < count; ++i)
            {
                Transform pTrans = pTransform.GetChild(i);
                pTrans.position = new Vector3(-100000,-100000,-100000);
                Desytroy(pTrans.gameObject);
            }
            pTransform.DetachChildren();
#endif
        }
        //------------------------------------------------------
        public static string FormBytes(long b)
        {
            if (b < 0)
                return "Unknown";
            if (b < 512)
#if UNITY_64 && UNITY_LINUX
		return string.Format("{0:f2} B",b);
#else
                return string.Format("{0:f2} B", b);
#endif
            if (b < 512 * 1024)
                return string.Format("{0:f2} KB", b / 1024.0);

            b /= 1024;
            if (b < 512 * 1024)
                return string.Format("{0:f2} MB", b / 1024.0);

            b /= 1024;
            return string.Format("{0:f2} GB", b / 1024.0);
        }
        //------------------------------------------------------
        public static string GetSystemLanuage(SystemLanguage language = SystemLanguage.Unknown)
        {
            if (language == SystemLanguage.Unknown) language = Application.systemLanguage;
            switch (Application.systemLanguage)
            {
                case SystemLanguage.English: return "en";
                case SystemLanguage.Belarusian: return "ru"; //尔罗斯
                case SystemLanguage.Japanese: return "ja"; //日本
                case SystemLanguage.Chinese:
                case SystemLanguage.ChineseSimplified: return "zh-CH";
                case SystemLanguage.ChineseTraditional: return "zh-TW";
                case SystemLanguage.Arabic: return "ar";
                case SystemLanguage.German: return "de";    //德语
                case SystemLanguage.French: return "fr";    //法语
                case SystemLanguage.Korean: return "ko";    //韩语
                case SystemLanguage.Portuguese: return "pt";    //葡萄牙语
                case SystemLanguage.Thai: return "th";    //泰语
                case SystemLanguage.Turkish: return "tr";    //土耳其语。
                case SystemLanguage.Indonesian: return "id";    //印度尼西亚语
                case SystemLanguage.Spanish: return "es";    //西班牙语
                case SystemLanguage.Vietnamese: return "vi";    //越南语
                case SystemLanguage.Italian: return "it";    //意大利语
                case SystemLanguage.Polish: return "pl";    //波兰语
                case SystemLanguage.Dutch: return "nl";    //荷兰语
                case SystemLanguage.Faroese: return "fa";    //波斯语
                case SystemLanguage.Romanian: return "ro";    //罗马尼亚语
                case SystemLanguage.Estonian: return "tl";    //菲律宾语
                case SystemLanguage.Czech: return "cs";    //捷克语
                case SystemLanguage.Greek: return "el";    //希腊语
                case SystemLanguage.Hungarian: return "hu";    //匈牙利语
                case SystemLanguage.Swedish: return "sv";    //瑞典语
                case SystemLanguage.Hebrew: return "hi";    //印地语
                case SystemLanguage.Norwegian: return "nb";    //挪威语
                                                               //              case SystemLanguage.xxx: return "te";    //泰卢固语
                                                               //              case SystemLanguage.xxx: return "bn";    //孟加拉语
                                                               //              case SystemLanguage.xxx: return "ta";    //泰米尔语
                                                               //              case SystemLanguage.xx: return "ms";    //马来语
                                                               //              case SystemLanguage.Bulgarian: return "my";    //缅甸语
            }
            return "en";
        }
    }
}