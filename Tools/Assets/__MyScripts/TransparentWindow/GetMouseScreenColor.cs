using System.Collections;
using UnityEngine;
// 必须挂载在Camera对象上
[RequireComponent(typeof(Camera))]
public class GetMouseScreenColor : MonoBehaviour
{
    // 存储当前鼠标位置的颜色
    private Color currentMouseColor;
    // 是否显示调试信息
    public bool showDebugInfo = true;
    public UnityEngine.UI.Image colorPreviewImage; // 可选：UI Image组件，用于显示颜色预览

    // 颜色读取的时间间隔（秒），可在Inspector面板调整
    [Header("读取间隔设置")]
    [Tooltip("两次读取颜色的最小时间间隔（秒）")]
    public float colorReadInterval = 0.1f;
    // 临时存储鼠标位置（用于在渲染阶段使用）
    private Vector2 tempMousePos;


    private void Start()
    {
        StartCoroutine(GetScreenColorCoroutine());
    }

    /// <summary>
    /// 用协程确保ReadPixels在正确的渲染帧时机调用
    /// </summary>
    /// <returns></returns>
    private IEnumerator GetScreenColorCoroutine()
    {
        while (true)
        {
            // 关键：等待帧结束，让Unity完成当前帧的渲染，确保帧缓冲区可读
            yield return new WaitForEndOfFrame();

            Color color = GetScreenColorAtMousePosition();
            currentMouseColor = color;
            if (colorPreviewImage)
            {
                colorPreviewImage.color = color;
            }
            Debug.Log($"鼠标位置颜色: R={color.r:F2}, G={color.g:F2}, B={color.b:F2}, A={color.a:F2}");

            yield return new WaitForSeconds(colorReadInterval);
        }
    }

    /// <summary>
    /// 核心取色逻辑（仅在WaitForEndOfFrame后调用）
    /// </summary>
    /// <returns></returns>
    private Color GetScreenColorAtMousePosition()
    {
        Vector2 mousePosition = Input.mousePosition;

        // 边界检查
        if (mousePosition.x < 0 || mousePosition.x >= Screen.width ||
            mousePosition.y < 0 || mousePosition.y >= Screen.height)
        {
            Debug.LogWarning("鼠标超出屏幕范围，无法获取颜色");
            return Color.clear;
        }

        Texture2D tempTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        try
        {
            // 此时调用ReadPixels已处于安全的渲染帧阶段
            tempTexture.ReadPixels(new Rect(mousePosition.x, mousePosition.y, 1, 1), 0, 0);
            tempTexture.Apply();
            return tempTexture.GetPixel(0, 0);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"读取屏幕颜色失败: {e.Message}");
            return Color.clear;
        }
        finally
        {
            Destroy(tempTexture);
        }
    }
}