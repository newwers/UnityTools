using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonoManagerTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        TestMonoClass testMonoClass = new TestMonoClass();
        MonoManager.Instance.AddUpdateEvent(testMonoClass.TestUpdate);
        MonoManager.Instance.StartCoroutine(testMonoClass.TestIEnumerator());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

public class TestMonoClass
{
    public void TestUpdate()
    {
        Debug.Log("测试Update");
    }

    public IEnumerator TestIEnumerator()
    {
        Debug.Log("TestIEnumerator1");
        yield return new WaitForSeconds(1f);
        Debug.Log("TestIEnumerator2");
    }
}
