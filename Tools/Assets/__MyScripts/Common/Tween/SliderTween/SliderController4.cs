using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderController4 : MonoBehaviour
{
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

    #region 颜色插值相关变量
    [Header("是否开启血条颜色插值")]
    public bool IsLerpHPBarColor = false;

    [Header("满血时血条颜色")]
    public Color FullHPColor = Color.green;
    [Header("满血时数值")]
    public float FullHPRange = 1f;

    [Header("健康时血条颜色")]
    public Color HealthyHPColor = Color.yellow;
    [Range(0,1)]
    [Header("健康血量最低值")]
    public float HealthyHPRange = 0.85f;

    [Header("半血时血条颜色")]
    public Color HalfHPColor = Color.red;
    [Range(0, 1)]
    [Header("半血血量最低值")]
    public float HalfHPRange = 0.4f;

    [Header("临死时血条颜色")]
    public Color DangerousHPColor = Color.red;
    [Header("没血时数值")]
    public float NeverHPRange = 0f;

    #endregion
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
        UpdateHPBar(damage);
    }

    private void OnAddBtnClick()
    {
        int damage = UnityEngine.Random.Range(MinDamage, MaxDamage);
        UpdateHPBar(damage);
    }
    /// <summary>
    /// 更新血条
    /// </summary>
    /// <param name="damage">造成的伤害,正数时加血,负数时减血</param>
    private void UpdateHPBar(int damage)
    {
        HP += damage;
        HP = Mathf.Clamp(HP, MinHP, MaxHP);//限制一下最大最小值
        Value.text = HP.ToString();

        //计算新的伤害的过度起始数值(oldValue)和终止数值(newValue)
        float oldValue = HPImage.fillAmount;
        float newValue = (float)HP / MaxHP;
        //如果在开始新的携程前,之前旧的过度还未结束,那么就停止,防止干扰
        if (_Coroutine != null)
        {
            StopCoroutine(_Coroutine);
        }
        _Coroutine = StartCoroutine(TweenProgressBar(oldValue, newValue));

    }
    /// <summary>
    /// 处理血条过渡效果
    /// </summary>
    /// <param name="oldValue">原本血量(0到1)</param>
    /// <param name="newValue">新的血量(0到1)</param>
    /// <returns></returns>
    IEnumerator TweenProgressBar(float oldValue, float newValue)
    {
        float step = (newValue - oldValue) / TransitionTime;
        for (int i = 1; i <= TransitionTime; i++)
        {
            HPImage.fillAmount += step;
            //print(HPImage.fillAmount);
            
            //两种血条插值的方式
            if (IsLerpHPBarColor == true)
            {
                LerpUpdateHPBarColor(HPImage.fillAmount, HPImage);
            }
            else
            {
                UpdateHPBarColor(HPImage.fillAmount, HPImage);
            }
            
            yield return null;
        }
    }
    
    /// <summary>
    /// 更新血条颜色
    /// </summary>
    /// <param name="value"></param>
    private void UpdateHPBarColor(float value,Image hpImage)
    {
        if (Mathf.Abs(value - FullHPRange) <= 0.001f)//因为会出现0.999的情况,所以加了一个绝对值判断
        {
            //满血量,
            hpImage.color = FullHPColor;
        }else if (value >= HealthyHPRange)
        {
            //健康血量,
            hpImage.color = HealthyHPColor;
        }
        else if (value >= HalfHPRange)
        {
            //半血血量,
            hpImage.color = HalfHPColor;
        }
        else
        {
            //危险血量
            hpImage.color = DangerousHPColor;
        }
    }

    /// <summary>
    /// 插值更新血条颜色
    /// </summary>
    /// <param name="value"></param>
    private void LerpUpdateHPBarColor(float value, Image hpImage)
    {
        if (value >= HealthyHPRange)
        {
            //健康血量,
            //计算出当前数值,在健康血量和半血血量之间的插值
            //这边会产生3个临时变量,在每帧更新的时候,所以会有大量GC产生
            //float temp1 = FullHPRange - HealthyHPRange;
            //float temp2 = value - HealthyHPRange;
            //float percent = temp1 / temp2;

            //hpImage.color = Color.Lerp(FullHPColor,HealthyHPColor,percent);

            //优化
            hpImage.color = Color.Lerp(HealthyHPColor,FullHPColor, (value - HealthyHPRange)/(FullHPRange - HealthyHPRange));
        }
        else if (value >= HalfHPRange)
        {
            //半血血量,
            hpImage.color = Color.Lerp(HalfHPColor, HealthyHPColor, (value - HalfHPRange)/(HealthyHPRange - HalfHPRange));
        }
        else
        {
            //危险血量
            hpImage.color = Color.Lerp(DangerousHPColor, HalfHPColor, (value - NeverHPRange) /(HalfHPRange - NeverHPRange));
        }
    }
}
