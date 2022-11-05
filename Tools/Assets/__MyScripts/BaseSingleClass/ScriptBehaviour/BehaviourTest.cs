using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BehaviourTest : MonoBehaviour
{
    private void Awake()
    {
        print("Awake");
    }
    private void OnEnable()
    {
        print("OnEnable");
    }
    private void Start()
    {
        print("Start");
        HashCodeTest script = new HashCodeTest();
        int hash1 = script.GetHashCode();//608222976
        int hash2 = typeof(HashCodeTest).GetHashCode();//-440405496
        int hash3 = script.GetType().GetHashCode(); ;//-440405496
        HashCodeTest script2 = new HashCodeTest();
        int hash4 = script2.GetHashCode();//970243840
        int hash5 = typeof(HashCodeTest).GetHashCode();//-440405496
        int hash6 = script2.GetType().GetHashCode(); ;//-440405496

        print("script.GetHashCode() = " + hash1);
        print("typeof(HashCodeTest).GetHashCode() = " + hash2);
        print("script.GetType().GetHashCode() = " + hash3);

        print("script2.GetHashCode() = " + hash4);
        print("typeof(HashCodeTest).GetHashCode() = " + hash5);
        print("script2.GetType().GetHashCode() = " + hash6);
    }
    private void OnDisable()
    {
        print("OnDisable");
    }
    private void OnDestroy()
    {
        print("OnDestroy");
    }
}

public class HashCodeTest
{

}
