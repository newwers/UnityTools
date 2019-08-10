/*
	newwer
*/
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace Asset.Core
{
    /// <summary>
    /// 管理所有音频
    /// </summary>
	public class AudioManager : MonoBehaviour {


        public static AudioManager Instance;

        [Header("BGM的路径(相对Assets)")]
        public string BGMAudioClipPath = "";
        [Header("背景音频的格式,好像不支持MP3格式")]
        public AudioType BGMAudioType = AudioType.WAV;



        //当播放时,获取当前是否有audiosource组件,如果有,判断是否正在播放,如果没有,就播放,有就创建新的

        public List<AudioSource> audioSources;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }

        void Start () {
            StartCoroutine(PlayBackgroundMusic());
        }
	


        /// <summary>
        /// 播放背景音乐
        /// </summary>
        private IEnumerator PlayBackgroundMusic()
        {
            //获取audiosource组件
            var audioSource = GetAudioSource();
            //资源加载管理
            var uri = new System.Uri(Path.Combine(Application.dataPath, BGMAudioClipPath));
            
            print(uri);
            if (string.IsNullOrEmpty(BGMAudioClipPath))
            {
                //如果没有填写背景音乐路径的情况下,就不加载
                yield break;
            }

            //这边有个注意,音频尽量用wav格式,然后Cool Edit导出时用的是Windows PCM的Wav 然后再设置Load type 为Streaming的类型(测试过,不改也可以加载)
            using (UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(uri, BGMAudioType))
            {
                yield return request.SendWebRequest();
                if (request.isNetworkError)
                {
                    print(request.error);
                }
                else
                {
                    AudioClip audioClip = DownloadHandlerAudioClip.GetContent(request);
                    audioSource.clip = audioClip;
                    audioSource.playOnAwake = true;
                    audioSource.loop = true;
                    audioSource.volume = 0.5f;
                    audioSource.Play();
                }
            }
        }


        /// <summary>
        /// AudioSource组件管理
        /// 获取一个未播放状态的AudioSource
        /// </summary>
        /// <returns></returns>
        public AudioSource GetAudioSource()
        {
            AudioSource audioSource = null;
            foreach (var item in audioSources)
            {
                if (item.isPlaying == false)
                {
                    audioSource = item;//如果有空闲的 AudioSource 组件,就取
                }
            }

            //所有 AudioSource 都播放的情况下,新增一个 AudioSource 组件
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSources.Add(audioSource);
            }
            return audioSource;
        }


        /// <summary>
        /// 直接播放一个音频片段,调用者无需处理AudioSource的组件
        /// 只需要传递进来要播放的音频即可
        /// </summary>
        public void PlayAudio(AudioClip audioClip,ulong deley = 0)
        {
            var audioSource = GetAudioSource();
            audioSource.clip = audioClip;
            audioSource.loop = false;
            audioSource.Play(deley);
        }

        //TODO:有一个需求,根据传递进来的音频文件名称,找到对应文件位置,进行加载文件
    }
}
