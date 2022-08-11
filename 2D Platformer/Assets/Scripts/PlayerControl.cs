using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControl : MonoBehaviour
{   
    private Vector2 moveDirection = Vector2.zero;
    public Controls controls;
    private InputAction move;
    private InputAction jump;
    private InputAction grab;

    [SerializeField] private bool isGrounded,isOnWall;
    private float coyoteCounter;
    private float jumpBufferCounter;
    private bool canDoubleJump;
    [SerializeField] private bool isWallSliding;
    

    private Animator anim;
    private SpriteRenderer playerSR;
    
    [Header("Basic")]
    public float moveSpeed;
    public float jumpForce;
    public float doubleJumpForce;

    [Header("Advanced")]
    public float groundCheckOffset;
    public float wallLeftCheckOffset;
    public float wallRightCheckOffset;
    public float coyoteTime;
    public float jumpBufferLength;
    public float smallJumpForce;
    public float gravityMultiplier;
    public float slideSpeed;   

    [Header("Wall Jumping")]
    public float wallJumpTime;
    public Vector2 wallJumpForce;
    private bool wallJumping;

    [Header("References")]
    public Transform groundCheckPoint;
    public Transform wallLeftCheckPoint;
    public Transform wallRightCheckPoint;
    public LayerMask whatIsGround;
    public Rigidbody2D playerRB;
    private void Awake()
    {
        controls = new Controls();
    }
    private void OnEnable() //input sistemi aktive etme
    {
        move = controls.Main.Move;
        move.Enable();

        jump = controls.Main.Jump;
        jump.Enable();
        jump.performed += Jump;
        jump.canceled += Jump;

        grab = controls.Main.Grab;
        grab.Enable();
        grab.performed += Grab;
        grab.canceled += Grab;
    }
    private void OnDisable() 
    {
        move.Disable();
        jump.Disable();
        grab.Disable();
    }
    void Start()
    {
        anim = GetComponent<Animator>();
        playerSR = GetComponent<SpriteRenderer>();
    }
    void Update()
    {
        //saða sola hareket
        moveDirection = move.ReadValue<Vector2>();
        playerRB.velocity = new Vector2(moveSpeed* moveDirection.x, playerRB.velocity.y);

        //zemin tespiti
        isGrounded = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckOffset, whatIsGround);
        isOnWall = Physics2D.OverlapCircle(wallLeftCheckPoint.position, wallLeftCheckOffset, whatIsGround)
            || Physics2D.OverlapCircle(wallRightCheckPoint.position, wallRightCheckOffset, whatIsGround);
        //coyote time
        if (isGrounded)
        {
            coyoteCounter = coyoteTime;
        }
        else
        {
            coyoteCounter -= Time.deltaTime;
        }
        //hýzlý düþme
        if (playerRB.velocity.y < 0)
        {
            playerRB.velocity += Vector2.up * Physics2D.gravity.y * (gravityMultiplier - 1) * Time.deltaTime;
        }
        //double jump boolean
        if (isGrounded)
        {
            canDoubleJump = true;
        }        
        //wall slide
        if(isOnWall && !isGrounded && moveDirection.x !=0)
        {
            isWallSliding = true;
        }
        else
        {
            isWallSliding = false;
        }
        if (isWallSliding)
        {
            playerRB.velocity = new Vector2(playerRB.velocity.x,-slideSpeed);
        }
        //wall jump
        if (wallJumping)
        {
            playerRB.velocity = new Vector2(wallJumpForce.x * -moveDirection.x, wallJumpForce.y);
        }
        //animasyon
        anim.SetBool("isGrounded", isGrounded);
        anim.SetFloat("moveSpeed",Mathf.Abs(playerRB.velocity.x));

        //karakterin baktýðý yöne dönmesi
        if (playerRB.velocity.x < 0)
        {
            playerSR.flipX = true;
        }else if(playerRB.velocity.x > 0)
        {
            playerSR.flipX = false;
        }
    }
    //input sisteminden gelen zýplama eylemi
     private void Jump(InputAction.CallbackContext context)//CONTEXTI JUMP YAP
    {
        if (context.performed)
        {
            jumpBufferCounter = jumpBufferLength;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }
        //zýplama ve sonsuz zýplamayý önleme
        if (jumpBufferCounter >=0 && coyoteCounter > 0f)
        {            
            playerRB.velocity = new Vector2(playerRB.velocity.x, jumpForce);
            jumpBufferCounter = 0;
        }        
        else if(context.performed)
        {
            doubleJump();
        }
        //tuþtan elini çekince az zýplama
        if (context.canceled && playerRB.velocity.y > 0)
        {
            playerRB.velocity = new Vector2(playerRB.velocity.x, playerRB.velocity.y * smallJumpForce);
        }
        else if(context.canceled)
        {
            doubleJump();
        }
        if(context.performed && isWallSliding)
        {
            wallJumping = true;
            Invoke("setWallJumpFalse", wallJumpTime);
        }
    }
    public void doubleJump()
    {
        if (canDoubleJump)
        {
            playerRB.velocity = new Vector2(playerRB.velocity.x, jumpForce * doubleJumpForce);
            canDoubleJump = false;
        }
    }
    public void setWallJumpFalse()
    {
        wallJumping = false;
    }
    private void Grab(InputAction.CallbackContext context)
    {
        if(context.performed )
        {
            Debug.Log("ZZZZZZZ");
        }
    }
}
