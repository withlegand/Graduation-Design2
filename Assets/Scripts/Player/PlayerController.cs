using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

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

    [Header("��Ч")]
    [Tooltip("������Ч")]public AudioClip walkSound;
    [Tooltip("������Ч")] public AudioClip runSound;

    private Inventory Inventory;

    // Start is called before the first frame update
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        audioSource = GetComponent<AudioSource>();
        Inventory = GetComponentInChildren<Inventory>();
        walkSpeed = 4f;
        runSpeed = 6f;
        crouchSpeed = 2f;
        jumpForce = 0f;
        fallForce = 10f;
        playerHealth = 100f;
        crouchHeight = 1f;
        standHeight = characterController.height;
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
        PlayerFootSoundSet();
        Moving();
    }

    public void Moving()
    {
        //�ɿ��������������ͣ��
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        isRun = Input.GetKey(runinputname);
        isWalk = (Mathf.Abs(h) > 0 || Mathf.Abs(v) > 0) ? true : false;


        if (isRun && isGround && isCanCrouch && !isCrouching)
        {
            state = MovementsState.runing;
            Speed = runSpeed;
            print("run");
        }
        else if (isGround) //��������
        {
            state = MovementsState.walking;
            Speed = walkSpeed;
            if (isCrouching)//�¶�����
            {
                state = MovementsState.crouching;
                Speed = crouchSpeed;
            }
        }

        //if (isRun && isCrouching)
        //{
            //state = MovementsState.crouching;
            //Speed = crouchSpeed;
        //}

        
        //���������ƶ����򣨽��ٶȽ��й淶������������б����ʱ�ٶȱ��
        moveDirection = (transform.right* h +  transform.forward* v).normalized;
        characterController.Move(moveDirection*Speed*Time.deltaTime);//�����ƶ�

    }

    public void jump()
    {
        if (!isCanCrouch) return;
        isJump = Input.GetKey(jumpinputname);
        //�ж�����ڵ����� �Ҵ�ʱ�ڵ����� ���ܽ�����Ծ
        if (isJump && isGround)
        {
            isGround = false;
            jumpForce = 5f;//������Ծ����
        }
        //
        else if (!isJump && isGround)
        {
            isGround = false;
        }

        //��ʱ������Ծ�� ���������Ҳ��ڵ�����
        if (!isGround)
        {
            jumpForce = jumpForce - fallForce * Time.deltaTime;//ÿ�뽫��Ծ�������ۼ� ʹ������
            Vector3 jump = new Vector3(0, jumpForce * Time.deltaTime, 0);//����Ծ����ת��ΪV3����
            collisionFlags = characterController.Move(jump);//���ý�ɫ�������ƶ����� ���Ϸ���ģ����Ծ
            //Debug.Log("collisonFlags:" + collisionFlags);
            //Debug.Log("characterController.isGround:" + characterController.isGrounded);

            //�ж�����ڵ�����
            //CollisionFlags.Below->�ڵ�����

            if (collisionFlags == CollisionFlags.Below)
            {
                isGround = true;
                jumpForce = -2f;
            }
            //if (isGround && collisionFlags == CollisionFlags.None)
            //{
            //    isGround = false;
            //}
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

    public void PickUpWeapon(int itemID,GameObject weapon)
    {
        //��������������������ӣ����򲹳䱸��
        if (Inventory.weapons.Contains(weapon))
        {
            weapon.GetComponent<Weapon_AutomaticGun>().bulletLeft = weapon.GetComponent<Weapon_AutomaticGun>().bulletMag * 5;
            weapon.GetComponent<Weapon_AutomaticGun>().UpdateAmmoUI();
            print("��������ڴ�ǹ�����䱸��");
            return;
        }
        else
        {
            Inventory.AddWeapon(weapon);
        }
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
