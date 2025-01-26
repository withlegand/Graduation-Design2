using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerController: MonoBehaviour
{
    private CharacterController characterController;
    public Vector3 moveDirection;

    [Header("�����ֵ")]
    public float Speed;
    [Tooltip("�����ٶ�")]public float walkSpeed;
    [Tooltip("�����ٶ�")] public float runSpeed;
    [Tooltip("�¶������ٶ�")] public float crouchSpeed;

    [Tooltip("��Ծ��")] public float jumpForce;
    [Tooltip("������")] public float fallForce;
    [Tooltip("�¶�ʱ�����Ҹ߶�")] public float crouchHeight;
    [Tooltip("����վ��ʱ����Ҹ߶�")] public float standHeight;

    [Header("��λ����")]
    [Tooltip("���ܰ���")] public KeyCode runinputname = KeyCode.LeftShift;
    [Tooltip("��Ծ����")] public KeyCode jumpinputname = KeyCode.Space;
    [Tooltip("�¶װ���")] public KeyCode crouchinputname = KeyCode.LeftControl;

    [Header("��������ж�")]
    public MovementsState state;
    private CollisionFlags collisionFlags;
    public bool isWalk;//�ж�����Ƿ�����
    public bool isRun;//�ж�����Ƿ���
    public bool isJump;//�ж�����Ƿ���Ծ
    public bool isGround;//�ж��ڰ����Ƿ��ڵ�����
    public bool isCanCrouch;//�ж�����Ƿ�����¶�
    public bool isCrouching;//�ж�����Ƿ����¶�

    public LayerMask crouchLayerMask;

    // Start is called before the first frame update
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        walkSpeed = 4f;
        runSpeed = 6f;
        crouchSpeed = 2f;
        jumpForce = 0f;
        fallForce = 10f;
        crouchHeight = 1f;
        standHeight = characterController.height;
    }

    // Update is called once per frame
    void Update()
    {
        CanCrouch();
        if (Input.GetKey(crouchinputname))
        {
            Crouch(true);
        }
        else
        {
            Crouch(false);
        }

        jump();
        Moving();
    }

    public void Moving()
    {
        //�ɿ��������������ͣ��
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        isRun = Input.GetKey(runinputname);
        isWalk = (Mathf.Abs(h) > 0 || Mathf.Abs(v) > 0) ? true : false;


        if (isRun && isGround)
        {
            state = MovementsState.runing;
            Speed = runSpeed;
            print("run");
        }
        else if (isGround) //��������
        {
            state = MovementsState.walking;
            Speed = walkSpeed;
        }

        
        //���������ƶ����򣨽��ٶȽ��й淶������������б����ʱ�ٶȱ��
        moveDirection = (transform.right* h +  transform.forward* v).normalized;
        characterController.Move(moveDirection*Speed*Time.deltaTime);//�����ƶ�

    }

    public void jump()
    {
        isJump = Input.GetKey(jumpinputname);
        //�ж�����ڵ����� �Ҵ�ʱ�ڵ����� ���ܽ�����Ծ
        if (isJump && isGround) 
        {
            isGround = false;
            jumpForce = 5f;//������Ծ����
        }

        //��ʱ������Ծ�� ���������Ҳ��ڵ�����
        if (!isGround)
        {
            jumpForce = jumpForce - fallForce * Time.deltaTime;//ÿ�뽫��Ծ�������ۼ� ʹ������
            Vector3 jump = new Vector3(0, jumpForce * Time.deltaTime, 0);//����Ծ����ת��ΪV3����
            collisionFlags = characterController.Move(jump);//���ý�ɫ�������ƶ����� ���Ϸ���ģ����Ծ

            //�ж�����ڵ�����
            //CollisionFlags.Below->�ڵ�����

            if (collisionFlags == CollisionFlags.Below)
            {
                isGround = true;
                jumpForce = 0f;
            }
            if (isGround && collisionFlags == CollisionFlags.None)
            {
                isGround = false;
            }
        }

    }

    public void CanCrouch()
    {
        //��ȡ����ͷ���ĸ߶�V3λ��
        Vector3 spherelocation = transform.position + new Vector3(0,0.3f,0) + Vector3.up * standHeight;
        //����ͷ�����Ƿ������� ���ж��Ƿ�����¶�
        isCanCrouch = (Physics.OverlapSphere(spherelocation, characterController.radius, crouchLayerMask).Length) == 0;

        print("iscancrouch"+isCanCrouch);
    }

    public void Crouch(bool newCrouching)
    {
        if (!isCanCrouch) return;//�����¶�ʱ�������������ܽ���վ��
        isCrouching = newCrouching;

        characterController.height = isCrouching ? crouchHeight : standHeight;//�����¶�״̬�����¶�ʱ��ĸ߶Ⱥ�ս���ĸ߶�
        characterController.center = characterController.height / 2.0f * Vector3.up;//����ɫ������������λ��Y����ͷ�����¼���һ��ĸ߶�

    }
    public enum MovementsState
    {
        walking,
        runing,
        crouch,
        idle
    }
        
}
