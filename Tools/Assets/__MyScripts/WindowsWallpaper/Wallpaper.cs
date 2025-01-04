using System;
using TMPro;
using UnityEngine;

//Starting Here
public class Wallpaper : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI windowMouse;
    private IntPtr _workerW;
    private IntPtr _unityHandle;
    private bool _pressed = false;
    private bool _state = true;



    void Start()
    {

        _workerW = IntPtr.Zero;

        //Setup everything for Wallpaper
        InitializeWindowParenting();
        //gey unity handle
        _unityHandle = Win32InputWrapper.FindUnityWindow();

#if !UNITY_EDITOR
        Win32Wrapper.SetParent(_unityHandle, _workerW);
        Screen.SetResolution(1920, 1080, FullScreenMode.FullScreenWindow);
#endif

    }

    private void Update()
    {
        //Get Mouse position because when unity run in background-wallpaper 
        //It cannot be passed through any input functions.
        //Vector3 mPos = Win32InputWrapper.GetMousePosVector3();
        Vector2 mPos = Win32InputWrapper.GetMousePosVector2();
        windowMouse.SetText("MousePos " + "X : " + mPos.x + " | Y : " + mPos.y);

        //Check for button presses for the same reason as the above method. 
        //We are unable to use unity input reception. 
        //Any key values ​​can be found in win32keycode. 
        //To add additional keys, see: microsoft-virtual-key codes
        if (Win32InputWrapper.GetKeyDown(win32keycode.A) && Win32InputWrapper.GetKeyDown(win32keycode.CTRL))
        {
            if (!_pressed)
            {
                ToggleWallPaper();
                _pressed = true;
            }
        }
        else if (_pressed)
        {
            _pressed = false;
        }

    }

    //Setup WorkerW
    private void InitializeWindowParenting()
    {
        IntPtr program = Win32Wrapper.FindWindow("Progman", null);

        IntPtr result = IntPtr.Zero;

        Win32Wrapper.SendMessageTimeout(program, 0x052C, new IntPtr(0), IntPtr.Zero, Win32Wrapper.SendMessageTimeoutFlags.SMTO_NORMAL, 1000, out result);

        Win32Wrapper.EnumWindows(new Win32Wrapper.EnumWindowsProc((tophandle, topparamhandle) =>
        {
            IntPtr p = Win32Wrapper.FindWindowEx(tophandle, IntPtr.Zero, "SHELLDLL_DefView", IntPtr.Zero);
            if (p != IntPtr.Zero)
            {
                _workerW = Win32Wrapper.FindWindowEx(IntPtr.Zero, tophandle, "WorkerW", IntPtr.Zero);
            }
            return true;
        }), IntPtr.Zero);

    }

    public void ToggleWallPaper(bool toggle)
    {
        if (toggle)
        {
            Win32Wrapper.SetParent(_unityHandle, _workerW);
            Screen.SetResolution(1920, 1080, FullScreenMode.FullScreenWindow);
            _state = true;
        }
        else
        {

            Win32Wrapper.SetParent(_unityHandle, IntPtr.Zero);
            Screen.SetResolution(720, 480, FullScreenMode.Windowed);
            _state = false;
        }
    }

    //Toggle Wallpaper on/off 
    private void ToggleWallPaper()
    {
        if (_state)
        {
            //This will disable wallpaper mode
            Win32Wrapper.SetParent(_unityHandle, IntPtr.Zero);
            Screen.SetResolution(720, 480, FullScreenMode.Windowed);
            _state = false;
            return;
        }
        else
        {
            //This one will enable wallpaper mode
            Win32Wrapper.SetParent(_unityHandle, _workerW);
            Screen.SetResolution(1920, 1080, FullScreenMode.FullScreenWindow);
            _state = true;
        }
    }
}
