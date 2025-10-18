using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Z.UI;

public class CommonTipsPanel : BaseMonoSingleClass<CommonTipsPanel>, IUI
{
    public UIReferenceComponent ui;

    private Button m_ysebutton_button;
    private TextMeshProUGUI m_nobtntext_textmeshprougui;
    private TextMeshProUGUI m_tipstext_textmeshprougui;
    private Toggle m_prompttoggle_toggle;
    private TextMeshProUGUI m_prompttoggletext_textmeshprougui;
    private RectTransform m_root_recttransform;
    private TextMeshProUGUI m_yesbtntext_textmeshprougui;
    private Button m_nobutton_button;

    private Action m_yesAction;
    private Action m_noAction;
    private Action<bool> m_toggleAction;
    private bool m_isToggleFlag = false;

    public static void Show(Action yesAction, Action noAction, Action<bool> ToggleAction, string tips, bool isShowPromptToggle = false, string yesBtnText = "Yes", string noBtnText = "No")
    {
        if (Instance.m_tipstext_textmeshprougui == null)
        {
            Instance.Init();
        }

        Instance.m_yesAction = yesAction;
        Instance.m_noAction = noAction;
        Instance.m_toggleAction = ToggleAction;

        Instance.m_tipstext_textmeshprougui.text = tips;
        Instance.m_yesbtntext_textmeshprougui.text = yesBtnText;
        Instance.m_nobtntext_textmeshprougui.text = noBtnText;

        UIUtil.SetActive(Instance.m_prompttoggle_toggle, isShowPromptToggle);

        Instance.OnShow();
    }

    private void Start()
    {
        if (m_tipstext_textmeshprougui == null)
        {
            Init();
        }
    }

    public void Init()
    {
        m_ysebutton_button = ui.GetUI<UnityEngine.UI.Button>("YseButton_Button");
        m_ysebutton_button.onClick.AddListener(OnYesBtnClick);
        m_yesbtntext_textmeshprougui = ui.GetUI<TMPro.TextMeshProUGUI>("YesBtnText_TextMeshProUGUI");

        m_nobutton_button = ui.GetUI<UnityEngine.UI.Button>("NoButton_Button");
        m_nobutton_button.onClick.AddListener(OnNoBtnClick);
        m_nobtntext_textmeshprougui = ui.GetUI<TMPro.TextMeshProUGUI>("NoBtnText_TextMeshProUGUI");


        m_tipstext_textmeshprougui = ui.GetUI<TMPro.TextMeshProUGUI>("TipsText_TextMeshProUGUI");

        m_prompttoggle_toggle = ui.GetUI<UnityEngine.UI.Toggle>("PromptToggle_Toggle");
        m_prompttoggle_toggle.onValueChanged.AddListener(OnToggleValueChange);
        m_prompttoggletext_textmeshprougui = ui.GetUI<TMPro.TextMeshProUGUI>("PromptToggleText_TextMeshProUGUI");

        m_root_recttransform = ui.GetUI<UnityEngine.RectTransform>("Root_RectTransform");
    }

    private void OnToggleValueChange(bool value)
    {
        m_isToggleFlag = value;
        m_toggleAction?.Invoke(value);
    }

    private void OnNoBtnClick()
    {
        m_noAction?.Invoke();
        OnHide();
    }

    private void OnYesBtnClick()
    {
        m_yesAction?.Invoke();
        OnHide();
    }

    public bool IsShow()
    {
        return m_root_recttransform.gameObject.activeInHierarchy;
    }

    public void OnHide()
    {
        UIUtil.SetActive(m_root_recttransform, false);
    }

    public void OnShow()
    {
        UIUtil.SetActive(m_root_recttransform, true);
    }
}
