void WindEffect_float(float3 position, float3 vertexColor, float windFreq, float windAmp, float waveScale, float stiffness, out float3 outPosition)
{
    float verticalInfluence = clamp(position.y * 2.0, 0, 1);
    verticalInfluence *= verticalInfluence;
    
    float phase = _Time.y * windFreq;
    float2 windDir = float2(sin(phase), cos(phase * 0.8)) * 0.3;
    float2 offset = windDir * windAmp * verticalInfluence;
    
    float wave = sin(_Time.y * windFreq * 1.5 + position.x * waveScale) * 0.08;
    offset.x += wave * verticalInfluence * (1.0 - stiffness);
    
    offset *= vertexColor.g * 1.2;
    
    outPosition = position + float3(offset.x, 0, 0);
}
//根据世界位置调整风效果的函数
void WindEffect2_float(
    float3 position, // 顶点位置
    float3 worldPosition, // 世界位置
    float3 vertexColor, // 顶点色
    float windFreq, // 风频率
    float windAmp, // 风振幅
    float waveScale, // 波形缩放
    float stiffness, // 刚度
    float mouseIntensity, // 新增：鼠标交互强度
    float mouseFrequency, // 新增：鼠标交互频率
    out float3 outPosition // 输出位置
)
{
    // 计算垂直影响（顶部受影响更大）
    float verticalInfluence = clamp(position.y * 2.0, 0, 1);
    verticalInfluence *= verticalInfluence;
    
    // 基于世界位置生成唯一相位偏移
    // 使用世界位置xz坐标创建伪随机相位
    float2 worldHash = frac(worldPosition.xz * 0.1);
    float worldPhase = sin(worldHash.x * 123.4 + worldHash.y * 567.8) * 6.28;
    
    // 主风相位（时间 + 世界位置偏移）
    float phase = _Time.y * windFreq;
    float2 windDir = float2(sin(phase), cos(phase * 0.8)) * 0.3;
    float2 offset = windDir * windAmp * verticalInfluence;
    
    // 波形偏移（同样加入世界位置变化）
    float wavePhase = _Time.y * windFreq * 1.5 + worldPosition.x * waveScale;
    wavePhase += worldHash.x * 3.14; // 添加世界位置随机偏移
    
    float wave = sin(wavePhase) * 0.08;
    offset.x += wave * verticalInfluence * (1.0 - stiffness);
    
    // 基于顶点色的绿色通道调整偏移量
    offset *= vertexColor.g * 1.2;
    
    // 新增：鼠标交互效果
    if (mouseIntensity > 0)
    {
        // 高强度晃动
        float mousePhase = _Time.y * mouseFrequency;
        float2 mouseOffset = float2(
            sin(mousePhase + worldPosition.x * 0.5) * 0.5 +
            sin(mousePhase * 2.3 + worldPosition.z * 0.3) * 0.3,
            0
        );
        
        // 添加衰减效果（距离树干越远，影响越大）
        float trunkDistance = 1.0 - saturate(distance(position.xz, float2(0, 0)) * 2.0);
        mouseOffset *= mouseIntensity * verticalInfluence * trunkDistance;
        
        // 叠加到原偏移
        offset += mouseOffset;
    }
    
    // 应用最终偏移
    outPosition = position + float3(offset.x, 0, 0);
}