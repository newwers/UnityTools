using System;

namespace StateMachineSystem
{
    [Serializable]
    public enum StateType
    {
        Idle,
        Walk,
        Stay,
        Interact_Enter,
        Interact_Idle,
        Interact_Exit,
    }
}
