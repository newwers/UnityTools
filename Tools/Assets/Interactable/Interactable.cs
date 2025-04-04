using UnityEngine;

public class Interactable : MonoBehaviour
{
    // 交互方法，可在子类中重写
    public virtual void Interact()
    {
        Debug.Log("交互: " + gameObject.name);
    }
}
