using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Speed")]
    public float walkSpeed;                     // 行走移动速度
    public float runSpeed;                      // 跑步移动速度
    public float crouchSpeed;                   // 下蹲时移动速度
    public float ladderClimbSpeed;              // 爬梯子移动速度
    public float swimSpeed;                     // 游泳移动速度
    public float ledgeClimbSpeed;               // 梯子顶端过渡速度
    public float jumpForce;                     // 跳跃力
    public float waterJumpForce;                // 跳跃力
    public float gravityScale;                  // 重力参数

    [Header("CheckerSize")]
    public float groundCheckRadius;             // 地面判定检测半径
    public float ladderCheckDistance;           // 梯子判定检测距离
    public float slopeCheckDistance;            // 斜坡判定检测距离
    public float maxSlopeAngle;                 // 最大可移动斜坡角度

    public Vector2 ceilingCheckDistance;        // 下蹲时头顶检测距离
    public Vector2 ledgeOffset;                 // 梯子顶端过渡位移

    [Header("Ref")]
    public Transform groundCheck;               // 地面检测器
    public Transform ladderCheck;               // 梯子检测器
    public Transform ceilingCheck;              // 下蹲时头顶检测器
    public LayerMask whatIsGround;              // 地面层
    public LayerMask whatIsLadder;              // 梯子层
    public LayerMask whatIsWater;               // 水层
    public PhysicsMaterial2D noFriction;        // 光滑摩擦
    public PhysicsMaterial2D fullFriction;      // 最大摩擦

    private float error = 0.001f;               // 速度为零时的误差
    private float xInput;                       // 水平输入
    private float yInput;                       // 垂直输入
    private float slopeDownAngle;               // 斜坡垂直夹角
    private float slopeSideAngle;               // 斜坡水平夹角
    private float lastSlopeAngle;               // 上一个斜坡角度

    private int facingDirection = 1;            // 面朝方向

    [Header("State")]
    private bool canFlip = true;                // 翻转判定
    // 为了触发剧情
    public bool isGrounded;                    // 接触地面判定
    private bool isOnSlope;                     // 接触斜坡判定
    private bool canWalkOnSlope;                // 可移动斜坡判定
    private bool jumpPressed;                   // 跳跃键判定
    private bool canJump;                       // 可以跳跃判定
    private bool isIdle;                        // 待机判定    
    private bool isWalking;                     // 行走判定
    private bool isCrouching;                   // 下蹲判定
    private bool isRunning;                     // 跑步判定
    private bool shiftInput;                    // 跑步按键判定
    private bool isJumping;                     // 跳跃中判定
    private bool isTouchingLadder;              // 接触梯子判定
    private bool isOnLadder;                    // 在梯子上判定
    private bool isOnLedge;                     // 在梯子顶部判定
    private bool isInWater;                     // 在水中

    private Vector2 newVelocity;                // 速度变量
    private Vector2 newForce;                   // 力学变量
    private Vector2 capsuleColliderSize;        // 碰撞体大小
    private Vector2 ledgeOffsetRemain;          // 梯子顶端过渡位移剩余量
    private Vector2 slopeNormalPerp;            // 斜坡角度法线

    private Rigidbody2D rb;                     // 角色刚体
    private CapsuleCollider2D cc;               // 角色碰撞体
    private CapsuleCollider2D ccCrouch;         // 角色碰撞体
    private Animator anim;                      // 角色动画控制器


    private void Start()
    {// 获得组件
        rb = GetComponent<Rigidbody2D>();
        cc = GetComponent<CapsuleCollider2D>();
        ccCrouch = GetComponents<CapsuleCollider2D>()[1];
        anim = GetComponent<Animator>();

        capsuleColliderSize = cc.size;
        ledgeOffsetRemain = ledgeOffset;
    }

    private void Update()
    {
        // related to all graphic and input
        CheckInput();
        CheckAnimState();
        UpdateAnimation();
    }

    private void FixedUpdate()
    {
        // related to physics and movement
        CheckSurrounding();
        FlipCheck();
        SlopeCheck();
        LadderCheck();
        ApplyMovement();
    }

    private void ApplyMovement()
    {
        // 启用站立时的碰撞体；禁用下蹲时的碰撞体
        cc.enabled = true;
        ccCrouch.enabled = false;

        // 若在梯子上
        if (isOnLadder)
        {
            // 只提供垂直方向的速度
            newVelocity = new Vector2(0.0f, ladderClimbSpeed * yInput);
            rb.velocity = newVelocity;

            // 如果到达了ladder的顶端，静止所有运动，并让角色在一段时间内移动到梯子上方的平台
            if (!isTouchingLadder)
            {
                isOnLedge = true;
                LedgeClimb();
            }
        }
        // 若在水中
        else if (isInWater)
        {
            // 只提供水平方向的游泳速度，下沉速度由水决定
            newVelocity.Set(swimSpeed * xInput, rb.velocity.y);
            rb.velocity = newVelocity;
        }
        // 若在平地上行走/跑动
        else if (isGrounded && !isOnSlope && !isJumping && !isCrouching)
        {
            if (shiftInput)
                newVelocity = new Vector2(runSpeed * xInput, 0.0f);
            else
                newVelocity = new Vector2(walkSpeed * xInput, 0.0f);
            rb.velocity = newVelocity;
        }
        //在平地上下蹲
        else if (isGrounded && !isOnSlope && !isJumping && isCrouching)//if not on slope and crouching
        {
            cc.enabled = false;
            ccCrouch.enabled = true;
            newVelocity = new Vector2(crouchSpeed * xInput, 0.0f);
            rb.velocity = newVelocity;
        }
        //在斜坡上行走/跑动
        else if (isGrounded && isOnSlope && canWalkOnSlope && !isJumping && !isCrouching) //If on slope and not crouchin
        {
            if (shiftInput)
                newVelocity.Set(runSpeed * slopeNormalPerp.x * -xInput, runSpeed * slopeNormalPerp.y * -xInput);
            else
                newVelocity.Set(walkSpeed * slopeNormalPerp.x * -xInput, walkSpeed * slopeNormalPerp.y * -xInput);
            rb.velocity = newVelocity;
        }
        //在斜坡上下蹲
        else if (isGrounded && isOnSlope && canWalkOnSlope && !isJumping && isCrouching) //If on slope and not crouchin
        {
            cc.enabled = false;
            ccCrouch.enabled = true;
            newVelocity.Set(crouchSpeed * slopeNormalPerp.x * -xInput, crouchSpeed * slopeNormalPerp.y * -xInput);
            rb.velocity = newVelocity;
        }
        //在不能走的斜坡上离开
        else if (isGrounded && isOnSlope && !canWalkOnSlope && !isJumping)
        {
            newVelocity.Set(walkSpeed * slopeNormalPerp.x * -xInput, -Mathf.Abs(rb.velocity.y));
            rb.velocity = newVelocity;
        }
        //在空中
        else if (!isGrounded) 
        {
            // 速度不能被改变
            newVelocity.Set(rb.velocity.x, rb.velocity.y);
            rb.velocity = newVelocity;
        }

        // if jump is pressed
        if (jumpPressed == true)
            Jump();
    }

    private void CheckAnimState()
    {

        // 检测人物是否在向上跳跃
        if (rb.velocity.y <= error)
            isJumping = false;
        
        // 检测人物是否可以二次跳跃
        canJump = false;
        // 若人物在接触地面并在可行走的斜坡/平地上 / 若人物在水中
        if ((isGrounded && !isJumping && slopeDownAngle <= maxSlopeAngle && slopeSideAngle <= maxSlopeAngle && !isCrouching) || (isInWater && !isJumping && !isCrouching))
            canJump = true;
        

        // 人物靠墙但是玩家仍然按移动键
        if (Mathf.Abs(rb.velocity.x) < error && Mathf.Abs(rb.velocity.y) < error && xInput != 0)
        {
            isIdle = false;
            isWalking = true;
        }
        // 人物静止
        else if (Mathf.Abs(rb.velocity.x) < error && Mathf.Abs(rb.velocity.y) < error)
        {
            isIdle = true;
            isWalking = false;
        }
        // 人物在地面移动
        else if (Mathf.Abs(rb.velocity.x) > error && isGrounded)
        {
            isWalking = true;
            isIdle = false;
        }

        // 人物在地面跑动
        isRunning = false;
        if (isWalking && shiftInput && !isCrouching)
            isRunning = true;

        // 人物在地面下蹲
        isCrouching = false;
        // 若人在地上（不在梯子上）按下蹲
        if (isGrounded && !isOnLadder && !isOnLedge && yInput == -1f)
            isCrouching = true;
        // 若人在地上（头顶上有东西）但未按下蹲
        else if (isGrounded && !isOnLadder && !isOnLedge && Physics2D.OverlapBox(ceilingCheck.position, ceilingCheckDistance, 0, whatIsGround))
            isCrouching = true;
        
    }

    private void UpdateAnimation()
    {
        anim.SetBool("isIdle", isIdle);
        anim.SetBool("isWalking", isWalking);
        anim.SetBool("isGrounded", isGrounded);
        anim.SetFloat("yVelocity", rb.velocity.y);
        anim.SetFloat("xVelocity", rb.velocity.x);
        anim.SetBool("isOnLadder", isOnLadder);
        anim.SetBool("isOnLedge", isOnLedge);
        anim.SetBool("isRunning", isRunning);
        anim.SetBool("isCrouching", isCrouching);
        anim.SetBool("isInWater", isInWater);
    }

    private void CheckInput()
    {
        xInput = Input.GetAxisRaw("Horizontal");
        yInput = Input.GetAxisRaw("Vertical");
        shiftInput = Input.GetKey(KeyCode.LeftShift);

        if (xInput == 1 && facingDirection == -1)
        {
            Flip();
        }
        else if (xInput == -1 && facingDirection == 1)
        {
            Flip();
        }

        if (Input.GetButtonDown("Jump"))
        {
            jumpPressed = true;
        }

    }

    private void CheckSurrounding()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, whatIsGround);
        isTouchingLadder = Physics2D.Raycast(ladderCheck.position, Vector2.right * facingDirection, ladderCheckDistance, whatIsLadder);
    }

    //slope machenism
    private void SlopeCheck()
    {
        Vector2 checkPos = transform.position - (Vector3)(new Vector2(0.0f, capsuleColliderSize.y / 2));

        SlopeCheckHorizontal(checkPos);
        SlopeCheckVertical(checkPos);
    }

    private void SlopeCheckHorizontal(Vector2 checkPos)
    {
        RaycastHit2D slopeHitFront = Physics2D.Raycast(checkPos, transform.right, slopeCheckDistance, whatIsGround);
        RaycastHit2D slopeHitBack = Physics2D.Raycast(checkPos, -transform.right, slopeCheckDistance, whatIsGround);

        if (slopeHitFront)
        {
            isOnSlope = true;
            slopeSideAngle = Vector2.Angle(slopeHitFront.normal, Vector2.up);

        }
        else if (slopeHitBack)
        {
            isOnSlope = true;
            slopeSideAngle = Vector2.Angle(slopeHitBack.normal, Vector2.up);
        }
        else
        {
            slopeSideAngle = 0.0f;
            isOnSlope = false;
        }
    }

    private void SlopeCheckVertical(Vector2 checkPos)
    {
        RaycastHit2D hit = Physics2D.Raycast(checkPos, Vector2.down, slopeCheckDistance, whatIsGround);

        if (hit)
        {
            slopeNormalPerp = Vector2.Perpendicular(hit.normal).normalized;

            slopeDownAngle = Vector2.Angle(hit.normal, Vector2.up);

            if (slopeDownAngle != lastSlopeAngle)
            {
                isOnSlope = true;
            }

            lastSlopeAngle = slopeDownAngle;

            Debug.DrawLine(hit.point, hit.point + slopeCheckDistance * slopeNormalPerp, Color.cyan);
            Debug.DrawLine(hit.point, hit.point + slopeCheckDistance * hit.normal, Color.green);
            //Debug.DrawRay(hit.point, slopeNormalPerp, Color.cyan);
            //Debug.DrawRay(hit.point,  hit.normal, Color.green);
        }

        if (slopeDownAngle > maxSlopeAngle || slopeSideAngle > maxSlopeAngle)
        {
            canWalkOnSlope = false;
        }
        else
        {
            canWalkOnSlope = true;
        }

        if (isOnSlope && canWalkOnSlope && xInput == 0.0f)
        {
            rb.sharedMaterial = fullFriction;
        }
        else
        {
            rb.sharedMaterial = noFriction;
        }
    }

    private void LadderCheck()
    {
        if (!isOnLadder && isTouchingLadder && yInput > 0.0f)
        {
            isOnLadder = true;
            transform.position = new Vector2(Physics2D.Raycast(ladderCheck.position, Vector2.right * facingDirection, ladderCheckDistance, whatIsLadder).point.x, transform.position.y);
        }
        else if (isOnLadder)
        {
            rb.sharedMaterial = fullFriction;
            rb.gravityScale = 0.0f;

            //按jump键跳下来
            if (jumpPressed)
            {
                //取消update与fixupdate中的延迟
                jumpPressed = false;

                rb.sharedMaterial = noFriction;
                rb.gravityScale = gravityScale;
                isOnLadder = false;
            }
        }
    }

    private void Jump()
    {
        //在FixedUpdate中重制Jump键为false，在update中再次检测，提供平滑手感
        jumpPressed = false;
        if (canJump)
        {
            canJump = false;
            isJumping = true;
            //水平速度保持为跳起前速度不变， 将垂直速度变为0之后再给予一个冲力
            newVelocity.Set(rb.velocity.x, 0.0f);
            rb.velocity = newVelocity;
            if (isGrounded)
                newForce.Set(0.0f, jumpForce);
            else
                newForce.Set(0.0f, waterJumpForce);
            rb.AddForce(newForce, ForceMode2D.Impulse);
        }
    }

    private void LedgeClimb()
    {
        //静止速度
        rb.velocity = Vector2.zero;
        //每帧位移一点
        transform.Translate(new Vector3(ledgeOffset.x * Time.fixedDeltaTime * ledgeClimbSpeed, ledgeOffset.y * Time.fixedDeltaTime * ledgeClimbSpeed, 0));
        //检测是否位移完成
        ledgeOffsetRemain -= new Vector2(ledgeOffset.x * Time.fixedDeltaTime * ledgeClimbSpeed, ledgeOffset.y * Time.fixedDeltaTime * ledgeClimbSpeed);
        //若完成，恢复角色原有物理性质
        if (ledgeOffsetRemain.x <= 0.0f && ledgeOffsetRemain.y <= 0.0f)
        {
            isOnLadder = false;
            isOnLedge = false;
            ledgeOffsetRemain = ledgeOffset;

            // set gravity and friction back
            rb.sharedMaterial = noFriction;
            rb.gravityScale = gravityScale;
        }
    }

    private void FlipCheck()
    {
        if (isOnLadder || (!isGrounded && !isInWater))
        {
            canFlip = false;
        }
        else
        {
            canFlip = true;
        }
    }

    private void Flip()
    {
        if (canFlip)
        {
            facingDirection *= -1;
            transform.Rotate(0.0f, 180.0f, 0.0f);
        }
    }

    private void OnDrawGizmos()
    {
        // 接触地面判定辅助线
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        // 下蹲时头顶判定辅助线
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(ceilingCheck.position, new Vector3(ceilingCheckDistance.x, ceilingCheckDistance.y, 0.0f));
        // 接触地面判定辅助线
        Gizmos.color = Color.red;
        Gizmos.DrawLine(ladderCheck.position, ladderCheck.position + new Vector3(facingDirection * ladderCheckDistance, 0.0f, 0.0f));
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Water")
        {
            isInWater = true;
            // 在水中将重力系数改为0.5
            rb.gravityScale = 0.5f;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Water")
        {
            isInWater = false;
            rb.gravityScale = gravityScale;
        }
    }
}

