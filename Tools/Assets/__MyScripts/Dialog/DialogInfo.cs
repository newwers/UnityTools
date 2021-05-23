using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "DialogInfo",menuName = "DialogMenu/Create DialogInfo")]
public class DialogInfo : ScriptableObject
{
    [Tooltip("对话执行的顺序")]
    public int dialogIndex = 0;
    /// <summary>
    /// 是否启用这个对话
    /// </summary>
    public bool isEnable = true;

    public DialogType dialogType;

    [TextArea]
    public string dialogContent;

    public AudioClip dialogAudioClip;






    public DialogInfo(DialogType dialogType, string dialogContent)
    {
        this.dialogType = dialogType;
        this.dialogContent = dialogContent;
    }

    public DialogInfo(DialogType dialogType, string dialogContent, AudioClip dialogAudioClip) : this(dialogType, dialogContent)
    {
        this.dialogAudioClip = dialogAudioClip;
    }
}

/// <summary>
/// 对话者
/// </summary>
public enum DialogType
{
    /// <summary>
    /// 自己
    /// </summary>
    Self,
    /// <summary>
    /// Non-Player Characte
    /// </summary>
    NPC,
}
