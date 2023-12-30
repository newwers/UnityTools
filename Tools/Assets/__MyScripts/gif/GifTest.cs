
#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Drawing;
//using UnityEngine.UI;
using System.IO;
using System.Drawing.Imaging;//需要导入 unity安装目录\Editor\Data\MonoBleedingEdge\lib\mono\4.5\System.Drawing.dll  并且将unity得目标平台设置为pc才行  API Compatibility Level 设置为 .Net Framework
using System;


namespace Gif.Encode
{


    public class GifTest : MonoBehaviour
    {

        public string path;
        public UnityEngine.UI.RawImage unityRawImage;

        public List<Texture2D> textures;
        public bool bPlay = false;
        [Header("倒放")]
        public bool bReversal = false;
        public int index = 0;
        [Header("一秒几张图片")]
        public int fps = 24;
        [Header("图片之间延迟")]
        public int SaveFrameDelay = 80;
        [Header("图片品质")]
        public int Quality = 10;

        private float m_Timer = 0;
        private float m_FixTimer = 0;

        public void Play()
        {
            Load();
            m_Timer = 0;
            m_FixTimer = 1f / 24f;
            bPlay =  true;
        }
        //------------------------------------------------------
        public void OrderReverseSave(string path)
        {
            if (textures == null || textures.Count == 0)
            {
                Load();
            }
            var gifEncoder = OrderSave(path);
            ReverseSave(path,gifEncoder);

            //保存结束
            gifEncoder.Finish();

            Debug.Log("保存结束");
        }
        //------------------------------------------------------
        public void ReverseSaveGif(string path)
        {
            if (textures == null || textures.Count == 0)
            {
                Load();
            }

            var gifEncoder = ReverseSave(path);
            //保存结束
            gifEncoder.Finish();

            Debug.Log("保存结束");
        }
        //------------------------------------------------------
        GifEncoder OrderSave(string path, GifEncoder gifEncoder = null)
        {
            Debug.Log($"path:{path}");

            if (string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            // 每帧之间的延迟时间（以毫秒为单位）
            int frameDelay = SaveFrameDelay; // 这里设置为每帧100毫秒的延迟，可以根据需要进行调整


            var delays = GetFrameDelays();

            // 创建一个GifEncoder实例
            if (gifEncoder == null)
            {
                gifEncoder = new GifEncoder(0, Quality);//（0表示无限循环）

                // 开始保存GIF文件
                gifEncoder.Start(path);
            }


            for (int i = 0; i < textures.Count; i++)
            {
                var texture = textures[i];

                // 添加帧数据到GIF编码器
                var frame = new GifFrame(texture.width, texture.height);
                frame.SetPixel(texture.GetPixels32());
                gifEncoder.AddFrame(frame);

                // 设置帧延迟时间
                //if (i >= 0 && i < delays.Length)
                //{
                //    frameDelay = delays[i];
                //}
                //else
                {
                    frameDelay = SaveFrameDelay;
                }
                gifEncoder.SetDelay(frameDelay);
            }

            return gifEncoder;
        }
        //------------------------------------------------------
        /// <summary>
        /// 倒序保存
        /// </summary>
        /// <param name="path"></param>
        GifEncoder ReverseSave(string path, GifEncoder gifEncoder=null)
        {
            Debug.Log($"path:{path}");

            if (string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            // 每帧之间的延迟时间（以毫秒为单位）
            int frameDelay = SaveFrameDelay; // 这里设置为每帧100毫秒的延迟，可以根据需要进行调整


            var delays = GetFrameDelays();

            // 创建一个GifEncoder实例
            if (gifEncoder == null)
            {
                gifEncoder = new GifEncoder(0, Quality);//（0表示无限循环）

                // 开始保存GIF文件
                gifEncoder.Start(path);
            }

            // 循环处理每一帧

            for (int i = textures.Count - 1; i >= 0; i--)
            {
                // 将Texture2D转换为字节数组
                //byte[] frameData = textures[i].EncodeToPNG();
                var texture = textures[i];

                // 添加帧数据到GIF编码器
                var frame = new GifFrame(texture.width, texture.height);
                //for (int x = 0; x < texture.width; x++)
                //{
                //    for (int y = 0; y < texture.height; y++)
                //    {
                //        frame.SetPixel((uint)x, (uint)y, texture.GetPixel(x, y));
                //    }
                //}
                frame.SetPixel(texture.GetPixels32());
                gifEncoder.AddFrame(frame);

                // 设置帧延迟时间
                //if (i >= 0 && i < delays.Length)
                //{
                //    frameDelay = delays[i];
                //}
                //else
                {
                    frameDelay = SaveFrameDelay;
                }
                gifEncoder.SetDelay(frameDelay);
            }



            return gifEncoder;

        }
        
        public void TextureListSave(string foldenrPath)
        {
            int index = 0;
            foreach (Texture2D texture in textures)
            {
                byte[] bytes = texture.EncodeToPNG();
                File.WriteAllBytes(foldenrPath + "/" + index + ".png", bytes);
                index++;
            }
        }
        //------------------------------------------------------

        void Load()
        {
            index = 0;
            textures.Clear();

            var gifImage = System.Drawing.Image.FromFile(path);
            FrameDimension dimension = new FrameDimension(gifImage.FrameDimensionsList[0]);// img.FrameDimensionsList : 获取 GUID 的数组，这些 GUID 表示此 Image 中帧的维数。
            int frameCount = gifImage.GetFrameCount(dimension);//帧数
            Debug.Log($"frameCount:{frameCount}");
            for (int i = 0; i < frameCount; i++)
            {
                gifImage.SelectActiveFrame(dimension, i);//设置显示得帧数  (选择由维度和索引指定的帧)

                //将gif得帧转化为unity里面得texture2D
                Bitmap bitmap = new Bitmap(gifImage.Width, gifImage.Height);// Bitmap 继承自 Image
                System.Drawing.Graphics.FromImage(bitmap).DrawImage(gifImage, System.Drawing.Point.Empty);
                Texture2D texture2D = new Texture2D(bitmap.Width, bitmap.Height);
                //填充texture2D像素
                for (int x = 0; x < bitmap.Width; x++)
                {
                    for (int y = 0; y < bitmap.Height; y++)
                    {
                        System.Drawing.Color color = bitmap.GetPixel(x, y);
                        texture2D.SetPixel(x,bitmap.Height - 1 - y,new UnityEngine.Color(color.R/255f,color.G/255f,color.B / 255f,color.A/255f));
                    }
                }
                texture2D.Apply();

                textures.Add(texture2D);
            }


        }
        //------------------------------------------------------
        public int[] GetFrameDelays()
        {
            using (Image gifImage = Image.FromFile(path))
            {
                // 获取GIF文件中的所有元数据信息
                PropertyItem[] propertyItems = gifImage.PropertyItems;

                // 查找间隔时间（每帧之间的延迟时间）元数据
                int frameDelayId = 0x5100; // GIF文件中的间隔时间元数据ID
                int[] frameDelays = new int[gifImage.GetFrameCount(FrameDimension.Time)];

                int i = 0;
                foreach (PropertyItem propertyItem in propertyItems)
                {
                    if (propertyItem.Id == frameDelayId)
                    {
                        // 将字节数组转换为整数，以获取帧延迟时间（以1/100秒为单位）
                        int delayTime = BitConverter.ToInt32(propertyItem.Value, 0);
                        frameDelays[i++] = delayTime * 10; // 将延迟时间转换为毫秒
                    }
                }

                return frameDelays;
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (bPlay)
            {
                if (bReversal)
                {
                    

                    if (m_Timer >= m_FixTimer)
                    {
                        index--;
                        if (index < 0)
                        {
                            index = textures.Count - 1;
                        }

                        m_Timer = 0;
                    }
                    else
                    {
                        m_Timer += Time.deltaTime;
                    }
                }
                else
                {
                    if (m_Timer >= m_FixTimer)
                    {
                        index++;
                        m_Timer= 0;
                    }
                    else
                    {
                        m_Timer += Time.deltaTime;
                    }
                }
                
                unityRawImage.texture = textures[(int)(index) % textures.Count];
                if (index == 1)
                {
                    unityRawImage.SetNativeSize();
                }
            }
        }
    }

    [CustomEditor(typeof(GifTest))]
    public class GifTestEditor : Editor
    {
        GifTest m_pGif;

        private void OnEnable()
        {
            m_pGif = (GifTest)target;
        }
        //------------------------------------------------------
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("选择路径"))
            {
                m_pGif.path = EditorUtility.OpenFilePanel("选择gif", "", "");
            }
            if (GUILayout.Button("播放"))
            {
                m_pGif.Play();
            }
            if (GUILayout.Button("倒序保存"))
            {
                //string path = EditorUtility.SaveFilePanelInProject("保存gif", "1.gif", "gif","请选择一个路径进行保存gif文件");//Assets/gif/10.gif
                string path = EditorUtility.SaveFilePanel("保存gif", Application.dataPath, "0.gif","gif");//D:/Test/Test/Assets/gif/0.gif
                
                m_pGif.ReverseSaveGif(path);

                UnityEditor.AssetDatabase.Refresh();
            }
            if (GUILayout.Button("正序+倒序保存"))
            {
                //string path = EditorUtility.SaveFilePanelInProject("保存gif", "1.gif", "gif","请选择一个路径进行保存gif文件");//Assets/gif/10.gif
                string path = EditorUtility.SaveFilePanel("保存gif", Application.dataPath, "0.gif", "gif");//D:/Test/Test/Assets/gif/0.gif
                m_pGif.OrderReverseSave(path);

                UnityEditor.AssetDatabase.Refresh();
            }
            if (GUILayout.Button("gif转图片保存"))
            {
                string path = EditorUtility.SaveFolderPanel("保存图片路径", Application.dataPath, "");
                m_pGif.TextureListSave(path);

                UnityEditor.AssetDatabase.Refresh();
            }
        }
    }
}
#endif