using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControl : MonoBehaviour
{

    private Vector2 moveDirection = Vector2.zero;
    private float grabValue;
    private float jumpValue;
    private float dashValue;
    public Controls controls;
    private InputAction move;
    private InputAction jump;
    private InputAction grab;
    private InputAction dash;

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
    [SerializeField] private bool isDashing;
    [SerializeField] private bool isWallJumping;

    [Header("Jump")]
    public float moveSpeed;
    public float jumpForce;
    public float doubleJumpForce;
    public bool doubleJumpAbility;
    public float coyoteTime;
    public float jumpBufferLength;
    public float smallJumpForce;
    public float gravityMultiplier;

    [Header("Buttons")]
    private bool jumpButtonDown;
    private bool jumpButtonUp;
    private bool jumpButtonStarted;

    [Header("Dashing")]
    public float dashPower;
    public float dashTime;
    public float dashCooldown;
    public bool canDash = true;

    [Header("Collision Radius")]
    public float groundCheckOffset;
    public float wallLeftCheckOffset;
    public float wallRightCheckOffset;

    [Header("Wall Jump")]
    public float slideSpeed;
    public float climbSpeed;
    public bool canWallJumping = true;
    public float wallJumpTime = 0.1f;

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

        dash = controls.Main.Dash;
        dash.Enable();
        dash.performed += _ => Dash();
    }
    private void OnDisable()
    {
        move.Disable();
        jump.Disable();
        grab.Disable();
        dash.Disable();
    }
    void Start()
    {
        anim = GetComponent<Animator>();
        playerSR = GetComponent<SpriteRenderer>();
        gravityStore = playerRB.gravityScale;
    }
    void FixedUpdate()
    {
        if (isWallJumping)
        {
            return;
        }
        if (isDashing)
        {
            return;
        }
        //saða sola hareket
        grabValue = grab.ReadValue<float>();//basýlýyorsa 1 basýlmýyorsa 0
        jumpValue = jump.ReadValue<float>();
        dashValue = dash.ReadValue<float>();
        moveDirection = move.ReadValue<Vector2>();
        Debug.Log(moveDirection);
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
        if (isGrounded )
        {
            canDoubleJump = true;
        }
        //wall slide
        if (moveDirection.x == -1 && isOnWallLeft && !isGrounded)
        {
            isWallSliding = true;
        }
        else if (moveDirection.x == 1 && isOnWallRight && !isGrounded)
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
        //animasyon
        anim.SetBool("isGrounded", isGrounded);
        anim.SetFloat("moveSpeed", Mathf.Abs(playerRB.velocity.x));
        wallGrab();
        if (playerRB.velocity.x < 0)
        {
            playerSR.flipX = true;
        }
        else if (playerRB.velocity.x > 0)
        {
            playerSR.flipX = false;
        }
    }
    //input sisteminden gelen zýplama eylemi
    public void Jump(InputAction.CallbackContext context)//CONTEXTI JUMP YAP
    {
        jumpButtonDown = context.performed;
        jumpButtonUp = context.canceled;
        jumpButtonStarted = context.started;

        if (context.performed)
        {
            jumpBufferCounter = jumpBufferLength;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }
        //zýplama ve sonsuz zýplamayý önleme
        if (jumpBufferCounter >= 0 && coyoteCounter > 0f)
        {
            playerRB.velocity = new Vector2(playerRB.velocity.x, jumpForce);
            jumpBufferCounter = 0;
        }
        else if (context.performed)
        {
            doubleJump();
        }
        
        //tuþtan elini çekince az zýplama
        if (context.canceled && playerRB.velocity.y > 0)
        {
            playerRB.velocity = new Vector2(playerRB.velocity.x, playerRB.velocity.y * smallJumpForce);
        }
        else if (context.canceled)
        {
            doubleJump();
        }
        //wall jumping
        if(context.performed && (isOnWallLeft || isOnWallRight) && canWallJumping)
        {
            StartCoroutine(WallJumping());
        }
    }
    public void doubleJump()
    {
        if (doubleJumpAbility)
        {
            if (canDoubleJump)
            {
                playerRB.velocity = new Vector2(playerRB.velocity.x, jumpForce * doubleJumpForce);
                canDoubleJump = false;
            }
        }

    }
    private IEnumerator WallJumping()
    {
       // canWallJumping = false;
        isWallJumping = true;
        if (isOnWallRight)
        {
            playerRB.velocity = new Vector2(-moveSpeed, jumpForce);           
        }
        else if (isOnWallLeft)
        {
            playerRB.velocity = new Vector2(moveSpeed, jumpForce);            
        }
        yield return new WaitForSeconds(wallJumpTime);
        isWallJumping = false;
    }
    private void wallGrab()
    {
        //wall grab
        if (isGrabbing)
        {
            playerRB.gravityScale = 0f;
            playerRB.velocity = Vector2.zero;
        }
        else
        {
            playerRB.gravityScale = gravityStore;
        }
        //climb and slide
        if (isGrabbing && moveDirection.y == -1 )
        {
            playerRB.velocity = new Vector2(playerRB.velocity.x, -slideSpeed); 
        }
        else if(isGrabbing && moveDirection.y == 1)
        {
            playerRB.velocity = new Vector2(playerRB.velocity.x, climbSpeed);
        }
        //jumping while grabbing
        if(isGrabbing && jumpButtonDown)
        {
            playerRB.velocity = new Vector2(playerRB.velocity.x, jumpForce);
            isGrabbing = false;
        }
        else if(isGrabbing && jumpButtonUp)
        {
            playerRB.velocity = new Vector2(playerRB.velocity.x, playerRB.velocity.y * smallJumpForce);
        }
    }
    private void Grab(InputAction.CallbackContext context)
    {
        if (context.performed && (isOnWallLeft || isOnWallRight) && !isGrounded)
        {
            isGrabbing = true;
            canWallJumping = false;
        }
        else if(context.canceled)
        {
            isGrabbing = false;
            canWallJumping = true;
        }
    }
    private void Dash()
    {
        if (canDash)
        {
            StartCoroutine(Dashing());
        }
       
    }
    private IEnumerator Dashing()
    {

        canDash = false;
        isDashing = true;
        playerRB.gravityScale = 0;
        if (moveDirection.x != 0 && moveDirection.y != 0)
        {
            playerRB.velocity = new Vector2(moveDirection.x * dashPower, moveDirection.y * dashPower);
        }
        else if (moveDirection.x != 0)
        {
            playerRB.velocity = new Vector2(moveDirection.x * dashPower, 0f);
        }
        else if (moveDirection.y != 0)
        {
            playerRB.velocity = new Vector2(0f, moveDirection.y * dashPower);
            playerRB.gravityScale = gravityStore;
        }
        yield return new WaitForSeconds(dashTime);
        playerRB.gravityScale = gravityStore;
        isDashing = false;
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;

    }
}
