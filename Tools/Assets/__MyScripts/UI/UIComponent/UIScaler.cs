/*
在 Unity 的 Canvas Scaler 中选择 Constant Pixel Size 模式时，UI 元素的大小会以固定像素值显示（即 UI 的像素尺寸与设置的数值严格一致），不会随屏幕分辨率 / 尺寸自动缩放
 unity canvas scaler中UI scale mode 设置为Constant pixel size 并且以1920*1080为基础
 通过脚本动态计算缩放因子，确保UI在不同分辨率下保持一致的视觉效果
 */

using UnityEngine;
using UnityEngine.UI;
[ExecuteInEditMode]
[RequireComponent(typeof(CanvasScaler))]
public class UIScaler : MonoBehaviour
{
    // 基础设计分辨率（参考分辨率）
    private const float ReferenceWidth = 1920f;
    private const float ReferenceHeight = 1080f;

    private CanvasScaler canvasScaler;

    void Start()
    {
        canvasScaler = GetComponent<CanvasScaler>();
        UpdateScaleFactor();
    }


    private void UpdateScaleFactor()
    {
        // 计算当前屏幕与参考分辨率的比例因子
        float widthRatio = Screen.width / ReferenceWidth;
        float heightRatio = Screen.height / ReferenceHeight;


        // 取较小值，确保UI在所有分辨率下都能完整显示
        float scaleFactor = Mathf.Min(widthRatio, heightRatio);

        // 设置CanvasScaler的缩放因子
        if (canvasScaler)
        {
            canvasScaler.scaleFactor = scaleFactor;
            LogManager.Log($"Screen Width: {Screen.width}, Screen Height: {Screen.height},widthRatio:{widthRatio},heightRatio:{heightRatio},scaleFactor:{scaleFactor}");
        }
    }
    // 可选：屏幕分辨率变化时重新计算（例如窗口缩放、横屏/竖屏切换）
    void OnRectTransformDimensionsChange()
    {
        UpdateScaleFactor();
    }
}