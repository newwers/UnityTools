using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;//package manager  com.unity.nuget.newtonsoft-json

namespace NetworkUtil
{
    public static class HttpUtil
    {
     
        /// <summary>
        /// Get 请求
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="endPoint"></param>
        /// <returns></returns>
        public static async Task<T> Get<T>(string endPoint)
        {
            var getRequest = CreateRequest(endPoint);
            getRequest.SendWebRequest();

            while (!getRequest.isDone) await Task.Delay(10);//等待请求响应

            return JsonConvert.DeserializeObject<T>(getRequest.downloadHandler.text);
        }

        public static async Task<T> Post<T>(string endPoint,object payLoad)
        {
            var postRequest = CreateRequest(endPoint,RequestType.POST,payLoad);
            postRequest.SendWebRequest();

            while (!postRequest.isDone) await Task.Delay(10);//等待请求响应

            return JsonConvert.DeserializeObject<T>(postRequest.downloadHandler.text);
        }

        /// <summary>
        /// Json Http格式请求
        /// </summary>
        /// <param name="path"></param>
        /// <param name="type"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private static UnityWebRequest CreateRequest(string path,RequestType type = RequestType.GET, object data = null)
        {
            var request = new UnityWebRequest(path, type.ToString());

            if (data != null)
            {
                var bodyRaw = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data));//将data转成字节数组
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            }

            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            return request;
        }
    }

    public enum RequestType
    {
        GET=0,
        POST = 1,
        PUT = 2
    }
}

