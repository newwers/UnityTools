using System.Collections.Generic;
using UnityEngine;

public class LaserLauncher : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public int maxReflectTimes = 5; // 最大反射次数
    public float maxDistance = 100f; // 激光最大长度
    public LayerMask reflectiveLayer; // 反射物体所在的层

    public List<ReflectInteractable> reflectInteractables; // 反射交互对象

    void Update()
    {
        //if (Input.GetMouseButtonDown(0))
        //{

        //}
        FireLaser();
    }

    void FireLaser()
    {
        Vector3 origin = lineRenderer.transform.position;
        Vector3 direction = (lineRenderer.transform.forward).normalized;


        // 初始化 LineRenderer
        lineRenderer.positionCount = 0;
        lineRenderer.useWorldSpace = true;

        RaycastHit hit;
        if (Physics.Raycast(origin, direction, out hit, maxDistance))
        {
            // 递归发射激光并处理反射
            ReflectLaser(origin, direction, 0);
        }
    }

    private void ReflectLaser(Vector3 origin, Vector3 direction, int reflectCount)
    {
        if (reflectCount >= maxReflectTimes) return; // 达到最大反射次数则停止

        RaycastHit hit;
        if (Physics.Raycast(origin, direction, out hit, maxDistance, reflectiveLayer))
        {
            //print("ray hit:" + hit.collider.name);
            if (hit.collider.CompareTag("RayTarget"))
            {
                // 如果碰撞到目标物体，则停止反射
                //print("ray hit:" + hit.collider.name);
                var ani = hit.collider.gameObject.GetComponent<Animator>();
                if (ani)
                {
                    ani.SetBool("IsOpen", true);
                }
                Game.Instance.OnHitRayTarget();
                gameObject.SetActive(false); // 激光发射器关闭
                for (int i = 0; i < reflectInteractables.Count; i++)
                {
                    reflectInteractables[i].EndRotate();
                }

                return;
            }



            Vector3 hitPoint = hit.point; // 碰撞点
            Vector3 normal = hit.normal; // 碰撞面的法线

            // 更新 LineRenderer 的路径
            lineRenderer.positionCount += 1;
            lineRenderer.SetPosition(lineRenderer.positionCount - 1, origin);
            lineRenderer.positionCount += 1;
            lineRenderer.SetPosition(lineRenderer.positionCount - 1, hitPoint);

            if (hit.collider.CompareTag("Wall"))//如果碰撞到墙壁，则停止反射
            {
                return;
            }

            // 计算反射方向
            Vector3 reflectedDirection = Vector3.Reflect(direction, normal);

            // 递归处理下一次反射
            ReflectLaser(hitPoint + reflectedDirection * 0.01f, reflectedDirection, reflectCount + 1);
        }
        else
        {
            // 如果没有碰撞，则将激光延伸到最大距离
            Vector3 endPoint = origin + direction * maxDistance;
            lineRenderer.positionCount += 1;
            lineRenderer.SetPosition(lineRenderer.positionCount - 1, origin);
            lineRenderer.positionCount += 1;
            lineRenderer.SetPosition(lineRenderer.positionCount - 1, endPoint);
        }
    }

}
