using System;
using UnityEngine;

namespace Z.Dialog
{
    
    [CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue Data", order = 51)]
    public class DialogueData : ScriptableObject
    {
        

        public string speakerName;
        public Sprite speakerIcon;
        public Sprite bgIcon;
        public Color bgColor;
        /// <summary>
        /// 缺省设置
        /// </summary>
        public bool isDefaultSet = false;

        [TextArea]
        public string dialogue;


        public AudioClip dialogAudioClip;
       
    }
}