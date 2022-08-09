using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public Transform target;

    public Transform farBackground, middleBackground;
    void Start()
    {
        
    }

    
    void Update()
    {
        transform.position = new Vector3(target.position.x, transform.position.y, transform.position.z);
        
    }
}
