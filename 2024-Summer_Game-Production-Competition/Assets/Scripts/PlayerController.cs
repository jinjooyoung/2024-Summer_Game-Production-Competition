using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float sensY = -100.0f; // ���콺 Y�� ����
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

        // yRotation �� ���� (-90������ 90�� ���̷�)
        yRotation = Mathf.Clamp(yRotation, -90.0f, 90.0f);

        // Y������ ĳ���� ȸ��
        transform.rotation = Quaternion.Euler(0, yRotation, 0);
    }
}
