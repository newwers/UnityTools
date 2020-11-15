
//  Copyright (c) 2012 Calvin Rien
//        http://the.darktable.com
//
//  This software is provided 'as-is', without any express or implied warranty. In
//  no event will the authors be held liable for any damages arising from the use
//  of this software.
//
//  Permission is granted to anyone to use this software for any purpose,
//  including commercial applications, and to alter it and redistribute it freely,
//  subject to the following restrictions:
//
//  1. The origin of this software must not be misrepresented; you must not claim
//  that you wrote the original software. If you use this software in a product,
//  an acknowledgment in the product documentation would be appreciated but is not
//  required.
//
//  2. Altered source versions must be plainly marked as such, and must not be
//  misrepresented as being the original software.
//
//  3. This notice may not be removed or altered from any source distribution.
//
//  =============================================================================
//
//  derived from Gregorio Zanon's script
//  http://forum.unity3d.com/threads/119295-Writing-AudioListener.GetOutputData-to-wav-problem?p=806734&viewfull=1#post806734

using System;
using System.IO;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// wav是微软开发的一种音频文件格式.
//  它符合它符合RIFF(Resource Interchange File Format)文件规范，用于保存Windows平台的音频信息资源，
//  被Windows平台及其应用程序所广泛支持。
//  标准化的wav文件都是44.1k的采样率，采用16位的数字表示。
//  但是我们的成了里面采样率是16k的，采用16为的数字表示。
//  wav文件分为两个部分，第一个部分是wav头文件，第二个部分是PCM编码的音频数据部分。
/// </summary>
public static class SavWav
{

    const int HEADER_SIZE = 44;

    public static bool Save(string filename, AudioClip clip)
    {
        if (!filename.ToLower().EndsWith(".wav"))
        {
            filename += ".wav";
        }

        var filepath = Path.Combine(Application.streamingAssetsPath, filename);

        Debug.Log(filepath);

        // Make sure directory exists if user is saving to sub dir.
        Directory.CreateDirectory(Path.GetDirectoryName(filepath));

        using (var fileStream = CreateEmpty(filepath))
        {

            ConvertAndWrite(fileStream, clip);

            WriteHeader(fileStream, clip);
        }

        return true; // TODO: return false if there's a failure saving the file
    }

    public static AudioClip TrimSilence(AudioClip clip, float min)
    {
        var samples = new float[clip.samples];

        clip.GetData(samples, 0);

        return TrimSilence(new List<float>(samples), min, clip.channels, clip.frequency);
    }

    public static AudioClip TrimSilence(List<float> samples, float min, int channels, int hz)
    {
        return TrimSilence(samples, min, channels, hz, false, false);
    }

    public static AudioClip TrimSilence(List<float> samples, float min, int channels, int hz, bool _3D, bool stream)
    {
        int i;

        for (i = 0; i < samples.Count; i++)
        {
            if (Mathf.Abs(samples[i]) > min)
            {
                break;
            }
        }

        samples.RemoveRange(0, i);

        for (i = samples.Count - 1; i > 0; i--)
        {
            if (Mathf.Abs(samples[i]) > min)
            {
                break;
            }
        }

        samples.RemoveRange(i, samples.Count - i);

        var clip = AudioClip.Create("TempClip", samples.Count, channels, hz, _3D, stream);

        clip.SetData(samples.ToArray(), 0);

        return clip;
    }

    static FileStream CreateEmpty(string filepath)
    {
        var fileStream = new FileStream(filepath, FileMode.Create);
        byte emptyByte = new byte();

        for (int i = 0; i < HEADER_SIZE; i++) //preparing the header
        {
            fileStream.WriteByte(emptyByte);
        }

        return fileStream;
    }

    static void ConvertAndWrite(FileStream fileStream, AudioClip clip)
    {

        var samples = new float[clip.samples];

        clip.GetData(samples, 0);

        Int16[] intData = new Int16[samples.Length];
        //converting in 2 float[] steps to Int16[], //then Int16[] to Byte[]

        Byte[] bytesData = new Byte[samples.Length * 2];
        //bytesData array is twice the size of
        //dataSource array because a float converted in Int16 is 2 bytes.

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

    /// <summary>
    /// 相当于对音频的头部字节进行设置,
    /// 再头部字节组中,定义了这个音频数据的类型等各种参数
    /// </summary>
    /// <param name="fileStream"></param>
    /// <param name="clip"></param>
    static void WriteHeader(FileStream fileStream, AudioClip clip)
    {

        var hz = clip.frequency;//16000
        var channels = clip.channels;//1
        var samples = clip.samples;//80000

        fileStream.Seek(0, SeekOrigin.Begin);//将FileStream流当前的位置进行重新设置  第二个参数 SeekOrigin origin，这个参数是相对哪里，其中有枚举：开始位置、当前位置、未尾位置

        Byte[] riff = System.Text.Encoding.UTF8.GetBytes("RIFF");//Resource Interchange File Format //资源交换文件标志（RIFF）
        fileStream.Write(riff, 0, 4);

        Byte[] chunkSize = BitConverter.GetBytes(fileStream.Length - 8);//从下个地址开始到文件尾的总字节数 Length of file, minus the first 8 bytes of the RIFF description.   (4 bytes for "WAVE" + 24 bytes for format chunk length + 8 bytes for data chunk description + actual sample data size.)
        fileStream.Write(chunkSize, 0, 4);

        Byte[] wave = System.Text.Encoding.UTF8.GetBytes("WAVE");//WAV文件标志（WAVE）
        fileStream.Write(wave, 0, 4);

        Byte[] fmt = System.Text.Encoding.UTF8.GetBytes("fmt ");//波形格式标志（fmt ），最后一位空格。
        fileStream.Write(fmt, 0, 4);

        Byte[] subChunk1 = BitConverter.GetBytes(16);//The format chunk length. This is always 16. //过滤字节（一般为00000010H）
        fileStream.Write(subChunk1, 0, 4);

        UInt16 two = 2;
        UInt16 one = 1;

        Byte[] audioFormat = BitConverter.GetBytes(one);//File padding. Always 1. //格式种类（值为1时，表示数据为线性PCM编码）
        fileStream.Write(audioFormat, 0, 2);

        Byte[] numChannels = BitConverter.GetBytes(channels);//Number of channels. Either 1 for mono(单声道),  or 2 for stereo.(立体声) //通道数，单声道为1，双声道为2
        fileStream.Write(numChannels, 0, 2);

        Byte[] sampleRate = BitConverter.GetBytes(hz);//Sample rate. 采样频率
        fileStream.Write(sampleRate, 0, 4);

        Byte[] byteRate = BitConverter.GetBytes(hz * channels * 2); // sampleRate * bytesPerSample*number of channels, here 44100*2*2   //Number of bits per sample. 每秒多少字节 = 采样率 * 频道 * 2 //波形数据传输速率（每秒平均字节数）
        fileStream.Write(byteRate, 0, 4);

        UInt16 blockAlign = (ushort)(channels * 2);// Bytes per sample. 1 for 8 bit mono, 2 for 8 bit stereo or  16 bit mono, 4 for 16 bit stereo.//DATA数据块长度，字节。
        fileStream.Write(BitConverter.GetBytes(blockAlign), 0, 2);

        UInt16 bps = 16;
        Byte[] bitsPerSample = BitConverter.GetBytes(bps);//Number of bits per sample. 每个采样率的位数 这边表示16位  //PCM位宽
        fileStream.Write(bitsPerSample, 0, 2);

        Byte[] datastring = System.Text.Encoding.UTF8.GetBytes("data");//开始data块结构
        fileStream.Write(datastring, 0, 4);

        Byte[] subChunk2 = BitConverter.GetBytes(samples * channels * 2);// Length of data, in bytes
        fileStream.Write(subChunk2, 0, 4);

        //      fileStream.Close();
    }
}