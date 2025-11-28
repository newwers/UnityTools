#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;


public class ActionDataEditor : EditorWindow
{
    // 动画预览相关变量
    [Header("动画预览设置")]
    private bool showAnimationPreview = true;
    private bool isPlayingPreview = false;
    private float previewTime = 0f;
    private float previewSpeed = 0.01f;
    private GameObject previewInstance; // 预览角色的实例
    private AnimationClip previewAnimationClip;

    // 帧预览控制
    private int previewFrameIndex = 0;
    private bool showFrameByFrame = false;
    private float lastFrameUpdateTime = 0f;

    // 预览相机控制
    private Vector3 previewCameraOffset = new Vector3(2, 1, -3);
    private float previewCameraDistance = 5f;
    private bool autoRotateCamera = false;
    private float cameraRotationSpeed = 10f;

    private ActionManager actionManager;
    private Vector2 scrollPosition;
    private int selectedAttackIndex = 0;
    private bool showTimeSettings = true;
    private bool showMovementSettings = true;
    private bool showEffects = true;

    // 攻击框预览设置
    private bool showHitboxPreview = true;
    private GameObject previewCharacter; // 用于预览的角色对象
    private Vector3 previewPosition = Vector3.zero; // 预览位置
    private bool previewFacingRight = true; // 预览朝向

    private bool showMultiFrameSettings = true;
    private Vector2 frameDataScrollPos;
    private int selectedFrameIndex = 0;

    // 所有可编辑的攻击数据列表
    private List<AttackActionData> allAttacks = new List<AttackActionData>();

    // 用于存储最后打开的文件路径的键
    private const string LastOpenPathKey = "AttackDataEditor_LastOpenPath";

    // 复制粘贴相关变量
    private static string copiedActionDataJson; // 静态变量，用于存储复制的ActionData
    private bool hasCopiedData = false; // 标记是否有可粘贴的数据



    [MenuItem("Tools/Attack Editor")]
    public static void ShowWindow()
    {
        GetWindow<ActionDataEditor>("角色行为编辑器");
    }

    private void OnEnable()
    {
        // 窗口启用时尝试加载最后打开的文件
        LoadLastOpenFile();

        // 订阅Scene视图的绘制事件
        SceneView.duringSceneGui += OnSceneGUI;

        // 初始化预览系统
        EditorApplication.update += OnEditorUpdate;

        // 尝试加载最后设置的预览角色
        LoadLastPreviewCharacter();
    }

    private void OnDisable()
    {
        // 清理预览资源
        if (previewInstance != null)
        {
            DestroyImmediate(previewInstance);
        }

#if UNITY_EDITOR
        if (AnimationMode.InAnimationMode())
        {
            AnimationMode.StopAnimationMode();
        }
#endif

        SceneView.duringSceneGui -= OnSceneGUI;
        EditorApplication.update -= OnEditorUpdate;
    }

    private void OnEditorUpdate()
    {
        // 更新动画预览
        UpdateAnimationPreview();

        // 需要重绘Scene视图
        if (isPlayingPreview || showFrameByFrame)
        {
            SceneView.RepaintAll();
        }
    }

    private void OnGUI()
    {
        DrawToolbar();
        DrawPreviewSettings();

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        if (actionManager == null)
        {
            DrawSetupSection();
        }
        else
        {
            DrawAttackManager();
            DrawAttackEditor();
        }

        EditorGUILayout.EndScrollView();
    }

    // 在Scene视图中绘制攻击框预览
    private void OnSceneGUI(SceneView sceneView)
    {
        if (!showHitboxPreview || actionManager == null) return;
        if (allAttacks == null || selectedAttackIndex < 0 || selectedAttackIndex >= allAttacks.Count) return;

        var currentAttack = allAttacks[selectedAttackIndex];
        if (currentAttack == null) return;

        // 如果有动画预览，使用预览帧；否则使用选择的帧
        int displayFrame = showAnimationPreview ? previewFrameIndex : selectedFrameIndex;

        // 获取帧数据并绘制
        var frameData = currentAttack.GetFrameData(displayFrame);
        if (frameData != null && frameData.isAttackFrame)
        {
            DrawFrameHitboxPreview(frameData, sceneView);
        }

        // 显示调试信息
        Handles.BeginGUI();
        GUILayout.BeginArea(new Rect(10, 140, 300, 150));
        GUILayout.BeginVertical(EditorStyles.helpBox);

        GUILayout.Label($"攻击框预览");
        GUILayout.Label($"显示帧: {displayFrame}");

        if (showAnimationPreview)
        {
            GUILayout.Label($"预览模式: {(isPlayingPreview ? "播放中" : "暂停")}");
            GUILayout.Label($"预览进度: {previewTime:P1}");
        }

        if (frameData != null)
        {
            GUILayout.Label($"攻击帧: {frameData.isAttackFrame}");
            GUILayout.Label($"形状: {frameData.hitboxType}");
        }

        GUILayout.EndVertical();
        GUILayout.EndArea();
        Handles.EndGUI();

        // 绘制预览角色和动画信息
        if (showAnimationPreview && previewInstance != null)
        {
            DrawPreviewInfo(sceneView);
        }
    }

    // 加载最后设置的预览角色
    private void LoadLastPreviewCharacter()
    {
        if (actionManager)
        {
            SetPreviewCharacter(actionManager.character);
        }
    }

    // 设置预览角色时保存路径
    private void SetPreviewCharacter(GameObject characterPrefab)
    {
        previewCharacter = characterPrefab;

        // 保存路径到EditorPrefs
        if (previewCharacter != null)
        {
            string path = AssetDatabase.GetAssetPath(previewCharacter);
            if (!string.IsNullOrEmpty(path))
            {
                if (actionManager.character != characterPrefab)
                {
                    actionManager.character = characterPrefab;
                }
            }
        }

        // 清理旧的预览实例
        if (previewInstance != null)
        {
            DestroyImmediate(previewInstance);
        }

        if (previewCharacter != null)
        {
            // 创建预览实例
            previewInstance = Instantiate(previewCharacter);
            previewInstance.hideFlags = HideFlags.HideAndDontSave;

            // 设置预览位置
            previewInstance.transform.position = previewPosition;
            previewInstance.transform.rotation = Quaternion.identity;

            // 应用朝向
            Vector3 scale = previewInstance.transform.localScale;
            scale.x = Mathf.Abs(scale.x) * (previewFacingRight ? 1 : -1);
            previewInstance.transform.localScale = scale;

            Debug.Log($"创建预览角色: {previewInstance.name}");
        }
    }

    // 新增：绘制预览信息
    private void DrawPreviewInfo(SceneView sceneView)
    {
        Handles.BeginGUI();

        // 在Scene视图右上角显示预览信息
        GUILayout.BeginArea(new Rect(sceneView.position.width - 310, 10, 300, 120));
        GUILayout.BeginVertical(EditorStyles.helpBox);

        GUILayout.Label("动画预览", EditorStyles.boldLabel);
        GUILayout.Label($"角色: {previewInstance.name}");

        if (previewAnimationClip != null)
        {
            GUILayout.Label($"动画: {previewAnimationClip.name}");
            GUILayout.Label($"帧: {previewFrameIndex}/{Mathf.RoundToInt(previewAnimationClip.length * previewAnimationClip.frameRate)}");
            GUILayout.Label($"时间: {previewTime * previewAnimationClip.length:F2}s");
        }

        GUILayout.EndVertical();
        GUILayout.EndArea();

        Handles.EndGUI();

        // 绘制帧标记线
        if (previewAnimationClip != null)
        {
            float frameDuration = 1f / previewAnimationClip.frameRate;
            float currentTime = previewFrameIndex * frameDuration;

            // 在角色位置绘制时间标记
            Vector3 markerPos = previewInstance.transform.position + Vector3.up * 2f;
            Handles.color = Color.yellow;
            Handles.Label(markerPos, $"Frame {previewFrameIndex}\n{currentTime:F2}s");
            Handles.DrawWireDisc(markerPos, Vector3.forward, 0.3f);
        }
    }

    // 绘制动画预览设置面板
    private void DrawPreviewSettings()
    {
        if (actionManager == null) return;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("动画预览设置", EditorStyles.boldLabel);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        showAnimationPreview = EditorGUILayout.Toggle("启用动画预览", showAnimationPreview);

        if (showAnimationPreview)
        {
            // 预览角色选择
            EditorGUILayout.BeginHorizontal();
            GameObject newPreviewCharacter = (GameObject)EditorGUILayout.ObjectField("预览角色", previewCharacter, typeof(GameObject), false);

            if (newPreviewCharacter != previewCharacter)
            {
                SetPreviewCharacter(newPreviewCharacter);
            }

            if (GUILayout.Button("使用默认", GUILayout.Width(80)))
            {
                SetPreviewCharacter(newPreviewCharacter);
            }
            EditorGUILayout.EndHorizontal();

            // 动画预览控制
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(isPlayingPreview ? "暂停" : "播放", GUILayout.Width(60)))
            {
                TogglePreviewPlayback();
            }

            if (GUILayout.Button("停止", GUILayout.Width(60)))
            {
                StopPreview();
            }

            if (GUILayout.Button("重置", GUILayout.Width(60)))
            {
                ResetPreview();
            }

            previewSpeed = EditorGUILayout.Slider("播放速度", previewSpeed, 0.0f, 3f);
            EditorGUILayout.EndHorizontal();

            // 帧控制
            EditorGUILayout.BeginHorizontal();
            showFrameByFrame = EditorGUILayout.Toggle("逐帧模式", showFrameByFrame, GUILayout.Width(100));

            if (showFrameByFrame)
            {
                EditorGUILayout.Space(20);
                if (GUILayout.Button("上一帧", GUILayout.Width(60)))
                {
                    PreviousFrame();
                }

                if (GUILayout.Button("下一帧", GUILayout.Width(60)))
                {
                    NextFrame();
                }

                EditorGUILayout.LabelField($"当前帧: {previewFrameIndex}", GUILayout.Width(80));
            }
            EditorGUILayout.EndHorizontal();

            // 帧跳转控制
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("跳转到帧:", GUILayout.Width(70));

            int targetFrame = EditorGUILayout.IntField(previewFrameIndex, GUILayout.Width(60));
            if (targetFrame != previewFrameIndex && GUILayout.Button("跳转", GUILayout.Width(50)))
            {
                JumpToFrame(targetFrame);
            }

            EditorGUILayout.EndHorizontal();

            // 进度条
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("进度:", GUILayout.Width(40));
            float newTime = EditorGUILayout.Slider(previewTime, 0f, 1f);
            if (Mathf.Abs(newTime - previewTime) > 0.001f)
            {
                previewTime = newTime;
                UpdatePreviewTime();
            }
            EditorGUILayout.EndHorizontal();

            // 相机控制
            EditorGUILayout.BeginHorizontal();
            autoRotateCamera = EditorGUILayout.Toggle("自动旋转预览相机", autoRotateCamera, GUILayout.Width(120));
            if (autoRotateCamera)
            {
                cameraRotationSpeed = EditorGUILayout.Slider("旋转速度", cameraRotationSpeed, 1f, 30f);
            }
            EditorGUILayout.EndHorizontal();

            // 显示当前动画信息
            if (previewAnimationClip != null)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField($"动画: {previewAnimationClip.name}", EditorStyles.miniBoldLabel);
                EditorGUILayout.LabelField($"时长: {previewAnimationClip.length:F2}秒", EditorStyles.miniLabel);
                EditorGUILayout.LabelField($"帧率: {previewAnimationClip.frameRate}fps", EditorStyles.miniLabel);
                EditorGUILayout.LabelField($"总帧数: {Mathf.RoundToInt(previewAnimationClip.length * previewAnimationClip.frameRate)}", EditorStyles.miniLabel);
                EditorGUILayout.EndVertical();
            }
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();
    }

    // 更新动画预览
    private void UpdateAnimationPreview()
    {
        if (!showAnimationPreview || previewInstance == null || previewAnimationClip == null) return;

        // 只在时间变化或播放状态下更新
        float previousTime = previewTime;

        if (isPlayingPreview && !showFrameByFrame)
        {
            // 正常播放模式
            previewTime += Time.deltaTime * previewSpeed / previewAnimationClip.length;
            if (previewTime >= 1f)
            {
                previewTime = 0f;
            }
        }

        // 只有当时间确实变化时才更新
        if (Mathf.Abs(previewTime - previousTime) > 0.001f ||
            (showFrameByFrame && Time.realtimeSinceStartup - lastFrameUpdateTime > 0.1f))
        {
            UpdatePreviewTime();
        }

        // 更新相机自动旋转
        if (autoRotateCamera)
        {
            UpdatePreviewCamera();
        }
    }

    // 添加动画剪辑变化检测
    private void OnPreviewAnimationClipChanged()
    {
        if (previewAnimationClip != null && previewInstance != null)
        {
            // 重新初始化动画系统
            Animation animationComponent = previewInstance.GetComponent<Animation>();
            if (animationComponent != null)
            {
                animationComponent.clip = previewAnimationClip;
                if (!animationComponent.IsPlaying(previewAnimationClip.name))
                {
                    animationComponent.AddClip(previewAnimationClip, previewAnimationClip.name);
                }
            }

            // 重置预览时间
            previewTime = 0f;
            UpdatePreviewTime();
        }
    }

    // 更新预览时间
    private void UpdatePreviewTime()
    {
        if (previewAnimationClip != null && previewInstance != null)
        {


            UseAnimationModeForPreview();


            // 计算当前帧索引
            int totalFrames = Mathf.RoundToInt(previewAnimationClip.length * previewAnimationClip.frameRate);
            previewFrameIndex = Mathf.FloorToInt(previewTime * totalFrames);
            previewFrameIndex = Mathf.Clamp(previewFrameIndex, 0, totalFrames - 1);

            // 更新Scene视图
            SceneView.RepaintAll();

            //Debug.Log($"预览时间: {previewTime:F2}, 帧: {previewFrameIndex}/{totalFrames}");
        }
    }

    // 使用AnimationMode进行高级预览
    private void UseAnimationModeForPreview()
    {
#if UNITY_EDITOR
        if (!AnimationMode.InAnimationMode())
        {
            AnimationMode.StartAnimationMode();
        }

        // 为所有子对象的变换属性记录动画
        var renderers = previewInstance.GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            AnimationMode.BeginSampling();
            AnimationMode.SampleAnimationClip(previewInstance, previewAnimationClip, previewTime * previewAnimationClip.length);
            AnimationMode.EndSampling();
        }
#endif
    }

    // 预览控制方法
    private void TogglePreviewPlayback()
    {
        isPlayingPreview = !isPlayingPreview;
        showFrameByFrame = false;
    }

    private void StopPreview()
    {
        isPlayingPreview = false;
        previewTime = 0f;

        UpdatePreviewTime();
    }

    private void ResetPreview()
    {
        previewTime = 0f;
        previewFrameIndex = 0;
        UpdatePreviewTime();
    }

    private void NextFrame()
    {
        if (previewAnimationClip != null)
        {
            int totalFrames = Mathf.RoundToInt(previewAnimationClip.length * previewAnimationClip.frameRate);
            previewFrameIndex = (previewFrameIndex + 1) % totalFrames;
            previewTime = previewFrameIndex / (float)totalFrames;
            UpdatePreviewTime();
            lastFrameUpdateTime = Time.realtimeSinceStartup;
        }
    }

    private void PreviousFrame()
    {
        if (previewAnimationClip != null)
        {
            int totalFrames = Mathf.RoundToInt(previewAnimationClip.length * previewAnimationClip.frameRate);
            previewFrameIndex = (previewFrameIndex - 1 + totalFrames) % totalFrames;
            previewTime = previewFrameIndex / (float)totalFrames;
            UpdatePreviewTime();
            lastFrameUpdateTime = Time.realtimeSinceStartup;
        }
    }

    // 新增：更新预览相机
    private void UpdatePreviewCamera()
    {
        SceneView sceneView = SceneView.lastActiveSceneView;
        if (sceneView != null && previewInstance != null)
        {
            // 计算相机位置（围绕角色旋转）
            float angle = Time.realtimeSinceStartup * cameraRotationSpeed * Mathf.Deg2Rad;
            Vector3 cameraPos = previewInstance.transform.position +
                              new Vector3(Mathf.Cos(angle) * previewCameraDistance,
                                         previewCameraOffset.y,
                                         Mathf.Sin(angle) * previewCameraDistance);

            sceneView.LookAt(cameraPos, Quaternion.LookRotation(previewInstance.transform.position - cameraPos));
        }
    }


    private void DrawFrameHitboxPreview(AttackFrameData frameData, SceneView sceneView)
    {
        // 绘制特定帧的攻击框（与全局攻击框绘制类似）
        Vector3 characterPosition = previewPosition;
        bool facingRight = previewFacingRight;

        if (previewCharacter != null)
        {
            characterPosition = previewCharacter.transform.position;
            facingRight = previewCharacter.transform.lossyScale.x > 0;
        }

        Vector2 hitboxOffset = frameData.hitboxOffset;
        float facingMultiplier = facingRight ? 1 : -1;

        Vector3 hitboxCenter = characterPosition +
            new Vector3(
                hitboxOffset.x * facingMultiplier,
                hitboxOffset.y,
                0
            );

        // 根据帧数据的形状绘制...
        Handles.color = Color.red;
        switch (frameData.hitboxType)
        {
            case HitboxType.Rectangle:
                DrawRectangleHitbox(hitboxCenter, frameData.hitboxSize, facingMultiplier);
                break;
            case HitboxType.Circle:
                DrawCircleHitbox(hitboxCenter, frameData.hitboxRadius);
                break;
            case HitboxType.Capsule:
                DrawCapsuleHitbox(hitboxCenter, frameData.hitboxSize, frameData.hitboxRadius, facingMultiplier);
                break;
            case HitboxType.Sector:
                DrawSectorHitbox(hitboxCenter, frameData.hitboxRadius, frameData.hitboxAngle, facingMultiplier);
                break;
            case HitboxType.Line:
                DrawLineHitbox(hitboxCenter, frameData.hitboxEndPoint, frameData.hitboxRadius, facingMultiplier);
                break;
        }
    }

    // 各种形状的绘制方法
    private void DrawRectangleHitbox(Vector3 center, Vector2 size, float facingMultiplier)
    {
        Handles.DrawWireCube(center, new Vector3(size.x, size.y, 0.1f));
    }

    private void DrawCircleHitbox(Vector3 center, float radius)
    {
        Handles.DrawWireDisc(center, Vector3.forward, radius);
    }

    private void DrawCapsuleHitbox(Vector3 center, Vector2 size, float radius, float facingMultiplier)
    {
        // 胶囊形由两个半圆和中间矩形组成
        Vector3 startPoint = center - new Vector3(size.x * 0.5f * facingMultiplier, 0, 0);
        Vector3 endPoint = center + new Vector3(size.x * 0.5f * facingMultiplier, 0, 0);

        // 绘制两个半圆
        Handles.DrawWireArc(startPoint, Vector3.forward, Vector3.up, 180, radius);
        Handles.DrawWireArc(endPoint, Vector3.forward, Vector3.down, 180, radius);

        // 绘制连接线
        Handles.DrawLine(startPoint + Vector3.up * radius, endPoint + Vector3.up * radius);
        Handles.DrawLine(startPoint + Vector3.down * radius, endPoint + Vector3.down * radius);
    }

    private void DrawSectorHitbox(Vector3 center, float radius, float angle, float facingMultiplier)
    {
        // 扇形绘制
        float startAngle = -angle * 0.5f * facingMultiplier;
        float endAngle = angle * 0.5f * facingMultiplier;

        Handles.DrawWireArc(center, Vector3.forward, Quaternion.Euler(0, 0, startAngle) * Vector3.right, angle, radius);

        // 绘制扇形边界线
        Vector3 startDir = Quaternion.Euler(0, 0, startAngle) * Vector3.right;
        Vector3 endDir = Quaternion.Euler(0, 0, endAngle) * Vector3.right;
        Handles.DrawLine(center, center + startDir * radius);
        Handles.DrawLine(center, center + endDir * radius);
    }

    private void DrawLineHitbox(Vector3 center, Vector2 endPoint, float width, float facingMultiplier)
    {
        Vector3 worldEndPoint = center + new Vector3(endPoint.x * facingMultiplier, endPoint.y, 0);

        // 绘制线段
        Handles.DrawLine(center, worldEndPoint);

        // 绘制线段的宽度表示
        Vector3 direction = (worldEndPoint - center).normalized;
        Vector3 perpendicular = Vector3.Cross(direction, Vector3.forward).normalized * width * 0.5f;

        Handles.DrawLine(center + perpendicular, worldEndPoint + perpendicular);
        Handles.DrawLine(center - perpendicular, worldEndPoint - perpendicular);
        Handles.DrawLine(center + perpendicular, center - perpendicular);
        Handles.DrawLine(worldEndPoint + perpendicular, worldEndPoint - perpendicular);
    }


    private void DrawToolbar()
    {
        GUILayout.BeginHorizontal(EditorStyles.toolbar);

        if (GUILayout.Button("加载攻击配置", EditorStyles.toolbarButton))
        {
            string path = EditorUtility.OpenFilePanel("选择ActionManager", "Assets", "asset");
            if (!string.IsNullOrEmpty(path))
            {
                path = "Assets" + path.Substring(Application.dataPath.Length);
                actionManager = AssetDatabase.LoadAssetAtPath<ActionManager>(path);
                // 保存路径到EditorPrefs
                EditorPrefs.SetString(LastOpenPathKey, path);
                // 更新攻击列表
                UpdateAllAttacksList();
            }
        }

        if (GUILayout.Button("新建攻击配置", EditorStyles.toolbarButton))
        {
            CreateNewAttackManager();
        }

        GUILayout.FlexibleSpace();



        if (actionManager != null)
        {
            if (GUILayout.Button("保存", EditorStyles.toolbarButton))
            {
                EditorUtility.SetDirty(actionManager);
                // 标记所有攻击数据为脏
                foreach (var attack in allAttacks)
                {
                    if (attack != null)
                    {
                        EditorUtility.SetDirty(attack);
                    }
                }
                AssetDatabase.SaveAssets();
                EditorUtility.DisplayDialog("保存成功", "攻击配置已保存", "确定");
            }
        }

        GUILayout.EndHorizontal();
    }

    // 加载项目中所有ActionData的方法
    private void LoadAllActionDataInProject()
    {
        allAttacks.Clear();

        // 查找所有ActionData资源
        string[] guids = AssetDatabase.FindAssets("t:AttackActionData");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var attackData = AssetDatabase.LoadAssetAtPath<AttackActionData>(path);
            if (attackData != null)
            {
                allAttacks.Add(attackData);
            }
        }

        // 按名称排序
        allAttacks.Sort((a, b) => string.Compare(a.acitonName, b.acitonName));

        Debug.Log($"找到 {allAttacks.Count} 个ActionData资源");

        // 如果列表不为空，默认选择第一个
        if (allAttacks.Count > 0)
        {
            selectedAttackIndex = 0;
        }
    }

    // 尝试加载最后打开的文件
    private void LoadLastOpenFile()
    {
        if (EditorPrefs.HasKey(LastOpenPathKey))
        {
            string lastPath = EditorPrefs.GetString(LastOpenPathKey);
            // 检查路径是否有效且文件存在
            if (!string.IsNullOrEmpty(lastPath) && AssetDatabase.LoadAssetAtPath<ActionManager>(lastPath) != null)
            {
                actionManager = AssetDatabase.LoadAssetAtPath<ActionManager>(lastPath);
            }
            else
            {
                // 如果文件不存在，清除无效的路径记录
                EditorPrefs.DeleteKey(LastOpenPathKey);
            }
        }
    }

    private void DrawSetupSection()
    {
        EditorGUILayout.HelpBox("请加载或创建一个ActionManager来开始编辑攻击数据", MessageType.Info);

        if (GUILayout.Button("创建新的ActionManager", GUILayout.Height(40)))
        {
            CreateNewAttackManager();
        }
    }

    private void DrawAttackManager()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("攻击管理器", EditorStyles.boldLabel);

        // 当手动更改攻击管理器时，更新最后打开的路径
        ActionManager newAttackManager = (ActionManager)EditorGUILayout.ObjectField("攻击管理器", actionManager, typeof(ActionManager), false);
        if (newAttackManager != actionManager)
        {
            actionManager = newAttackManager;
            if (actionManager != null)
            {
                string path = AssetDatabase.GetAssetPath(actionManager);
                EditorPrefs.SetString(LastOpenPathKey, path);
            }
            else
            {
                EditorPrefs.DeleteKey(LastOpenPathKey);
            }
        }

        if (actionManager.normalAttacks == null)
            actionManager.normalAttacks = new AttackActionData[0];

        // 普通攻击序列编辑
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("普通攻击序列", EditorStyles.boldLabel);

        int newCount = EditorGUILayout.IntField("攻击数量", actionManager.normalAttacks.Length);
        if (newCount != actionManager.normalAttacks.Length)
        {
            System.Array.Resize(ref actionManager.normalAttacks, newCount);
        }

        for (int i = 0; i < actionManager.normalAttacks.Length; i++)
        {
            EditorGUILayout.BeginHorizontal();
            actionManager.normalAttacks[i] = (AttackActionData)EditorGUILayout.ObjectField(
                $"攻击 {i + 1}", actionManager.normalAttacks[i], typeof(AttackActionData), false);

            // 添加跳转按钮
            if (actionManager.normalAttacks[i] != null)
            {
                if (GUILayout.Button("跳转", GUILayout.Width(50)))
                {
                    Selection.activeObject = actionManager.normalAttacks[i];
                    EditorGUIUtility.PingObject(actionManager.normalAttacks[i]);
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        // 特殊攻击
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("特殊攻击", EditorStyles.boldLabel);

        DrawSpecialAttackField("冲刺攻击", ref actionManager.dashAttack);
        DrawSpecialAttackField("跳跃攻击", ref actionManager.jumpAttack);
        DrawSpecialAttackField("特殊攻击", ref actionManager.specialAttack);
        DrawSpecialAttackField("特殊攻击2", ref actionManager.specialAttack2);
        DrawSpecialAttackField("重攻击", ref actionManager.heavyAttack);
        DrawSpecialAttackField("弹反攻击", ref actionManager.parryAttack);
    }

    private void DrawSpecialAttackField(string label, ref AttackActionData attackData)
    {
        EditorGUILayout.BeginHorizontal();
        attackData = (AttackActionData)EditorGUILayout.ObjectField(label, attackData, typeof(AttackActionData), false);

        if (attackData != null)
        {
            if (GUILayout.Button("跳转", GUILayout.Width(50)))
            {
                Selection.activeObject = attackData;
                EditorGUIUtility.PingObject(attackData);
            }
        }
        EditorGUILayout.EndHorizontal();
    }

    private void DrawAttackEditor()
    {
        if (actionManager == null) return;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("攻击详细编辑", EditorStyles.boldLabel);

        // 加载所有ActionData按钮
        if (GUILayout.Button("加载所有ActionData", EditorStyles.miniButtonMid))
        {
            LoadAllActionDataInProject();
        }

        // 更新所有攻击列表
        //UpdateAllAttacksList();

        if (allAttacks.Count == 0)
        {
            EditorGUILayout.HelpBox("没有可编辑的攻击数据", MessageType.Info);
            return;
        }

        // 攻击选择区域
        DrawAttackSelection();

        var currentAttack = allAttacks[selectedAttackIndex];
        if (currentAttack != null)
        {
            DrawAttackDetails(currentAttack);
        }
    }

    // 更新所有攻击列表
    private void UpdateAllAttacksList()
    {
        allAttacks.Clear();

        if (actionManager == null) return;

        LoadAllActionDataInProject();
    }

    // 绘制攻击选择界面
    private void DrawAttackSelection()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        EditorGUILayout.LabelField("选择要编辑的攻击", EditorStyles.boldLabel);

        // 显示所有ActionData的选择下拉菜单
        if (allAttacks.Count > 0)
        {
            string[] attackNames = allAttacks.Select(a => a.acitonName).ToArray();
            int newSelectedIndex = EditorGUILayout.Popup("攻击选择", selectedAttackIndex, attackNames);

            if (newSelectedIndex != selectedAttackIndex)
            {
                selectedAttackIndex = newSelectedIndex;
                ActionData selectedAttack = allAttacks[selectedAttackIndex];

                // 更新预览动画
                if (showAnimationPreview && selectedAttack.animationClip != null)
                {
                    previewAnimationClip = selectedAttack.animationClip;
                    OnPreviewAnimationClipChanged();
                }

                // 尝试找到此ActionData所属的ActionManager
                FindAttackManagerForSelectedAttack(selectedAttack);
            }

            // 显示当前选择的ActionData
            ActionData currentAttack = allAttacks[selectedAttackIndex];
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.LabelField($"攻击名称: {currentAttack.acitonName}", EditorStyles.miniBoldLabel);
            EditorGUILayout.LabelField($"动画片段: {(currentAttack.animationClip != null ? currentAttack.animationClip.name : "无")}", EditorStyles.miniLabel);
            EditorGUILayout.LabelField($"路径: {AssetDatabase.GetAssetPath(currentAttack)}", EditorStyles.miniLabel);

            EditorGUILayout.EndVertical();
        }
        else
        {
            EditorGUILayout.HelpBox("没有找到ActionData资源，点击工具栏上的'加载所有ActionData'按钮", MessageType.Info);
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();
    }

    // 查找包含选定ActionData的ActionManager
    private void FindAttackManagerForSelectedAttack(ActionData attackData)
    {
        if (actionManager != null && actionManager.normalAttacks != null)
        {
            // 检查当前ActionManager是否包含此ActionData
            if (Array.IndexOf(actionManager.normalAttacks, attackData) >= 0 ||
                actionManager.dashAttack == attackData ||
                actionManager.jumpAttack == attackData ||
                actionManager.specialAttack == attackData ||
                actionManager.specialAttack2 == attackData ||
                actionManager.heavyAttack == attackData)
            {
                // 已经在当前ActionManager中
                return;
            }
        }

        // 查找所有ActionManager资源
        string[] guids = AssetDatabase.FindAssets("t:ActionManager");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ActionManager manager = AssetDatabase.LoadAssetAtPath<ActionManager>(path);
            if (manager != null)
            {
                // 检查此ActionManager是否包含选定的ActionData
                if ((manager.normalAttacks != null && Array.IndexOf(manager.normalAttacks, attackData) >= 0) ||
                    manager.dashAttack == attackData ||
                    manager.jumpAttack == attackData ||
                    manager.specialAttack == attackData ||
                    manager.specialAttack2 == attackData ||
                    manager.heavyAttack == attackData)
                {
                    actionManager = manager;
                    Debug.Log($"找到包含此ActionData的ActionManager: {manager.name}");
                    return;
                }
            }
        }

        Debug.Log("未找到包含此ActionData的ActionManager");
    }


    private void DrawAttackDetails(AttackActionData attack)
    {
        // 设置当前预览动画
        if (attack.animationClip != null && attack.animationClip != previewAnimationClip)
        {
            previewAnimationClip = attack.animationClip;
            if (showAnimationPreview)
            {
                OnPreviewAnimationClipChanged();
            }
        }

        // 添加复制粘贴按钮
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("复制粘贴", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();

        // 复制按钮
        if (GUILayout.Button("复制 ActionData", GUILayout.Height(25)))
        {
            CopyActionData(attack);
        }

        // 粘贴按钮（只有在有复制数据时才可用）
        EditorGUI.BeginDisabledGroup(!hasCopiedData);
        if (GUILayout.Button("粘贴到当前 ActionData", GUILayout.Height(25)))
        {
            PasteActionData(attack);
        }
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.EndHorizontal();

        // 显示复制状态
        if (hasCopiedData && string.IsNullOrEmpty(copiedActionDataJson) == false)
        {
            EditorGUILayout.HelpBox("已复制 ActionData 数据", MessageType.Info);
        }

        attack.priority = EditorGUILayout.IntField("优先级", attack.priority);
        EditorGUILayout.LabelField("优先级决定是否能打断别人行为,100以下不打断,100-200是普通攻击范围,300是僵直", EditorStyles.helpBox);

        // 基础设置
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("攻击数据文件", GUILayout.Width(100));
        EditorGUILayout.ObjectField(attack, typeof(ActionData), false);
        if (GUILayout.Button("在Project中定位", GUILayout.Width(100)))
        {
            Selection.activeObject = attack;
            EditorGUIUtility.PingObject(attack);
        }
        EditorGUILayout.EndHorizontal();

        attack.acitonName = EditorGUILayout.TextField("攻击名称", attack.acitonName);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("攻击触发方式", EditorStyles.boldLabel);

        attack.triggerType = (AttackTriggerType)EditorGUILayout.EnumPopup("触发类型", attack.triggerType);

        if (attack.triggerType == AttackTriggerType.LongPress || attack.triggerType == AttackTriggerType.Hold)
        {
            attack.longPressTimeThreshold = EditorGUILayout.Slider("长按时间阈值", attack.longPressTimeThreshold, 0.1f, 2f);
        }

        // 动画片段设置，显示时长信息
        EditorGUILayout.BeginHorizontal();
        attack.animationClip = (AnimationClip)EditorGUILayout.ObjectField("动画片段", attack.animationClip, typeof(AnimationClip), false);

        if (attack.animationClip != null)
        {
            float clipLength = attack.animationClip.length;
            EditorGUILayout.LabelField($"{clipLength:F3}秒,{attack.animationClip.frameRate}fps", GUILayout.Width(120));

            // 比较动画时长和配置时长的差异
            float totalConfiguredTime = attack.TotalDuration;
            float timeDifference = Mathf.Abs(clipLength - totalConfiguredTime);

            if (timeDifference > 0.1f)
            {
                EditorGUILayout.LabelField("⚠️ 时长不匹配", EditorStyles.miniLabel, GUILayout.Width(80));
            }
            else
            {
                EditorGUILayout.LabelField("✓ 时长匹配", EditorStyles.miniLabel, GUILayout.Width(80));
            }
        }
        EditorGUILayout.EndHorizontal();


        // 在基础设置部分添加动画参数配置
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("动画参数配置", EditorStyles.boldLabel);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        // 显示参数数组
        for (int i = 0; i < attack.animationParameters.Count; i++)
        {
            var param = attack.animationParameters[i];

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            param.type = (ActionData.AnimationParameterType)EditorGUILayout.EnumPopup("参数类型", param.type);

            if (GUILayout.Button("删除", GUILayout.Width(50)))
            {
                attack.animationParameters.RemoveAt(i);
                EditorUtility.SetDirty(attack);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndVertical();
                return;
            }
            EditorGUILayout.EndHorizontal();

            param.parameterName = EditorGUILayout.TextField("参数名称", param.parameterName);

            // 根据选择的参数类型显示不同的值字段
            switch (param.type)
            {
                case ActionData.AnimationParameterType.Int:
                    param.animationIntValue = EditorGUILayout.IntField("整数值", param.animationIntValue);
                    break;
                case ActionData.AnimationParameterType.Float:
                    param.animationFloatValue = EditorGUILayout.FloatField("浮点值", param.animationFloatValue);
                    break;
                case ActionData.AnimationParameterType.Bool:
                    param.animationBoolValue = EditorGUILayout.Toggle("布尔值", param.animationBoolValue);
                    break;
                case ActionData.AnimationParameterType.Trigger:
                    EditorGUILayout.HelpBox("Trigger类型不需要设置值", MessageType.Info);
                    break;
            }
            EditorGUILayout.EndVertical();
        }

        // 添加新参数按钮
        if (GUILayout.Button("添加新参数"))
        {
            attack.animationParameters.Add(new ActionData.AnimationParameter());
            EditorUtility.SetDirty(attack);
        }

        EditorGUILayout.EndVertical();




        // 在攻击属性部分添加实时预览开关
        // 多帧攻击框设置
        showMultiFrameSettings = EditorGUILayout.Foldout(showMultiFrameSettings, "多帧攻击框设置", true);
        if (showMultiFrameSettings)
        {
            EditorGUI.indentLevel++;

            // 添加预览同步按钮
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("同步到预览帧"))
            {
                SyncToPreviewFrame(attack);
            }

            if (GUILayout.Button("从当前帧创建"))
            {
                CreateFrameAtPreviewFrame(attack);
            }

            // 跳转到选定帧按钮
            if (GUILayout.Button("跳转到选定帧"))
            {
                if (selectedFrameIndex >= 0)
                {
                    JumpToFrame(selectedFrameIndex);
                }
            }
            EditorGUILayout.EndHorizontal();

            // 显示多帧检测状态（只读）
            bool isMultiFrame = attack.ActualActiveFrames > 1;
            EditorGUILayout.LabelField("多帧检测状态:", isMultiFrame ? "启用" : "单帧");
            EditorGUILayout.LabelField($"检测间隔: {attack.DetectionInterval:F3}秒", EditorStyles.miniLabel);
            EditorGUILayout.LabelField($"攻击帧数: {attack.ActualActiveFrames}", EditorStyles.miniLabel);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("帧数据管理", EditorStyles.miniBoldLabel);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("从动画生成帧数据"))
            {
                attack.GenerateFrameDataFromAnimation();
                EditorUtility.SetDirty(attack);
            }

            // 添加创建新帧数据的按钮
            if (GUILayout.Button("创建新帧数据"))
            {
                CreateNewFrameData(attack);
            }

            if (GUILayout.Button("清空帧数据"))
            {
                if (EditorUtility.DisplayDialog("确认清空", "确定要清空所有帧数据吗？", "确定", "取消"))
                {
                    attack.frameData.Clear();
                    EditorUtility.SetDirty(attack);
                }
            }

            EditorGUILayout.EndHorizontal();

            // 同步全局设置按钮
            if (GUILayout.Button("同步全局设置到所有帧"))
            {
                if (EditorUtility.DisplayDialog("同步设置", "将当前攻击数据的全局设置同步到所有帧？", "确定", "取消"))
                {
                    attack.SyncGlobalSettingsToFrames();
                    EditorUtility.SetDirty(attack);
                }
            }

            // 帧数据列表
            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"帧数据 ({attack.frameData.Count} 帧)");

            frameDataScrollPos = EditorGUILayout.BeginScrollView(frameDataScrollPos, GUILayout.Height(200));

            for (int i = 0; i < attack.frameData.Count; i++)
            {
                DrawFrameData(attack.frameData[i], i, attack);
            }

            EditorGUILayout.EndScrollView();

            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space();

        int totalFrames = 0;
        // 显示动画时长和配置时长的对比
        if (attack.animationClip != null)
        {
            float clipLength = attack.animationClip.length;
            float totalConfiguredTime = attack.TotalDuration;
            float timeDifference = clipLength - totalConfiguredTime;
            totalFrames = Mathf.RoundToInt(clipLength * attack.animationClip.frameRate);
            if (attack.frameRate != attack.animationClip.frameRate)//同步动画播放帧率
            {
                attack.frameRate = attack.animationClip.frameRate;
            }

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("动画时长对比", EditorStyles.miniBoldLabel);
            EditorGUILayout.LabelField($"动画片段: {clipLength:F3}秒", EditorStyles.miniLabel);
            EditorGUILayout.LabelField($"总帧数: {totalFrames}", EditorStyles.miniLabel);
            EditorGUILayout.LabelField($"每帧时长: {1 / attack.animationClip.frameRate:F3}秒", EditorStyles.miniLabel);
            EditorGUILayout.LabelField($"配置总时长: {totalConfiguredTime:F3}秒", EditorStyles.miniLabel);

            if (Mathf.Abs(timeDifference) > 0.01f)
            {
                string differenceText = timeDifference > 0 ?
                    $"动画比配置长 {timeDifference:F2}秒" :
                    $"动画比配置短 {Mathf.Abs(timeDifference):F2}秒";

                EditorGUILayout.LabelField(differenceText,
                    timeDifference > 0 ? EditorStyles.miniLabel :
                    new GUIStyle(EditorStyles.miniLabel) { normal = { textColor = Color.red } });

                // 提供自动同步按钮
                if (GUILayout.Button("将配置时长同步到动画时长", GUILayout.Height(20)))
                {
                    SyncToAnimationLength(attack);
                }

            }
            else
            {
                EditorGUILayout.LabelField("✓ 时长完美匹配",
                    new GUIStyle(EditorStyles.miniLabel) { normal = { textColor = Color.green } });
            }
            EditorGUILayout.EndVertical();
        }

        // 时间设置（可折叠）
        showTimeSettings = EditorGUILayout.Foldout(showTimeSettings, "时间设置", true);
        if (showTimeSettings)
        {
            EditorGUI.indentLevel++;

            EditorGUILayout.LabelField($"总时长: {attack.TotalDuration:F2}秒", EditorStyles.helpBox);

            EditorGUI.BeginChangeCheck(); // 开始检查变化

            attack.windUpTime = EditorGUILayout.Slider("前摇时间", attack.windUpTime, 0, 2f);
            attack.ActualWindUpFrames = EditorGUILayout.IntSlider("前摇动画实际帧数(用来计算攻击动画播放速度)", attack.ActualWindUpFrames, 0, totalFrames);
            EditorGUILayout.LabelField($"配置前摇帧数: {attack.windUpFrames}", EditorStyles.miniLabel);

            EditorGUILayout.LabelField($"只有在攻击阶段才进行命中检测", EditorStyles.helpBox);
            attack.activeTime = EditorGUILayout.Slider("攻击中时间", attack.activeTime, 0, 2f);
            attack.ActualActiveFrames = EditorGUILayout.IntSlider("攻击中动画实际帧数(用来计算攻击动画播放速度)", attack.ActualActiveFrames, 0, totalFrames);
            EditorGUILayout.LabelField($"配置攻击中帧数: {attack.activeFrames}", EditorStyles.miniLabel);

            attack.recoveryTime = EditorGUILayout.Slider("后摇时间", attack.recoveryTime, 0, 2f);
            attack.ActualRecoveryFrames = EditorGUILayout.IntSlider("后摇动画实际帧数(用来计算攻击动画播放速度)", attack.ActualRecoveryFrames, 0, totalFrames);
            EditorGUILayout.LabelField($"配置后摇帧数: {attack.recoveryFrames}", EditorStyles.miniLabel);

            attack.comboWindow = EditorGUILayout.Slider("连招窗口", attack.comboWindow, 0, 1f);
            EditorGUILayout.LabelField($"连招开始时间: {attack.ComboStartTime:F2}秒", EditorStyles.miniLabel);

            if (EditorGUI.EndChangeCheck()) // 如果值发生变化
            {
                // 手动触发帧数重新计算
                attack.OnValidate(); // 调用OnValidate重新计算帧数
                EditorUtility.SetDirty(attack); // 标记为脏，确保保存
            }

            EditorGUI.indentLevel--;
        }


        // 位移设置
        showMovementSettings = EditorGUILayout.Foldout(showMovementSettings, "位移设置", true);
        if (showMovementSettings)
        {
            EditorGUI.indentLevel++;

            attack.enableMovement = EditorGUILayout.Toggle("启用位移", attack.enableMovement);
            if (attack.enableMovement)
            {
                attack.IsAccumulateForce = EditorGUILayout.Toggle("累加力", attack.IsAccumulateForce);
                attack.movementSpeed = EditorGUILayout.Vector2Field("位移速度", attack.movementSpeed);
                attack.movementCurve = EditorGUILayout.CurveField("位移曲线", attack.movementCurve);
            }

            EditorGUI.indentLevel--;
        }

        // 特殊效果
        showEffects = EditorGUILayout.Foldout(showEffects, "特殊效果", true);
        if (showEffects)
        {
            EditorGUI.indentLevel++;

            attack.hitEffect = (GameObject)EditorGUILayout.ObjectField("命中特效", attack.hitEffect, typeof(GameObject), false);
            attack.attackSound = (AudioClip)EditorGUILayout.ObjectField("攻击音效", attack.attackSound, typeof(AudioClip), false);
            attack.canCancel = EditorGUILayout.Toggle("可被取消", attack.canCancel);
            attack.canCombo = EditorGUILayout.Toggle("可连招", attack.canCombo);
            attack.resetComboOnMiss = EditorGUILayout.Toggle("打空重置连招", attack.resetComboOnMiss);

            EditorGUI.indentLevel--;
        }

        // 预览时间轴
        DrawTimelinePreview(attack);
    }

    #region 复制黏贴

    /// <summary>
    /// 复制ActionData数据
    /// </summary>
    private void CopyActionData(ActionData source)
    {
        if (source == null) return;

        try
        {
            // 使用JsonUtility进行序列化
            copiedActionDataJson = JsonUtility.ToJson(source, true);
            hasCopiedData = true;

            Debug.Log($"已成功复制 ActionData: {source.acitonName}");
        }
        catch (Exception e)
        {
            Debug.LogError($"复制 ActionData 时出错: {e.Message}");
            copiedActionDataJson = null;
            hasCopiedData = false;
        }
    }

    /// <summary>
    /// 粘贴ActionData数据到当前对象
    /// </summary>
    private void PasteActionData(ActionData target)
    {
        if (target == null || string.IsNullOrEmpty(copiedActionDataJson)) return;

        try
        {
            var actionName = target.acitonName;
            // 记录操作以便撤销
            Undo.RecordObject(target, "Paste ActionData");

            // 使用JsonUtility进行反序列化
            JsonUtility.FromJsonOverwrite(copiedActionDataJson, target);

            // 触发验证以确保数据正确
            target.OnValidate();

            target.acitonName = actionName; // 保持原有名称不变 

            // 标记为脏以保存更改
            EditorUtility.SetDirty(target);
            AssetDatabase.SaveAssets();

            Debug.Log($"已成功粘贴 ActionData 到: {target.acitonName}");

            // 刷新UI
            Repaint();
        }
        catch (Exception e)
        {
            Debug.LogError($"粘贴 ActionData 时出错: {e.Message}");
        }
    }
    #endregion

    // 同步到预览帧
    private void SyncToPreviewFrame(AttackActionData attack)
    {
        if (previewFrameIndex >= 0 && previewFrameIndex < attack.frameData.Count)
        {
            selectedFrameIndex = previewFrameIndex;

            // 如果启用了动画预览，也跳转到该帧
            if (showAnimationPreview)
            {
                JumpToFrame(selectedFrameIndex);
            }

            Debug.Log($"同步到预览帧: {previewFrameIndex}");
        }
        else
        {
            Debug.LogWarning($"预览帧 {previewFrameIndex} 超出帧数据范围");
        }
    }

    // 在预览帧创建新帧数据
    private void CreateFrameAtPreviewFrame(AttackActionData attack)
    {
        AttackFrameData newFrame = new AttackFrameData();
        newFrame.frameIndex = previewFrameIndex;

        // 设置默认值（使用攻击数据的全局设置）
        newFrame.isAttackFrame = true;
        newFrame.damage = 1;
        newFrame.hitEffect = attack.hitEffect;
        newFrame.hitSound = attack.attackSound;

        attack.frameData.Add(newFrame);
        EditorUtility.SetDirty(attack);

        // 自动选择新创建的帧
        selectedFrameIndex = newFrame.frameIndex;

        // 跳转到新创建的帧
        if (showAnimationPreview)
        {
            JumpToFrame(selectedFrameIndex);
        }

        Debug.Log($"在预览帧 {previewFrameIndex} 创建新帧数据");
    }

    // 添加创建新帧数据的方法
    private void CreateNewFrameData(AttackActionData attack)
    {
        AttackFrameData newFrame = new AttackFrameData();

        // 设置帧索引为当前帧数
        newFrame.frameIndex = attack.frameData.Count;

        // 设置默认值
        newFrame.isAttackFrame = true;
        newFrame.damage = 1;
        newFrame.hitEffect = attack.hitEffect;
        newFrame.hitSound = attack.attackSound;

        attack.frameData.Add(newFrame);
        EditorUtility.SetDirty(attack);

        // 自动选择新创建的帧
        selectedFrameIndex = newFrame.frameIndex;

        Debug.Log($"创建新的帧数据: 第{newFrame.frameIndex}帧");
    }


    // 修改DrawFrameData方法，添加帧索引编辑功能
    private void DrawFrameData(AttackFrameData frameData, int index, AttackActionData attack)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        EditorGUILayout.BeginHorizontal();
        bool isExpanded = EditorGUILayout.Foldout(frameData.frameIndex == selectedFrameIndex,
            $"第 {frameData.frameIndex} 帧", true);

        // 添加帧索引编辑
        EditorGUILayout.LabelField("帧索引:", GUILayout.Width(50));
        int newFrameIndex = EditorGUILayout.IntField(frameData.frameIndex, GUILayout.Width(40));
        if (newFrameIndex != frameData.frameIndex)
        {
            // 检查帧索引是否重复
            if (attack.frameData.Find(f => f.frameIndex == newFrameIndex && f != frameData) == null)
            {
                frameData.frameIndex = newFrameIndex;
                EditorUtility.SetDirty(attack);
            }
            else
            {
                EditorUtility.DisplayDialog("错误", $"帧索引 {newFrameIndex} 已存在!", "确定");
            }
        }

        frameData.isAttackFrame = EditorGUILayout.Toggle("攻击帧", frameData.isAttackFrame);

        if (GUILayout.Button("选择", GUILayout.Width(50)))
        {
            selectedFrameIndex = frameData.frameIndex;

            // 同步预览动画到选定帧
            if (showAnimationPreview && previewAnimationClip != null)
            {
                JumpToFrame(selectedFrameIndex);
            }
        }

        if (GUILayout.Button("删除", GUILayout.Width(50)))
        {
            if (EditorUtility.DisplayDialog("确认删除", $"确定要删除第{frameData.frameIndex}帧吗？", "确定", "取消"))
            {
                attack.frameData.RemoveAt(index);
                EditorUtility.SetDirty(attack);
                return;
            }
        }

        EditorGUILayout.EndHorizontal();

        if (isExpanded && frameData.frameIndex == selectedFrameIndex)
        {
            EditorGUI.indentLevel++;

            // 帧属性编辑
            frameData.hitLayers = LayerMaskField("攻击层级", frameData.hitLayers);
            frameData.damage = EditorGUILayout.IntField(new GUIContent("附加伤害", "最终伤害 = 技能基础伤害(baseDamage) + 此附加伤害"), frameData.damage);
            frameData.knockbackForce = EditorGUILayout.Vector2Field(new GUIContent("附加击退力", "最终击退力 = 技能基础击退力 + 此附加击退力"), frameData.knockbackForce);
            frameData.allowIndependentHit = EditorGUILayout.Toggle(new GUIContent("独立伤害帧", "允许此帧在同一次攻击中再次对已命中的目标造成伤害"), frameData.allowIndependentHit);

            // 效果列表设置
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("效果列表", EditorStyles.miniBoldLabel);

            // 创建序列化对象来正确编辑effects列表
            // 备用方案：手动绘制效果列表
            DrawEffectsListManually(frameData, attack);

            // 攻击框形状编辑
            EditorGUI.BeginChangeCheck();
            frameData.hitboxType = (HitboxType)EditorGUILayout.EnumPopup("攻击框形状", frameData.hitboxType);

            // 根据形状显示不同参数
            switch (frameData.hitboxType)
            {
                case HitboxType.Rectangle:
                    frameData.hitboxOffset = EditorGUILayout.Vector2Field("攻击框偏移", frameData.hitboxOffset);
                    frameData.hitboxSize = EditorGUILayout.Vector2Field("攻击框大小", frameData.hitboxSize);
                    break;

                case HitboxType.Circle:
                    frameData.hitboxOffset = EditorGUILayout.Vector2Field("圆心偏移", frameData.hitboxOffset);
                    frameData.hitboxRadius = EditorGUILayout.FloatField("半径", frameData.hitboxRadius);
                    break;

                case HitboxType.Capsule:
                    frameData.hitboxOffset = EditorGUILayout.Vector2Field("胶囊偏移", frameData.hitboxOffset);
                    frameData.hitboxSize = EditorGUILayout.Vector2Field("胶囊大小", frameData.hitboxSize);
                    frameData.hitboxRadius = EditorGUILayout.FloatField("胶囊半径", frameData.hitboxRadius);
                    break;

                case HitboxType.Sector:
                    frameData.hitboxOffset = EditorGUILayout.Vector2Field("扇形偏移", frameData.hitboxOffset);
                    frameData.hitboxRadius = EditorGUILayout.FloatField("扇形半径", frameData.hitboxRadius);
                    frameData.hitboxAngle = EditorGUILayout.Slider("扇形角度", frameData.hitboxAngle, 0f, 360f);
                    break;

                case HitboxType.Line:
                    frameData.hitboxOffset = EditorGUILayout.Vector2Field("线段偏移", frameData.hitboxOffset);
                    frameData.hitboxEndPoint = EditorGUILayout.Vector2Field("线段终点", frameData.hitboxEndPoint);
                    frameData.hitboxRadius = EditorGUILayout.FloatField("线段宽度", frameData.hitboxRadius);
                    break;
            }

            if (EditorGUI.EndChangeCheck())
            {
                SceneView.RepaintAll(); // 实时更新Scene视图中的预览
                EditorUtility.SetDirty(attack);
            }

            // 特效设置
            frameData.hitEffect = (GameObject)EditorGUILayout.ObjectField("命中特效", frameData.hitEffect, typeof(GameObject), false);
            frameData.hitSound = (AudioClip)EditorGUILayout.ObjectField("命中音效", frameData.hitSound, typeof(AudioClip), false);

            EditorGUI.indentLevel--;
        }

        EditorGUILayout.EndVertical();
    }

    // 手动绘制效果列表的备用方法
    private void DrawEffectsListManually(AttackFrameData frameData, AttackActionData attackActionData)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        // 显示当前效果数量
        EditorGUILayout.LabelField($"效果数量: {frameData.effects.Count}");

        // 绘制每个效果
        for (int i = 0; i < frameData.effects.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();

            // 效果对象字段
            frameData.effects[i] = (EffectData)EditorGUILayout.ObjectField($"效果 {i + 1}", frameData.effects[i], typeof(EffectData), false);

            // 删除按钮
            if (GUILayout.Button("删除", GUILayout.Width(50)))
            {
                frameData.effects.RemoveAt(i);
                i--; // 调整索引
                EditorUtility.SetDirty(attackActionData);
            }

            EditorGUILayout.EndHorizontal();

            // 显示效果详情（如果已分配）
            if (frameData.effects[i] != null)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField($"名称: {frameData.effects[i].effectName}", EditorStyles.miniLabel);
                EditorGUILayout.LabelField($"类型: {frameData.effects[i].category}", EditorStyles.miniLabel);
                EditorGUILayout.LabelField($"持续时间: {frameData.effects[i].duration}秒", EditorStyles.miniLabel);
                EditorGUI.indentLevel--;
            }
        }

        // 添加新效果按钮
        if (GUILayout.Button("添加新效果"))
        {
            frameData.effects.Add(null);
            EditorUtility.SetDirty(attackActionData);
        }

        EditorGUILayout.EndVertical();
    }

    // 跳转到指定帧的方法
    private void JumpToFrame(int frameIndex)
    {
        if (previewAnimationClip == null) return;

        int totalFrames = Mathf.RoundToInt(previewAnimationClip.length * previewAnimationClip.frameRate);

        // 确保帧索引在有效范围内
        frameIndex = Mathf.Clamp(frameIndex, 0, totalFrames - 1);

        // 计算对应的时间位置
        previewTime = frameIndex / (float)totalFrames;

        // 停止预览播放
        isPlayingPreview = false;
        showFrameByFrame = false;

        // 更新预览
        UpdatePreviewTime();

        Debug.Log($"跳转到第 {frameIndex} 帧 (时间: {previewTime * previewAnimationClip.length:F2}s)");
    }


    private void SyncToAnimationLength(AttackActionData attack)
    {
        if (attack.animationClip != null)
        {
            float frameLength = 1f / attack.animationClip.frameRate;

            // 保持各阶段比例，调整到动画时长
            attack.windUpTime = frameLength * attack.ActualWindUpFrames;
            attack.activeTime = frameLength * attack.ActualActiveFrames;
            attack.recoveryTime = frameLength * attack.ActualRecoveryFrames;

            attack.OnValidate();//刷新一下自动帧率计算

            EditorUtility.SetDirty(attack);
            Debug.Log($"已将攻击 {attack.acitonName} 的配置时长同步到动画时长: {attack.TotalDuration:F2}秒");
        }
    }


    private void DrawTimelinePreview(AttackActionData attack)
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("时间轴预览", EditorStyles.boldLabel);

        Rect rect = GUILayoutUtility.GetRect(400, 80);
        EditorGUI.DrawRect(rect, new Color(0.2f, 0.2f, 0.2f));

        float totalWidth = rect.width;
        float windUpWidth = (attack.windUpTime / attack.TotalDuration) * totalWidth;
        float activeWidth = (attack.activeTime / attack.TotalDuration) * totalWidth;
        float recoveryWidth = (attack.recoveryTime / attack.TotalDuration) * totalWidth;

        // 绘制前摇阶段
        Rect windUpRect = new Rect(rect.x, rect.y, windUpWidth, rect.height);
        EditorGUI.DrawRect(windUpRect, Color.yellow);
        GUI.Label(windUpRect, $"前摇\n{attack.windUpTime:F2}s", EditorStyles.centeredGreyMiniLabel);

        // 绘制攻击中阶段
        Rect activeRect = new Rect(rect.x + windUpWidth, rect.y, activeWidth, rect.height);
        EditorGUI.DrawRect(activeRect, Color.red);
        GUI.Label(activeRect, $"攻击中\n{attack.activeTime:F2}s", EditorStyles.centeredGreyMiniLabel);

        // 绘制后摇阶段
        Rect recoveryRect = new Rect(rect.x + windUpWidth + activeWidth, rect.y, recoveryWidth, rect.height);
        EditorGUI.DrawRect(recoveryRect, Color.blue);
        GUI.Label(recoveryRect, $"后摇\n{attack.recoveryTime:F2}s", EditorStyles.centeredGreyMiniLabel);

        // 绘制连招窗口
        float comboStartX = (attack.ComboStartTime / attack.TotalDuration) * totalWidth;
        float comboWidth = (attack.comboWindow / attack.TotalDuration) * totalWidth;
        Rect comboRect = new Rect(rect.x + comboStartX, rect.y + rect.height - 8, comboWidth, 8);
        EditorGUI.DrawRect(comboRect, Color.green);
        GUI.Label(new Rect(comboRect.x, comboRect.y - 15, comboRect.width, 12),
                 $"连招窗口\n{attack.comboWindow:F2}s",
                 new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.UpperCenter, normal = { textColor = Color.white } });

        // 显示动画时长对比标记（如果存在动画片段）
        if (attack.animationClip != null)
        {
            float clipLength = attack.animationClip.length;
            float markerX = (clipLength / attack.TotalDuration) * totalWidth;

            if (markerX <= totalWidth)
            {
                // 动画结束标记
                EditorGUI.DrawRect(new Rect(rect.x + markerX, rect.y, 2, rect.height), Color.white);
                GUI.Label(new Rect(rect.x + markerX - 30, rect.y - 20, 60, 15),
                         $"动画结束\n{clipLength:F2}s",
                         new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.UpperCenter, normal = { textColor = Color.white } });
            }
        }

        // 底部标签
        Rect labelRect = new Rect(rect.x, rect.y + rect.height + 5, totalWidth, 20);
        GUI.Label(labelRect, $"总时长: {attack.TotalDuration:F2}秒 | 前摇: {attack.windUpTime:F2}秒 | 攻击中: {attack.activeTime:F2}秒 | 后摇: {attack.recoveryTime:F2}秒",
                 EditorStyles.centeredGreyMiniLabel);
    }

    private void CreateNewAttackManager()
    {
        ActionManager newManager = CreateInstance<ActionManager>();

        string path = EditorUtility.SaveFilePanel("保存ActionManager", "Assets", "NewAttackManager", "asset");
        if (!string.IsNullOrEmpty(path))
        {
            path = "Assets" + path.Substring(Application.dataPath.Length);
            AssetDatabase.CreateAsset(newManager, path);
            AssetDatabase.SaveAssets();
            actionManager = newManager;

            // 保存新创建的文件路径
            EditorPrefs.SetString(LastOpenPathKey, path);

            // 选中新创建的资源
            Selection.activeObject = newManager;
            EditorGUIUtility.PingObject(newManager);
        }
    }

    // 绘制层级字段（改进的LayerMask字段）
    private LayerMask LayerMaskField(string label, LayerMask layerMask)
    {
        List<string> layers = new List<string>();
        List<int> layerNumbers = new List<int>();

        for (int i = 0; i < 32; i++)
        {
            string layerName = LayerMask.LayerToName(i);
            if (!string.IsNullOrEmpty(layerName))
            {
                layers.Add(layerName);
                layerNumbers.Add(i);
            }
        }

        int maskWithoutEmpty = 0;
        for (int i = 0; i < layerNumbers.Count; i++)
        {
            if (((1 << layerNumbers[i]) & layerMask.value) != 0)
                maskWithoutEmpty |= (1 << i);
        }

        maskWithoutEmpty = EditorGUILayout.MaskField(label, maskWithoutEmpty, layers.ToArray());

        int mask = 0;
        for (int i = 0; i < layerNumbers.Count; i++)
        {
            if ((maskWithoutEmpty & (1 << i)) != 0)
                mask |= (1 << layerNumbers[i]);
        }

        layerMask.value = mask;
        return layerMask;
    }

}
#endif