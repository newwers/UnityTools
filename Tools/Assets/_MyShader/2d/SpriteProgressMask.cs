/*
 用来代码设置SpriteRenderer的材质属性块，自动计算精灵的UV偏移和尺寸,传入shade参数
 */

using UnityEngine;


#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode] // 允许在编辑模式下运行
[RequireComponent(typeof(SpriteRenderer))] // 确保有SpriteRenderer组件
public class SpriteShaderMaskHelper : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;
    /// <summary>
    /// 用于临时修改材质属性而不创建新的材质实例。
    /// 这一点很重要，因为直接修改材质会影响所有使用该材质的对象，而 MaterialPropertyBlock 可以避免这种情况，同时节省内存
    /// </summary>
    private MaterialPropertyBlock _materialPropertyBlock;

    // 缓存上一次的精灵引用和材质，用于检测变化
    private Sprite _lastSprite;
    private Material _lastMaterial;

    void OnEnable()
    {
        // 获取SpriteRenderer组件
        _spriteRenderer = GetComponent<SpriteRenderer>();

        // 初始化材质属性块
        _materialPropertyBlock = new MaterialPropertyBlock();

        // 初始更新一次
        UpdateShaderProperties();
    }

    void Update()
    {
        // 在编辑器模式下持续检查变化
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            // 检查精灵或材质是否变化
            if (_spriteRenderer.sprite != _lastSprite ||
                (_spriteRenderer.sharedMaterial != null &&
                 _spriteRenderer.sharedMaterial != _lastMaterial))
            {
                UpdateShaderProperties();
            }
        }
#endif
    }

    public void SetProgress(float progress)
    {
        if (_spriteRenderer == null)
        {
            Debug.LogWarning("SpriteRenderer is missing", this);
            return;
        }
        if (_materialPropertyBlock == null)
        {
            _materialPropertyBlock = new MaterialPropertyBlock();
        }
        _materialPropertyBlock.Clear();
        _spriteRenderer.GetPropertyBlock(_materialPropertyBlock);
        if (_materialPropertyBlock != null)
        {
            _materialPropertyBlock.SetFloat("_MaskProgress", Mathf.Clamp01(progress));
            _spriteRenderer.SetPropertyBlock(_materialPropertyBlock);
        }
    }

    // 更新着色器属性
    void UpdateShaderProperties()
    {
        if (_spriteRenderer == null || _spriteRenderer.sprite == null)
        {
            Debug.LogWarning("SpriteRenderer or Sprite is missing", this);
            return;
        }

        // 获取当前精灵和纹理
        Sprite sprite = _spriteRenderer.sprite;
        Texture2D texture = sprite.texture;

        if (texture == null)
        {
            Debug.LogError("Sprite texture is missing", this);
            return;
        }

        // 计算UV空间的偏移量和尺寸
        Rect spriteRect = sprite.rect;

        Vector2 offset = new Vector2(
            spriteRect.x / texture.width,
            spriteRect.y / texture.height
        );

        Vector2 size = new Vector2(
            spriteRect.width / texture.width,
            spriteRect.height / texture.height
        );

        // 应用材质属性块
        _materialPropertyBlock.Clear();
        _spriteRenderer.GetPropertyBlock(_materialPropertyBlock);
        _materialPropertyBlock.SetVector("_SpriteOffset", offset);
        _materialPropertyBlock.SetVector("_SpriteSize", size);
        _spriteRenderer.SetPropertyBlock(_materialPropertyBlock);

        // 更新缓存
        _lastSprite = sprite;
        _lastMaterial = _spriteRenderer.sharedMaterial;

#if UNITY_EDITOR
        // 标记场景为已修改
        if (!Application.isPlaying)
        {
            EditorUtility.SetDirty(this);
        }
#endif
    }

#if UNITY_EDITOR
    // 编辑器菜单项 - 用于为所有精灵设置属性
    [MenuItem("GameObject/2D Object/Sprite Shader Mask Helper", false, 10)]
    public static void CreateSpriteMaskHelper()
    {
        if (Selection.activeGameObject != null)
        {
            var helper = Selection.activeGameObject.GetComponent<SpriteShaderMaskHelper>();
            if (helper == null)
            {
                helper = Selection.activeGameObject.AddComponent<SpriteShaderMaskHelper>();
                EditorUtility.SetDirty(helper);
            }
        }
        else
        {
            Debug.LogWarning("Please select a GameObject with a SpriteRenderer first.");
        }
    }

    // 验证菜单项是否可用
    [MenuItem("GameObject/2D Object/Sprite Shader Mask Helper", true)]
    public static bool ValidateCreateSpriteMaskHelper()
    {
        return Selection.activeGameObject != null &&
               Selection.activeGameObject.GetComponent<SpriteRenderer>() != null;
    }
#endif
}