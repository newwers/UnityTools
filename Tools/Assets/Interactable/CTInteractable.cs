using UnityEngine;

public class CTInteractable : Interactable
{
    private Animator animator; // 抽屉的动画控制器
    private bool isOpen = false; // 抽屉的打开状态

    private void Start()
    {
        // 获取抽屉的动画控制器组件
        animator = GetComponent<Animator>();
    }

    public override void Interact()
    {
        // 切换抽屉的打开状态
        isOpen = !isOpen;

        if (isOpen)
        {
            // 播放打开抽屉的动画
            animator.SetBool("IsOpen", true);
        }
        else
        {
            // 播放关闭抽屉的动画
            animator.SetBool("IsOpen", false);
        }
    }
}
