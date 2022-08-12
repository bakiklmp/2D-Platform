using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControl : MonoBehaviour
{   
    private Vector2 moveDirection = Vector2.zero;
    private float grabValue;
    private float jumpValue;
    public Controls controls;
    private InputAction move;
    private InputAction jump;
    private InputAction grab;

    private float coyoteCounter;
    private float jumpBufferCounter;
    private bool canDoubleJump;
    private float gravityStore;
    
    private Animator anim;
    private SpriteRenderer playerSR;

    [Header("Checks")]
    [SerializeField] private bool isGrounded;
    [SerializeField] private bool isOnWallLeft;
    [SerializeField] private bool isOnWallRight;
    [SerializeField] private bool isWallSliding;
    [SerializeField] private bool isGrabbing;

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

    [Header("Wall Jump")]
    public float wallJumpTime = 0.1f;
    private float wallJumpCounter;

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
        gravityStore = playerRB.gravityScale;
        
    }
    void Update()
    {
        if (wallJumpCounter <= 0)
        {
            //saða sola hareket
            grabValue = grab.ReadValue<float>();//basýlýyorsa 1 basýlmýyorsa 0
            jumpValue = jump.ReadValue<float>();
            moveDirection = move.ReadValue<Vector2>();
            playerRB.velocity = new Vector2(moveSpeed * moveDirection.x, playerRB.velocity.y);
            //zemin tespiti
            isGrounded = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckOffset, whatIsGround);
            isOnWallLeft = Physics2D.OverlapCircle(wallLeftCheckPoint.position, wallLeftCheckOffset, whatIsGround);
            isOnWallRight = Physics2D.OverlapCircle(wallRightCheckPoint.position, wallRightCheckOffset, whatIsGround);
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
            if ((isOnWallLeft||isOnWallRight) && !isGrounded)
            {
                isWallSliding = true;
            }
            else
            {
                isWallSliding = false;
            }
            if (isWallSliding)
            {
                playerRB.velocity = new Vector2(playerRB.velocity.x, -slideSpeed);
            }
            wallGrab();
            //animasyon
            anim.SetBool("isGrounded", isGrounded);
            anim.SetFloat("moveSpeed", Mathf.Abs(playerRB.velocity.x));

            //karakterin baktýðý yöne dönmesi
            if (playerRB.velocity.x < 0)
            {
                playerSR.flipX = true;
            }
            else if (playerRB.velocity.x > 0)
            {
                playerSR.flipX = false;
            }
        }
        else
        {
            wallJumpCounter -= Time.deltaTime;
        }   
    }
    //input sisteminden gelen zýplama eylemi
     public void Jump(InputAction.CallbackContext context)//CONTEXTI JUMP YAP
    {
        if(context.performed)
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
            
        }
        else if(context.canceled)
        {
            doubleJump();
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
    private void wallGrab()
    {
        //wall grab
        if (isGrabbing)
        {
            playerRB.gravityScale = 0f;
            playerRB.velocity = Vector2.zero;
            if (jumpValue==1)
            {
                wallJumpCounter = wallJumpTime;
                if (isOnWallRight)
                {
                    playerRB.velocity = new Vector2(-moveSpeed, jumpForce);
                    playerRB.gravityScale = gravityStore;
                    isGrabbing = false;

                }
                else if (isOnWallLeft)
                {
                    playerRB.velocity = new Vector2(moveSpeed, jumpForce);
                    playerRB.gravityScale = gravityStore;
                    isGrabbing = false;
                    doubleJump();
                }
            }
        }
        else
        {
            playerRB.gravityScale = gravityStore;
        }
    }
    private void Grab(InputAction.CallbackContext context)
    {        
            if (context.performed && (isOnWallLeft || isOnWallRight) && !isGrounded)
            {
                isGrabbing = true;
            }
            else
            {
                isGrabbing = false;
            }        
    }
 
}
