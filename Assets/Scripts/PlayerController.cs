using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController: MonoBehaviour
{
    private CharacterController characterController;
    public Vector3 moveDirection;

    [Header("�����ֵ")]
    private float Speed;
    [Tooltip("�����ٶ�")]public float walkSpeed;
    [Tooltip("�����ٶ�")] public float runSpeed;
    [Tooltip("�¶������ٶ�")] public float crouchSpeed;

    // Start is called before the first frame update
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        walkSpeed = 4f;
        runSpeed = 6f;
        crouchSpeed = 2f;
    }

    // Update is called once per frame
    void Update()
    {
        Moving();
    }

    public void Moving()
    {
        //�ɿ��������������ͣ��
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Speed = walkSpeed;
        //���������ƶ����򣨽��ٶȽ��й淶������������б����ʱ�ٶȱ��
        moveDirection = (transform.right* h +  transform.forward* v).normalized;
        characterController.Move(moveDirection*Speed*Time.deltaTime);//�����ƶ�

    }
}
