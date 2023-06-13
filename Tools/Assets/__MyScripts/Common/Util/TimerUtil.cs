/********************************************************************
生成日期:	17:9:2019   16:19
类    名: 	TimerUtil
作    者:	HappLI
描    述:	时间工具集
*********************************************************************/
using Framework.Core;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace TopGame.Base
{
    public static class TimerUtil
    {
        //-----------------------------------------------------
        /// <summary>
        /// 将时间转换成00:00:00的格式,秒为单位
        /// 例如 90 对应 00:01:30
        /// </summary>
        /// <param name="time">单位秒</param>
        /// <returns></returns>
        public static string GetTimeForString(long time, bool isHourZeroHide = false)
        {
            long hour = time / 3600;
            long minute = (time % 3600) / 60;
            long second = time % 60;
            if (isHourZeroHide && hour == 0)
            {
                return BaseUtil.stringBuilder.Append(minute.ToString("00")).Append(":").Append(second.ToString("00")).ToString();
            }
            return BaseUtil.stringBuilder.Append(hour.ToString("00")).Append(":").Append(minute.ToString("00")).Append(":").Append(second.ToString("00")).ToString();
        }
        //-----------------------------------------------------
        /// <summary>
        /// 大于1天时,将时间转换成x天x小时,秒为单位,
        /// </summary>
        /// <param name="time">单位秒</param>
        /// <returns></returns>
        public static string GetTimeForStringDayFormat(long time)
        {
            if (time >= 86400)
            {
                long day = time / 86400;
                long dayLeft = time % 86400;
                long hour = dayLeft / 3600;

                return BaseUtil.stringBuilder.Append(day.ToString()).Append(Base.GlobalUtil.ToLocalization(80022272)).Append(" ").Append(hour.ToString()).Append(Base.GlobalUtil.ToLocalization(80022273)).ToString();
            }
            else
            {
                return GetTimeForString(time);
            }
        }

        //-----------------------------------------------------
        /// <summary>
        /// 将时间转换成x天x小时,秒为单位
        /// </summary>
        /// <param name="time">单位秒</param>
        /// <returns></returns>
        public static string GetTimeForStringHourFormat(long time)
        {
            long day = time / 86400;
            long dayLeft = time % 86400;
            long hour = dayLeft / 3600;

            return BaseUtil.stringBuilder.Append(day.ToString()).Append(Base.GlobalUtil.ToLocalization(80020126)).Append(hour.ToString()).Append(Base.GlobalUtil.ToLocalization(80020127)).ToString(); ;
        }


        //-----------------------------------------------------
        /// <summary>
        /// 大于24小时，显示X天
        /// 大于1小时，小于24小时，显示X小时
        /// 小于1小时，显示倒计时，例如59:59
        /// </summary>
        /// <param name="time">单位秒</param>
        /// <returns></returns>
        public static string GetTimeFormatForString(long time)
        {
            if (time > 86400)
            {
                return BaseUtil.stringBuilder.Append(Mathf.CeilToInt(time / 86400)).Append(Base.GlobalUtil.ToLocalization(80020126)).ToString();
            }
            else if (time > 3600)
            {
                return BaseUtil.stringBuilder.Append(Mathf.CeilToInt(time / 3600)).Append(Base.GlobalUtil.ToLocalization(80020127)).ToString();
            }

            return new TimeSpan(time * 10000000).ToString("mm\\:ss");
        }
        //------------------------------------------------------
        /// <summary>
        /// 建造时间显示转换成时间：
        /// 小于60秒显示：XXs
        /// 60秒 ~60分钟：XXmXXs
        /// 大于60分钟：XhXm
        /// 整数分钟时,不显示秒,整数小时时,不显示分
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static string GetTimeString(long time)
        {
            
            if (time > 3600)
            {
                if (time % 3600 / 60 == 0)
                {
                    return BaseUtil.stringBuilder.Append(Mathf.CeilToInt(time / 3600)).Append("h").ToString();
                }
                return BaseUtil.stringBuilder.Append(Mathf.CeilToInt(time / 3600)).Append("h").Append(Mathf.CeilToInt(time % 3600 / 60)).Append("m").ToString();
            }
            else if(time > 60)
            {
                if (time % 60 == 0)
                {
                    return BaseUtil.stringBuilder.Append(Mathf.CeilToInt(time / 60)).Append("m").ToString();
                }
                return BaseUtil.stringBuilder.Append(Mathf.CeilToInt(time / 60)).Append("m").Append(Mathf.CeilToInt(time % 60)).Append("s").ToString();
            }
            

            return BaseUtil.stringBuilder.Append(Mathf.CeilToInt(time)).Append("s").ToString();
        }
        //-----------------------------------------------------
        /// <summary>
        /// 超过一天显示X天00:00:00
        /// 小于一天显示 00:00:00
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static string GetTimeFormatForString2(long time)
        {
            if (time >= 86400)
            {
                long day = time / 86400;
                long dayLeft = time % 86400;

                long hour = dayLeft / 3600;
                long minute = (dayLeft % 3600) / 60;
                long second = ((dayLeft % 3600) % 60) % 60;

                return BaseUtil.stringBuilder.Append(day.ToString()).Append(Base.GlobalUtil.ToLocalization(80020126)).Append(hour.ToString("00")).Append(":").Append(minute.ToString("00")).Append(":").Append(second.ToString("00")).ToString();
            }
            else
            {
                return GetTimeForString(time);
            }
        }
        //------------------------------------------------------
        /// <summary>
        /// 获取本地当前时间戳,单位毫秒
        /// </summary>
        /// <returns></returns>
        public static long GetClientTimeStamp()
        {
            return (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000;
        }
        //------------------------------------------------------
        /// <summary>
        /// 获取本地当前时间戳,单位秒
        /// </summary>
        /// <returns></returns>
        public static long GetClientTimeStampSecond()
        {
            return (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000;
        }
        //------------------------------------------------------
        public static string GetTimeStringFromTimeStamp(long timeStamp, string format = "yyyy-MM-dd HH:mm:ss:ffff")
        {
            System.DateTime startTime = new System.DateTime(1970, 1, 1, 8, 0, 0);
            return startTime.AddSeconds(timeStamp).ToString(format);
        }
        //-----------------------------------------------------
        /// <summary>
        /// 大于1小时，显示00:00:00:00小时
        /// 小于1小时，显示，例如00:00:00
        /// </summary>
        /// <param name="time">单位秒</param>
        /// <returns></returns>
        public static string GetMillisecondTime(float time)
        {
            if (time > 3600)
            {
                new TimeSpan((long)(time * 10000000)).ToString("hh\\:mm\\:ss\\:ff");
            }

            return new TimeSpan((long)(time * 10000000)).ToString("mm\\:ss\\:ff");
        }
    }
}