using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float sensY = -100.0f; // 마우스 Y축 감도
    private float yRotation = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        yRotation = transform.eulerAngles.y;
    }

    // Update is called once per frame
    void Update()
    {
        float mouseY = Input.GetAxis("Mouse Y") * Time.deltaTime * sensY;
        yRotation += mouseY;

        // yRotation 값 제한 (-90도에서 90도 사이로)
        yRotation = Mathf.Clamp(yRotation, -90.0f, 90.0f);

        // Y축으로 캐릭터 회전
        transform.rotation = Quaternion.Euler(0, yRotation, 0);
    }
}
