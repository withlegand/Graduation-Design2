using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLook : MonoBehaviour
{
    [Tooltip("鼠标灵敏度")] public float mouseSenstivity = 400f;
    private Transform playerBody;// wanjia weizhi 
    private float yRotation = 0f;//摄像机上下旋转的数值

    private CharacterController characterController;
    [Tooltip("当前摄像机的初始高度")] public float height = 1.8f;
    private float interpolationSpeed = 12f;//高度变化的平滑值

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

        yRotation -= MouseY;//将上下旋转的轴值进行累计
        yRotation = Mathf.Clamp(yRotation, -60f, 60f);
        transform.localRotation = Quaternion.Euler(yRotation, 0f, 0f);//摄像机上下旋转
        playerBody.Rotate(Vector3.up * MouseX);//玩家左右移动

        //摄像机高度随人物高度变化
        float heightTarget = characterController.height * 0.9f;
        height = Mathf.Lerp(height, heightTarget, interpolationSpeed * Time.deltaTime);
        transform.localPosition = Vector3.up * height;
    }
}
