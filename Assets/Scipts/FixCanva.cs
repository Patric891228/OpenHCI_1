using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixCanva : MonoBehaviour
{
    public Camera xrCamera;  
    public Vector3 offset = new Vector3(0.4f, 0.3f, 0.5f); // Offset from the camera


    void Start()
    {
        if(xrCamera != null)
        {
            transform.SetParent(xrCamera.transform);
            transform.localPosition = offset;
            transform.localRotation = Quaternion.identity;
        }
    }

    void LateUpdate()
    {
        transform.LookAt(xrCamera.transform);
        transform.Rotate(0, 180, 0);  
    }
}
