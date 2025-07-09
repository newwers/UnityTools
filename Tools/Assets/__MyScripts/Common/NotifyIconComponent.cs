using Pal;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;
using Z.Event;

/// <summary>
/// 右下角图标
/// </summary>
public class NotifyIconComponent : MonoBehaviour
{
    private NotifyIcon notifyIcon;
    System.Windows.Forms.ContextMenu m_ContextMenu;

    public Texture2D icon;

    void Start()
    {
        DontDestroyOnLoad(this);
        print("NotifyIconComponent.Start");
        //print("iconPath2:" + iconPath);

#if !UNITY_EDITOR  //编辑器模式下,会导致第二次启动,编辑器崩溃,重复注册窗口类造成的

        notifyIcon = new NotifyIcon();

        notifyIcon.Icon = TextureToIcon(icon);//设置图标
        notifyIcon.Visible = true;//设置右下角可见
        notifyIcon.Text = GameManager.Instance.Config.notifyIconText;//设置鼠标放上去提示词

        notifyIcon.BalloonTipTitle = GameManager.Instance.Config.BalloonTipTitle;
        notifyIcon.BalloonTipText = GameManager.Instance.Config.BalloonTipText;

        LoadContextMenu();
#endif
    }

    private void OnEnable()
    {
        EventsSystem.StartListening("RefreshContextMenu",LoadContextMenu);
    }


    private void OnDisable()
    {
        EventsSystem.StopListening("RefreshContextMenu", LoadContextMenu);

        if (notifyIcon != null)
        {
            //notifyIcon.Visible = false;
            //notifyIcon.Dispose();
            notifyIcon = null;
            //GC.Collect();
            print("NotifyIconComponent.OnDisable");
        }


    }

    //void OnDestroy()
    //{
    //    print("NotifyIconComponent.OnDestroy");
    //}


    void LoadContextMenu()
    {
        //设置菜单
        m_ContextMenu = new System.Windows.Forms.ContextMenu();
        MenuItem menuItem_Exit = new MenuItem();
        MenuItem menuItem2 = new MenuItem();
        MenuItem menuItem3 = new MenuItem();
        MenuItem menuItem4 = new MenuItem();
        MenuItem menuItem5 = new MenuItem();
        MenuItem menuItem6 = new MenuItem();
        MenuItem menuItem7 = new MenuItem();
        MenuItem menuItem8 = new MenuItem();
        MenuItem menuItem9 = new MenuItem();
        MenuItem menuItem10 = new MenuItem();
        MenuItem menuItem11 = new MenuItem();
        MenuItem menuItem12 = new MenuItem();
        MenuItem menuItem13 = new MenuItem();
        MenuItem menuItem14 = new MenuItem();


        MenuItem menuItem_Mode = new MenuItem();
        MenuItem menuItem_Action = new MenuItem();
        MenuItem menuItem_Expression = new MenuItem();
        MenuItem menuItem_Expression_Eyes = new MenuItem();
        MenuItem menuItem_Expression_Mouth = new MenuItem();
        MenuItem menuItem_Sounds = new MenuItem();
        MenuItem menuItem_Other = new MenuItem();
        MenuItem menuItem_MiniGame = new MenuItem();
        MenuItem menuItem_SummonPal = new MenuItem();

        m_ContextMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] { menuItem_Mode, menuItem_Action, menuItem_Expression, menuItem_Sounds, menuItem_SummonPal, menuItem_Other, menuItem_MiniGame });
        m_ContextMenu.MenuItems.Add(CreateMenuItem(m_ContextMenu.MenuItems.Count-2, "预留1", TestLoad));
        m_ContextMenu.MenuItems.Add(CreateMenuItem(m_ContextMenu.MenuItems.Count - 2, "预留2", null));
        m_ContextMenu.MenuItems.Add(CreateMenuItem(m_ContextMenu.MenuItems.Count - 2, "预留3", null));
        m_ContextMenu.MenuItems.Add(menuItem_Exit);


        //设置菜单高度
        //for (int i = 0; i < menu.MenuItems.Count; i++)
        //{
        //    MenuItem menuItem = menu.MenuItems[i];
        //    menuItem.OwnerDraw = true;
        //    //menuItem.DrawItem += MenuItem_DrawItem;
        //    menuItem.MeasureItem += MenuItem_MeasureItem;
        //}

        //设置菜单按钮
        menuItem_Exit.Index = m_ContextMenu.MenuItems.Count - 1;
        menuItem_Exit.Text = "退出";
        menuItem_Exit.Click += new System.EventHandler(MenuItem1_Click);


        //模式
        menuItem2.Index = 0;
        menuItem2.Text = "原地待机模式";
        menuItem2.Click += new System.EventHandler(MenuItem2_Click);

        menuItem3.Index = 1;
        menuItem3.Text = "跟随鼠标走动模式";
        menuItem3.Click += new System.EventHandler(MenuItem3_Click);


        menuItem_Mode.Index = 0;
        menuItem_Mode.Text = "模式";
        menuItem_Mode.MenuItems.AddRange((new System.Windows.Forms.MenuItem[] { menuItem2, menuItem3 }));
        menuItem_Mode.MenuItems.Add(CreateMenuItem(2, "满屏跑模式",FullScreenMove));


        //动作

        menuItem6.Index = 0;
        menuItem6.Text = "待机";
        menuItem6.Click += new System.EventHandler(MenuItem6_Click);

        menuItem11.Index = 1;
        menuItem11.Text = "休息动作1";
        menuItem11.Click += new System.EventHandler(MenuItem11_Click);

        menuItem12.Index = 2;
        menuItem12.Text = "休息动作2";
        menuItem12.Click += new System.EventHandler(MenuItem12_Click);

        menuItem13.Index = 3;
        menuItem13.Text = "休息动作3";
        menuItem13.Click += new System.EventHandler(MenuItem13_Click);


        menuItem_Action.Index = 1;
        menuItem_Action.Text = "动作";


        var menuItem_Action1 = CreateMenuItem(0, "动作1", null);
        var menuItem_Action2 = CreateMenuItem(1, "动作2", null);
        menuItem_Action.MenuItems.Add(menuItem_Action1);
        menuItem_Action.MenuItems.Add(menuItem_Action2);

        menuItem_Action1.MenuItems.AddRange(new MenuItem[] { menuItem6, menuItem11, menuItem12, menuItem13 });

        menuItem_Action1.MenuItems.Add(CreateMenuItem(4, "受伤", PlayDamageAction));
        menuItem_Action1.MenuItems.Add(CreateMenuItem(5, "挑衅", PlayEncountAction));
        menuItem_Action2.MenuItems.Add(CreateMenuItem(6, "吃", PlayEatAction));
        menuItem_Action2.MenuItems.Add(CreateMenuItem(7, "技能1", PlayFarSkillAction));
        menuItem_Action2.MenuItems.Add(CreateMenuItem(8, "技能2~", PlaySkill2Action));
        menuItem_Action2.MenuItems.Add(CreateMenuItem(9, "睡觉", PlaySleepAction));
        menuItem_Action2.MenuItems.Add(CreateMenuItem(10, "起床", PlaySleepEndAction));
        menuItem_Action2.MenuItems.Add(CreateMenuItem(11, "特殊动作", PlaySpecialAction));



        //表情


        menuItem_Expression_Eyes.Index = 0;
        menuItem_Expression_Eyes.Text = "眼睛";


        menuItem_Expression_Eyes.MenuItems.Add(CreateMenuItem(0, "眼睛1", menuItem_Expression_Eyes1_Click));
        menuItem_Expression_Eyes.MenuItems.Add(CreateMenuItem(1, "眼睛2", menuItem_Expression_Eyes2_Click));
        menuItem_Expression_Eyes.MenuItems.Add(CreateMenuItem(2, "眼睛3", menuItem_Expression_Eyes3_Click));
        menuItem_Expression_Eyes.MenuItems.Add(CreateMenuItem(3, "眼睛4", menuItem_Expression_Eyes4_Click));
        menuItem_Expression_Eyes.MenuItems.Add(CreateMenuItem(4, "眼睛5", menuItem_Expression_Eyes5_Click));
        menuItem_Expression_Eyes.MenuItems.Add(CreateMenuItem(5, "眼睛6", menuItem_Expression_Eyes6_Click));
        menuItem_Expression_Eyes.MenuItems.Add(CreateMenuItem(6, "眼睛7", menuItem_Expression_Eyes7_Click));


        menuItem_Expression_Mouth.Index = 1;
        menuItem_Expression_Mouth.Text = "嘴巴";


        menuItem_Expression_Mouth.MenuItems.Add(CreateMenuItem(0, "嘴型1", menuItem_Expression_Mouth1_Click));
        menuItem_Expression_Mouth.MenuItems.Add(CreateMenuItem(1, "嘴型2", menuItem_Expression_Mouth2_Click));
        menuItem_Expression_Mouth.MenuItems.Add(CreateMenuItem(2, "嘴型3", menuItem_Expression_Mouth3_Click));
        menuItem_Expression_Mouth.MenuItems.Add(CreateMenuItem(3, "嘴型4", menuItem_Expression_Mouth4_Click));

        menuItem_Expression.Index = 2;
        menuItem_Expression.Text = "表情";
        menuItem_Expression.MenuItems.AddRange(new MenuItem[] { menuItem_Expression_Eyes, menuItem_Expression_Mouth });


        //声音
        menuItem_Sounds.Index = 3;
        menuItem_Sounds.Text = "声音";
        menuItem_Sounds.MenuItems.Add(CreateMenuItem(0, "声音1", menuItem_Sounds1_Click));
        menuItem_Sounds.MenuItems.Add(CreateMenuItem(1, "声音2", menuItem_Sounds2_Click));
        menuItem_Sounds.MenuItems.Add(CreateMenuItem(2, "声音3", menuItem_Sounds3_Click));
        menuItem_Sounds.MenuItems.Add(CreateMenuItem(3, "声音4", menuItem_Sounds4_Click));
        menuItem_Sounds.MenuItems.Add(CreateMenuItem(4, "声音5", menuItem_Sounds5_Click));
        menuItem_Sounds.MenuItems.Add(CreateMenuItem(5, "声音6", menuItem_Sounds6_Click));

        //小游戏
        menuItem_MiniGame.Index = m_ContextMenu.MenuItems.Count - 2;
        menuItem_MiniGame.Text = "小游戏";
        menuItem_MiniGame.MenuItems.Add(CreateMenuItem(0, "回到主场景", GoToMainScene));
        menuItem_MiniGame.MenuItems.Add(CreateMenuItem(1, "小游戏1", MenuItem10_Click));
        menuItem_MiniGame.MenuItems.Add(CreateMenuItem(2, "小游戏2", MiniGame2_Click));

        //menuItem_MiniGame.OwnerDraw = true; // 设置为自定义绘制
        //menuItem_MiniGame.MeasureItem += (sender, e) =>
        //{
        //    e.ItemHeight = 60; // 设置按钮高度
        //    //e.ItemWidth = 200; // 设置按钮宽度
        //};


        //其他

        menuItem4.Index = 0;
        menuItem4.Text = "置顶";
        menuItem4.Click += new System.EventHandler(MenuItem4_Click);

        menuItem5.Index = 1;
        menuItem5.Text = "取消置顶";
        menuItem5.Click += new System.EventHandler(MenuItem5_Click);


        menuItem7.Index = 2;
        menuItem7.Text = "开启重力";
        menuItem7.Click += new System.EventHandler(MenuItem7_Click);

        menuItem8.Index = 3;
        menuItem8.Text = "关闭重力";
        menuItem8.Click += new System.EventHandler(MenuItem8_Click);

        menuItem_Other.Index = m_ContextMenu.MenuItems.Count - 3;
        menuItem_Other.Text = "其他";
        menuItem_Other.MenuItems.AddRange(new MenuItem[] { menuItem4, menuItem5, menuItem7, menuItem8 });

        menuItem_Other.MenuItems.Add(CreateMenuItem(4, "开启帕鲁头部跟随鼠标", EnableHeadRig));
        menuItem_Other.MenuItems.Add(CreateMenuItem(4, "关闭帕鲁头部跟随鼠标", DisableHeadRig));


        //召唤帕鲁
        menuItem_SummonPal.Index = m_ContextMenu.MenuItems.Count - 4;
        menuItem_SummonPal.Text = "帕鲁";
        var menuItem_SummonPal_summon = CreateMenuItem(0, "召唤", null);
        var menuItem_SummonPal_remote = CreateMenuItem(1, "叫回", null);
        menuItem_SummonPal.MenuItems.Add(menuItem_SummonPal_summon);
        menuItem_SummonPal.MenuItems.Add(menuItem_SummonPal_remote);

        PalMenu[] palMenu = PalManager.Instance.GetMenuItems();
        for (int i = 0; i < palMenu.Length; i++)
        {
            var menuItem = palMenu[i];
            menuItem_SummonPal_summon.MenuItems.Add(CreateMenuItem(i, menuItem.ShowName, (o, e) => { EventsSystem.TriggerEvent<string>("SummonPal", menuItem.AssetPath); }));
            menuItem_SummonPal_remote.MenuItems.Add(CreateMenuItem(i, menuItem.ShowName, (o, e) => { EventsSystem.TriggerEvent<string>("RemotePal", menuItem.AssetPath); }));
        }


        //menuItem_SummonPal_summon.MenuItems.Add(CreateMenuItem(0, "捣蛋猫", SummonPinkCat));
        //menuItem_SummonPal_summon.MenuItems.Add(CreateMenuItem(1, "疾旋鼬", SummonWeaselDragon));
        //menuItem_SummonPal_summon.MenuItems.Add(CreateMenuItem(2, "棉悠悠", SummonSheepBall));
        //menuItem_SummonPal_summon.MenuItems.Add(CreateMenuItem(3, "天羽龙", SummonSkyDragon));
        //menuItem_SummonPal_summon.MenuItems.Add(CreateMenuItem(4, "花冠龙", SummonFlowerDinosaur));


        //menuItem_SummonPal_remote.MenuItems.Add(CreateMenuItem(0, "捣蛋猫", RemotePinkCat));
        //menuItem_SummonPal_remote.MenuItems.Add(CreateMenuItem(1, "疾旋鼬", RemoteWeaselDragon));
        //menuItem_SummonPal_remote.MenuItems.Add(CreateMenuItem(2, "棉悠悠", RemoteSheepBall));
        //menuItem_SummonPal_remote.MenuItems.Add(CreateMenuItem(3, "天羽龙", RemoteSkyDragon));
        //menuItem_SummonPal_remote.MenuItems.Add(CreateMenuItem(4, "花冠龙", RemoteFlowerDinosaur));



        // 自定义绘制二级菜单按钮大小
        //menuItem_Other.OwnerDraw = true; // 设置为自定义绘制
        //menuItem_Other.MeasureItem += (sender, e) =>
        //{
        //    e.ItemHeight = 60; // 设置按钮高度
        //    //e.ItemWidth = 200; // 设置按钮宽度
        //};

        notifyIcon.ContextMenu = m_ContextMenu;

        //监听事件
        notifyIcon.DoubleClick -= NotifyIcon_DoubleClick;
        notifyIcon.DoubleClick += NotifyIcon_DoubleClick;
        notifyIcon.MouseClick -= OnNotifyIconMouseClick;
        notifyIcon.MouseClick += OnNotifyIconMouseClick;
        //notifyIcon.MouseMove += NotifyIcon_MouseMove;
    }


    private void TestLoad(object sender, EventArgs e)
    {
        var assets = ResourceLoadManager.Instance.Load<GameObject>("Assets/Prefabs/PinkCat.prefab");
        if (assets)
        {
            GameObject go = Instantiate(assets);
        }
    }
    private void DisableHeadRig(object sender, EventArgs e)
    {
        EventsSystem.TriggerEvent<bool>("SetHeadRig",false);
    }

    private void EnableHeadRig(object sender, EventArgs e)
    {
        EventsSystem.TriggerEvent<bool>("SetHeadRig", true);
    }

    private void MenuItem_MeasureItem(object sender, MeasureItemEventArgs e)
    {
        e.ItemHeight += 20;
        e.ItemWidth += 20;
    }

    private void MenuItem_DrawItem(object sender, DrawItemEventArgs e)
    {
        // 设置菜单项的高度

        Rectangle rect = e.Bounds;
        rect.Height += 20;

        // 绘制菜单项


        e.DrawBackground();
        e.Graphics.DrawString(((MenuItem)sender).Text, e.Font, Brushes.Black, e.Bounds.Left, e.Bounds.Top);

        //SizeF mySizeF = e.Graphics.MeasureString(((MenuItem)sender).Text, myFont);
        e.Graphics.DrawRectangle(Pens.White, new Rectangle(e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height));
    }

    private void PlaySleepAction(object sender, EventArgs e)
    {
        EventsSystem.TriggerEvent("PlaySleepAction");
    }

    private void PlaySleepEndAction(object sender, EventArgs e)
    {
        EventsSystem.TriggerEvent("PlaySleepEndAction");
    }

    private void PlaySkill2Action(object sender, EventArgs e)
    {
        EventsSystem.TriggerEvent("PlaySkill2Action");
    }

    private void PlaySpecialAction(object sender, EventArgs e)
    {
        EventsSystem.TriggerEvent("PlaySpecialAction");
    }

    private void PlayFarSkillAction(object sender, EventArgs e)
    {
        EventsSystem.TriggerEvent("PlayFarSkillAction");
    }

    private void PlayEatAction(object sender, EventArgs e)
    {
        EventsSystem.TriggerEvent("PlayEatAction");
    }

    private void PlayEncountAction(object sender, EventArgs e)
    {
        EventsSystem.TriggerEvent("PlayEncountAction");
    }

    private void PlayDamageAction(object sender, EventArgs e)
    {
        EventsSystem.TriggerEvent("PlayDamageAction");
    }

    private void HideWindows(object sender, EventArgs e)
    {
        TaskbarIconController.HideWindows();
    }

    private void ShowWindows(object sender, EventArgs e)
    {
        TaskbarIconController.ShowWindows();
    }

    private void HideTaskBarState(object sender, EventArgs e)
    {
        TaskbarIconController.HideTaskbarIcon();
    }

    private void ShowTaskBarState(object sender, EventArgs e)
    {
        TaskbarIconController.ShowTaskbarIcon();
    }

    private void menuItem_Sounds6_Click(object sender, EventArgs e)
    {
        //声音6
        EventsSystem.TriggerEvent<int>("PlayAudio", 6);
    }

    private void menuItem_Sounds5_Click(object sender, EventArgs e)
    {
        //声音5
        EventsSystem.TriggerEvent<int>("PlayAudio", 5);
    }

    private void menuItem_Sounds4_Click(object sender, EventArgs e)
    {
        //声音4
        EventsSystem.TriggerEvent<int>("PlayAudio", 4);
    }

    private void menuItem_Sounds3_Click(object sender, EventArgs e)
    {
        //声音3
        EventsSystem.TriggerEvent<int>("PlayAudio", 3);
    }

    private void menuItem_Sounds2_Click(object sender, EventArgs e)
    {
        //声音2
        EventsSystem.TriggerEvent<int>("PlayAudio", 2);
    }

    private void menuItem_Sounds1_Click(object sender, EventArgs e)
    {
        //声音1
        EventsSystem.TriggerEvent<int>("PlayAudio", 1);
    }

    private void menuItem_Expression_Eyes7_Click(object sender, EventArgs e)
    {
        //眼睛7
        EventsSystem.TriggerEvent("SetEyes", 7);
    }

    private void menuItem_Expression_Eyes6_Click(object sender, EventArgs e)
    {
        //眼睛6
        EventsSystem.TriggerEvent("SetEyes", 6);
    }

    private void menuItem_Expression_Eyes5_Click(object sender, EventArgs e)
    {
        //眼睛5
        EventsSystem.TriggerEvent("SetEyes", 5);
    }

    private void menuItem_Expression_Eyes4_Click(object sender, EventArgs e)
    {
        //眼睛4
        EventsSystem.TriggerEvent("SetEyes", 4);
    }

    private void menuItem_Expression_Eyes3_Click(object sender, EventArgs e)
    {
        //眼睛3
        EventsSystem.TriggerEvent("SetEyes", 3);
    }

    private void menuItem_Expression_Eyes2_Click(object sender, EventArgs e)
    {
        //眼睛2
        EventsSystem.TriggerEvent("SetEyes", 2);
    }

    private void menuItem_Expression_Eyes1_Click(object sender, EventArgs e)
    {
        //眼睛1
        EventsSystem.TriggerEvent("SetEyes", 1);
    }

    private void menuItem_Expression_Mouth4_Click(object sender, EventArgs e)
    {
        //嘴型4
        EventsSystem.TriggerEvent("SetMouth", 4);
    }

    private void menuItem_Expression_Mouth3_Click(object sender, EventArgs e)
    {
        //嘴型3
        EventsSystem.TriggerEvent("SetMouth", 3);
    }

    private void menuItem_Expression_Mouth2_Click(object sender, EventArgs e)
    {
        //嘴型2
        EventsSystem.TriggerEvent("SetMouth", 2);
    }

    private void menuItem_Expression_Mouth1_Click(object sender, EventArgs e)
    {
        //嘴型1
        EventsSystem.TriggerEvent("SetMouth", 1);
    }

    MenuItem CreateMenuItem(int index,string text,EventHandler Click)
    {
        MenuItem menuItem = new MenuItem();
        menuItem.Index = 0;
        menuItem.Text = text;
        menuItem.Click += Click;

        return menuItem;
    }


    private void MenuItem13_Click(object sender, EventArgs e)
    {
        //休息动作3
        EventsSystem.TriggerEvent("SetIdleState", 3);
    }

    private void MenuItem12_Click(object sender, EventArgs e)
    {
        //休息动作2,坐下
        EventsSystem.TriggerEvent("SetIdleState", 2);
    }

    private void MenuItem11_Click(object sender, EventArgs e)
    {
        //休息动作1,挠头
        EventsSystem.TriggerEvent("SetIdleState", 1);
    }
    private void MenuItem6_Click(object sender, EventArgs e)
    {
        //待机
        EventsSystem.TriggerEvent("SetIdleState",0);
    }


    private void MiniGame2_Click(object sender, EventArgs e)
    {
        //小游戏2 图片配对
        SceneManager.LoadScene(5);

        //修改菜单
        m_ContextMenu.MenuItems.Clear();
        m_ContextMenu.MenuItems.Add(CreateMenuItem(0, "回到主场景", GoToMainScene));
    }

    private void MenuItem10_Click(object sender, EventArgs e)
    {
        //小游戏
        SceneManager.LoadScene(2);

        //修改菜单
        m_ContextMenu.MenuItems.Clear();
        m_ContextMenu.MenuItems.Add(CreateMenuItem(0, "回到主场景", GoToMainScene));
    }

    private void GoToMainScene(object sender, EventArgs e)
    {
        //回到主场景
        SceneManager.LoadScene(1);

        LoadContextMenu();
    }



    private void RemoteWeaselDragon(object sender, EventArgs e)
    {
        //移除鼬
        EventsSystem.TriggerEvent<string>("RemotePal", "Assets/Prefabs/WeaselDragon.prefab");
    }

    private void RemotePinkCat(object sender, EventArgs e)
    {
        //移除猫
        EventsSystem.TriggerEvent<string>("RemotePal", "Assets/Prefabs/PinkCat.prefab");
    }

    private void SummonPinkCat(object sender, EventArgs e)
    {
        //召唤猫
        EventsSystem.TriggerEvent<string>("SummonPal", "Assets/Prefabs/PinkCat.prefab");
    }


    private void SummonWeaselDragon(object sender, EventArgs e)
    {
        //召唤鼬子帕鲁
        EventsSystem.TriggerEvent<string>("SummonPal", "Assets/Prefabs/WeaselDragon.prefab");
    }

    private void RemoteSheepBall(object sender, EventArgs e)
    {
        //移除羊
        EventsSystem.TriggerEvent<string>("RemotePal", "Assets/Prefabs/SheepBall.prefab");
    }

    private void SummonSheepBall(object sender, EventArgs e)
    {
        //召唤羊
        EventsSystem.TriggerEvent<string>("SummonPal", "Assets/Prefabs/SheepBall.prefab");
    }


    private void RemoteSkyDragon(object sender, EventArgs e)
    {
        //移除天羽龙
        EventsSystem.TriggerEvent<string>("RemotePal", "Assets/Prefabs/SkyDragon.prefab");
    }

    private void SummonSkyDragon(object sender, EventArgs e)
    {
        //召唤天羽龙
        EventsSystem.TriggerEvent<string>("SummonPal", "Assets/Prefabs/SkyDragon.prefab");
    }

    private void RemoteFlowerDinosaur(object sender, EventArgs e)
    {
        //移除花冠龙
        EventsSystem.TriggerEvent<string>("RemotePal", "Assets/Prefabs/FlowerDinosaur.prefab");
    }

    private void SummonFlowerDinosaur(object sender, EventArgs e)
    {
        //召唤花冠龙
        EventsSystem.TriggerEvent<string>("SummonPal", "Assets/Prefabs/FlowerDinosaur.prefab");
    }


    private void MenuItem8_Click(object sender, EventArgs e)
    {
        //关闭重力
        EventsSystem.TriggerEvent<bool>("SetUseGravity", false);
    }

    private void MenuItem7_Click(object sender, EventArgs e)
    {
        //开启重力
        EventsSystem.TriggerEvent<bool>("SetUseGravity",true);
    }


    private void MenuItem5_Click(object sender, EventArgs e)
    {
        //取消置顶.
        PalManager.Instance.SetWindowTop(false);
    }

    private void MenuItem4_Click(object sender, EventArgs e)
    {
        //置顶
        PalManager.Instance.SetWindowTop(true);
    }

    private void EnableRagdoll(object sender, EventArgs e)
    {
        //开启ragdoll
        EventsSystem.TriggerEvent<bool>("SetRagdoll",true);
    }

    private void CloseRagdoll(object sender, EventArgs e)
    {
        //关闭ragdoll
        EventsSystem.TriggerEvent<bool>("SetRagdoll", false);
    }

    private void MenuItem3_Click(object sender, EventArgs e)
    {
        //跟随鼠标走动模式
        PalManager.Instance.SetFollowMousePos(true);
        EventsSystem.TriggerEvent<bool>("FullScreenMove", false);
    }

    private void MenuItem2_Click(object sender, EventArgs e)
    {
        //设置原地待机
        PalManager.Instance.SetFollowMousePos(false);
        EventsSystem.TriggerEvent<bool>("FullScreenMove", false);
    }


    private void FullScreenMove(object sender, EventArgs e)
    {
        //帕鲁满屏跑模式
        PalManager.Instance.SetFollowMousePos(false);

        EventsSystem.TriggerEvent<bool>("FullScreenMove", true);
    }

    private void MenuItem1_Click(object sender, EventArgs e)
    {
        System.Windows.Forms.Application.Exit();//退出应用
    }

    private void NotifyIcon_MouseMove(object sender, MouseEventArgs e)
    {
        print("鼠标从NotifyIcon上移动过");
    }

    void OnNotifyIconMouseClick(object sender, MouseEventArgs e)
    {
        if (PalManager.Instance != null)
        {
            PalManager.Instance.OnNotifyIconClickEvent(sender, e);
        }

        // 判断鼠标左键或右键点击
        if (e.Button == MouseButtons.Left)
        {
            Debug.Log("Left mouse button clicked");

            // 显示气泡
            notifyIcon.ShowBalloonTip(500, "捣蛋猫", "开始捣蛋~", ToolTipIcon.Info);

            
        }
        //else if (e.Button == MouseButtons.Right)
        //{
        //    //Debug.Log("Right mouse button clicked");
        //    notifyIcon.ShowBalloonTip(1000, "捣蛋猫", "开始导弹~", ToolTipIcon.Warning);
        //}
    }


    private void NotifyIcon_DoubleClick(object sender, EventArgs e)
    {
        // Handle double click event
        print("NotifyIcon_DoubleClick!!!");
    }


    public static Icon TextureToIcon(Texture2D texture)
    {
        Bitmap bitmap = new Bitmap(texture.width, texture.height);

        for (int x = 0; x < texture.width; x++)
        {
            for (int y = 0; y < texture.height; y++)
            {
                UnityEngine.Color pixelColor = texture.GetPixel(x, texture.height - 1 - y); // 调整读取像素的顺序,否则上下反了
                System.Drawing.Color color = System.Drawing.Color.FromArgb((int)(pixelColor.a * 255), (int)(pixelColor.r * 255), (int)(pixelColor.g * 255), (int)(pixelColor.b * 255));

                bitmap.SetPixel(x, y, color);
            }
        }

        Icon icon = Icon.FromHandle(bitmap.GetHicon());

        return icon;
    }
}
