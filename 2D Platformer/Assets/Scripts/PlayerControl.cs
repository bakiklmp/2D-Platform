using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControl : MonoBehaviour
{
    public float moveSpeed;
    public float jumpForce;
    public Rigidbody2D playerRB;
    public Controls controls;
    public float jumpDownForce;

    public float coyoteTime;
    private float coyoteCounter;

    public float jumpBufferLength;
    private float jumpBufferCounter;

    private bool isGrounded;
    public Transform groundCheckPoint;
    public LayerMask whatIsGround;

    private bool canDoubleJump;

    private Animator anim;
    private SpriteRenderer playerSR;

    Vector2 moveDirection = Vector2.zero;
    private InputAction move;
    private InputAction jump;
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
    }
    private void OnDisable() 
    {
        move.Disable();
        jump.Disable();
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
        isGrounded = Physics2D.OverlapCircle(groundCheckPoint.position, .2f, whatIsGround);
        //coyote time
        if (isGrounded)
        {
            coyoteCounter = coyoteTime;
        }
        else
        {
            coyoteCounter -= Time.deltaTime;
        }
        

        //double jump boolean
        if (isGrounded)
        {
            canDoubleJump = true;
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
            //double jump
            if (canDoubleJump)
            {
                playerRB.velocity = new Vector2(playerRB.velocity.x, jumpForce);
                canDoubleJump = false;
            }
        }
        //tuþtan elini çekince az zýplama
        if (context.canceled && playerRB.velocity.y > 0)
        {
            playerRB.velocity = new Vector2(playerRB.velocity.x, playerRB.velocity.y * jumpDownForce);
        }
        else if(context.canceled)
        {
            if (canDoubleJump)
            {
                playerRB.velocity = new Vector2(playerRB.velocity.x, jumpForce);
                canDoubleJump = false;
            }
        }             
    }
}
