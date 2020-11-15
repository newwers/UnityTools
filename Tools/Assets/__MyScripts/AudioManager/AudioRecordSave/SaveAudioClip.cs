using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SaveAudioClip : MonoBehaviour
{
    public Button saveBtn;

    public string fileName = "record";

    public AudioSource audioSource;

    private void Start()
    {
        saveBtn.onClick.AddListener(OnSaveBtnClick);
    }

    private void OnSaveBtnClick()
    {
        string filePath = Application.streamingAssetsPath + "/" + fileName + ".wav";
        int i = 1;
        while (Tools.FileTool.FileTools.ExistFile(filePath))//判断是否有存在的录音文件
        {
            filePath = Application.streamingAssetsPath + "/" + fileName + i + ".wav";
            i++;
        }
        print("保存的文件路径 = " + filePath);

        //另一种保存方式,未测试,只能在编辑器模式下使用 将保存Ogg Vorbis或Ogg Theora文件到指定的路径。
        //UnityEditor.EditorUtility.ExtractOggFile(audioSource.clip, filePath);
        SavWav.Save(filePath, audioSource.clip);
    }
}
