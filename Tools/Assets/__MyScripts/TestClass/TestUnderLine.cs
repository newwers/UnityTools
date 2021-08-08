using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestUnderLine : MonoBehaviour
{
    public UnityEngine.UI.Text text;
    public string str = "下划<a href=1>线</a>测试111";
    // Start is called before the first frame update
    void Start()
    {
        text.text = str;    
    }
    
}
