using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Notification.Subscribe(InputManager.Instance.InputKeyDown_signal, OnKeyDown);
        InputManager.Instance.AddKeyCodeDown(KeyCode.Q);
    }

    void OnKeyDown(object args)
    {
        KeyCode keyCode = (KeyCode)args;
        print("OnKeyDown:" + keyCode.ToString());
    }
}
