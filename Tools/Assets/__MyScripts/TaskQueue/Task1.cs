using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;

public class Task1 : MonoBehaviour
{

    public GameObject cube;

    private GameObject m_Cube;

    // Start is called before the first frame update
    void OnEnable()
    {
        var queueManager_Instance = QueueManager.Instance;

        QueueTask queueTask1 = new QueueTask(CreateCube,null);
        queueManager_Instance.m_QueueDic.Add(queueTask1);//添加一个任务进去

        QueueTask queueTask2 = new QueueTask(RotateCube,null);
        queueManager_Instance.m_QueueDic.Add(queueTask2);//添加一个任务进去


        queueManager_Instance.StartQueue();//启动队列任务
    }

    void CreateCube(QueueTask queueTask)
    {
        StartCoroutine(IECreateCube(queueTask));        
    }

    IEnumerator IECreateCube(QueueTask queueTask)
    {
        yield return new WaitForSeconds(2f);
        m_Cube = Instantiate(cube) as GameObject;
        yield return new WaitForSeconds(1f);
        queueTask.OnComplete();
    }

    private void RotateCube(QueueTask queueTask)
    {
        m_Cube.transform.Rotate(new Vector3(0, 45, 0));
        queueTask.OnComplete();
    }



 
}
