using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace MyWorld
{
    public class LookAtCamera : MonoBehaviour
    {
        public Transform target;

        private void Start()
        {
            FindTarget();
        }

        void Update()
        {
            if (target)
            {
                transform.LookAt(target);
            }
            if (target == null)
            {
                FindTarget();
            }
        }

        public void FindTarget()
        {
            target = Camera.main.transform;
        }
    }
}
