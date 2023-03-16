using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    void Update()
    {
        var movement = new float3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        var camera = Camera.main.gameObject;
        // transform.rotation = quaternion.Euler(0,camera.transform.rotation.y,0);
        var playerObject = gameObject;
        Quaternion playerTransformRotation; 
        playerTransformRotation = camera.transform.rotation;
        playerTransformRotation.x = 0;
        playerTransformRotation.z = 0;
        playerObject.transform.rotation = playerTransformRotation;
        
        //Move the character towards its orientation
        float3 rotatedDirection = math.rotate(transform.rotation, movement);
        rotatedDirection.y = 0;
        transform.position +=
            new Vector3(rotatedDirection.x, rotatedDirection.y, rotatedDirection.z) * 5 * Time.deltaTime;
        // transform.position += rotatedDirection * 5 * Time.deltaTime;

    }
}
