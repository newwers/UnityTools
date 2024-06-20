using MyWorld;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z.UI;

namespace MyWorld
{
    public class CommonTipsPanel : BaseUIController
    {

        public UIReferenceComponent ui;

        public void ShowTips(string title,string tips)
        {
            RefreshUI(title,tips);
            gameObject.SetActive(true);
        }
        private void Start()
        {
            Player.OnGameResetAction += Player_OnGameResetAction;

            OnHide();
        }

        private void OnDestroy()
        {
            Player.OnGameOverAction -= Player_OnGameResetAction;
        }

        private void Player_OnGameResetAction()
        {
            OnHide();
        }


        public override void OnHide()
        {
            base.OnHide();
            gameObject.SetActive(false);
        }

        private void RefreshUI(string title,string tips)
        {
            TMPro.TextMeshProUGUI title_textmeshprougui = ui.GetUI<TMPro.TextMeshProUGUI>("Title_TextMeshProUGUI");
            UIUtil.SetLabel(title_textmeshprougui, title);
            TMPro.TextMeshProUGUI contenttext_textmeshprougui = ui.GetUI<TMPro.TextMeshProUGUI>("ContentText_TextMeshProUGUI");
            UIUtil.SetLabel(contenttext_textmeshprougui, tips);

        }

    }
}
