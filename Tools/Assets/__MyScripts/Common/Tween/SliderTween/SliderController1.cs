using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderController1 : MonoBehaviour {


    public Button AddButton;
    public Button SubButton;
    public Text Value;
    public Slider HPSlider;

    public int HP = 1000;

    private void Awake()
    {
        AddButton.onClick.AddListener(OnAddBtnClick);
        SubButton.onClick.AddListener(OnSubBtnClick);

        //初始化UI状态
        HP = 1000;
        HPSlider.maxValue = 1000;
        HPSlider.minValue = 0;
        HPSlider.value = 1000;
        Value.text = HP.ToString();
    }

    private void OnSubBtnClick()
    {
        int damage = UnityEngine.Random.Range(-100, -1);
        UpdateSlider(damage);
    }

    private void OnAddBtnClick()
    {
        int damage = UnityEngine.Random.Range(1, 100);
        UpdateSlider(damage);
    }

    private void UpdateSlider(int damage)
    {
        HPSlider.value += damage;
        HP = (int)HPSlider.value;
        Value.text = HP.ToString();
    }


	
}
