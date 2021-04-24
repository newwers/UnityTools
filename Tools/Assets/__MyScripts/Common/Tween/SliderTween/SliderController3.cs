using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderController3 : MonoBehaviour {

    public Button AddButton;
    public Button SubButton;
    public Text Value;
    public Image HPImage;

    [Header("当前血量")]
    public int HP = 1000;
    [Header("最大血量")]
    public int MaxHP = 1000;
    [Header("最小血量")]
    public int MinHP = 0;
    [Header("过度时间(单位帧)")]
    public int TransitionTime = 30;

    [Header("最小伤害")]
    public int MinDamage = 1;

    [Header("最大伤害,如果加血就在之间取一个随机数,是掉血那么就取反")]
    public int MaxDamage = 100;

    private Coroutine _Coroutine;

    private void Awake()
    {
        AddButton.onClick.AddListener(OnAddBtnClick);
        SubButton.onClick.AddListener(OnSubBtnClick);

        //初始化UI状态
        HP = MaxHP;
        HPImage.fillAmount = 1f;
        Value.text = HP.ToString();
    }

    private void OnSubBtnClick()
    {
        int damage = UnityEngine.Random.Range(-MaxDamage, -MinDamage);
        UpdateSlider(damage);
    }

    private void OnAddBtnClick()
    {
        int damage = UnityEngine.Random.Range(MinDamage, MaxDamage);
        UpdateSlider(damage);
    }

    private void UpdateSlider(int damage)
    {
        HP += damage;
        HP = Mathf.Clamp(HP, MinHP, MaxHP);//限制一下最大最小值
        Value.text = HP.ToString();

        float oldValue = HPImage.fillAmount;
        float newValue = (float)HP / MaxHP;
        if (_Coroutine != null)
        {
            StopCoroutine(_Coroutine);
        }
        _Coroutine = StartCoroutine(TweenProgressBar(oldValue,newValue));
        
    }

    IEnumerator TweenProgressBar(float oldValue,float newValue)
    {
        float step = (newValue - oldValue) / TransitionTime;
        for (int i = 1; i <= TransitionTime; i++)
        {
            HPImage.fillAmount += step;
            //print(HPImage.fillAmount);
            
            yield return null;
        }
    }

}
