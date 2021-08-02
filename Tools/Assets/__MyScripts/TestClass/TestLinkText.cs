using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using zdq.UI;

public class TestLinkText : MonoBehaviour
{
    public LinkText linkText;
    public UnityEngine.UI.Image img;
    // Start is called before the first frame update
    void Start()
    {
        linkText?.onHrefClick.AddListener(OnClick);
        
    }

    private void OnClick(string str)
    {
        print(str);
        //Application.OpenURL(str);
        print(img.rectTransform.rect);
    }

}
