using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using zdq.InputModule;

namespace zdq.Test
{

    public class JoystickTestMove : MonoBehaviour
    {
        public JoystickController controller;
        public float Speed;

        void Update()
        {
            if (controller && controller.eventData != null)
            {
                transform.Translate(new Vector3((controller.eventData.position - controller.eventData.pressPosition).x, 0, (controller.eventData.position - controller.eventData.pressPosition).y) * Time.deltaTime * Speed);
            }
        }
    }
}
