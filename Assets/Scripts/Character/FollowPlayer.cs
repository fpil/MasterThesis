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
        if (targetPosition == null)
        {
            Debug.Log("No player follow target");
            return;
        }
        mainCamera.gameObject.transform.position = new Vector3(targetPosition.x ,0, targetPosition.z) + offset;
        mainCamera.gameObject.transform.rotation = targetRotation;
    }
   
}
