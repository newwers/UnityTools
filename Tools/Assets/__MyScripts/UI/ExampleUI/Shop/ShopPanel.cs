using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;
using Z.UI;

namespace MyWorld
{
    public class ShopPanel : BaseUIController
    {

        public List<ShopItem> vShopItemList;
        public UIReferenceComponent prefab;
        public UIReferenceComponent buyPanel;

        List<UIReferenceComponent> prefabPool = UnityEngine.Pool.ListPool<UIReferenceComponent>.Get();
        private ShopItem m_CurBuyItem;

        private TextMeshProUGUI m_buyitemname_textmeshprougui;
        private TMP_InputField m_input_tmp_inputfield;
        private Button m_buybtn_button;


        private void Start()
        {
            if (buyPanel)
            {
                m_buyitemname_textmeshprougui = buyPanel.GetUI<TMPro.TextMeshProUGUI>("BuyItemName_TextMeshProUGUI");
                m_input_tmp_inputfield = buyPanel.GetUI<TMPro.TMP_InputField>("Input_TMP_InputField");
                m_buybtn_button = buyPanel.GetUI<UnityEngine.UI.Button>("BuyBtn_Button");
                if (m_buybtn_button)
                {
                    m_buybtn_button.onClick.AddListener(OnBuyBtnClick);
                }
            }
        }


        public override void OnHide()
        {
            base.OnHide();

            for (int i = 0; i < prefabPool.Count; i++)
            {
                UIUtil.SetActive(prefabPool[i], false);
            }
            m_CurBuyItem = null;
        }

        public override void RefreshUI()
        {
            base.RefreshUI();

            for (int i = 0; i < prefabPool.Count; i++)
            {
                UIUtil.SetActive(prefabPool[i], false);
            }

            for (int i = 0;i< vShopItemList.Count; i++)
            {
                var item = vShopItemList[i];
                if (item != null)
                {
                    var ui = GetItem();
                    if (ui != null)
                    {
                        TMPro.TextMeshProUGUI name_textmeshprougui = ui.GetUI<TMPro.TextMeshProUGUI>("name_TextMeshProUGUI");
                        UIUtil.SetLabel(name_textmeshprougui, item.itemName);
                        TMPro.TextMeshProUGUI des_textmeshprougui = ui.GetUI<TMPro.TextMeshProUGUI>("des_TextMeshProUGUI");
                        UIUtil.SetLabel(des_textmeshprougui, item.description);
                        TMPro.TextMeshProUGUI price_textmeshprougui = ui.GetUI<TMPro.TextMeshProUGUI>("price_TextMeshProUGUI");
                        UIUtil.SetLabel(price_textmeshprougui, $"￥{item.price}");
                        UnityEngine.UI.Button buybtn_button = ui.GetUI<UnityEngine.UI.Button>("buyBtn_Button");
                        if (buybtn_button)
                        {
                            buybtn_button.onClick.RemoveAllListeners();
                            buybtn_button.onClick.AddListener(() =>
                            {
                                OnClick(item);
                            });
                        }
                        TMPro.TextMeshProUGUI num_textmeshprougui = ui.GetUI<TMPro.TextMeshProUGUI>("num_TextMeshProUGUI");
                        UIUtil.SetLabel(num_textmeshprougui, $"{item.num}");
                    }
                }
            }
        }

        private void OnClick(ShopItem item)
        {
            //弹出购买弹窗
            m_CurBuyItem = item;

            UIUtil.SetActive(buyPanel, true);
            RefreshBuyPanel();

        }

        UIReferenceComponent GetItem()
        {
            for (int i = 0; i < prefabPool.Count; i++)
            {
                if (prefabPool[i] == null) continue;
                if (!prefabPool[i].gameObject.activeInHierarchy)
                {
                    prefabPool[i].gameObject.SetActive(true);
                    return prefabPool[i];
                }
            }

            if (prefab == null)
            {
                return null;
            }

            var cell = Instantiate<UIReferenceComponent>(prefab, prefab.transform.parent);

            prefabPool.Add(cell);
            cell.gameObject.SetActive(true);
            return cell;
        }

        #region Buypanel Logic

        void RefreshBuyPanel()
        {
            if (!m_CurBuyItem)
            {
                return;
            }
            UIUtil.SetLabel(m_buyitemname_textmeshprougui, m_CurBuyItem.itemName);

            if (m_input_tmp_inputfield)
            {
                m_input_tmp_inputfield.text = "1";
            }
        }

        private void OnBuyBtnClick()
        {
            if (m_CurBuyItem == null)
            {
                print("当前未选择商品");

                return;
            }

            long buyNum = int.Parse(m_input_tmp_inputfield.text);

            //判断价格
            if (Player.Instance.Money < m_CurBuyItem.price * buyNum)
            {
                print("价格不足");
                return;//价格不足
            }

            //判断数量
            if (m_CurBuyItem.num <= 0)
            {
                print("数量不足");
                return;//数量不足
            }

            //扣除金钱
            Player.Instance.Money -= m_CurBuyItem.price * buyNum;

            m_CurBuyItem.num -= buyNum;

            //添加到背包
            var buyItem = m_CurBuyItem.Clone();
            buyItem.num = buyNum;
            Player.Instance.AddItem(buyItem);

            RefreshUI();
        }

        #endregion 
    }
}
