using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace zdq.Zombie
{
    /// <summary>
    /// 基础控制
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        public Transform MoveTarget;
        public Transform RotateTarget;

        public float MoveSpeed = 10f;
        public float RotateSpeed = 5f;

        


        Vector3 m_MouseInput;
        Vector3 m_MoveDir;
        bool m_IsUpDown;


        private void Awake()
        {
            if (MoveTarget == null)
            {
                MoveTarget = transform;
            }
            if (RotateTarget == null)
            {
                RotateTarget = transform;
            }
        }

        private void Update()
        {
            m_MouseInput.x = Input.GetAxis("Mouse X");
            m_MouseInput.y = Input.GetAxis("Mouse Y");
            m_MouseInput.z = 0;
            m_MoveDir.x = Input.GetAxis("Horizontal");
            m_MoveDir.z = Input.GetAxis("Vertical");
            m_IsUpDown = Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.E);
            m_MoveDir.y = m_IsUpDown? (Input.GetKey(KeyCode.Q) ? -1 :Input.GetKey(KeyCode.E) ? 1 : 0): 0;

            MoveTarget.position += MoveTarget.forward * m_MoveDir.z * Time.deltaTime * MoveSpeed;//朝着当前方向前后
            MoveTarget.position += MoveTarget.right * m_MoveDir.x * Time.deltaTime * MoveSpeed;//当前方向左右
            MoveTarget.position += MoveTarget.up * m_MoveDir.y * Time.deltaTime * MoveSpeed;//当前方向上下

            RotateTarget.Rotate(Vector3.up, m_MouseInput.x * RotateSpeed * Time.deltaTime);//左右旋转
            RotateTarget.Rotate(Vector3.right, -m_MouseInput.y * RotateSpeed * Time.deltaTime);//上下旋转
            RotateTarget.rotation = Quaternion.Euler(RotateTarget.rotation.eulerAngles.x, RotateTarget.rotation.eulerAngles.y,0);//锁定z轴
        }


    }
}
