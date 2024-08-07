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
    public List<Transform> platesPos; // 접시위에 재료를 올려놓을 위치 
    private Transform heldObject = null; // 현재 들고 있는 접시
    
    [Header("싱크대")] 
    public List<Transform> snik; // 싱크대 Transform 리스트
    private bool isWashing = false;
    private float washStartTime;
    public float washDuration = 2f; // 설거지에 걸리는 시간 (초)
    
    [Header("찬장")]
    public List<Transform> cabinetPositions; // 찬장의 위치 Transform 리스트

    [Header("중앙 분리 테이블")]
    public List<Transform> stuffs; // 상자 안의 재료들의 Transform 리스트

    [Header("소스")] 
    public List<Transform> sources;
    private bool isPouring = false;
    private float pourStartTime;
    public float pourDuration = 2f; // 소스를 뿌리는 시간 (초)
    private Source currentSource;
    private Source heldSource;


    [Header("애니메이션")]
    public Animator animator;
    public GameObject player;
    public string speedParameterName = "Speed";
    public string holdingParameterName = "Holding";

    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    void Update()
    {
        MoveLogic();

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (!pickupActivated)
            {
                // 접시를 집거나 재료를 집음
                Pickup();
                PickOnStuffShareTable();
                PickStuffOnPlate();
            }
            else
            {
                PutPlateInCabinet();
                PutStuffsOnThePlate();
            }
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

        if (Input.GetKey(KeyCode.Alpha3))
        {
            if (!isPouring)
            {
                StartPouring();
            }
            else
            {
                ContinuePouring();
            }
        }
        
        if (Input.GetKeyUp(KeyCode.Alpha3))
        {
            StopPouring();
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

            // 애니메이션 파라미터 float 값 코드 추가
            animator.SetFloat(speedParameterName, inputDirection.magnitude);                            //블랜딩 애니메이션 값에 넣어 준다. 
        }
        else
        {
            // 이동하지 않을 때 속도를 0으로 설정
            rigidbody.velocity = Vector3.zero;

            // 애니메이션 파라미터 float 값 코드 추가
            animator.SetFloat(speedParameterName, 0);
        }
    }

    void Pickup()
    {
        if (!pickupActivated) // 플레이어가 무언가를 들고 있지 않다면
        {
            // 애니메이션 파라미터 bool 값 코드 추가
            animator.SetBool(holdingParameterName, false);

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
                
                // 애니메이션 파라미터 bool 값 코드 추가
                animator.SetBool(holdingParameterName, true);
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
            // 애니메이션 파라미터 bool 값 코드 추가
            animator.SetBool(holdingParameterName, true);

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
            // 애니메이션 파라미터 bool 값 코드 추가
            animator.SetBool(holdingParameterName, true);

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
                            // 애니메이션 파라미터 bool 값 코드 추가
                            animator.SetBool(holdingParameterName, false);

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
            // 애니메이션 파라미터 bool 값 코드 추가
            animator.SetBool(holdingParameterName, false);

            Debug.Log("플레이어가 접시를 들고 있지 않습니다.");
        }
    }

    void PickOnStuffShareTable()
    {
        if (!pickupActivated) // 플레이어가 무언가를 들고 있지 않다면
        {
            // 애니메이션 파라미터 bool 값 코드 추가
            animator.SetBool(holdingParameterName, false);

            Transform nearestStuff = null;
            float minDistance = float.MaxValue;

            // 모든 재료들과의 거리 계산
            foreach (Transform itemTransform in stuffs)
            {
                float distance = Vector3.Distance(playerTransform.position, itemTransform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestStuff = itemTransform;
                }
            }

            if (nearestStuff != null && minDistance <= maxPickupDistance) // 가장 가까운 재료가 충분히 가깝다면
            {
                // 애니메이션 파라미터 bool 값 코드 추가
                animator.SetBool(holdingParameterName, true);

                pickupActivated = true; // 재료를 손에 든다
                heldObject = nearestStuff;
                heldObject.position = holdPosition.position; // 재료의 위치를 holdPosition으로 이동
                heldObject.SetParent(holdPosition); // 재료를 holdPosition의 자식으로 설정
                Debug.Log("재료를 획득합니다.");
            }
        }
        else // 플레이어가 무언가를 들고 있으면
        {
            // 애니메이션 파라미터 bool 값 코드 추가
            animator.SetBool(holdingParameterName, true);

            Debug.Log("이미 재료를 들고 있습니다.");
        }
    }

    void PutStuffsOnThePlate()
    {
        if (heldObject != null) // 플레이어가 무언가를 들고 있는 경우
        {
            // 현재 들고 있는 객체가 재료인지 확인합니다.
            if (stuffs.Contains(heldObject))
            {
                Debug.Log("재료를 들고 있습니다.");

                Transform nearestPlate = null;
                float minDistance = float.MaxValue;

                // 가장 가까운 접시를 찾습니다.
                foreach (Transform plateTransform in plates)
                {
                    float distance = Vector3.Distance(playerTransform.position, plateTransform.position);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        nearestPlate = plateTransform;
                    }
                }

                // 가장 가까운 접시가 충분히 가깝고, 깨끗한 상태이며, 접시 위에 재료가 없는 경우
                if (nearestPlate != null && minDistance <= maxPickupDistance)
                {
                    Plate plateComponent = nearestPlate.GetComponent<Plate>();
                    if (plateComponent != null)
                    {
                        Debug.Log("가까운 접시를 찾았습니다.");

                        if (plateComponent.plateType == Plate.PlateType.Clean)
                        {
                            Debug.Log("접시가 깨끗합니다.");

                            bool hasStuff = false;

                            if (!hasStuff)
                            {
                                Debug.Log("접시 위에 재료가 없습니다.");

                                // 들고 있는 재료를 접시 위에 놓습니다.
                                heldObject.SetParent(nearestPlate);
                                heldObject.position = platesPos[plates.IndexOf(nearestPlate)].position; // 접시의 위치에 맞게 재료 위치 설정
                                heldObject.rotation = platesPos[plates.IndexOf(nearestPlate)].rotation; // 접시의 회전에 맞게 재료 회전 설정

                                pickupActivated = false; // 재료를 내려놓았으므로 플레이어는 더 이상 무언가를 들고 있지 않음
                                heldObject = null;
                                Debug.Log("재료를 접시 위에 놓았습니다.");

                                // 애니메이션 파라미터 bool 값 코드 추가
                                animator.SetBool(holdingParameterName, false);
                            }
                        }
                        else
                        {
                            Debug.Log("접시가 깨끗하지 않습니다.");
                        }
                    }
                }
            }
        }
    }

    void PickStuffOnPlate()
    {
        if (!pickupActivated) // 플레이어가 무언가를 들고 있지 않은 경우
        {
            // 애니메이션 파라미터 bool 값 코드 추가
            animator.SetBool(holdingParameterName, false);

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
                Plate plateComponent = nearestPlate.GetComponent<Plate>();
                if (plateComponent != null && plateComponent.plateType == Plate.PlateType.Clean) // 접시가 깨끗한 상태라면
                {
                    Transform stuffPos = platesPos[plates.IndexOf(nearestPlate)]; // 접시 위에 재료를 올려놓을 위치를 찾습니다.

                    if (stuffPos.childCount > 0) // 접시 위에 자식 객체가 있는지 확인
                    {
                        // 애니메이션 파라미터 bool 값 코드 추가
                        animator.SetBool(holdingParameterName, true);

                        // 접시 위에 있는 재료를 들고 있는 위치로 옮긴다.
                        Transform stuff = stuffPos.GetChild(0); // 접시 위에 있는 재료를 선택
                        heldObject = stuff;
                        heldObject.SetParent(holdPosition); // 재료를 holdPosition의 자식으로 설정
                        heldObject.position = holdPosition.position; // 재료의 위치를 holdPosition으로 이동
                        pickupActivated = true; // 플레이어가 재료를 들고 있는 상태로 변경
                        Debug.Log("접시 위의 재료를 들었습니다.");
                    }
                    else
                    {
                        Debug.Log("접시 위에 재료가 없습니다.");
                    }
                }
            }
        }
        else
        {
            // 애니메이션 파라미터 bool 값 코드 추가
            animator.SetBool(holdingParameterName, true);

            Debug.Log("이미 무언가를 들고 있습니다.");
        }
    }

    void StartPouring()
    {
        if (heldObject != null)
        {
            Plate heldPlate = heldObject.GetComponent<Plate>();

            if (heldPlate != null && heldPlate.plateType == Plate.PlateType.Clean)
            {
                foreach (Transform source in sources)
                {
                    float distanceToSource = Vector3.Distance(playerTransform.position, source.position);
                    if (distanceToSource <= maxPickupDistance)
                    {
                        Source sourceComponent = source.GetComponent<Source>();
                        if (sourceComponent != null && !sourceComponent.isDepleted)
                        {
                            if (!isPouring)
                            {
                                isPouring = true;
                                pourStartTime = Time.time;
                                currentSource = sourceComponent;
                                Debug.Log($"{sourceComponent.sourceType} 소스 뿌리기 시작");
                            }
                            return;
                        }
                        else if (sourceComponent != null && sourceComponent.isDepleted)
                        {
                            Debug.Log($"{sourceComponent.sourceType} 소스가 다 뿌려졌습니다.");
                        }
                    }
                }
                Debug.Log("소스와 너무 멀리 떨어져 있습니다.");
            }
            else
            {
                Debug.Log("깨끗한 접시가 필요합니다.");
            }
        }
        else
        {
            Debug.Log("플레이어가 접시를 들고 있지 않습니다.");
        }
    }

    void ContinuePouring()
    {
        if (isPouring)
        {
            float elapsed = Time.time - pourStartTime;
            if (elapsed >= pourDuration)
            {
                isPouring = false;
                if (currentSource != null)
                {
                    currentSource.isDepleted = true;
                    Debug.Log($"{currentSource.sourceType} 소스를 다 뿌렸습니다.");
                }
            }
            else
            {
                Debug.Log("소스 뿌리는 중... (" + elapsed + "/" + pourDuration + "초)");
            }
        }
    }

    void StopPouring()
    {
        if (isPouring)
        {
            isPouring = false;
            Debug.Log("소스 뿌리기를 멈췄습니다.");
        }
    }
}
