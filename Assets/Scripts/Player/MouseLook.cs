using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLook : MonoBehaviour
{
    [Tooltip("���������")] public float mouseSenstivity = 400f;
    private Transform playerBody;// wanjia weizhi 
    private float yRotation = 0f;//�����������ת����ֵ

    private CharacterController characterController;
    [Tooltip("��ǰ������ĳ�ʼ�߶�")] public float height = 1.8f;
    private float interpolationSpeed = 12f;//�߶ȱ仯��ƽ��ֵ

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        playerBody = transform.GetComponentInParent<PlayerController>().transform;
        characterController = GetComponentInParent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        float MouseX = Input.GetAxis("Mouse X") * mouseSenstivity * Time.deltaTime;
        float MouseY = Input.GetAxis("Mouse Y") * mouseSenstivity * Time.deltaTime;

        yRotation -= MouseY;//��������ת����ֵ�����ۼ�
        yRotation = Mathf.Clamp(yRotation, -60f, 60f);
        transform.localRotation = Quaternion.Euler(yRotation, 0f, 0f);//�����������ת
        playerBody.Rotate(Vector3.up * MouseX);//��������ƶ�

        //������߶�������߶ȱ仯
        float heightTarget = characterController.height * 0.9f;
        height = Mathf.Lerp(height, heightTarget, interpolationSpeed * Time.deltaTime);
        transform.localPosition = Vector3.up * height;
    }
}
