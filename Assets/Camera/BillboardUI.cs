using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillboardUI : MonoBehaviour
{
    private Camera _Camera;

    public void Start()
    {
        _Camera = GameObject.FindGameObjectWithTag("MainCamera")
        .GetComponent<Camera>();
    }

    // Update is called once per frame
    public void Update()
    {
        Vector3 dir = _Camera.transform.position - transform.position;
        transform.rotation = Quaternion.LookRotation(dir);
    }
}
