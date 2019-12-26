/*
	newwer
*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace Tools.FileTool
{
    /// <summary>
    /// 文件工具类,
    /// 功能有:文件检查,文件读取,文件写入
    /// 待实现功能:文件移动,文件重命名,实际需求如果需要可以实现
    /// </summary>
    public static class FileTools
    {
        /// <summary>
        /// 创建文件夹
        /// </summary>
        /// <param name="path"></param>
        public static void CreateDirectory(string path)
        {
            if ( !ExistDirectory(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        /// <summary>
        /// 判断文件是否存在
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <returns></returns>
        public static bool ExistFile(string path)
        {
            bool isExist = File.Exists(path);

            if (isExist == true)
            {
                Debug.Log("存在文件：" + path);
            }
            else
            {
                Debug.Log("不存在文件：" + path);
            }

            return isExist;
        }

        /// <summary>
        /// 判断文件夹是否存在
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <returns></returns>
        public static bool ExistDirectory(string path)
        {
            bool isExist = Directory.Exists(path);

            if (isExist == true)
            {
                Debug.Log("存在文件夹：" + path);
            }
            else
            {
                Debug.Log("不存在文件夹：" + path);
            }

            return isExist;
        }


        /// <summary>
        /// 写入文件(如果路径不存在就创建一个),默认UTF-8
        /// 通过StreamWriter的方式实现
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <param name="content">文件内容</param>
        ///  /// <param name="encoding">编码格式,默认UTF-8</param>
        ///   /// <param name="append">覆盖还是追加</param>
        public static void WriteFile(string path, string content, Encoding encoding, bool append = false)
        {
            
            try
            {
                //using 语句内执行的对象,会在执行完后自动调用dispose函数进行释放资源
                using (StreamWriter writer = new StreamWriter(path, append, encoding))
                {
                    writer.Write(content);
                }
                //Debug.Log("文件写入完成:" + path);
            }
            catch (System.Exception e )
            {

                Debug.Log("文件写入失败:" + path + "," + e);
            }

        }

        /// <summary>
        /// 通过字节进行文件写入
        /// </summary>
        /// <param name="path"></param>
        /// <param name="content"></param>
        public static void WriteFile(string path, byte[] content)
        {
            try
            {
                File.WriteAllBytes(path, content);
                Debug.Log("文件写入完成:" + path);
            }
            catch (System.Exception e)
            {

                Debug.Log("文件写入失败:" + path + "," + e);
            }

        }

        /// <summary>
        /// 文件读取,通过流的方式,默认UTF-8编码格式
        /// 用Encoding.UTF8.GetString(bytes);进行读取
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <param name="encoding">编码格式,默认UTF-8</param>
        /// <returns></returns>
        public static string ReadFile(string path, Encoding encoding = null)
        {
            if (!ExistFile(path))
            {
                return null;//不存在文件
            }
            string content = "";
            if (encoding == null)
            {
                encoding = Encoding.UTF8;
            }
            try
            {
                //using 语句内执行的对象,会在执行完后自动调用dispose函数进行释放资源
                using (StreamReader reader = new StreamReader(path, encoding))
                {
                    content = reader.ReadToEnd();
                }
                Debug.Log("文件读取完成:" + content);
                return content;
            }
            catch (System.Exception e)
            {
                Debug.Log("文件读取失败:" + path + "," + e);
                return null;
                throw new IOException();
            }
        }


        public static byte[] ReadFile(string path)
        {
            
            try
            {
                byte[] content = File.ReadAllBytes(path);
                return content;
            }
            catch (System.Exception e)
            {
                Debug.Log("文件读取失败:" + path + "," + e);
                throw new IOException();
            }
        }

        /// <summary>
        /// 按照行进行读取文件
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string[] ReadFileLine(string path)
        {

            try
            {
                string[] content = File.ReadAllLines(path);
                return content;
            }
            catch (System.Exception e)
            {
                Debug.Log("文件读取失败:" + path + "," + e);
                throw new IOException();
            }
        }

        /// <summary>
        /// 将字节数组转为UTF8格式的字符串
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string ByteToString(byte[] bytes)
        {
            return Encoding.UTF8.GetString(bytes);
        }



        #region AudioClip转成本地wav音频

        /// <summary>
        /// AudioClip转成本地wav音频
        /// </summary>
        /// <param name="clip"></param>
        /// <param name="path"></param>
        public static void Save(AudioClip clip, string path)
        {
            string filePath = Path.GetDirectoryName(path);
            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }
            using (FileStream fileStream = CreateEmpty(path))
            {
                ConvertAndWrite(fileStream, clip);
                WriteHeader(fileStream, clip);
            }
        }

        private static void ConvertAndWrite(FileStream fileStream, AudioClip clip)
        {

            float[] samples = new float[clip.samples];

            clip.GetData(samples, 0);

            Int16[] intData = new Int16[samples.Length];

            Byte[] bytesData = new Byte[samples.Length * 2];

            int rescaleFactor = 32767; //to convert float to Int16  

            for (int i = 0; i < samples.Length; i++)
            {
                intData[i] = (short)(samples[i] * rescaleFactor);
                Byte[] byteArr = new Byte[2];
                byteArr = BitConverter.GetBytes(intData[i]);
                byteArr.CopyTo(bytesData, i * 2);
            }
            fileStream.Write(bytesData, 0, bytesData.Length);
        }

        private static FileStream CreateEmpty(string filepath)
        {
            FileStream fileStream = new FileStream(filepath, FileMode.Create);
            byte emptyByte = new byte();

            for (int i = 0; i < 44; i++) //preparing the header  
            {
                fileStream.WriteByte(emptyByte);
            }

            return fileStream;
        }
        private static void WriteHeader(FileStream stream, AudioClip clip)
        {
            int hz = clip.frequency;
            int channels = clip.channels;
            int samples = clip.samples;

            stream.Seek(0, SeekOrigin.Begin);

            Byte[] riff = System.Text.Encoding.UTF8.GetBytes("RIFF");
            stream.Write(riff, 0, 4);

            Byte[] chunkSize = BitConverter.GetBytes(stream.Length - 8);
            stream.Write(chunkSize, 0, 4);

            Byte[] wave = System.Text.Encoding.UTF8.GetBytes("WAVE");
            stream.Write(wave, 0, 4);

            Byte[] fmt = System.Text.Encoding.UTF8.GetBytes("fmt ");
            stream.Write(fmt, 0, 4);

            Byte[] subChunk1 = BitConverter.GetBytes(16);
            stream.Write(subChunk1, 0, 4);

            UInt16 two = 2;
            UInt16 one = 1;

            Byte[] audioFormat = BitConverter.GetBytes(one);
            stream.Write(audioFormat, 0, 2);

            Byte[] numChannels = BitConverter.GetBytes(channels);
            stream.Write(numChannels, 0, 2);

            Byte[] sampleRate = BitConverter.GetBytes(hz);
            stream.Write(sampleRate, 0, 4);

            Byte[] byteRate = BitConverter.GetBytes(hz * channels * 2); // sampleRate * bytesPerSample*number of channels, here 44100*2*2  
            stream.Write(byteRate, 0, 4);

            UInt16 blockAlign = (ushort)(channels * 2);
            stream.Write(BitConverter.GetBytes(blockAlign), 0, 2);

            UInt16 bps = 16;
            Byte[] bitsPerSample = BitConverter.GetBytes(bps);
            stream.Write(bitsPerSample, 0, 2);

            Byte[] datastring = System.Text.Encoding.UTF8.GetBytes("data");
            stream.Write(datastring, 0, 4);

            Byte[] subChunk2 = BitConverter.GetBytes(samples * channels * 2);
            stream.Write(subChunk2, 0, 4);

        }

        #endregion
    }
}



