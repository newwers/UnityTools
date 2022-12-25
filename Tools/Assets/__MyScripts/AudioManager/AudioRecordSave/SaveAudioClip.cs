using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SaveAudioClip : MonoBehaviour
{
    public Button recordBtn;
    public Button saveBtn;
    public TMP_Dropdown dropdown;

    public string fileName = "record";
    /// <summary>
    /// 录制时长
    /// </summary>
    public int lengthSec = 3;
    bool m_bRecording = false;

    public AudioSource audioSource;

    private void Start()
    {
        recordBtn.onClick.AddListener(OnRecordBtnClick);
        saveBtn.onClick.AddListener(OnSaveBtnClick);

        dropdown.options.Clear();

        if (Microphone.devices.Length <= 0)
        {
            print("缺少麦克风设备");
            dropdown.options.Add(new  TMP_Dropdown.OptionData("缺少麦克风设备"));
            return;
        }

        foreach (var item in Microphone.devices)
        {
            dropdown.options.Add(new TMP_Dropdown.OptionData(item));
        }
        dropdown.onValueChanged.AddListener(OnDeviceDropdownValueChange);
        dropdown.value = 0;
        dropdown.RefreshShownValue();
    }
    
    private void OnDeviceDropdownValueChange(int index)
    {
        print("index:" + index);

    }

    public void OnRecordBtnClick()
    {
        if (dropdown == null)
        {
            return;
        }
        if (Microphone.devices.Length <= 0)
        {
            print("缺少麦克风设备");
            recordBtn.image.color = Color.yellow;
            return;
        }

        var deviceName = Microphone.devices[dropdown.value];


        

        var devices = Microphone.devices;
        for (int i = 0; i < devices.Length; i++)
        {
            print("设备index:" + i + ",名称:" + devices[i]);
        }

        Debug.Log("录制设备名称:" + deviceName.ToString());

        //Microphone.GetDeviceCaps(null, out minFreq, out maxFreq);

        if (m_bRecording)
        {
            recordBtn.image.color = Color.white;
            Microphone.End(deviceName);
            m_bRecording = false;
        }
        else
        {
            recordBtn.image.color = Color.red;
            var clip = Microphone.Start(deviceName, false, lengthSec, 44100);
            audioSource.clip = clip;
            m_bRecording = true;
        }
        
    }

    public void OnSaveBtnClick()
    {
        string filePath = Application.dataPath + "/Audios/AudioClips/" + fileName + ".wav";
        int i = 1;
        while (Tools.FileTool.FileTools.ExistFile(filePath))//判断是否有存在的录音文件
        {
            filePath = Application.streamingAssetsPath + "/Audios/AudioClips/" + fileName + i + ".wav";
            i++;
        }
        print("保存的文件路径 = " + filePath);

        //另一种保存方式,未测试,只能在编辑器模式下使用 将保存Ogg Vorbis或Ogg Theora文件到指定的路径。
        //UnityEditor.EditorUtility.ExtractOggFile(audioSource.clip, filePath);
        SavWav.Save(filePath, audioSource.clip);
    }
}
