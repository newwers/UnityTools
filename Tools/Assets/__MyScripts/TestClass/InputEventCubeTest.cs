using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputEventCubeTest : MonoBehaviour
{
    public InputModule.InputEvent input;
    public GameObject cube;
    private void Awake()
    {
        input.OnClick += OnClick;
        input.OnSlideLeft += OnSlideLeft;
        input.OnSlideRight += OnSlideRight;
        input.OnRotationBegin += OnRotationBegin;
        input.OnRotation += OnRotation;
        input.OnRotationEnd += OnRotationEnd;
    }

    private void OnRotationEnd()
    {
        
    }

    private void OnRotation(float delta)
    {
        if (cube == null)
        {
            return;
        }
        cube.transform.Rotate(new Vector3(0, -delta, 0));
    }

    private void OnRotationBegin()
    {
        
    }

    private void OnSlideRight()
    {
        if (cube == null)
        {
            return;
        }
        cube.transform.localScale -= new Vector3(0.1f, 0.1f, 0.1f);
    }

    private void OnSlideLeft()
    {
        if (cube == null)
        {
            return;
        }
        cube.transform.localScale += new Vector3(0.1f, 0.1f, 0.1f);
    }

    private void OnClick()
    {
        MeshRenderer r = cube.GetComponent<MeshRenderer>();
        if (r)
        {
            r.material.color = new Color(Random.Range(0.5f, 1), Random.Range(0.5f, 1), Random.Range(0.5f, 1));
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
