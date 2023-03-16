using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    
    public Vector3 offset;
    Vector3 targetPosition;
    Quaternion targetRotation;

    public Camera mainCamera;

    public static FollowPlayer Instance;
    private float xRotation; 
    private float YRotation;
    private float camSpeed = 255f; 

    private void Awake()
    {
        if (Instance != null)
        {
            DestroyImmediate(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {
        mainCamera = Camera.main;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void UpdateTargetPosition(float3 position)
    {
        targetPosition = position;
    }
    public void UpdateTargetRotation(Quaternion rotation)
    {
        targetRotation = rotation;
    }

    private void LateUpdate()
    {
        //Rotate camera 
        float mouseY = Input.GetAxis("Mouse Y") * Time.deltaTime * camSpeed;
        float mouseX = Input.GetAxis("Mouse X") * Time.deltaTime * camSpeed;
        xRotation -= mouseY;
        //Clamp x rotation
        xRotation = Mathf.Clamp(xRotation, -80f, 80f); 
        YRotation += mouseX;

        
        if (targetPosition == null)
        {
            Debug.Log("No player follow target");
            return;
        }

        //Update camera position and rotation 
        var camera = mainCamera.gameObject;
        camera.transform.position = new Vector3(targetPosition.x ,targetPosition.y, targetPosition.z) + offset;
        camera.gameObject.transform.rotation = Quaternion.Euler(xRotation, YRotation, 0);
    }
}
