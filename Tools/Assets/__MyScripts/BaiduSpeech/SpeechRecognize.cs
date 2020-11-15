using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using LitJson;
using UnityEngine.UI;
using UnityEngine.Events;

[RequireComponent(typeof(AudioSource))]
public class SpeechRecognize : MonoBehaviour {

    public string APP_ID = "14131279";
    public string API_KEY = "E4kRdXVk4GmFROT6IubGGXpy";
    public string SECRET_KEY = "duBtLVsVHxQ81r8E59ScSzRR8BRGhG9c";

    /// <summary>
    /// 获取tocken的请求地址
    /// 返回json数据,每次获取token数据都不一样
    /// </summary>
    public string GetTokenUrl = "https://aip.baidubce.com/oauth/2.0/token";
    /// <summary>
    /// 获取到的Token
    /// </summary>
    public string Access_Token = "";

    /// <summary>
    /// 录音按钮
    /// </summary>
    [Header("录音按钮")]
    public Button RecordButton;

    [Header("是否在录音的状态")]
    public bool isRecording = false;
    [Header("麦克风设备的名字")]
    public string MicrophoneName = "";
    /// <summary>
    /// 录制的音频片段
    /// </summary>
    public AudioClip RecordAudioClip;

    /// <summary>
    /// 播放音频的音源
    /// </summary>
    public AudioSource audioSource;

    /// <summary>
    /// 语音识别地址
    /// </summary>
    public string SpeechRecognition_Address = "https://vop.baidu.com/server_api";
    /// <summary>
    /// 语音合成地址
    /// </summary>
    public string SpeechSynthesis_Address = "http://tsn.baidu.com/text2audio";
    [Header("识别到的文字")]
    public string RecognizeContent = "";
    [Header("录音的时长")]
    public int recordTime = 5;

    private void Start()
    {
        GetToken(GetTokenUrl);

        RecordButton.onClick.AddListener(OnRecordButtonClick);

        //获取电脑上第一个录音设备的名称
        if (Microphone.devices.Length > 0)
        {
            MicrophoneName = Microphone.devices[0];
        }
        else
        {
            Debug.LogError("当前设备缺少麦克风设备进行录制");
        }

        audioSource = GetComponent<AudioSource>();
    }

    /// <summary>
    /// 录音按钮点击事件
    /// </summary>
    private void OnRecordButtonClick()
    {
        if (isRecording == false)
        {
            //开始录制
            isRecording = true;
            //开始录制,录制长度5秒,频率16000
            RecordAudioClip = Microphone.Start(MicrophoneName, false, recordTime, 16000);

            RecordButton.GetComponent<Image>().color = Color.red;
        }
        else
        {
            //结束录制
            isRecording = false;

            Microphone.End(MicrophoneName);
            //修改按钮的颜色
            RecordButton.GetComponent<Image>().color = Color.white;
            //播放录制的语音
            audioSource.clip = RecordAudioClip;
            audioSource.Play();
            //audioSource.PlayOneShot(RecordAudioClip);
            //开始语音识别
            StartCoroutine(Recognition(RecordAudioClip,recordTime));
        }
    }

    /// <summary>
    /// 获取token
    /// </summary>
    /// <param name="url"></param>
    public void GetToken(string url)
    {
        WWWForm form = new WWWForm();
        form.AddField("grant_type", "client_credentials");
        form.AddField("client_id", API_KEY);
        form.AddField("client_secret", SECRET_KEY);

        StartCoroutine(HttpPostRequest(url,form));
    }

    /// <summary>
    /// post http请求
    /// </summary>
    /// <param name="url">请求的地址</param>
    /// <param name="form">请求地址带的参数</param>
    /// <returns></returns>
    IEnumerator HttpPostRequest(string url, WWWForm form)
    {
        UnityWebRequest unityWebRequest = UnityWebRequest.Post(url, form);

        yield return unityWebRequest.SendWebRequest();

        if (unityWebRequest.isNetworkError)
        {
            Debug.Log("网络错误:" + unityWebRequest.error);
        }
        else
        {
            if (unityWebRequest.responseCode == 200)
            {
                string result = unityWebRequest.downloadHandler.text;
                print("成功获取到数据:" + result);
                OnGetHttpResponse_Success(result);
            }
            else
            {
                print("状态码不为200:" + unityWebRequest.responseCode);
            } 
        }

    }
    
    /// <summary>
    /// 当成功获取到服务器返回的json数据时,进行解析
    /// </summary>
    /// <param name="result">json数据</param>
    private void OnGetHttpResponse_Success(string result)
    {
        AccessToken accessToken = JsonMapper.ToObject<AccessToken>(result);
        Access_Token = accessToken.access_token;
    }

    /// <summary>
    /// 语音识别
    /// </summary>
    /// <param name="audioClip"></param>
    /// <returns></returns>
    IEnumerator Recognition(AudioClip audioClip,int recordTime)
    {
        WWWForm form = new WWWForm();

        string url = string.Format("{0}?cuid={1}&token={2}", SpeechRecognition_Address, SystemInfo.deviceUniqueIdentifier, Access_Token);
        /*
        float[] samples = new float[16000 * recordTime * audioClip.channels];
        //将audioclip填充到数组中
        audioClip.GetData(samples, 0);
        
        short[] sampleShort = new short[samples.Length];
        for (int i = 0; i < samples.Length; i++)
        {
            sampleShort[i] = (short)(samples[i] * short.MaxValue);
        }
        byte[] data = new byte[sampleShort.Length * 2];
        Buffer.BlockCopy(sampleShort, 0, data, 0, data.Length);
        
        */

        byte[] data = ConvertAudioClipToPCM16(audioClip);


        form.AddBinaryData("audio", data);
        UnityWebRequest request = UnityWebRequest.Post(url, form);
        request.SetRequestHeader("Content-Type", "audio/pcm;rate=16000");
        yield return request.SendWebRequest();

        if (request.isNetworkError)
        {
            Debug.Log("网络错误:" + request.error);
        }
        else
        {
            if (request.responseCode == 200)
            {
                string result = request.downloadHandler.text;
                print("成功获取到数据:" + result);

                RecognizeResult resultContent = JsonMapper.ToObject<RecognizeResult>(result);
                if (resultContent.result != null)
                {
                    RecognizeContent = resultContent.result[0];
                }
                else
                {
                    RecognizeContent = resultContent.ToString();
                }
                RecordButton.GetComponentInChildren<Text>().text = RecognizeContent;

            }
            else
            {
                print("状态码不为200:" + request.responseCode);
            }
        }
    }


    /// <summary>
    /// 将Unity的AudioClip数据转化为PCM格式16bit数据
    /// </summary>
    /// <param name="clip"></param>
    /// <returns></returns>
    public static byte[] ConvertAudioClipToPCM16(AudioClip clip)
    {
        var samples = new float[clip.samples * clip.channels];
        clip.GetData(samples, 0);
        var samples_int16 = new short[samples.Length];

        for (var index = 0; index < samples.Length; index++)
        {
            samples_int16[index] = (short)(samples[index] * short.MaxValue);
        }

        var byteArray = new byte[samples_int16.Length * 2];
        Buffer.BlockCopy(samples_int16, 0, byteArray, 0, byteArray.Length);

        return byteArray;
    }




}

/// <summary>
/// AccessToken序列化json的对象
/// </summary>
public class AccessToken
{
    public string access_token;

    public int expires_in;

    public string session_key;

    public string scope;

    public string refresh_token;

    public string session_secret;

    /*
    
    {
    "access_token": "24.b243f17d64fa69b413d827f6a0965846.2592000.1542375343.282335-14131279",
    "session_key": "9mzdWWhYL0oUaqTY7WohNY0Fhd8Wxm4M7t4bTtlaq9/fyw7RXgztqR8+tmnAFpgywswOL3CQsU/v6PZ3ijK91/RmmiLb9Q==",
    "scope": "audio_voice_assistant_get audio_tts_post public brain_all_scope wise_adapt lebo_resource_base lightservice_public hetu_basic lightcms_map_poi kaidian_kaidian ApsMisTest_Test权限 vis-classify_flower lpq_开放 cop_helloScope ApsMis_fangdi_permission smartapp_snsapi_base iop_autocar oauth_tp_app smartapp_smart_game_openapi oauth_sessionkey",
    "refresh_token": "25.c2cf87484f244b6ef3d1d6a330727700.315360000.1855143343.282335-14131279",
    "session_secret": "7b9a68a03cbad17db3d13985dc7690d2",
    "expires_in": 2592000
}
    

    */
}

/// <summary>
/// 语音识别成功后,返回的json数据格式
/// </summary>
public class RecognizeResult
{
    public string corpus_no;

    public string err_msg;

    public int err_no;
    /// <summary>
    /// 语音识别到的结果
    /// </summary>
    public List<string> result;

    public string sn;

    //{"corpus_no":"6612962645817945596","err_msg":"success.","err_no":0,"result":["你今年多大，"],"sn":"845877030391539700349"}
}
