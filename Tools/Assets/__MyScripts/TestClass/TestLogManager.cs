using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class TestLogManager : MonoBehaviour {

	// Use this for initialization
	void Start () {
        LogManager.Log("test");
        LogManager.LogError("testError");

        Debug.LogError("testDebug");
    }


}
