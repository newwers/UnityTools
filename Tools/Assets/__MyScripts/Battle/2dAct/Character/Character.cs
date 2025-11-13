using UnityEngine;

public class Character : MonoBehaviour
{
    private InputHandler inputHandler;
    private CharacterLogic logic;
    private CharacterAnimation ani;

    private void Awake()
    {
        inputHandler = GetComponent<InputHandler>();
        logic = GetComponent<CharacterLogic>();
        ani = GetComponent<CharacterAnimation>();
    }

    private void Update()
    {
        // 更新顺序：输入层 -> 逻辑层 -> 动画层
        // 每个层都有自己的Update方法，这里主要是协调作用
    }

    private void FixedUpdate()
    {
        // 物理更新主要在逻辑层处理
    }
}