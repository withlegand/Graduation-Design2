using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

/// <summary>
/// ��Һ��Ŀ�����
/// ���ܣ�
/// 1. �ƶ����ƣ�����/����/�¶ף�
/// 2. ��Ծ���¶��߼�
/// 3. ״̬��������ֵ����Ч������������
/// </summary>
public class PlayerController: MonoBehaviour
{
    private CharacterController characterController;
    public Vector3 moveDirection;
    private AudioSource audioSource;

    [Header("�����ֵ")]
    public float Speed;
    [Tooltip("�����ٶ�")]public float walkSpeed;
    [Tooltip("�����ٶ�")] public float runSpeed;
    [Tooltip("�¶������ٶ�")] public float crouchSpeed;
    [Tooltip("�������ֵ")] public float playerHealth;

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
    private bool playerIsDead;//�ж�����Ƿ�����
    private bool isDemage;//�ж�����Ƿ�����

    public LayerMask crouchLayerMask;
    public Text playerHealthUI;

    [Header("���п��Ʋ���")]
    [Tooltip("�����ƶ����ٶ�")] public float airAcceleration = 10f;   // ����ֵ��5~10
    [Tooltip("������")] public float maxAirSpeed = 3f;           // ����ֵ���Ե��ڵ��汼���ٶ�
    private Vector3 currentVelocity; // ��ǰʵ���ٶȣ��������ԣ�
    private Vector3 wishDir;         // ���뷽�������ƶ�����
    public float jumpHeight = 1f;

    [Header("��Ч")]
    [Tooltip("������Ч")]public AudioClip walkSound;
    [Tooltip("������Ч")] public AudioClip runSound;

    
    public Inventory Inventory;   


    // Start is called before the first frame update
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        audioSource = GetComponent<AudioSource>();
        Inventory = GetComponentInChildren<Inventory>();
        walkSpeed = 2f;
        runSpeed = 3f;
        crouchSpeed = 1f;
        jumpForce = 0f;
        fallForce = 10f;
        playerHealth = 100f;
        crouchHeight = 1f;
        standHeight = characterController.height;//��ʼվ���߶�
        playerHealthUI.text = "����ֵ" + playerHealth;
    }

    // Update is called once per frame
    void Update()
    {
        CanCrouch();
        if (Input.GetKey(crouchinputname))//������Ctrl��
        {
            Crouch(true);
        }
        else
        {
            Crouch(false);
        }

        jump();
        PlayerFootSoundSet();//�Ų�������
        Moving();

        
    }

    public void Moving()
    {
        //�ɿ��������������ͣ��
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        isRun = Input.GetKey(runinputname);
        isWalk = (Mathf.Abs(h) > 0 || Mathf.Abs(v) > 0) ? true : false;

        // �������뷽�����뷽��
        wishDir = (transform.right * h + transform.forward * v).normalized;

        if (isRun && isGround && isCanCrouch && !isCrouching)
        {
            state = MovementsState.runing;
            currentVelocity = wishDir *  runSpeed;
            Speed = runSpeed;
            print("run");
        }
        else if (isGround) //��������
        {
            state = MovementsState.walking;
            currentVelocity = wishDir * walkSpeed;
            Speed = walkSpeed;
            if (isCrouching)//�¶�����
            {
                state = MovementsState.crouching;
                currentVelocity = wishDir * crouchSpeed;
                Speed = crouchSpeed;
            }
        }
        if(!isGround)
        {
            // �����ƶ���ͨ�����ٶ��𲽵�������
            Vector3 speedAddition = wishDir * airAcceleration * Time.deltaTime;
            currentVelocity += speedAddition;

            // ����ˮƽ�ٶȲ��������ֵ
            Vector3 horizontalVel = new Vector3(currentVelocity.x, 0, currentVelocity.z);
            if (horizontalVel.magnitude > maxAirSpeed)
            {
                horizontalVel = horizontalVel.normalized * maxAirSpeed;
                currentVelocity = new Vector3(horizontalVel.x, currentVelocity.y, horizontalVel.z);
            }
        }

        //if (isRun && isCrouching)
        //{
            //state = MovementsState.crouching;
            //Speed = crouchSpeed;
        //}

        
        //���������ƶ����򣨽��ٶȽ��й淶������ֹ����б����ʱ�ٶȱ��
        moveDirection = (transform.right* h +  transform.forward* v).normalized;
        currentVelocity.y += Physics.gravity.y * Time.deltaTime;
        characterController.Move(currentVelocity * Time.deltaTime);//�����ƶ�

        // ִ���ƶ��������ײ
        CollisionFlags flags = characterController.Move(currentVelocity * Time.deltaTime);

        // ���µ���״̬
        isGround = (flags & CollisionFlags.Below) != 0;

        // ��غ����ô�ֱ�ٶ�
        if (isGround && currentVelocity.y < 0)
        {
            currentVelocity.y = 0;
        }
    }

    public void jump()
    {
        if (!isCanCrouch) return;
        isJump = Input.GetKey(jumpinputname);
        //�ж�����ڵ����� �Ҵ�ʱ�ڵ����� ���ܽ�����Ծ
        if (isJump && isGround)
        {
            
            currentVelocity.y = Mathf.Sqrt(2 * Mathf.Abs(Physics.gravity.y) * jumpHeight);
            isGround = false;
        }
        
    }

    public void CanCrouch()
    {
        //��ȡ����ͷ���ĸ߶�V3λ��
        Vector3 spherelocation = transform.position + new Vector3(0,0.3f,0) + Vector3.up * standHeight;
        //����ͷ�����Ƿ������� ���ж��Ƿ�����¶�
        isCanCrouch = (Physics.OverlapSphere(spherelocation, characterController.radius, crouchLayerMask).Length)==0;

        //Collider[] colis = Physics.OverlapSphere(spherelocation, characterController.radius, crouchLayerMask);
       // for (int i = 0; i < colis.Length; i++)
        //{
            //print("colis:" +  colis[i].name);
        //}

        //print("spherelocation:" + spherelocation);
        //print("iscancrouch:"+isCanCrouch);
    }

    public void Crouch(bool newCrouching)
    {
        if (!isCanCrouch) return;//�����¶�ʱ�������������ܽ���վ��
        isCrouching = newCrouching;

        characterController.height = isCrouching ? crouchHeight : standHeight;//�����¶�״̬�����¶�ʱ��ĸ߶Ⱥ�ս���ĸ߶�
        characterController.center = characterController.height / 2.0f * Vector3.up;//����ɫ������������λ��Y����ͷ�����¼���һ��ĸ߶�

    }

    public void PlayerFootSoundSet()
    {
        if (isGround && moveDirection.sqrMagnitude > 0)
        {
            audioSource.clip = isRun ? runSound : walkSound;
            if (!audioSource.isPlaying)
            {
                //�������ߺͱ�����Ч
                audioSource.Play();
                print("��Ч����");
            }
        }
        else
        {
            if (audioSource.isPlaying)
            {
                //��Ч��ͣ
                audioSource.Pause();
            }
        }
        //�¶�ʱ������������Ч
        if (isCrouching)
        {
            if (audioSource.isPlaying)
            {
                audioSource.Pause();
            }
        }
    }

    public void PickUpWeapon(int itemID, GameObject weapon, Weapon_AutomaticGun.WeaponType type)
    {
        Inventory.AddWeapon(weapon, type);
    }



    public void PlayerHealth(float demage)
    {
        playerHealth -= demage;
        isDemage = true;
        playerHealthUI.text = "����ֵ" + playerHealth;
        if (playerHealth <= 0)
        {
            playerIsDead = true;
            playerHealthUI.text = "�������";
            Time.timeScale = 0;//��Ϸ��ͣ
        }
    }


    public enum MovementsState
    {
        walking,
        runing,
        crouching,
        idle
    }
        
}
