using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerController: MonoBehaviour
{
    private CharacterController characterController;
    public Vector3 moveDirection;

    [Header("玩家数值")]
    public float Speed;
    [Tooltip("行走速度")]public float walkSpeed;
    [Tooltip("奔跑速度")] public float runSpeed;
    [Tooltip("下蹲行走速度")] public float crouchSpeed;

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
        //松开按键人物会立即停下
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
        else if (isGround) //正常行走
        {
            state = MovementsState.walking;
            Speed = walkSpeed;
        }

        
        //设置人物移动方向（将速度进行规范化，放置人物斜向走时速度变大）
        moveDirection = (transform.right* h +  transform.forward* v).normalized;
        characterController.Move(moveDirection*Speed*Time.deltaTime);//人物移动

    }

    public void jump()
    {
        isJump = Input.GetKey(jumpinputname);
        //判断玩家在地面上 且此时在地面上 才能进行跳跃
        if (isJump && isGround) 
        {
            isGround = false;
            jumpForce = 5f;//设置跳跃力度
        }

        //此时按下跳跃键 人物跳起且不在地面上
        if (!isGround)
        {
            jumpForce = jumpForce - fallForce * Time.deltaTime;//每秒将跳跃键进行累减 使其下落
            Vector3 jump = new Vector3(0, jumpForce * Time.deltaTime, 0);//将跳跃力度转换为V3坐标
            collisionFlags = characterController.Move(jump);//调用角色控制器移动方法 向上方法模拟跳跃

            //判断玩家在地面上
            //CollisionFlags.Below->在地面上

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
        //获取人物头顶的高度V3位置
        Vector3 spherelocation = transform.position + new Vector3(0,0.3f,0) + Vector3.up * standHeight;
        //根据头顶上是否有物体 来判断是否可以下蹲
        isCanCrouch = (Physics.OverlapSphere(spherelocation, characterController.radius, crouchLayerMask).Length) == 0;

        print("iscancrouch"+isCanCrouch);
    }

    public void Crouch(bool newCrouching)
    {
        if (!isCanCrouch) return;//不可下蹲时（在隧道里），不能进行站立
        isCrouching = newCrouching;

        characterController.height = isCrouching ? crouchHeight : standHeight;//根据下蹲状态设置下蹲时候的高度和战力的高度
        characterController.center = characterController.height / 2.0f * Vector3.up;//将角色控制器的中心位置Y，从头顶往下减少一半的高度

    }
    public enum MovementsState
    {
        walking,
        runing,
        crouch,
        idle
    }
        
}
