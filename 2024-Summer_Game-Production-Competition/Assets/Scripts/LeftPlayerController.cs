using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeftPlayerController : MonoBehaviour
{
    private Rigidbody rigidbody;
    
    [Header("1P 플레이어 움직임")]
    public float speed = 8f;
    public float rotationSpeed = 720f; // 초당 회전 속도 (도 단위)

    [Header("재료 획득 관련")]
    public float maxPickupDistance = 2f; // 재료를 줍기 위한 최대 거리
    public bool pickupActivated = false; // 플레이어가 무언가를 들고 있는지 여부
    public Transform playerTransform; // 플레이어의 Transform
    public Transform holdPosition; // 플레이어의 자식 오브젝트로, 재료를 들고 있는 위치
    public List<Transform> boxContents; // 상자 안의 재료들의 Transform 리스트
    private Transform heldObject = null; // 현재 들고 있는 재료

    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    void Update()
    {
        MoveLogic();
        Pickup();
    }

    void MoveLogic()
    {
        float xInput = Input.GetAxisRaw("LeftHorizontal");
        float zInput = Input.GetAxisRaw("LeftVertical");

        Vector3 inputDirection = new Vector3(xInput, 0, zInput).normalized;

        if (inputDirection.magnitude >= 0.1f)
        {
            // 이동 속도 설정
            Vector3 moveVelocity = inputDirection * speed;
            rigidbody.velocity = moveVelocity;

            // 목표 회전 계산
            Quaternion targetRotation = Quaternion.LookRotation(inputDirection, Vector3.up);

            // 플레이어 회전
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        else
        {
            // 이동하지 않을 때 속도를 0으로 설정
            rigidbody.velocity = Vector3.zero;
        }
    }

    void Pickup()
    {
        if (Input.GetKeyDown(KeyCode.B)) // B 버튼을 누르면
        {
            if (!pickupActivated) // 플레이어가 무언가를 들고 있지 않다면
            {
                Transform nearestItem = null;
                float minDistance = float.MaxValue;

                // 모든 재료들과의 거리 계산
                foreach (Transform itemTransform in boxContents)
                {
                    float distance = Vector3.Distance(playerTransform.position, itemTransform.position);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        nearestItem = itemTransform;
                    }
                }

                if (nearestItem != null && minDistance <= maxPickupDistance) // 가장 가까운 재료가 충분히 가깝다면
                {
                    pickupActivated = true; // 재료를 손에 든다
                    heldObject = nearestItem;
                    heldObject.position = holdPosition.position; // 재료의 위치를 holdPosition으로 이동
                    heldObject.SetParent(holdPosition); // 재료를 holdPosition의 자식으로 설정
                    Debug.Log("재료랑 가깝고 아무것도 들고있지 않아서 재료를 획득합니다");
                }
            }
            else // 플레이어가 무언가를 들고 있으면
            {
                // 추가 동작 (예: 재료를 내려놓는 기능)
                Debug.Log("이미 재료를 들고 있습니다.");
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Stuff")
        {
            Debug.Log("재료찾음!");
        }
    }
}
