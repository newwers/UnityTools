using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace MyWorld
{
    public class AttributeTest : MonoBehaviour
    {
        [MinMax(0, 10)]
        public Vector2 pos;

        [Button("µ„ª˜≤‚ ‘")]
        public float speed;

        [Button("int≤‚ ‘")]
        public int aaa = 555;

        void Start()
        {

        }


        void Update()
        {

        }
    }
}
