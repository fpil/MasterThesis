using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public Camera mainCamera;

    private float camSpeed = 255f;
    private float xRotation; 
    private float YRotation;
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false; 
    }

    void Update()
    { 
        mainCamera = Camera.main;
        float mouseX = Input.GetAxis("Mouse X") * Time.deltaTime * camSpeed;  
        float mouseY = Input.GetAxis("Mouse Y") * Time.deltaTime * camSpeed;  
        
        xRotation -= mouseY;
        //Clamp x rotation
        xRotation = Mathf.Clamp(xRotation, -80f, 80f); 
        YRotation += mouseX;
        
        // camera.transform.position = new Vector3(targetPosition.x ,targetPosition.y, targetPosition.z) + offset;
        mainCamera.gameObject.transform.rotation = Quaternion.Euler(xRotation, YRotation, 0);
    }
}
