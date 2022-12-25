using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace MyWorld
{
    public class DontDestroyOnLoad : MonoBehaviour
    {
        
        void Start()
        {
            DontDestroyOnLoad(gameObject);
        }

    }
}
