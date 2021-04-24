using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderController2 : MonoBehaviour {

    public Button AddButton;
    public Button SubButton;
    public Text Value;
    public Image HPImage;

    public int HP = 1000;

    private void Awake()
    {
        AddButton.onClick.AddListener(OnAddBtnClick);
        SubButton.onClick.AddListener(OnSubBtnClick);

        //初始化UI状态
        HP = 1000;
        HPImage.fillAmount = 1f;
        Value.text = HP.ToString();
    }

    private void OnSubBtnClick()
    {
        float damage = UnityEngine.Random.Range(-100f, -1f);
        UpdateSlider(damage);
    }

    private void OnAddBtnClick()
    {
        float damage = UnityEngine.Random.Range(1f, 100f);
        UpdateSlider(damage);
    }

    private void UpdateSlider(float damage)
    {
        HPImage.fillAmount += damage/1000f;
        if (HPImage.fillAmount < 0)
        {
            HPImage.fillAmount = 0;
        }
        HP = (int)(HPImage.fillAmount * 1000);
        Value.text = HP.ToString();
    }

}
