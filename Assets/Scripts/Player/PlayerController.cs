using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

/// <summary>
/// 玩家核心控制器
/// 功能：
/// 1. 移动控制（行走/奔跑/下蹲）
/// 2. 跳跃和下蹲逻辑
/// 3. 状态管理（生命值、音效、武器交互）
/// </summary>
public class PlayerController: MonoBehaviour
{
    private CharacterController characterController;
    public Vector3 moveDirection;
    private AudioSource audioSource;

    [Header("玩家数值")]
    public float Speed;
    [Tooltip("行走速度")]public float walkSpeed;
    [Tooltip("奔跑速度")] public float runSpeed;
    [Tooltip("下蹲行走速度")] public float crouchSpeed;
    [Tooltip("玩家生命值")] public float playerHealth;

    [Tooltip("跳跃力")] public float jumpForce;
    [Tooltip("下落力")] public float fallForce;
    [Tooltip("下蹲时候的玩家高度")] public float crouchHeight;
    [Tooltip("正常站立时的玩家高度")] public float standHeight;

    [Header("键位设置")]
    [Tooltip("奔跑按键")] public KeyCode runinputname = KeyCode.LeftShift;
    [Tooltip("跳跃按键")] public KeyCode jumpinputname = KeyCode.Space;
    [Tooltip("下蹲按键")] public KeyCode crouchinputname = KeyCode.LeftControl;

    [Header("玩家属性判断")]
    public MovementsState state;
    private CollisionFlags collisionFlags;
    public bool isWalk;//判断玩家是否行走
    public bool isRun;//判断玩家是否奔跑
    public bool isJump;//判断玩家是否跳跃
    public bool isGround;//判断挖安家是否在地面上
    public bool isCanCrouch;//判断玩家是否可以下蹲
    public bool isCrouching;//判断玩家是否在下蹲
    private bool playerIsDead;//判断玩家是否死亡
    private bool isDemage;//判断玩家是否受伤

    public LayerMask crouchLayerMask;
    public Text playerHealthUI;

    [Header("空中控制参数")]
    [Tooltip("空中移动加速度")] public float airAcceleration = 10f;   // 建议值：5~10
    [Tooltip("最大空速")] public float maxAirSpeed = 3f;           // 建议值：略低于地面奔跑速度
    private Vector3 currentVelocity; // 当前实际速度（包含惯性）
    private Vector3 wishDir;         // 输入方向（理想移动方向）
    public float jumpHeight = 1f;

    [Header("音效")]
    [Tooltip("行走音效")]public AudioClip walkSound;
    [Tooltip("奔跑音效")] public AudioClip runSound;

    
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
        standHeight = characterController.height;//初始站立高度
        playerHealthUI.text = "生命值" + playerHealth;
    }

    // Update is called once per frame
    void Update()
    {
        CanCrouch();
        if (Input.GetKey(crouchinputname))//按下左Ctrl键
        {
            Crouch(true);
        }
        else
        {
            Crouch(false);
        }

        jump();
        PlayerFootSoundSet();//脚步声控制
        Moving();

        
    }

    public void Moving()
    {
        //松开按键人物会立即停下
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        isRun = Input.GetKey(runinputname);
        isWalk = (Mathf.Abs(h) > 0 || Mathf.Abs(v) > 0) ? true : false;

        // 计算输入方向（理想方向）
        wishDir = (transform.right * h + transform.forward * v).normalized;

        if (isRun && isGround && isCanCrouch && !isCrouching)
        {
            state = MovementsState.runing;
            currentVelocity = wishDir *  runSpeed;
            Speed = runSpeed;
            print("run");
        }
        else if (isGround) //正常行走
        {
            state = MovementsState.walking;
            currentVelocity = wishDir * walkSpeed;
            Speed = walkSpeed;
            if (isCrouching)//下蹲行走
            {
                state = MovementsState.crouching;
                currentVelocity = wishDir * crouchSpeed;
                Speed = crouchSpeed;
            }
        }
        if(!isGround)
        {
            // 空中移动：通过加速度逐步调整方向
            Vector3 speedAddition = wishDir * airAcceleration * Time.deltaTime;
            currentVelocity += speedAddition;

            // 限制水平速度不超过最大值
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

        
        //设置人物移动方向（将速度进行规范化，防止人物斜向走时速度变大）
        moveDirection = (transform.right* h +  transform.forward* v).normalized;
        currentVelocity.y += Physics.gravity.y * Time.deltaTime;
        characterController.Move(currentVelocity * Time.deltaTime);//人物移动

        // 执行移动并检测碰撞
        CollisionFlags flags = characterController.Move(currentVelocity * Time.deltaTime);

        // 更新地面状态
        isGround = (flags & CollisionFlags.Below) != 0;

        // 落地后重置垂直速度
        if (isGround && currentVelocity.y < 0)
        {
            currentVelocity.y = 0;
        }
    }

    public void jump()
    {
        if (!isCanCrouch) return;
        isJump = Input.GetKey(jumpinputname);
        //判断玩家在地面上 且此时在地面上 才能进行跳跃
        if (isJump && isGround)
        {
            
            currentVelocity.y = Mathf.Sqrt(2 * Mathf.Abs(Physics.gravity.y) * jumpHeight);
            isGround = false;
        }
        
    }

    public void CanCrouch()
    {
        //获取人物头顶的高度V3位置
        Vector3 spherelocation = transform.position + new Vector3(0,0.3f,0) + Vector3.up * standHeight;
        //根据头顶上是否有物体 来判断是否可以下蹲
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
        if (!isCanCrouch) return;//不可下蹲时（在隧道里），不能进行站立
        isCrouching = newCrouching;

        characterController.height = isCrouching ? crouchHeight : standHeight;//根据下蹲状态设置下蹲时候的高度和战力的高度
        characterController.center = characterController.height / 2.0f * Vector3.up;//将角色控制器的中心位置Y，从头顶往下减少一半的高度

    }

    public void PlayerFootSoundSet()
    {
        if (isGround && moveDirection.sqrMagnitude > 0)
        {
            audioSource.clip = isRun ? runSound : walkSound;
            if (!audioSource.isPlaying)
            {
                //播放行走和奔跑音效
                audioSource.Play();
                print("音效播放");
            }
        }
        else
        {
            if (audioSource.isPlaying)
            {
                //音效暂停
                audioSource.Pause();
            }
        }
        //下蹲时不播放行走音效
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
        playerHealthUI.text = "生命值" + playerHealth;
        if (playerHealth <= 0)
        {
            playerIsDead = true;
            playerHealthUI.text = "玩家死亡";
            Time.timeScale = 0;//游戏暂停
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
