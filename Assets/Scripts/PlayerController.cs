using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController: MonoBehaviour
{
    private CharacterController characterController;
    public Vector3 moveDirection;

    [Header("玩家数值")]
    private float Speed;
    [Tooltip("行走速度")]public float walkSpeed;
    [Tooltip("奔跑速度")] public float runSpeed;
    [Tooltip("下蹲行走速度")] public float crouchSpeed;

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
        //松开按键人物会立即停下
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Speed = walkSpeed;
        //设置人物移动方向（将速度进行规范化，放置人物斜向走时速度变大）
        moveDirection = (transform.right* h +  transform.forward* v).normalized;
        characterController.Move(moveDirection*Speed*Time.deltaTime);//人物移动

    }
}
