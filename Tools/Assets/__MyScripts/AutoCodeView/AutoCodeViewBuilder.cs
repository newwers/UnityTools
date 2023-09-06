using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z.UI;

namespace AutoCode
{


    /// <summary>
    /// 给点一个ui serialize 然后生成一个logic,logic里面包含所有UI调用接口,用 partial 隔开,
    /// </summary>
    public class AutoCodeViewBuilder : AutoCodeBuilderBase
    {
        public UIReferenceComponent m_ui;

        public AutoCodeViewBuilder(string name, string filePath, UIReferenceComponent ui) : base(name, filePath)
        {
            m_ui = ui;
        }

        protected override void BuilderAutoCode()
        {
            if (m_ui == null)
            {
                return;
            }

            AddString("using UnityEngine;");
            AddString("using UnityEngine.UI;");
            AddString("namespace TopGame.UI");
            AddString("{");
            AddTabNum();

            AddString($"public partial class {m_FileName}");
            AddString("{");
            AddTabNum();


            //设置控件变量名
            for (int i = 0; i < m_ui.Datas.Count; i++)
            {
                var widget = m_ui.Datas[i];
                if (widget == null)
                {
                    continue;
                }
                string name = widget.name;
                if (!string.IsNullOrWhiteSpace(widget.name))
                {
                    name= widget.name;
                }

                AddString($"private {widget.GetType().ToString()} m_{name};");

                //Debug.Log($"widget type:{widget.widget.GetType()},type:{widget.assignType}");
            }

            

            AddString("//------------------------------------------------------");
            AddString("public void AwakeUI(UIBase pBase)");

            AddString("{");
            AddTabNum();

            AddString("if (pBase == null || pBase.ui == null) return;");

            //设置控件获取
            for (int i = 0; i < m_ui.Datas.Count; i++)
            {
                var widget = m_ui.Datas[i];
                if (widget == null)
                {
                    continue;
                }
                string name = widget.name;
                if (!string.IsNullOrWhiteSpace(widget.name))
                {
                    name = widget.name;
                }

                AddString($"m_{name} = pBase.ui.GetWidget<{widget.GetType().ToString()}>(\"{name}\");");

            }

            

            SubTabNum();
            AddString("}");

            //设置控件调用函数和点击事件 Text EventTriggerListener ListView 
            for (int i = 0; i < m_ui.Datas.Count; i++)
            {
                var widget = m_ui.Datas[i];
                if (widget == null)
                {
                    continue;
                }
                string name = widget.name;
                if (!string.IsNullOrWhiteSpace(widget.name))
                {
                    name = widget.name;
                }

                switch (widget.GetType().FullName)
                {
                    case "UnityEngine.UI.Text":
                        AddString("//------------------------------------------------------");
                        AddString($"public void Set{name}Label(uint key)");
                        AddString("{");
                        AddTabNum();

                        AddString($"UIUtil.SetLabel(m_{name}, key);");


                        SubTabNum();
                        AddString("}");
                        break;
                    case "TopGame.UI.EventTriggerListener":
                        AddString("//------------------------------------------------------");
                        AddString($"public void Set{name}Listener(EventTriggerListener.VoidDelegate click)");
                        AddString("{");
                        AddTabNum();

                        AddString($"if (m_{name}) m_{name}.onClick = click;");


                        SubTabNum();
                        AddString("}");
                        break;
                    case "TopGame.UI.ListView":
                        AddString("//------------------------------------------------------");
                        AddString($"public void RefreshHeroList(ListView list,int count, bool isScroll)");
                        AddString("{");
                        AddTabNum();

                        AddString($"if (list)");
                        AddString("{");
                        AddTabNum();

                        AddString("list.numItems = (uint)count;");
                        AddString("if (isScroll) list.content.anchoredPosition = Vector2.zero;");

                        SubTabNum();
                        AddString("}");

                        SubTabNum();
                        AddString("}");
                        break;
                }

                

            }

            

            SubTabNum();
            AddString("}");

            SubTabNum();
            AddString("}");
        }
    }
}
