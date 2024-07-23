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

    [Header("도마 관련")] 
    public List<Transform> cuttingBoard;
    public List<Transform> cuttingBoardPositions; // 도마에 재료를 놓을 위치의 Transform 리스트
    private int cuttingPressesRequired = 5; 
    private int cuttingPressCount = 0;

    [Header("중앙 분리 테이블")] 
    public List<Transform> shareTablePositions;

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
        
        if (Input.GetKeyDown(KeyCode.B)) // B 버튼을 누르면
        {
            Pickup();
            PutOnCuttingBoard();
            PutOnStuffShareTable();
        }

        if (Input.GetKeyDown(KeyCode.N))
        {
            CuttingStuff();
        }
        
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
                Debug.Log("재료 획득합니다");
                
                // 애니메이션 파라미터 bool 값 코드 추가
                animator.SetBool(holdingParameterName, true);
            }
        }
        else // 플레이어가 무언가를 들고 있으면
        {
            Debug.Log("이미 재료를 들고 있습니다.");
        }
    }

    void PutOnCuttingBoard()
    {
        if (heldObject != null) // 플레이어가 재료를 들고 있는 경우
        {
            // 애니메이션 파라미터 bool 값 코드 추가
            animator.SetBool(holdingParameterName, true);

            Stuff heldStuff = heldObject.GetComponent<Stuff>();

            if (heldStuff != null && heldStuff.stuffType == Stuff.StuffType.PrepIngredients) // 들고 있는 재료가 손질된 경우
            {
                Debug.Log("손질된 재료를 들고 있습니다. 아무 일도 일어나지 않습니다.");
                return;
            }

            for (int i = 0; i < cuttingBoard.Count; i++)
            {
                Transform board = cuttingBoard[i];
                Transform boardPosition = cuttingBoardPositions[i];
                float distanceToBoard = Vector3.Distance(playerTransform.position, board.position);

                if (distanceToBoard <= maxPickupDistance) // 플레이어와 도마의 거리가 충분히 가까운 경우
                {
                    if (boardPosition.childCount == 0) // 도마의 재료 포지션이 비어 있는 경우
                    {
                        // 애니메이션 파라미터 bool 값 코드 추가
                        animator.SetBool(holdingParameterName, false);

                        // 재료를 도마에 내려놓는다
                        heldObject.SetParent(boardPosition);
                        heldObject.position = boardPosition.position;
                        heldObject.rotation = boardPosition.rotation; // 재료의 회전도 도마의 포지션 회전으로 설정
                        pickupActivated = false;
                        heldObject = null;
                        Debug.Log("재료를 도마에 내려놓았습니다.");
                        return;
                    }
                    else
                    {
                        Debug.Log("도마에 이미 재료가 있습니다.");
                    }
                }
            }
        }
        else // 플레이어가 재료를 들고 있지 않은 경우
        {
            // 애니메이션 파라미터 bool 값 코드 추가
            animator.SetBool(holdingParameterName, false);

            Debug.Log("플레이어가 재료를 들고 있지 않습니다.");
        }
    }

    void CuttingStuff()
    {
        for (int i = 0; i < cuttingBoard.Count; i++)
        {
            Transform board = cuttingBoard[i];
            Transform boardPosition = cuttingBoardPositions[i];
            float distanceToBoard = Vector3.Distance(playerTransform.position, board.position);

            if (distanceToBoard <= maxPickupDistance && boardPosition.childCount > 0) // 플레이어가 도마에 가까이 있고 도마 위에 재료가 있을 경우
            {
                Transform ingredient = boardPosition.GetChild(0);
                Stuff stuff = ingredient.GetComponent<Stuff>();

                if (stuff != null && stuff.stuffType == Stuff.StuffType.NotPrepared) // 재료가 손질되지 않은 경우
                {
                    cuttingPressCount++;

                    if (cuttingPressCount >= cuttingPressesRequired)
                    {
                        // 손질된 재료로 교체
                        stuff.stuffType = Stuff.StuffType.PrepIngredients; // StuffType.PrepIngredients는 손질된 상태를 나타냄
                        Debug.Log("재료 손질 완료!");
                        cuttingPressCount = 0;
                    }
                    else
                    {
                        Debug.Log("재료 손질 중... (" + cuttingPressCount + "/" + cuttingPressesRequired + ")");
                    }
                    return;
                }
                else if (stuff != null && stuff.stuffType == Stuff.StuffType.PrepIngredients)
                {
                    Debug.Log("이미 손질된 재료입니다.");
                    return;
                }
            }
        }

        Debug.Log("도마에 손질할 재료가 없습니다.");
    }

    void PutOnStuffShareTable()
    {
        if (heldObject != null) // 플레이어가 무언가를 들고 있는 경우
        {
            // 애니메이션 파라미터 bool 값 코드 추가
            animator.SetBool(holdingParameterName, true);

            Stuff heldStuff = heldObject.GetComponent<Stuff>();

            if (heldStuff != null && heldStuff.stuffType == Stuff.StuffType.PrepIngredients) // 들고 있는 재료가 손질된 경우
            {
                for (int i = 0; i < shareTablePositions.Count; i++)
                {
                    Transform tablePosition = shareTablePositions[i];
                    float distanceToTable = Vector3.Distance(playerTransform.position, tablePosition.position);

                    if (distanceToTable <= maxPickupDistance) // 플레이어와 테이블의 거리가 충분히 가까운 경우
                    {
                        if (tablePosition.childCount == 0) // 테이블의 재료 포지션이 비어 있는 경우
                        {
                            // 애니메이션 파라미터 bool 값 코드 추가
                            animator.SetBool(holdingParameterName, false);

                            // 재료를 테이블에 내려놓는다
                            heldObject.SetParent(tablePosition);
                            heldObject.position = tablePosition.position;
                            heldObject.rotation = tablePosition.rotation; // 재료의 회전도 테이블의 포지션 회전으로 설정
                            pickupActivated = false;
                            heldObject = null;
                            Debug.Log("재료를 중앙 분리 테이블에 내려놓았습니다.");
                            return;
                        }
                    }
                }
                Debug.Log("중앙 분리 테이블의 모든 포지션이 이미 사용 중입니다.");
            }
            else
            {
                Debug.Log("손질된 재료가 아닙니다. 중앙 분리 테이블에 내려놓을 수 없습니다.");
            }
        }
        else
        {
            // 애니메이션 파라미터 bool 값 코드 추가
            animator.SetBool(holdingParameterName, false);

            Debug.Log("플레이어가 재료를 들고 있지 않습니다.");
        }
    }
}
