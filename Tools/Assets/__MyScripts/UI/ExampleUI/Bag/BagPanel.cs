using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Z.UI;

namespace MyWorld
{
    public class BagPanel : BaseUIController
    {
        public UIReferenceComponent prefab;
        List<UIReferenceComponent> prefabPool = UnityEngine.Pool.ListPool<UIReferenceComponent>.Get();

        private Item m_CurItem;

        public override void OnHide()
        {
            base.OnHide();

            for (int i = 0; i < prefabPool.Count; i++)
            {
                UIUtil.SetActive(prefabPool[i], false);
            }
            m_CurItem = null;
        }

        public override void RefreshUI()
        {
            base.RefreshUI();

            for (int i = 0; i < prefabPool.Count; i++)
            {
                UIUtil.SetActive(prefabPool[i], false);
            }
            var bag = Player.Instance.GetBagItem();
            foreach (var item in bag)
            {
                var ui = GetItem();
                if (ui != null)
                {
                    TMPro.TextMeshProUGUI name_textmeshprougui = ui.GetUI<TMPro.TextMeshProUGUI>("name_TextMeshProUGUI");
                    UIUtil.SetLabel(name_textmeshprougui, item.Value.itemName);
                    TMPro.TextMeshProUGUI des_textmeshprougui = ui.GetUI<TMPro.TextMeshProUGUI>("des_TextMeshProUGUI");
                    UIUtil.SetLabel(des_textmeshprougui, item.Value.description);
                    UnityEngine.UI.Button buybtn_button = ui.GetUI<UnityEngine.UI.Button>("buyBtn_Button");
                    if (buybtn_button)
                    {
                        buybtn_button.onClick.RemoveAllListeners();
                        buybtn_button.onClick.AddListener(() =>
                        {
                            OnClick(item.Value);
                        });
                    }
                    TMPro.TextMeshProUGUI num_textmeshprougui = ui.GetUI<TMPro.TextMeshProUGUI>("num_TextMeshProUGUI");
                    UIUtil.SetLabel(num_textmeshprougui, $"{item.Value.num}");
                }
            }
        }

        private void OnClick(Item item)
        {
            m_CurItem = item;

            //先使用一个,后面加数量显示
            var useItem = item.Clone();
            useItem.num = 1;

            //使用物品 
            Player.Instance.UseItem(useItem);

            RefreshUI();
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
    }
}
