// 自定义Shader路径，在Unity材质面板中显示为 Custom/XRayEffect_Complete
Shader "Custom/XRayEffect_Complete"
{
    // ===================== 材质面板可配置属性 =====================
    // 这些属性会显示在Unity Inspector面板中，支持实时调整参数
    Properties
    {
        // ---------------- 基础材质属性（和Standard Shader一致） ----------------
        _Color ("Main Color", Color) = (1,1,1,1)           // 主色调，会和纹理颜色相乘
        _MainTex ("Base (RGB)", 2D) = "white" {}           // 基础纹理贴图（漫反射/Albedo）
        _Glossiness ("Smoothness", Range(0,1)) = 0.5       // 光滑度（影响高光大小）
        _Metallic ("Metallic", Range(0,1)) = 0.0           // 金属度（0=非金属，1=纯金属）

        // ---------------- X光效果专属属性 ----------------
        _XRayColor ("XRay Color", Color) = (0, 1, 1, 0.5)  // X光颜色（RGBA），A通道控制基础透明度
        _EdgePower ("Edge Strength", Range(1, 10)) = 3     // 边缘强化强度（值越大，边缘越亮越窄）
        _FadePower ("Fade Strength", Range(0.1, 5)) = 1    // 整体衰减强度（值越大，X光整体越亮）
    }

    // ===================== 子着色器（核心渲染逻辑） =====================
    // SubShader是Shader的核心，可定义多个Pass实现多阶段渲染
    SubShader
    {
        // ---------------- 子着色器标签（控制渲染优先级/类型） ----------------
        Tags 
        { 
            "Queue" = "Geometry"      // 渲染队列：归为几何体队列（默认不透明物体队列）
            "IgnoreProjector" = "True"// 忽略投影器（避免被投影纹理影响）
            "RenderType" = "Opaque"   // 渲染类型：不透明物体（用于后期处理/替换着色器）
        }

        // ============== Pass 1: 正常渲染Pass（显示物体原本外观） ==============
        // 这个Pass的作用：和普通材质一样渲染物体的纹理、光照、阴影，保证未被遮挡时正常显示
        Pass
        {
            Name "FORWARD"                    // Pass名称，方便调试时识别
            Tags { "LightMode" = "ForwardBase" } // 光照模式：前向渲染基础通道（支持主光源/环境光/阴影）

            // ---------------- 渲染状态设置 ----------------
            ZWrite On                         // 开启深度写入：将物体深度信息写入深度缓冲，避免被后面的物体穿透
            ZTest LEqual                      // 深度测试规则：只有当前像素深度 ≤ 缓冲中深度时才渲染（正常渲染规则）
            Cull Back                         // 剔除背面：只渲染物体正面（节省性能，也可设为Off显示双面）
            Lighting On                       // 开启光照计算：启用Unity的光照系统

            // ---------------- CG程序块（顶点/片元着色器） ----------------
            CGPROGRAM
            // 编译指令：指定顶点着色器和片元着色器的函数名
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0                // 目标Shader模型：3.0（兼容大多数平台，支持更多特性）
            #pragma multi_compile_fwdbase     // 多编译指令：生成适配ForwardBase光照的变体（支持阴影等）

            // 引入Unity内置的CG库（包含常用函数/宏）
            #include "UnityCG.cginc"          // 基础工具函数（如坐标转换）
            #include "Lighting.cginc"         // 光照相关函数（如光源方向、光照颜色）
            #include "AutoLight.cginc"        // 自动光照/阴影相关函数

            // ---------------- 声明材质属性（关联Properties面板） ----------------
            sampler2D _MainTex;               // 纹理采样器（对应_MainTex）
            float4 _MainTex_ST;               // 纹理缩放/偏移参数（ST=Scale/Translate）
            float4 _Color;                    // 主色调（对应_Color）
            float _Glossiness;                // 光滑度（对应_Glossiness）
            float _Metallic;                  // 金属度（对应_Metallic）

            // ---------------- 顶点输入结构体（从Unity引擎传入的数据） ----------------
            // appdata：存储模型网格的原始数据（顶点、UV、法线等）
            struct appdata
            {
                float4 vertex : POSITION;     // 顶点位置（模型空间）
                float2 uv : TEXCOORD0;        // 第一套UV坐标（用于采样纹理）
                float3 normal : NORMAL;       // 顶点法线（模型空间）
            };

            // ---------------- 顶点输出/片元输入结构体 ----------------
            // v2f：vertex to fragment，顶点着色器输出给片元着色器的数据
            struct v2f
            {
                float2 uv : TEXCOORD0;        // 传递UV坐标（用于片元着色器采样纹理）
                float4 pos : SV_POSITION;     // 顶点位置（裁剪空间，必须有，用于屏幕显示）
                float3 worldNormal : TEXCOORD1;// 世界空间法线（用于光照计算）
                float3 worldPos : TEXCOORD2;  // 世界空间顶点位置（用于计算光照/视角方向）
                SHADOW_COORDS(3)              // 阴影坐标（宏定义，占用TEXCOORD3，用于阴影计算）
            };

            // ---------------- 顶点着色器函数 ----------------
            // 作用：将模型空间数据转换为裁剪空间，并计算片元着色器需要的中间数据
            v2f vert (appdata v)
            {
                v2f o; // 声明输出结构体
                
                // 1. 顶点坐标转换：模型空间 → 裁剪空间（必须步骤，否则物体无法正确显示在屏幕上）
                o.pos = UnityObjectToClipPos(v.vertex);
                
                // 2. UV坐标转换：应用缩放/偏移（支持在材质面板调整纹理平铺/偏移）
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                
                // 3. 法线转换：模型空间 → 世界空间（用于和世界空间的光照方向计算）
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                
                // 4. 顶点位置转换：模型空间 → 世界空间（用于计算光照/视角方向）
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                
                // 5. 传递阴影坐标（宏定义，用于片元着色器计算阴影衰减）
                TRANSFER_SHADOW(o); 
                
                return o;
            }

            // ---------------- 片元着色器函数 ----------------
            // 作用：逐像素计算最终颜色（决定屏幕上每个像素显示的颜色）
            fixed4 frag (v2f i) : SV_Target
            {
                // 1. 采样纹理颜色：根据UV坐标从_MainTex中获取像素颜色，并和主色调相乘
                fixed4 texColor = tex2D(_MainTex, i.uv) * _Color;
                
                // 2. 归一化法线和光照方向（确保向量长度为1，点积结果正确）
                float3 worldNormal = normalize(i.worldNormal);                  // 世界空间法线
                float3 worldLightDir = normalize(UnityWorldSpaceLightDir(i.worldPos)); // 世界空间光源方向
                float3 viewDir = normalize(UnityWorldSpaceViewDir(i.worldPos)); // 世界空间视角方向（从像素到摄像机）
                
                // 3. 计算漫反射光照（Lambert模型）
                // dot(法线, 光照方向)：值越大，像素越朝向光源，漫反射越强
                float diff = max(0, dot(worldNormal, worldLightDir));
                float3 diffuse = _LightColor0.rgb * diff * texColor.rgb; // 漫反射颜色 = 光源颜色 * 漫反射系数 * 纹理颜色
                
                // 4. 计算高光反射（Blinn-Phong模型）
                float3 halfDir = normalize(worldLightDir + viewDir); // 半程向量（光照方向+视角方向的中间向量）
                // pow：强化高光效果，_Glossiness*100将0-1的范围映射到0-100，更符合直观调整
                float spec = pow(max(0, dot(worldNormal, halfDir)), _Glossiness * 100);
                float3 specular = _LightColor0.rgb * spec * _Metallic; // 高光颜色 = 光源颜色 * 高光系数 * 金属度
                
                // 5. 计算环境光和阴影
                float shadow = SHADOW_ATTENUATION(i); // 阴影衰减系数（0=完全阴影，1=无阴影）
                float3 ambient = UNITY_LIGHTMODEL_AMBIENT.rgb * texColor.rgb; // 环境光颜色（全局基础光）
                
                // 6. 合并最终颜色：环境光 + (漫反射+高光) * 阴影衰减，Alpha通道使用纹理的Alpha
                fixed4 finalColor = fixed4(ambient + (diffuse + specular) * shadow, texColor.a);
                
                return finalColor; // 输出最终颜色到屏幕
            }
            ENDCG // 结束CG程序块
        }

        // ============== Pass 2: X光效果Pass（仅被遮挡时渲染） ==============
        // 这个Pass的作用：只在物体被其他物体遮挡的区域渲染半透明的X光轮廓效果
        Pass
        {
            Name "XRAY"                       // Pass名称，方便调试
            Tags { "LightMode" = "Always" }   // 光照模式：始终渲染（不依赖光照，忽略光源）

            // ---------------- 渲染状态设置（X光效果核心） ----------------
            ZWrite Off                        // 关闭深度写入：避免X光效果覆盖深度缓冲，影响其他物体
            ZTest Greater                     // 深度测试规则：只有当前像素深度 > 缓冲中深度时才渲染（即被遮挡的区域）
            Blend SrcAlpha OneMinusSrcAlpha   // 混合模式：透明度混合（SrcAlpha=源Alpha，OneMinusSrcAlpha=1-目标Alpha）
            Cull Back                         // 剔除背面（可改为Off显示双面X光）
            Lighting Off                      // 关闭光照计算：X光效果不受光源影响

            // ---------------- CG程序块（X光效果） ----------------
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            // ---------------- 声明X光属性（关联Properties面板） ----------------
            float4 _XRayColor;    // X光颜色
            float _EdgePower;     // 边缘强度
            float _FadePower;     // 衰减强度

            // ---------------- 顶点输入结构体 ----------------
            struct appdata
            {
                float4 vertex : POSITION; // 顶点位置（模型空间）
                float3 normal : NORMAL;   // 顶点法线（模型空间）
            };

            // ---------------- 顶点输出/片元输入结构体 ----------------
            struct v2f
            {
                float4 pos : SV_POSITION; // 裁剪空间顶点位置
                float3 viewDir : TEXCOORD0; // 模型空间视角方向
                float3 normal : TEXCOORD1;   // 模型空间法线
            };

            // ---------------- 顶点着色器函数 ----------------
            v2f vert (appdata v)
            {
                v2f o;
                // 顶点坐标转换：模型空间 → 裁剪空间
                o.pos = UnityObjectToClipPos(v.vertex);
                // 计算模型空间视角方向（从顶点到摄像机）并归一化
                o.viewDir = normalize(ObjSpaceViewDir(v.vertex));
                // 归一化法线（确保向量长度为1）
                o.normal = normalize(v.normal);
                return o;
            }

            // ---------------- 片元着色器函数（X光核心逻辑） ----------------
            fixed4 frag (v2f i) : SV_Target
            {
                // 1. 计算法线与视角方向的点积：值越小，像素越接近物体边缘
                float dotProduct = dot(i.normal, i.viewDir);
                
                // 2. 计算边缘强度：
                // abs(dotProduct)：取绝对值，让正反方向的边缘都生效
                // 1 - abs(...)：反转值（边缘处值更大）
                // pow(..., _EdgePower)：强化边缘效果（值越大，边缘越突出）
                // * _FadePower：调整整体亮度
                float edge = pow(1 - abs(dotProduct), _EdgePower) * _FadePower;
                
                // 3. 计算X光最终颜色：基础颜色 × 边缘强度（让边缘更亮，中心更透明）
                float4 finalColor = _XRayColor;
                finalColor.a *= edge; // Alpha通道乘以边缘强度，让边缘更不透明，中心更透明
                
                return finalColor;
            }
            ENDCG
        }
    }
    // 回退Shader：如果当前显卡不支持该Shader，使用Standard Shader替代（保证兼容性）
    FallBack "Standard"
}