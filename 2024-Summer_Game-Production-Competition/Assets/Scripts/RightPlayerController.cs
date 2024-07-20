using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RightPlayerController : MonoBehaviour
{
    private Rigidbody rigidbody;
    
    [Header("2P 플레이어 움직임")]
    public float speed = 8f;
    public float rotationSpeed = 720f; // 초당 회전 속도 (도 단위)      

    [Header("접시")]
    public float maxPickupDistance = 2f; // 접시를 줍기 위한 최대 거리
    public bool pickupActivated = false; // 플레이어가 무언가를 들고 있는지 여부
    public Transform playerTransform; // 플레이어의 Transform
    public Transform holdPosition; // 플레이어의 자식 오브젝트로, 접시를 들고 있는 위치
    public List<Transform> plates; // 접시들의 Transform 리스트
    private Transform heldObject = null; // 현재 들고 있는 접시
    
    [Header("싱크대")] 
    public List<Transform> snik; // 싱크대 Transform 리스트
    public List<Transform> snikPosition; // 싱크대 위치 Transform 리스트
    private bool isWashing = false;
    private float washStartTime;
    public float washDuration = 2f; // 설거지에 걸리는 시간 (초)
    
    [Header("찬장")]
    public List<Transform> cabinetPositions; // 찬장의 위치 Transform 리스트

    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    void Update()
    {
        MoveLogic();

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Pickup();
            PutPlateInCabinet();
        }

        if (Input.GetKey(KeyCode.Alpha2))
        {
            if (!isWashing)
            {
                StartWashing();
            }
            else
            {
                ContinueWashing();
            }
        }

        if (Input.GetKeyUp(KeyCode.Alpha2))
        {
            StopWashing();
        }
    }

    void MoveLogic()
    {
        float xInput = Input.GetAxisRaw("RightHorizontal");
        float zInput = Input.GetAxisRaw("RightVertical");

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
        if (!pickupActivated) // 플레이어가 무언가를 들고 있지 않다면
        {
            Transform nearestPlate = null;
            float minDistance = float.MaxValue;

            // 모든 접시들과의 거리 계산
            foreach (Transform plateTransform in plates)
            {
                float distance = Vector3.Distance(playerTransform.position, plateTransform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestPlate = plateTransform;
                }
            }

            if (nearestPlate != null && minDistance <= maxPickupDistance) // 가장 가까운 접시가 충분히 가깝다면
            {
                pickupActivated = true; // 접시를 손에 든다
                heldObject = nearestPlate;
                heldObject.position = holdPosition.position; // 접시의 위치를 holdPosition으로 이동
                heldObject.SetParent(holdPosition); // 접시를 holdPosition의 자식으로 설정
                Debug.Log("접시를 획득합니다.");
            }
        }
        else // 플레이어가 무언가를 들고 있으면
        {
            Debug.Log("이미 접시를 들고 있습니다.");
        }
    }

    void StartWashing()
    {
        if (heldObject != null)
        {
            Plate heldPlate = heldObject.GetComponent<Plate>();

            if (heldPlate != null && heldPlate.plateType == Plate.PlateType.Dirty)
            {
                foreach (Transform snikTransform in snik)
                {
                    float distanceToSink = Vector3.Distance(playerTransform.position, snikTransform.position);
                    if (distanceToSink <= maxPickupDistance)
                    {
                        isWashing = true;
                        washStartTime = Time.time;
                        Debug.Log("설거지 시작");
                        return;
                    }
                }
                Debug.Log("싱크대와 너무 멀리 떨어져 있습니다.");
            }
            else if (heldPlate != null && heldPlate.plateType == Plate.PlateType.Clean)
            {
                Debug.Log("이미 깨끗한 접시입니다.");
            }
        }
        else
        {
            Debug.Log("플레이어가 접시를 들고 있지 않습니다.");
        }
    }

    void ContinueWashing()
    {
        if (isWashing)
        {
            float elapsed = Time.time - washStartTime;
            if (elapsed >= washDuration)
            {
                Plate heldPlate = heldObject.GetComponent<Plate>();
                heldPlate.plateType = Plate.PlateType.Clean;
                isWashing = false;
                Debug.Log("접시를 설거지했습니다.");
            }
            else
            {
                Debug.Log("설거지 진행 중... (" + elapsed + "/" + washDuration + "초)");
            }
        }
    }

    void StopWashing()
    {
        if (isWashing)
        {
            isWashing = false;
            Debug.Log("설거지를 중단했습니다.");
        }
    }
    
    void PutPlateInCabinet()
    {
        if (heldObject != null) // 플레이어가 무언가를 들고 있는 경우
        {
            Plate heldPlate = heldObject.GetComponent<Plate>();

            if (heldPlate != null && heldPlate.plateType == Plate.PlateType.Clean) // 들고 있는 접시가 깨끗한 경우
            {
                foreach (Transform cabinetPosition in cabinetPositions)
                {
                    float distanceToCabinet = Vector3.Distance(playerTransform.position, cabinetPosition.position);

                    if (distanceToCabinet <= maxPickupDistance) // 플레이어와 찬장의 거리가 충분히 가까운 경우
                    {
                        if (cabinetPosition.childCount == 0) // 찬장의 위치가 비어 있는 경우
                        {
                            // 접시를 찬장에 내려놓는다
                            heldObject.SetParent(cabinetPosition);
                            heldObject.position = cabinetPosition.position;
                            heldObject.rotation = cabinetPosition.rotation; // 접시의 회전도 찬장의 포지션 회전으로 설정
                            pickupActivated = false;
                            heldObject = null;
                            Debug.Log("접시를 찬장에 내려놓았습니다.");
                            return;
                        }
                    }
                }
                Debug.Log("찬장의 모든 포지션이 이미 사용 중입니다.");
            }
            else
            {
                Debug.Log("깨끗한 접시가 아닙니다. 찬장에 내려놓을 수 없습니다.");
            }
        }
        else
        {
            Debug.Log("플레이어가 접시를 들고 있지 않습니다.");
        }
    }
}
