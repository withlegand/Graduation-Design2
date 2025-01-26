using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLook : MonoBehaviour
{
    [Tooltip("���������")] public float mouseSenstivity = 400f;
    private Transform playerBody;// wanjia weizhi 
    private float yRotation = 0f;//�����������ת����ֵ

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        playerBody = transform.GetComponentInParent<PlayerController>().transform;
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

    }
}
