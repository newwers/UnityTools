/********************************************************************
生成日期:	1:11:2020 10:06
类    名: 	TestEditor
作    者:	zdq
描    述:	方便测试用的脚本
*********************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using TopGame.Logic;
using TopGame.Data;
using System.Linq;
using TopGame.UI;
using TopGame.Core;
using TopGame.ED;
using TopGame.SvrData;
using Proto3;
using TopGame.Base;
using Framework.Plugin.Guide;
using System;
using System.IO;
using Framework.Logic;
using Framework.Base;
using Framework.UI;
using Framework.Data;
using Framework.Core;
using Framework.BattlePlus;
using TopGame.Net;

namespace TopGame
{
    public class TestEditor : EditorWindow
    {

        

        float m_TimeScale;


        enum ETab
        {
            Controller,
            RuntimeData,
            BuildingProduct,
        }
        static string[] TAB = new string[] { "操控", "运行数据", "城建产量" };

        ETab m_Tab = ETab.Controller;

        Vector2 m_Scroller = Vector2.zero;

        public static List<string> m_PathPops = new List<string>();
        public static CsvData_TargetPaths m_pPathAsset = null;

        UI.EUIType m_ShowUI = UI.EUIType.None;
        InputRecorder m_InputRecorder = new InputRecorder();


        [MenuItem("Tools/测试面板 _F6")]
        public static void ShowWindow2()
        {
            InitGlobalData();
            EditorWindow window = EditorWindow.GetWindow(typeof(TestEditor));
            window.titleContent = new GUIContent("测试面板");
            
            
        }

        [MenuItem("Tools/停止输入播放 %q")]
        public static void StopInputPlay()
        {
            InputRecorder.SStop();
            Debug.Log("停止输入播放");
        }

        private void OnEnable()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            //EditorApplication.update+= m_Input.Update;
            Debug.Log("TestEditor OnEnable");
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.update -= m_InputRecorder.Update;
            Debug.Log("TestEditor OnDisable");
            MouseHook.Stop();
            KeyboardHook.Stop();
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            //Debug.LogError("state:" + state);
            switch (state)
            {
                case PlayModeStateChange.EnteredEditMode:
                    break;
                case PlayModeStateChange.ExitingEditMode:
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                    m_TimeScale = 1;//进入游戏时,默认倍速1
                    MouseHook.Stop();
                    KeyboardHook.Stop();
                    break;
                case PlayModeStateChange.ExitingPlayMode:
                    break;
                default:
                    break;
            }
        }

        public static void InitGlobalData()
        {
            if (DataManager.getInstance() == null)
            {
                return;
            }
            m_pPathAsset = DataManager.getInstance().TargetPaths;

            m_PathPops.Clear();
            if (m_pPathAsset != null)
            {
                foreach (var db in m_pPathAsset.datas)
                {
                    if(db.Value.strName.Length>2)
                    {
                        string strTitle = db.Value.strName.Substring(0, 1);
                        m_PathPops.Add(strTitle + "/" + db.Value.strName + "[" + db.Key + "]");
                    }
                    else
                    {
                        m_PathPops.Add(db.Value.strName + "[" + db.Key + "]");
                    }
                }
            }

        }

        private void Update()
        {
            
        }


        private void OnFocus()
        {
            m_TimeScale = Time.timeScale;
            Debug.Log("OnFocus set m_TimeScale:" + m_TimeScale);
        }

        private void OnGUI()
        {
            


            Color color = GUI.color;
            GUILayout.BeginHorizontal();
            for (int i = 0; i < TAB.Length; ++i)
            {
                if (i == (int)m_Tab) GUI.color = Color.red;
                else GUI.color = color;
                if (GUILayout.Button(TAB[i]))
                {
                    m_Tab = (ETab)i;
                }
            }
            GUILayout.EndHorizontal();
            GUI.color = color;
            m_Scroller = EditorGUILayout.BeginScrollView(m_Scroller);
            try
            {
                switch (m_Tab)
                {
                    case ETab.Controller:
                        DrawController();
                        break;
                    case ETab.RuntimeData:
                        DrawRuntimeData();
                        break;
                    case ETab.BuildingProduct:
                        DrawBuildingProduct();
                        break;
                }
            }
            catch(Exception ex)
            {
                Debug.LogWarning(ex.ToString());
            }


            EditorGUILayout.EndScrollView();

            // 监听键盘按键
            Event e = Event.current;

            if (e != null && e.isKey && e.type == EventType.KeyDown)
            {
                KeyCode keyCode = e.keyCode;
                Debug.Log("Key Down: " + keyCode);

                Repaint();
            }
        }
#region 建筑生产模拟
        [System.Serializable]
        class BuildingItem
        {
            public int id;
            public int cd;
            public int onceProductPower =1;
            public int productCnt;
            public List<int> reqLists;

            [System.NonSerialized]
            public int runtimeCd;
            [System.NonSerialized]
            public int runtimeCnt;


            // 估算值
            [System.NonSerialized]
            public int runtimeCntEstimate;
            [System.NonSerialized]
            public int productCntEstimate;

            [System.NonSerialized]
            public int productAdjustCount;  // 补正次数

            [System.NonSerialized]
            public float estimateCostOverTime;     //消耗吞吐耗时       
            [System.NonSerialized]
            public List<int> reqEstimatePercent;    //预估消耗量占比

            [System.NonSerialized]
            public double reqCostEstimatePercent;    //所有生产线每秒消耗
        }
        [System.Serializable]
        class ProductorMoniter
        {
            public int projectRout = 1;
            public int m_nBuildingTime = 0;
            public List<BuildingItem> m_vBuildings = new List<BuildingItem>();
            public List<BuildingItem> m_vStores = new List<BuildingItem>();
        }
        ProductorMoniter m_pProductorMoniter = new ProductorMoniter();
        List<BuildingItem> m_vProductList = new List<BuildingItem>();
        void DrawBuildingProduct()
        {
            m_pProductorMoniter.m_nBuildingTime = EditorGUILayout.IntField("时长(秒)", m_pProductorMoniter.m_nBuildingTime);
            m_pProductorMoniter.projectRout = EditorGUILayout.IntField("方案", m_pProductorMoniter.projectRout);
            if (GUILayout.Button("添加库存建材"))
            {
                m_pProductorMoniter.m_vStores.Add(new BuildingItem() { id = m_pProductorMoniter.m_vStores.Count+1 });
            }
            if (m_pProductorMoniter.m_vStores.Count>0)
            {
                GUILayoutOption[] storeLayout = new GUILayoutOption[] { GUILayout.Width(position.width / 4) };
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("材料ID", storeLayout);
                EditorGUILayout.LabelField("库存数量", storeLayout);
                EditorGUILayout.LabelField("估计剩余库存", storeLayout);
                EditorGUILayout.LabelField("剩余库存", storeLayout);
                GUILayout.EndHorizontal();
                for(int i = 0; i < m_pProductorMoniter.m_vStores.Count; ++i)
                {
                    GUILayout.BeginHorizontal();
                    m_pProductorMoniter.m_vStores[i].id = EditorGUILayout.IntField(m_pProductorMoniter.m_vStores[i].id, storeLayout);
                    m_pProductorMoniter.m_vStores[i].productCnt = EditorGUILayout.IntField(m_pProductorMoniter.m_vStores[i].productCnt, storeLayout);
                    EditorGUILayout.LabelField(m_pProductorMoniter.m_vStores[i].runtimeCntEstimate.ToString(), storeLayout);
                    EditorGUILayout.LabelField(m_pProductorMoniter.m_vStores[i].runtimeCnt.ToString(), storeLayout);
                    GUILayout.EndHorizontal();
                }
            }
            if (m_pProductorMoniter.m_vStores.Count <= 0) return;
            GUILayout.Space(20);
            if (GUILayout.Button("添加生产建材"))
            {
                m_pProductorMoniter.m_vBuildings.Add(new BuildingItem() { id = m_pProductorMoniter.m_vBuildings.Count + 1 });
            }
            if (m_pProductorMoniter.m_vBuildings.Count > 0)
            {
                GUILayoutOption[] storeLayout = new GUILayoutOption[] { GUILayout.Width(position.width / (5+ m_pProductorMoniter.m_vStores.Count)) };
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("生产线", storeLayout);
                EditorGUILayout.LabelField("单次产出基数", storeLayout);
                EditorGUILayout.LabelField("估计产出", storeLayout);
                EditorGUILayout.LabelField("产出", storeLayout);
                EditorGUILayout.LabelField("生成CD(秒)", storeLayout);
                for(int i = 0; i < m_pProductorMoniter.m_vStores.Count;++i)
                    EditorGUILayout.LabelField("需要材料" +(i+1), storeLayout);
                GUILayout.EndHorizontal();

                for (int i = 0; i < m_pProductorMoniter.m_vBuildings.Count; ++i)
                {
                    GUILayout.BeginHorizontal();
                    m_pProductorMoniter.m_vBuildings[i].id = EditorGUILayout.IntField(m_pProductorMoniter.m_vBuildings[i].id, storeLayout);
                    m_pProductorMoniter.m_vBuildings[i].onceProductPower = EditorGUILayout.IntField(m_pProductorMoniter.m_vBuildings[i].onceProductPower, storeLayout);
                    EditorGUILayout.LabelField(m_pProductorMoniter.m_vBuildings[i].productCntEstimate.ToString(), storeLayout);
                    EditorGUILayout.LabelField(m_pProductorMoniter.m_vBuildings[i].productCnt.ToString(), storeLayout);
                    m_pProductorMoniter.m_vBuildings[i].cd = Mathf.Max(1, EditorGUILayout.IntField(m_pProductorMoniter.m_vBuildings[i].cd, storeLayout));
                    if(m_pProductorMoniter.m_vBuildings[i].reqLists == null)
                        m_pProductorMoniter.m_vBuildings[i].reqLists = new List<int>(m_pProductorMoniter.m_vStores.Count);
                    if(m_pProductorMoniter.m_vBuildings[i].reqLists.Count != m_pProductorMoniter.m_vStores.Count)
                    {
                        int[] temps = m_pProductorMoniter.m_vBuildings[i].reqLists.ToArray();
                        m_pProductorMoniter.m_vBuildings[i].reqLists.Clear();
                        for (int j = 0; j < m_pProductorMoniter.m_vStores.Count; ++j)
                        {
                            if (j < temps.Length) m_pProductorMoniter.m_vBuildings[i].reqLists.Add( temps[j]);
                            else m_pProductorMoniter.m_vBuildings[i].reqLists.Add(0);
                        }
                    }
                    for (int j = 0; j < m_pProductorMoniter.m_vStores.Count; ++j)
                    {
                        m_pProductorMoniter.m_vBuildings[i].reqLists[j] = EditorGUILayout.IntField(m_pProductorMoniter.m_vBuildings[i].reqLists[j], storeLayout);
                    }
                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.BeginHorizontal();
            if (m_pProductorMoniter.m_vBuildings.Count > 0 && GUILayout.Button("保存"))
            {
                string strFile = Application.dataPath + "/../EditorConfigs/";
                if (!System.IO.Directory.Exists(strFile))
                    System.IO.Directory.CreateDirectory(strFile);
                strFile += "ProcudtorMoniter.json";
                StreamWriter sw = new StreamWriter(File.Open(strFile, FileMode.OpenOrCreate, FileAccess.Write));
                sw.BaseStream.Position = 0;
                sw.BaseStream.SetLength(0);
                sw.BaseStream.Flush();
                sw.Write(JsonUtility.ToJson(m_pProductorMoniter, true));
                sw.Close();
            }
            if (GUILayout.Button("载入"))
            {
                string strFile = Application.dataPath + "/../EditorConfigs/ProcudtorMoniter.json";
                if (File.Exists(strFile))
                {
                    try
                    {
                        m_pProductorMoniter = JsonUtility.FromJson<ProductorMoniter>(File.ReadAllText(strFile));
                    }
                    catch (System.Exception ex)
                    {

                    }
                }
            }
            GUILayout.EndHorizontal();
            if (m_pProductorMoniter.m_vBuildings.Count > 0)
            {
                if (GUILayout.Button("模拟", new GUILayoutOption[] { GUILayout.Height(50) }))
                {
                    m_pProductorMoniter.m_vBuildings.Sort((BuildingItem oth1, BuildingItem oth2) => { return oth1.cd - oth2.cd; });

                    EditorUtility.DisplayProgressBar("", "", 0);
                    for (int i = 0; i < m_pProductorMoniter.m_vBuildings.Count; ++i)
                    {
                        m_pProductorMoniter.m_vBuildings[i].productAdjustCount = 0;
                        m_pProductorMoniter.m_vBuildings[i].runtimeCd = 0;
                        m_pProductorMoniter.m_vBuildings[i].productCnt = 0;
                        m_pProductorMoniter.m_vBuildings[i].productCntEstimate = 0;
                        m_pProductorMoniter.m_vBuildings[i].estimateCostOverTime = 0;
                    }
                    for (int i = 0; i < m_pProductorMoniter.m_vStores.Count; ++i)
                    {
                        m_pProductorMoniter.m_vStores[i].estimateCostOverTime = 0;
                        m_pProductorMoniter.m_vStores[i].runtimeCnt = (int)m_pProductorMoniter.m_vStores[i].productCnt;
                        m_pProductorMoniter.m_vStores[i].runtimeCntEstimate = (int)m_pProductorMoniter.m_vStores[i].productCnt;
                    }
                    int curSec = 0;
                    Framework.Base.ProfilerTicker ticker = new ProfilerTicker();
                    ticker.Start("模拟生产");
                    m_vProductList.Clear();
                    for (int i = 0; i < m_pProductorMoniter.m_vBuildings.Count; ++i)
                    {
                        bool bOk = true;
                        for (int j = 0; j < m_pProductorMoniter.m_vBuildings[i].reqLists.Count; ++j)
                        {
                            if (m_pProductorMoniter.m_vBuildings[i].reqLists[j] > m_pProductorMoniter.m_vStores[j].runtimeCnt)
                            {
                                bOk = false;
                                break;
                            }
                        }
                        if (bOk)
                        {
                            m_pProductorMoniter.m_vBuildings[i].runtimeCd = m_pProductorMoniter.m_vBuildings[i].cd;
                            for (int j = 0; j < m_pProductorMoniter.m_vBuildings[i].reqLists.Count; ++j)
                            {
                                m_pProductorMoniter.m_vStores[j].runtimeCnt -= m_pProductorMoniter.m_vBuildings[i].reqLists[j];
                            }
                            m_vProductList.Add(m_pProductorMoniter.m_vBuildings[i]);
                        }
                    }
                    while (curSec <= m_pProductorMoniter.m_nBuildingTime && m_vProductList.Count>0)
                    {
                        curSec++;
                        if (curSec % 10 == 0)
                            EditorUtility.DisplayProgressBar("模拟生产", "第" + curSec + " 秒", (float)curSec / (float)m_pProductorMoniter.m_nBuildingTime);
                      //  bool bAllOk = false;
                        for (int i = 0; i < m_vProductList.Count;)
                        {
                            if (m_vProductList[i].runtimeCd > 0)
                            {
                                m_vProductList[i].runtimeCd--;
                                if (m_vProductList[i].runtimeCd <= 0)
                                {
                                    m_vProductList[i].productCnt++;

                                    bool bOk = true;
                                    for (int j = 0; j < m_vProductList[i].reqLists.Count; ++j)
                                    {
                                        if (m_vProductList[i].reqLists[j] > m_pProductorMoniter.m_vStores[j].runtimeCnt)
                                        {
                                            bOk = false;
                                            break;
                                        }
                                    }
                                    if (bOk)
                                    {
                                        m_vProductList[i].runtimeCd = m_vProductList[i].cd;
                                        for (int j = 0; j < m_vProductList[i].reqLists.Count; ++j)
                                        {
                                            m_pProductorMoniter.m_vStores[j].runtimeCnt -= m_vProductList[i].reqLists[j];
                                        }
                                        ++i;
                                    }
                                    else
                                    {
                                        m_vProductList.RemoveAt(i);
                                    }
                                }
								else ++i;
                            }
                            else
                                ++i;
                        }
                     //   if (!bAllOk) break;
                    }
                    EditorUtility.ClearProgressBar();
                    long time = ticker.Stop();
                    string strTips = "";

                    // EditorUtility.DisplayProgressBar("生产估算", "", 0);
                    int remainBuildTime = m_pProductorMoniter.m_nBuildingTime;
                    if (m_pProductorMoniter.projectRout == 1)
                    {
                        int maxCd = 0;
                        for (int i = 0; i < m_pProductorMoniter.m_vBuildings.Count; ++i)
                        {
                            maxCd = Mathf.Max(maxCd, m_pProductorMoniter.m_vBuildings[i].cd);
                            m_pProductorMoniter.m_vBuildings[i].productCntEstimate = 0;
                            if (m_pProductorMoniter.m_vBuildings[i].cd <= m_pProductorMoniter.m_nBuildingTime)
                            {
                                bool bOk = true;
                                for (int j = 0; j < m_pProductorMoniter.m_vBuildings[i].reqLists.Count; ++j)
                                {
                                    if (m_pProductorMoniter.m_vBuildings[i].reqLists[j] > m_pProductorMoniter.m_vStores[j].runtimeCntEstimate)
                                    {
                                        bOk = false;
                                        break;
                                    }
                                }
                                if (bOk)
                                {
                                    for (int j = 0; j < m_pProductorMoniter.m_vBuildings[i].reqLists.Count; ++j)
                                    {
                                        m_pProductorMoniter.m_vStores[j].runtimeCntEstimate -= m_pProductorMoniter.m_vBuildings[i].reqLists[j];
                                    }
                                    m_pProductorMoniter.m_vBuildings[i].productCntEstimate++;
                                }
                            }
                        }  
                        remainBuildTime -= maxCd;

                        //step1 计算仓库存储在所有生产列所需量中，全部消耗用时
                        for (int i = 0; i < m_pProductorMoniter.m_vStores.Count; ++i)
                        {
                            m_pProductorMoniter.m_vStores[i].estimateCostOverTime = 0;
                            float costTimeRate = 0;
                            for (int j = 0; j < m_pProductorMoniter.m_vBuildings.Count; ++j)
                            {
                                if (m_pProductorMoniter.m_vBuildings[j].reqLists[i] > 0)
                                    costTimeRate += m_pProductorMoniter.m_vBuildings[j].reqLists[i] / (float)m_pProductorMoniter.m_vBuildings[j].cd;
                            }
                            if (costTimeRate > 0) m_pProductorMoniter.m_vStores[i].estimateCostOverTime = m_pProductorMoniter.m_vStores[i].runtimeCntEstimate / costTimeRate;
                            if (m_pProductorMoniter.m_vStores[i].estimateCostOverTime >= remainBuildTime) //如果大于建造时长
                                m_pProductorMoniter.m_vStores[i].estimateCostOverTime = remainBuildTime;
                        }

                        //! step2 计算每个生产项，在生产时长内，需要消耗的材料个数
                        for (int j = 0; j < m_pProductorMoniter.m_vBuildings.Count; ++j)
                        {
                            int cnt = int.MaxValue;
                            for (int i = 0; i < m_pProductorMoniter.m_vStores.Count; ++i)
                            {
                                cnt = Mathf.Min(cnt, (int)(m_pProductorMoniter.m_vStores[i].estimateCostOverTime / m_pProductorMoniter.m_vBuildings[j].cd));
                            }
                            m_pProductorMoniter.m_vBuildings[j].productCntEstimate += cnt;
                        }

                        //! step3 计算库存剩余
                        for (int i = 0; i < m_pProductorMoniter.m_vStores.Count; ++i)
                        {
                            for (int j = 0; j < m_pProductorMoniter.m_vBuildings.Count; ++j)
                            {
                                if(m_pProductorMoniter.m_vBuildings[j].productCntEstimate>1)
                                m_pProductorMoniter.m_vStores[i].runtimeCntEstimate -= (int)(m_pProductorMoniter.m_vBuildings[j].reqLists[i] * (m_pProductorMoniter.m_vBuildings[j].productCntEstimate-1));
                            }
                        }
                    }
                    else if (m_pProductorMoniter.projectRout == 2)
                    {
                        for (int j = 0; j < m_pProductorMoniter.m_vBuildings.Count; ++j)
                        {
                            m_pProductorMoniter.m_vBuildings[j].reqEstimatePercent = new List<int>();
                            for (int i = 0; i < m_pProductorMoniter.m_vStores.Count; ++i)
                                m_pProductorMoniter.m_vBuildings[j].reqEstimatePercent.Add(0);
                        }

                        //step1 计算用量占比
                        for (int i = 0; i < m_pProductorMoniter.m_vStores.Count; ++i)
                        {
                            m_pProductorMoniter.m_vStores[i].estimateCostOverTime = 0;
                            if (m_pProductorMoniter.m_vStores[i].runtimeCntEstimate <= 0) continue;
                            int totalCnt = 0;
                            for (int j = 0; j < m_pProductorMoniter.m_vBuildings.Count; ++j)
                            {
                                totalCnt += m_pProductorMoniter.m_vBuildings[j].reqLists[i];
                            }
                            if(totalCnt<=0) continue;
                            for (int j = 0; j < m_pProductorMoniter.m_vBuildings.Count; ++j)
                            {
                                if (m_pProductorMoniter.m_vBuildings[j].reqLists[i] > 0)
                                    m_pProductorMoniter.m_vBuildings[j].reqEstimatePercent[i] = (int)(m_pProductorMoniter.m_vStores[i].runtimeCntEstimate * (float)m_pProductorMoniter.m_vBuildings[j].reqLists[i] / (float)totalCnt);
                            }
                        }

                        //! step2 根据用量占比算出生产次数
                        for (int j = 0; j < m_pProductorMoniter.m_vBuildings.Count; ++j)
                        {
                            int productCnt = int.MaxValue;
                            for (int i = 0; i < m_pProductorMoniter.m_vStores.Count; ++i)
                            {
                                if (m_pProductorMoniter.m_vBuildings[j].reqEstimatePercent[i] > 0)
                                    productCnt = Mathf.Min(productCnt, (int)(m_pProductorMoniter.m_vBuildings[j].reqEstimatePercent[i] / m_pProductorMoniter.m_vBuildings[j].reqLists[i]));
                            }
                            if (productCnt >= int.MaxValue)
                                productCnt = 0;
                            //! 矫正次数
                            if(productCnt>0)
                            {
                                int costTime = productCnt * m_pProductorMoniter.m_vBuildings[j].cd;
                                if(costTime >remainBuildTime)
                                {
                                    productCnt = (int)(productCnt* remainBuildTime / costTime);
                                }
                            }
                            m_pProductorMoniter.m_vBuildings[j].productCntEstimate = productCnt;


                            //! 计算消耗库存量
                            if (productCnt > 0)
                            {
                                for (int i = 0; i < m_pProductorMoniter.m_vStores.Count; ++i)
                                    m_pProductorMoniter.m_vStores[i].runtimeCntEstimate -= m_pProductorMoniter.m_vBuildings[j].reqLists[i];
                            }
                        }
                    }
                    else if (m_pProductorMoniter.projectRout == 3)
                    {
                        m_vProductList.Clear();
                        //! step1 初始判断是否满足生产，满足则加生产次数，并扣除库存，加入生产队列
                        for (int i = 0; i < m_pProductorMoniter.m_vBuildings.Count; ++i)
                        {
                            m_pProductorMoniter.m_vBuildings[i].productCntEstimate = 0;
                            if (m_pProductorMoniter.m_vBuildings[i].cd <= m_pProductorMoniter.m_nBuildingTime)
                            {
                                bool bOk = true;
                                for (int j = 0; j < m_pProductorMoniter.m_vBuildings[i].reqLists.Count; ++j)
                                {
                                    if (m_pProductorMoniter.m_vBuildings[i].reqLists[j] > m_pProductorMoniter.m_vStores[j].runtimeCntEstimate)
                                    {
                                        bOk = false;
                                        break;
                                    }
                                }
                                if (bOk)
                                {
                                    for (int j = 0; j < m_pProductorMoniter.m_vBuildings[i].reqLists.Count; ++j)
                                    {
                                        m_pProductorMoniter.m_vStores[j].runtimeCntEstimate -= m_pProductorMoniter.m_vBuildings[i].reqLists[j];
                                    }
                                    m_pProductorMoniter.m_vBuildings[i].productCntEstimate++;

                                    bOk = true;
                                    for (int j = 0; j < m_pProductorMoniter.m_vBuildings[i].reqLists.Count; ++j)
                                    {
                                        if (m_pProductorMoniter.m_vBuildings[i].reqLists[j] > m_pProductorMoniter.m_vStores[j].runtimeCntEstimate)
                                        {
                                            bOk = false;
                                            break;
                                        }
                                    }
                                    if(bOk)
                                        m_vProductList.Add(m_pProductorMoniter.m_vBuildings[i]);
                                }
                            }
                        }
                        for (int j = 0; j < m_pProductorMoniter.m_vStores.Count; ++j)
                        {
                            m_pProductorMoniter.m_vStores[j].reqCostEstimatePercent = 0;
                        }
                        //! step2 ,计算消耗库存材料对应每秒消耗吞吐
                        for (int i =0; i < m_vProductList.Count; ++i)
                        {
                            for (int j = 0; j < m_vProductList[i].reqLists.Count; ++j)
                            {
                                m_pProductorMoniter.m_vStores[j].reqCostEstimatePercent += (double)m_vProductList[i].reqLists[j] / (double)(m_vProductList[i].cd);
                            }
                        }

                        //! step3，计算消耗库存需花总时长
                        int adjustBuidTime = remainBuildTime;
                        for (int j = 0; j < m_pProductorMoniter.m_vStores.Count; ++j)
                        {
                            int reqCnt = (int)(m_pProductorMoniter.m_vStores[j].reqCostEstimatePercent * remainBuildTime);
                        //    if(reqCnt > m_pProductorMoniter.m_vStores[j].runtimeCntEstimate)
                            {
                                int tempDT = (int)Mathf.FloorToInt((float)m_pProductorMoniter.m_vStores[j].runtimeCntEstimate / (float)m_pProductorMoniter.m_vStores[j].reqCostEstimatePercent);
                                if (tempDT < adjustBuidTime)
                                    adjustBuidTime = tempDT;
                            }
                        }
                        //! step4, 计算生产次数
                        for (int i = 0; i < m_vProductList.Count; ++i)
                        {
                            int pCnt =Mathf.FloorToInt((float)adjustBuidTime / (float)m_vProductList[i].cd);
                            m_vProductList[i].productCntEstimate += pCnt;
                            if(m_vProductList[i].productCntEstimate* m_vProductList[i].cd > m_pProductorMoniter.m_nBuildingTime)
                            {
                                m_vProductList[i].productCntEstimate = m_pProductorMoniter.m_nBuildingTime / m_vProductList[i].cd;
                            }

                            for(int j =0; j < m_vProductList[i].reqLists.Count; ++j)
                            {
                                m_pProductorMoniter.m_vStores[j].runtimeCntEstimate -= (int)(m_vProductList[i].reqLists[j]* pCnt);
                            }
                        }

                        //! step5, 根据剩余库存进行补正
                        int adjustLoop = 0;
                        if (adjustBuidTime < remainBuildTime)
                        {
                            remainBuildTime = m_pProductorMoniter.m_nBuildingTime;// - (int)adjustBuidTime;
                            while (m_vProductList.Count>0)
                            {
                                adjustLoop++;
                                bool bAllUnOk = true;
                                for (int i = 0; i < m_vProductList.Count;)
                                {
                                    float adjustTime = remainBuildTime - m_vProductList[i].cd * m_vProductList[i].productCntEstimate;
                                    if (m_vProductList[i].cd <= adjustTime && adjustTime > 0)
                                    {
                                        bool bOk = true;
                                        for (int j = 0; j < m_vProductList[i].reqLists.Count; ++j)
                                        {
                                            if (m_vProductList[i].reqLists[j] > m_pProductorMoniter.m_vStores[j].runtimeCntEstimate)
                                            {
                                                bOk = false;
                                                break;
                                            }
                                        }
                                        if (bOk)
                                        {
                                            for (int j = 0; j < m_vProductList[i].reqLists.Count; ++j)
                                            {
                                                m_pProductorMoniter.m_vStores[j].runtimeCntEstimate -= m_vProductList[i].reqLists[j];
                                            }
                                            m_vProductList[i].productCntEstimate++;
                                            m_vProductList[i].productAdjustCount++;
                                            bAllUnOk = false;
                                        }
                                        ++i;
                                    }
                                    else
                                    {
                                        m_vProductList.RemoveAt(i);
                                    }
                                }
                                if (bAllUnOk) break;
                            }
                        }
                        
                        strTips = "补正循环次数:" + adjustLoop + "\r\n";
                    }
                    if (m_pProductorMoniter.projectRout <=2)
                    {
                        //! step5 补正计算
                        int adjustLoop = 0;
                        while (true)
                        {
                            adjustLoop++;
                            bool bAllUnOk = true;
                            for (int i = 0; i < m_pProductorMoniter.m_vBuildings.Count; ++i)
                            {
                                float adjustTime = remainBuildTime - m_pProductorMoniter.m_vBuildings[i].cd * m_pProductorMoniter.m_vBuildings[i].productCntEstimate;
                                if (m_pProductorMoniter.m_vBuildings[i].cd <= adjustTime && adjustTime > 0)
                                {
                                    bool bOk = true;
                                    for (int j = 0; j < m_pProductorMoniter.m_vBuildings[i].reqLists.Count; ++j)
                                    {
                                        if (m_pProductorMoniter.m_vBuildings[i].reqLists[j] > m_pProductorMoniter.m_vStores[j].runtimeCntEstimate)
                                        {
                                            bOk = false;
                                            break;
                                        }
                                    }
                                    if (bOk)
                                    {
                                        for (int j = 0; j < m_pProductorMoniter.m_vBuildings[i].reqLists.Count; ++j)
                                        {
                                            m_pProductorMoniter.m_vStores[j].runtimeCntEstimate -= m_pProductorMoniter.m_vBuildings[i].reqLists[j];
                                        }
                                        m_pProductorMoniter.m_vBuildings[i].productCntEstimate++;
                                        m_pProductorMoniter.m_vBuildings[i].productAdjustCount++;
                                        bAllUnOk = false;
                                    }
                                }
                            }
                            if (bAllUnOk) break;
                        }
                        strTips = "补正循环次数:" + adjustLoop + "\r\n";
                    }
                    for (int i = 0; i < m_pProductorMoniter.m_vBuildings.Count; ++i)
                    {
                        strTips += m_pProductorMoniter.m_vBuildings[i].id + "生产线补正次数:" + m_pProductorMoniter.m_vBuildings[i].productAdjustCount + "\r\n";
                    }
                    EditorUtility.DisplayDialog("模拟完成", strTips, "好的");
                }
                EditorUtility.ClearProgressBar();
            }
        }
        #endregion

        void DrawRuntimeData()
        {
            if (GameInstance.getInstance() == null) return;
            EditorGUILayout.TextArea(GameInstance.getInstance().Print());
            if (GameInstance.getInstance().statesFactory != null)
                EditorGUILayout.TextArea(GameInstance.getInstance().statesFactory.Print());

            EditorGUILayout.LabelField("_MainLightShadowOffset0: " + Shader.GetGlobalVector("_MainLightShadowOffset0").ToString());
            EditorGUILayout.LabelField("_MainLightShadowOffset1: " + Shader.GetGlobalVector("_MainLightShadowOffset1").ToString());
            EditorGUILayout.LabelField("_MainLightShadowOffset2: " + Shader.GetGlobalVector("_MainLightShadowOffset2").ToString());
            EditorGUILayout.LabelField("_MainLightShadowOffset3: " + Shader.GetGlobalVector("_MainLightShadowOffset3").ToString());
            EditorGUILayout.LabelField("_MainLightShadowParams: " + Shader.GetGlobalVector("_MainLightShadowParams").ToString());

//             string globalBuff = "";
//             GameGlobalBuffer buffMgr= GameInstance.getInstance().globalBuff as GameGlobalBuffer;
//             if(buffMgr!=null)
//             {
//                 var buffs = buffMgr.GetGloablBuffs();
//                 foreach (var db in buffs)
//                 {
//                     var configData = db.configData as CsvData_GlobalBuff.GlobalBuffData;
//                     if (configData == null) continue;
//                     globalBuff += configData.id.ToString() + " Active:" + db.isActived + "  layer:" + db.GetMultiActiveLayer() + "   cd:" + db.nCooldown + "\r\n";
//                 }
//                 EditorGUILayout.TextArea(globalBuff);
//             }

        }

        void DrawController()
        {
            if (Framework.Module.ModuleManager.mainModule != null)
            {
                Framework.Module.ModuleManager.mainModule.TargetFrameRate = EditorGUILayout.IntSlider("帧率", Framework.Module.ModuleManager.mainModule.TargetFrameRate, 10, 120);
            }
            DrawDPI();
            TimeScale();
            GameInfo();
            Victory();
            Defeat();
            Invincible_Player();
            Invincible_Enemy();
            MaxMP();
            ShowPanel();
        //    SelectDungons();
            DrawAddGlobalBuff();
            SelectMode();
            UIAdapterTest();

            SkipGuide();
            PrintGuideGuid();
            PrintPassGuideID();
            PrintUnlockData();
            PrintBuildingState();
            TestFunc();
            DrawDungonLimitTime();
            ChangeAccount();
            UnlockAll();
            BesiegeEnter();
            AddItem();
            InputListen();
        }

        void UIAdapterTest()
        {
            UI.UIAdapter.AdapterLeft = EditorGUILayout.Slider("AdapterLeft", UI.UIAdapter.AdapterLeft,0,100);
            UI.UIAdapter.AdapterRight = EditorGUILayout.Slider("AdapterRight", UI.UIAdapter.AdapterRight, 0, 100);
            UI.UIAdapter.AdapterTop = EditorGUILayout.Slider("AdapterTop", UI.UIAdapter.AdapterTop, 0, 100);
            UI.UIAdapter.AdapterBottom = EditorGUILayout.Slider("AdapterBottom", UI.UIAdapter.AdapterBottom, 0, 100);
        }

        bool m_bShowInfoAndFPS = false;
        void GameInfo()
        {
            if (GameInstance.getInstance() == null) return;
            UI.UIGameInfo gameInfo = GameInstance.getInstance().uiManager.CastGetUI<UI.UIGameInfo>((ushort)EUIType.GameInfo, false);
            if (gameInfo == null || gameInfo.GetInstanceAble() == null) return;
            m_bShowInfoAndFPS = EditorGUILayout.Toggle("显示版本号和帧率",m_bShowInfoAndFPS);
            gameInfo.GetInstanceAble().gameObject.SetActive(m_bShowInfoAndFPS);

            UI.UILogin liginInfo = GameInstance.getInstance().uiManager.CastGetUI<UI.UILogin>((ushort)EUIType.Login, false);
            if (liginInfo == null ) return;
            var verText = liginInfo.GetWidget <UnityEngine.UI.Text>("Version_Text");
            if (verText) verText.gameObject.SetActive(m_bShowInfoAndFPS);
        }

        void DrawDPI()
        {
            QualitySettings.resolutionScalingFixedDPIFactor = EditorGUILayout.Slider("DPI", QualitySettings.resolutionScalingFixedDPIFactor, 0.2f, 10f);
        }

        /// <summary>
        /// 时间缩放管理
        /// </summary>
        void TimeScale()
        {
            if (Framework.Module.ModuleManager.mainModule == null) return;
                EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("时间缩放");

            Framework.Module.ModuleManager.mainModule.TimeScale = EditorGUILayout.Slider(Framework.Module.ModuleManager.mainModule.TimeScale, 0, 7);

            if (GUILayout.Button("重置"))
            {
                Framework.Module.ModuleManager.mainModule.TimeScale = 1;
            }
            EditorGUILayout.EndHorizontal();
        }

        void Victory()
        {
            if (GUILayout.Button("直接胜利"))
            {
                AbsMode mode = AState.CastCurrentMode<AbsMode>();
                if (mode != null)
                {
                    //设置全部通关
                    BattleDB battleDb = TopGame.SvrData.UserManager.getInstance().mySelf.ProxyDB<BattleDB>(EDBType.Battle);

                    PassCondition passCond = BattleKits.GetBattleLogic<PassCondition>(Framework.Module.ModuleManager.mainFramework);
                    if (passCond != null)
                    {
                        var m_PassParams = passCond.GetPassPassParams();
                        if (m_PassParams != null)
                        {
                            for (int i = 0; i < m_PassParams.Length; ++i)
                            {
                                if (m_PassParams[i].values == null) continue;
                                if ((1 << i) != 0)
                                {

                                    //填充通关数值
                                    for (int j = 0; j < m_PassParams[i].values.Length; ++j)
                                    {
                                        if (PassConditionChecker.IsSingleValue((EDungonPassBit)i))
                                            m_PassParams[i].values[j].runtimeValue = m_PassParams[i].values[j].value1;
                                        else
                                            m_PassParams[i].values[j].runtimeValue = m_PassParams[i].values[j].value2;
                                    }
                                }
                            }
                        }

                        //重置通关flag
                        var passFlag = passCond.GetPassFlag();
                        var passRelation = passCond.GetPassRelation();
                        passCond.SetPassData(passFlag, passRelation, m_PassParams, false);
                    }
                    battleDb.SetChapterStage(9);//设置战斗阶段
                    BattleKits.GetBattle(Framework.Module.ModuleManager.mainFramework).ForceVictory();
                }
            }
        }


        void Defeat()
        {
            if (GUILayout.Button("直接失败"))
            {
                AbsMode mode = AState.CastCurrentMode<AbsMode>();
                if (mode != null)
                {
                    BattleKits.GetBattle(Framework.Module.ModuleManager.mainFramework).ForceDefeated();
                }
            }
        }

        void Invincible_Player()
        {
            GUI.color = BattleStatus.bInvincible_Player ? Color.green : Color.white;
            if (GUILayout.Button("我方无敌"))
            {
                BattleStatus.bInvincible_Player = !BattleStatus.bInvincible_Player;
                GUI.color = Color.white;
            }

        }

        void Invincible_Enemy()
        {
            GUI.color = BattleStatus.bInvincible_Enemy ? Color.green : Color.white;
            if (GUILayout.Button("敌方无敌"))
            {
                BattleStatus.bInvincible_Enemy = !BattleStatus.bInvincible_Enemy;
            }
            GUI.color = Color.white;
        }

        void MaxMP()
        {
            if (GUILayout.Button("满蓝!"))
            {
                AbsMode mode = AState.CastCurrentMode<AbsMode>();
                if (mode != null)
                {
                    var pActor = mode.GetCurrentPlayer();
                    if (pActor != null)
                    {
                        pActor.GetActorParameter().GetRuntimeParam().sp = pActor.GetActorParameter().GetRuntimeParam().max_sp;
                    }
                }
                
            }
        }


        

        void ShowPanel()
        {
            if (GameInstance.getInstance() == null) return;
            if (GameInstance.getInstance().uiManager == null) return;
            GUILayout.BeginHorizontal();
            m_ShowUI = (UI.EUIType)Framework.ED.HandleUtilityWrapper.PopEnum("UI类型", m_ShowUI, null, null, true);
            if (GUILayout.Button("显示"))
            {
                UIHandle uiBase = GameInstance.getInstance().uiManager.ShowUI((ushort)m_ShowUI);
            }
            if (GUILayout.Button("隐藏"))
            {
                GameInstance.getInstance().uiManager.HideUI((ushort)m_ShowUI);
            }
            if (GUILayout.Button("关闭"))
            {
                GameInstance.getInstance().uiManager.CloseUI((ushort)m_ShowUI);
            }
            GUILayout.EndHorizontal();
        }

        //int m_SelectDungonsIndex = 0;
        //void SelectDungons()
        //{
        //    if (GameInstance.getInstance() == null) return;
        //    if (Application.isPlaying == false || Data.DataManager.getInstance() == null || Data.DataManager.getInstance().Dungons == null)
        //    {
        //        return;
        //    }
        //    GUILayout.BeginHorizontal();
        //    //读取配置
        //    List<string> showOptions = new List<string>();
        //    List<int> selectOptions = new List<int>();
        //    foreach (var db in Data.DataManager.getInstance().Dungons.datas)
        //    {
        //        showOptions.Add(db.Value.strName);
        //        selectOptions.Add((int)db.Value.nID);
        //    }
        //    m_SelectDungonsIndex = EditorGUILayout.IntPopup("选择Dungon:", m_SelectDungonsIndex, showOptions.ToArray(), selectOptions.ToArray());
        //    if (GUILayout.Button("进入关卡"))
        //    {
        //        TopGame.Logic.Battle battleState = TopGame.Logic.StateFactory.Get<TopGame.Logic.Battle>();
        //        //设置备战关卡
        //        //根据地图id找到对应的ChapterID
        //        uint chapterID = 0;
        //        ushort chapterLevel = 0;
        //        foreach (var item in DataManager.getInstance().Chapter.datas)
        //        {
        //            if (item.Value.mapId == selectOptions[m_SelectDungonsIndex])
        //            {
        //                chapterID = item.Value.id;
        //                Framework.Plugin.Logger.Info("设置当前关卡id:" + (int)item.Value.id);
        //                chapterLevel = (ushort)item.Value.level;
        //                break;
        //            }
        //        }
        //        if (chapterID != 0)
        //        {
        //            BattleDB battleDb = TopGame.SvrData.UserManager.getInstance().mySelf.ProxyDB<BattleDB>(Data.EDBType.Battle);
        //            battleDb.SetCurrentlevelType(Proto3.LevelTypeCode.Default);
        //            battleDb.SetCurrentLevel(Proto3.LevelTypeCode.Default, chapterID);
        //            GameInstance.getInstance().ChangeLocation(ELocationState.DungonPVE, ELoadingType.Loading);
        //        }
        //    }

        //    GUILayout.EndHorizontal();

        //    GUILayout.BeginHorizontal();
        //    m_nPVEChapterID = EditorGUILayout.IntField("PVE 关卡", m_nPVEChapterID);
        //    if (GUILayout.Button("进入"))
        //    {
        //        if (System.IO.File.Exists(EditorHelp.BinaryRootPath + "/Levels/" + m_nPVEChapterID + ".gk"))
        //        {
        //            GameInstance.getInstance().ChangeState(TopGame.Logic.EGameState.Battle, TopGame.Logic.EMode.PVE, ELoadingType.Loading, true);
        //        }
        //        else
        //        {
        //            EditorUtility.DisplayDialog("提示", "关卡不存在", "好吧");
        //        }
        //    }
        //    GUILayout.EndHorizontal();
        //}
        //------------------------------------------------------
        void DrawDungonLimitTime()
        {
            if (Application.isPlaying == false || GameInstance.getInstance() == null)
            {
                return;
            }
            PassCondition passCondition = BattleKits.GetBattleLogic<PassCondition>(Framework.Module.ModuleManager.mainFramework);
            if (passCondition == null || !passCondition.HasFlag(EDungonPassBit.LimitTime)) return;
            PassData passData = new PassData();
            if(passCondition.GetPassData(EDungonPassBit.LimitTime, ref passData) && passData.values!=null && passData.values.Length==1)
            {
                passData.values[0].value1 = EditorGUILayout.IntField("关卡限制时长", passData.values[0].value1);
            }
        }

        int m_nGlobalBuff = 0;
        void DrawAddGlobalBuff()
        {
            if (!Framework.Module.ModuleManager.startUpGame) return;
            if (Framework.Module.ModuleManager.mainModule == null) return;
            AFrameworkModule tempFramework = Framework.Module.ModuleManager.mainModule as AFrameworkModule;
            if (tempFramework == null) return;

            GUILayout.BeginHorizontal();
            m_nGlobalBuff = EditorGUILayout.IntField("全局buff", m_nGlobalBuff);
            if (GUILayout.Button("添加"))
            {
                GUI.FocusControl("");
                tempFramework.globalBuff.AddBuff((uint)m_nGlobalBuff);
            }
            if (GUILayout.Button("移除"))
            {
                GUI.FocusControl("");
                tempFramework.globalBuff.RemoveBuff((uint)m_nGlobalBuff);
            }
            GUILayout.EndHorizontal();
        }
        //------------------------------------------------------
        void PrintGuideGuid()
        {
            if (Application.isPlaying == false)
            {
                return;
            }

            if (GUILayout.Button("打印当前guide得Guide"))
            {
                GuideGuidUtl.PrintAllGuideGuid();
            }
        }
        //------------------------------------------------------
        void PrintPassGuideID()
        {
            if (Application.isPlaying == false)
            {
                return;
            }

            if (GUILayout.Button("打印执行过的引导组id"))
            {
                var flags = GuideSystem.getInstance().GetFlags();
                Debug.Log("开始打印执行过的引导组id");
                foreach (var id in flags)
                {
                    Debug.Log("id:" + id);
                }
            }
        }
        //------------------------------------------------------
        void PrintUnlockData()
        {
            if (Application.isPlaying == false)
            {
                return;
            }

            if (GUILayout.Button("打印当前解锁状态"))
            {
                if (GameInstance.getInstance() != null)
                    GameInstance.getInstance().unlockMgr.PrintDebug();
            }
        }
        //------------------------------------------------------
        void PrintBuildingState()
        {
//             if (Application.isPlaying == false)
//             {
//                 return;
//             }
// 
//             if (GUILayout.Button("打印地块状态状态"))
//             {
//                 BuildingDB buildDB = UserManager.getInstance().mySelf.ProxyDB<BuildingDB>(EDBType.Building);
// 
//                 var buildingInfos = buildDB.GetAllBuildingInfos();
//                 foreach (var building in buildingInfos)
//                 {
//                     if (building.Value != null)
//                     {
//                         Debug.Log("id:" + building.Value.ConfigData.buildType + ",state:" + building.Value.buildingState);
//                     }
//                 }
//             }
        }
        //------------------------------------------------------
        int m_SelectModeIndex = 0;
        void SelectMode()
        {
            if (Application.isPlaying == false || Data.DataManager.getInstance() == null)
            {
                return;
            }
            GUILayout.BeginHorizontal();
            //读取配置
            string[] showOptions = Enum.GetNames(typeof(ELocationState));
            int[] selectOptions = new int[Enum.GetValues(typeof(ELocationState)).Length];
            int index = 0;
            foreach (int value in Enum.GetValues(typeof(ELocationState)))
            {
                selectOptions[index] = value;
                index++;
            }

            m_SelectModeIndex = EditorGUILayout.IntPopup("选择模式:", m_SelectModeIndex, showOptions, selectOptions);
            if (GUILayout.Button("进入"))
            {
                if (GameInstance.getInstance() != null)
                    GameInstance.getInstance().ChangeLocation((SvrData.ELocationState)m_SelectModeIndex, ELoadingType.Loading);
            }

            GUILayout.EndHorizontal();


        }
        //------------------------------------------------------
        private void TestFunc()
        {
            if (Application.isPlaying == false)
            {
                return;
            }

            if (GUILayout.Button("完成任务"))
            {
                var panel = UIManager.CastUI<BattlePanel>(EUIType.BattlePanel);
                if (panel != null)
                {
                    var logic = panel.GetLogic<BattleTaskLogic>();
                    logic.TestCompletedTask();
                }
            }
            if (GUILayout.Button("成就完成"))
            {
                //var panel = UIManager.CastUI<AchievementPanel>(EUIType.Achievement);
                //if (panel != null)
                //{
                //    panel.OnShowCompletedAni();
                //}
            }
            if (GUILayout.Button("测试"))
            {
                UI.UIUtil.ShowCommonTip(TipType.Yes_No_Close, 10016000);
            }
        }
        //------------------------------------------------------
        private void BesiegeEnter()
        {
#if USE_DIYLEVEL
            if (GameInstance.getInstance() == null)
            {
                return;
            }
            if (GUILayout.Button("进入围攻战斗模式"))
            {
                GameInstance.getInstance().ChangeState(EGameState.Battle, EMode.Besiege, ELoadingType.ModeTransition, true);
            }
#endif
        }
        //------------------------------------------------------
        void SkipGuide()
        {
            if (!Application.isPlaying || GuideSystem.getInstance() == null)
            {
                return;
            }

            if (GUILayout.Button("关闭当前引导"))
            {
                GuideSystem.getInstance().OverGuide();
            }
        }
        //------------------------------------------------------ 解锁全部功能
        void UnlockAll()
        {
            if (!Application.isPlaying || GuideSystem.getInstance() == null)
            {
                return;
            }

            if (GUILayout.Button("解锁全部功能"))
            {
                NetCommonHandler.WebGmReq("unlock_all");
            }
        }
        //------------------------------------------------------
        void ChangeAccount()
        {
            if (!Application.isPlaying || GameInstance.getInstance() == null)
            {
                return;
            }

            if (GUILayout.Button("切换账号"))
            {
                //GameInstance.getInstance().OnChangeAccount();
                GameInstance.getInstance().ChangeState(EGameState.Login);
            }
        }
        //------------------------------------------------------
        bool m_bIsInputListener;
        bool m_bIsPlayRecord;
        void InputListen()
        {
            var color = GUI.color;

            if (m_bIsInputListener)
            {
                GUI.color = Color.yellow;
                if (GUILayout.Button("停止录制"))
                {
                    m_InputRecorder.EndRecord();
                    MouseHook.Stop();
                    //KeyboardHook.Stop();
                    m_bIsInputListener = false;
                }
            }
            else
            {
                GUI.color = Color.white;
                if (GUILayout.Button("开始录制"))
                {
                    m_InputRecorder.StartRecord();
                    MouseHook.Start();
                    //KeyboardHook.Start();

                    m_bIsInputListener = true;
                }
            }
            if (m_bIsPlayRecord)
            {
                GUI.color = Color.yellow;
                if (GUILayout.Button("停止播放"))
                {
                    MouseHook.Stop();
                    m_bIsInputListener = false;
                    EditorApplication.update -= m_InputRecorder.Update;
                    m_InputRecorder.Stop();
                    m_bIsPlayRecord = false;
                    InputRecorder.OnStop -= InputRecorder_OnStop;
                }
            }
            else
            {
                GUI.color = Color.white;
                if (GUILayout.Button("播放录制"))
                {
                    MouseHook.Stop();
                    m_bIsInputListener = false;
                    EditorApplication.update += m_InputRecorder.Update;
                    InputRecorder.OnStop += InputRecorder_OnStop;
                    m_InputRecorder.Play();
                    m_bIsPlayRecord = true;
                }
            }
            
            GUI.color = color;
        }


        private void InputRecorder_OnStop()
        {
            MouseHook.Stop();
            m_bIsInputListener = false;
            EditorApplication.update -= m_InputRecorder.Update;
            m_bIsPlayRecord = false;
            InputRecorder.OnStop -= InputRecorder_OnStop;
        }

        //------------------------------------------------------
        int m_ChestNum = 1;
        int m_ChestIndex;
        string[] m_ChestNames = new string[] { "宝箱1", "宝箱2" , "宝箱3" };
        int m_AddItemID=1;
        int m_AddItemCount=100000;
        int m_AddPetID=10201;
        int m_AddPetCount=1;
        void AddItem()
        {
            if (Application.isPlaying == false || Data.DataManager.getInstance() == null)
            {
                return;
            }
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("添加宝箱数:");
            m_ChestNum = EditorGUILayout.IntField(m_ChestNum, GUILayout.Width(100));
            EditorGUILayout.LabelField("添加宝箱类型:");
            m_ChestIndex = EditorGUILayout.Popup(m_ChestIndex, m_ChestNames);
            if (GUILayout.Button("添加", GUILayout.Width(100)))
            {
                Debug.Log("m_ChestIndex:" + m_ChestIndex);
                List<ItemData> vdatas = new List<ItemData>();
                switch (m_ChestIndex)
                {
                    case 0:
                        vdatas.Add(new ItemData() { ConfigId = 1001, Value = m_ChestNum });
                        break;
                        case 1:
                        vdatas.Add(new ItemData() { ConfigId = 1002, Value = m_ChestNum });
                        break;
                        case 2:
                        vdatas.Add(new ItemData() { ConfigId = 1003, Value = m_ChestNum });
                        break;
                    default:
                        break;
                }

                Net.NetItemHandler.ItemDirtyRequest(EItemChangeType.Add, vdatas);
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("添加id:");
            m_AddItemID = EditorGUILayout.IntField(m_AddItemID, GUILayout.Width(100));
            EditorGUILayout.LabelField("添加数量:");
            m_AddItemCount = EditorGUILayout.IntField(m_AddItemCount);
            if (GUILayout.Button("添加", GUILayout.Width(100)))
            {
                var db = UserManager.Current.GetItemDB();
                List<ItemData> vdatas = new List<ItemData>();
                vdatas.Add(new ItemData() { ConfigId = m_AddItemID, Value = m_AddItemCount });
                Net.NetItemHandler.ItemDirtyRequest(EItemChangeType.Add, vdatas);
            //    db.AddItem((uint)m_AddItemID, m_AddItemCount);
            }

            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("添加宠物id:");
            m_AddPetID = EditorGUILayout.IntField(m_AddPetID, GUILayout.Width(100));
            EditorGUILayout.LabelField("添加宠物数量:");
            m_AddPetCount = EditorGUILayout.IntField(m_AddPetCount);
            if (GUILayout.Button("添加", GUILayout.Width(100)))
            {
                List<PetData> vdatas = new List<PetData>();
                vdatas.Add(new PetData() { ConfigId = m_AddPetID, Value = m_AddPetCount });

                if (!GameInstance.getInstance().IsOffline)
                {
                    Net.NetPetHandler.PetDirtyReq(EPetChangeType.Add, vdatas);
                }
                //    db.AddItem((uint)m_AddPetID, m_AddItemCount);
            }

            EditorGUILayout.EndHorizontal();
        }
    }
}