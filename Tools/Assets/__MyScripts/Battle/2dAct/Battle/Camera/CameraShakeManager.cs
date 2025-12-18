using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class CameraShakeData
{
    public float force;
    public Vector3 velocity;
    public float priority;
    public float startTime;
    public float duration;
}

public class CameraShakeManager : BaseMonoSingleClass<CameraShakeManager>
{
    public CinemachineImpulseSource impulseSource;
    private List<CameraShakeData> activeShakes = new List<CameraShakeData>();
    private CameraShakeData currentShake;

    protected override void Awake()
    {
        base.Awake();

        if (impulseSource == null)
        {
            LogManager.LogError("[CameraShakeManager] CinemachineImpulseSource组件缺失!");
        }
    }

    private void Update()
    {
        float currentTime = Time.unscaledTime;

        for (int i = activeShakes.Count - 1; i >= 0; i--)
        {
            if (currentTime - activeShakes[i].startTime >= activeShakes[i].duration)
            {
                activeShakes.RemoveAt(i);
            }
        }

        if (currentShake != null && currentTime - currentShake.startTime >= currentShake.duration)
        {
            currentShake = null;
        }
    }

    public void TriggerCameraShake(float force, Vector3 velocity, float priority = 0f, float duration = 0.2f)
    {
        if (impulseSource == null)
        {
            LogManager.LogWarning("[CameraShakeManager] CinemachineImpulseSource未初始化，无法触发镜头抖动");
            return;
        }

        CameraShakeData newShake = new CameraShakeData
        {
            force = force,
            velocity = velocity,
            priority = priority,
            startTime = Time.unscaledTime,
            duration = duration
        };

        if (currentShake == null || priority >= currentShake.priority)
        {
            if (currentShake != null && priority > currentShake.priority)
            {
                LogManager.Log($"[CameraShakeManager] 高优先级镜头抖动打断当前抖动 - 新优先级: {priority}, 旧优先级: {currentShake.priority}");
            }
            else if (currentShake != null && priority == currentShake.priority)
            {
                LogManager.Log($"[CameraShakeManager] 相同优先级镜头抖动覆盖当前抖动 - 优先级: {priority}");
            }

            currentShake = newShake;
            activeShakes.Add(newShake);

            impulseSource.ImpulseDefinition.ImpulseDuration = duration;
            impulseSource.GenerateImpulseAtPositionWithVelocity(transform.position, velocity * force);
            LogManager.Log($"[CameraShakeManager] 触发镜头抖动 - 力度: {force}, 方向: {velocity}, 优先级: {priority}");
        }
        else
        {
            LogManager.Log($"[CameraShakeManager] 镜头抖动请求被忽略 - 当前优先级: {currentShake.priority}, 请求优先级: {priority}");
        }
    }

    public void TriggerCameraShake(float force, float priority = 0f, float duration = 0.2f)
    {
        if (impulseSource == null)
        {
            LogManager.LogWarning("[CameraShakeManager] CinemachineImpulseSource未初始化，无法触发镜头抖动");
            return;
        }

        Vector3 defaultVelocity = impulseSource.DefaultVelocity;

        CameraShakeData newShake = new CameraShakeData
        {
            force = force,
            velocity = defaultVelocity,
            priority = priority,
            startTime = Time.unscaledTime,
            duration = duration
        };

        if (currentShake == null || priority >= currentShake.priority)
        {
            if (currentShake != null && priority > currentShake.priority)
            {
                LogManager.Log($"[CameraShakeManager] 高优先级镜头抖动打断当前抖动 - 新优先级: {priority}, 旧优先级: {currentShake.priority}");
            }
            else if (currentShake != null && priority == currentShake.priority)
            {
                LogManager.Log($"[CameraShakeManager] 相同优先级镜头抖动覆盖当前抖动 - 优先级: {priority}");
            }

            currentShake = newShake;
            activeShakes.Add(newShake);

            impulseSource.ImpulseDefinition.ImpulseDuration = duration;
            impulseSource.GenerateImpulseWithForce(force);
            LogManager.Log($"[CameraShakeManager] 触发镜头抖动 - 力度: {force}, 优先级: {priority}");
        }
        else
        {
            LogManager.Log($"[CameraShakeManager] 镜头抖动请求被忽略 - 当前优先级: {currentShake.priority}, 请求优先级: {priority}");
        }
    }

    public int GetActiveShakeCount()
    {
        return activeShakes.Count;
    }

    public CameraShakeData GetCurrentShake()
    {
        return currentShake;
    }

}
