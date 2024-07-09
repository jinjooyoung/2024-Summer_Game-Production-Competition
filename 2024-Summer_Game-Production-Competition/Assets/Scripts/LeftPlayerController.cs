using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeftPlayerController : MonoBehaviour
{
    private Rigidbody rigidbody;
    
    [Header("1P 플레이어 움직임")]
    public float speed = 8f;        

    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    void Update()
    {        
        float xInput = Input.GetAxisRaw("LeftHorizontal");
        float zInput = Input.GetAxisRaw("LeftVertical");
        
        float xSpeed = xInput * speed;
        float zSpeed = zInput * speed;
        
        Vector3 newVelocity = new Vector3 (xSpeed, 0, zSpeed);
        
        rigidbody.velocity = newVelocity;
    }
}