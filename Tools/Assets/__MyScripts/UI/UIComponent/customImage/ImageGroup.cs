using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class ImageGroupEvent : UnityEvent<int> { }

public class ImageGroup : MonoBehaviour
{
    [System.Serializable]
    public class ImageInfo
    {
        public Image image; // Image 组件
        public Sprite selectedSprite; // 选中状态的图片
        public Sprite normalSprite; // 默认状态的图片
        public int index; // Image 索引
    }

    [Header("Image 设置")]
    public List<ImageInfo> images = new List<ImageInfo>();

    [Header("事件设置")]
    public ImageGroupEvent OnImageSelected = new ImageGroupEvent(); // 当 Image 被选中时触发的事件
    public UnityEvent OnSelectionChanged = new UnityEvent(); // 当选择状态改变时触发

    private int _currentSelectedIndex = -1; // 当前选中的 Image 索引
    private Dictionary<Image, ImageInfo> _imageInfoMap = new Dictionary<Image, ImageInfo>();

    private void Awake()
    {
        InitializeImages();
    }

    private void InitializeImages()
    {
        _imageInfoMap.Clear();

        // 为每个 Image 设置初始状态
        for (int i = 0; i < images.Count; i++)
        {
            var imageInfo = images[i];

            if (imageInfo.image == null)
            {
                Debug.LogWarning($"Image 组 '{name}' 中存在未分配的 Image 对象!");
                continue;
            }

            // 设置 Image 索引
            imageInfo.index = i;
            _imageInfoMap[imageInfo.image] = imageInfo;

            // 设置初始状态图片
            if (imageInfo.normalSprite != null)
            {
                imageInfo.image.sprite = imageInfo.normalSprite;
            }
        }

        // 如果没有初始选择，设置第一个为选中状态
        if (_currentSelectedIndex == -1 && images.Count > 0)
        {
            SetSelected(0);
        }
    }

    // 设置选中的 Image
    public void SetSelected(int index)
    {
        // 确保索引在有效范围内
        if (index < 0 || index >= images.Count)
        {
            Debug.LogWarning($"试图设置的 Image 索引 '{index}' 超出范围 (0-{images.Count - 1})");
            return;
        }

        // 更新所有 Image 状态
        for (int i = 0; i < images.Count; i++)
        {
            ImageInfo info = images[i];

            if (i == index)
            {
                // 设置选中图片
                if (info.selectedSprite != null)
                {
                    info.image.sprite = info.selectedSprite;
                }

                // 更新选中索引
                _currentSelectedIndex = i;
            }
            else
            {
                // 设置普通图片
                if (info.normalSprite != null)
                {
                    info.image.sprite = info.normalSprite;
                }
            }
        }

        // 触发事件
        OnImageSelected?.Invoke(index);
        OnSelectionChanged?.Invoke();
    }

    // 获取当前选中的 Image 索引
    public int GetSelectedIndex()
    {
        return _currentSelectedIndex;
    }

    // 添加新 Image 到组中
    public void AddImage(Image image, Sprite normalSprite, Sprite selectedSprite)
    {
        ImageInfo newImage = new ImageInfo
        {
            image = image,
            normalSprite = normalSprite,
            selectedSprite = selectedSprite,
            index = images.Count
        };

        images.Add(newImage);
        InitializeImages(); // 重新初始化
    }

    // 从组中移除 Image
    public void RemoveImage(Image image)
    {
        for (int i = 0; i < images.Count; i++)
        {
            if (images[i].image == image)
            {
                images.RemoveAt(i);
                InitializeImages(); // 重新初始化
                break;
            }
        }
    }

    // 清除所有 Image
    public void ClearImages()
    {
        images.Clear();
        InitializeImages();
    }

    // 编辑器方法 - 自动收集所有子对象的 Image
    [ContextMenu("收集子 Image")]
    public void CollectChildImages()
    {
        images.Clear();
        Image[] childImages = GetComponentsInChildren<Image>(true);

        foreach (Image img in childImages)
        {
            images.Add(new ImageInfo
            {
                image = img,
                index = images.Count,
                normalSprite = img.sprite, // 获取默认图片
            });
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(ImageGroup))]
public class ImageGroupEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ImageGroup imageGroup = (ImageGroup)target;

        if (GUILayout.Button("收集子 Image"))
        {
            imageGroup.CollectChildImages();
        }

        if (GUILayout.Button("清除所有 Image"))
        {
            imageGroup.ClearImages();
        }
    }
}
#endif // UNITY_EDITOR