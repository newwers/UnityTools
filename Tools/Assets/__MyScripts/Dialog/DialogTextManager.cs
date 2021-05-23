using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class DialogTextManager : BaseMonoSingleClass<DialogTextManager>
{
    public Text dialog_Text;
    public GameObject nextIcon;
    public GameObject dialog_Btn;
    public Image headImage;
    public Texture2D selfHead;
    public Texture2D NPCHead;
    public AudioSource audioSource;


    [Header("每个对话完成显示时间")]
    public float coroutineTime = 2f;
    [Header("是否正在进行显示对话")]
    public bool IsPlaying = false;
    [Header("是否开启对话显示时间和语言匹配(开启后,设置的等待时间就和语言长度相同)")]
    public bool OpenAudioClipMatchingLength = false;

    private Queue<DialogInfo> dialogList = new Queue<DialogInfo>();

    private Coroutine dialogCoroutine;

    private string content;

    // Start is called before the first frame update
    void Start()
    {
        LoadAsset();
        IsPlaying = false;

        UIEventListener.Get(dialog_Btn).onPointerClick = OndialogBtnClick;

        dialogCoroutine = StartCoroutine(SetText(dialogList, dialog_Text));
    }

    private void LoadAsset()
    {
        List<DialogInfo> dialogInfos = GetAllAsset();
        DialogInfo[] tempList = new DialogInfo[dialogInfos.Count];
        for (int i = 0; i < dialogInfos.Count; i++)
        {
            if (dialogInfos[i] == null)
            {
                continue;
            }
            if (dialogInfos[i].isEnable)
            {
                tempList[dialogInfos[i].dialogIndex] = dialogInfos[i];//排序,用对话的顺序作为数组的索引
            }
        }

        foreach (var item in tempList)
        {
            if (item != null)
            {
                AddDialog(item);
            }
        }
    }

    /// <summary>
    /// 通过string[] arrStrAudioPath = Directory.GetFiles()；这个方法获取_Audio文件夹下所有对象的一个path，然后单个循环获取所有object
    /// </summary>
    /// <returns></returns>
    private List<DialogInfo> GetAllAsset()
    {

        //获取所有object的路径
        string[] arrStrAudioPath = Directory.GetFiles(Application.dataPath + "/DialogText/DialogInfos/", "*", SearchOption.AllDirectories);
        List<DialogInfo> list = new List<DialogInfo>();


        //循环遍历每一个路径，单独加载
        foreach (string strAudioPath in arrStrAudioPath)
        {
            if (strAudioPath.Contains(".meta"))
            {
                continue;
            }
            //替换路径中的反斜杠为正斜杠       
            string strTempPath = strAudioPath.Replace(@"\", "/");
            //截取我们需要的路径
            strTempPath = strTempPath.Substring(strTempPath.IndexOf("Assets"));
            //根据路径加载资源
            DialogInfo dialogInfo = ResourceLoadManager.Instance.Load<DialogInfo>(@strTempPath);
            if (dialogInfo != null)//过滤掉meta文件
            {
                list.Add(dialogInfo);
            }
        }
        return list;
    }

    public void AddDialog(DialogInfo dialog)
    {
        dialogList.Enqueue(dialog);
    }

    

    private void OndialogBtnClick()
    {
        if (IsPlaying)//播放中,点击显示全部
        {
            StopCoroutine(dialogCoroutine);
            dialog_Text.text = content;
            OnPlayingEnd();
        }
        else//播放完,点击切换到下一个
        {
            if (dialogList.Count > 0)
            {
                audioSource.Stop();
                dialogCoroutine = StartCoroutine(SetText(dialogList, dialog_Text));
            }
            else
            {
                OnDialogEnd();
            }
        }
    }

    IEnumerator SetText(Queue<DialogInfo> dialogList, Text text)
    {
        DialogInfo dialogInfo = dialogList.Dequeue();

        SetHeadImage(dialogInfo);
        OnPlayingBegin(text,dialogInfo);

        content = dialogInfo.dialogContent;

        if (OpenAudioClipMatchingLength)
        {
            coroutineTime = dialogInfo.dialogAudioClip.length;//让对话和语言匹配
        }
        

        for (int i = 0; i < content.Length; i++)
        {
            if (i >= content.Length)
            {
                break;
            }
            text.text += content.Substring(i,1);
            //print("time = " +  coroutineTime / (float)content.Length);
            yield return new WaitForSeconds(coroutineTime/(float)content.Length);
        }

        OnPlayingEnd();
    }

    /// <summary>
    /// 设置对话框头像
    /// </summary>
    /// <param name="dialogInfo"></param>
    private void SetHeadImage(DialogInfo dialogInfo)
    {
        switch (dialogInfo.dialogType)
        {
            case DialogType.Self:
                headImage.sprite = Sprite.Create(selfHead,new Rect(Vector2.zero,new Vector2(selfHead.width, selfHead.height)),Vector2.zero);
                break;
            case DialogType.NPC:
                headImage.sprite = Sprite.Create(NPCHead, new Rect(Vector2.zero, new Vector2(NPCHead.width, NPCHead.height)), Vector2.zero);
                break;
            default:
                break;
        }
        
    }

    void OnPlayingBegin(Text text, DialogInfo dialogInfo)
    {
        IsPlaying = true;
        text.text = "";
        nextIcon.SetActive(false);

        //播放音乐
        if (dialogInfo.dialogAudioClip != null)
        {
            audioSource.clip = dialogInfo.dialogAudioClip;
            audioSource.Play();
        }
    }

    void OnPlayingEnd()
    {
        IsPlaying = false;
        nextIcon.SetActive(true);
        
    }
    
    /// <summary>
    /// 当对话全部显示完后
    /// </summary>
    void OnDialogEnd()
    {
        this.gameObject.SetActive(false);
    }
}



