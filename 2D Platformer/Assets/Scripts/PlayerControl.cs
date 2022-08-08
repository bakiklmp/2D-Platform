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
    private void OnEnable()
    {
        move = controls.Main.Move;
        move.Enable();
        jump = controls.Main.Jump;
        jump.Enable();
        jump.performed += Jump;
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
        moveDirection = move.ReadValue<Vector2>();

        playerRB.velocity = new Vector2(moveSpeed* moveDirection.x, playerRB.velocity.y);

        isGrounded = Physics2D.OverlapCircle(groundCheckPoint.position, .2f, whatIsGround);

        if (isGrounded)
        {
            canDoubleJump = true;
        }
        anim.SetBool("isGrounded", isGrounded);
        anim.SetFloat("moveSpeed",Mathf.Abs(playerRB.velocity.x));

        if (playerRB.velocity.x < 0)
        {
            playerSR.flipX = true;
        }else if(playerRB.velocity.x > 0)
        {
            playerSR.flipX = false;
        }
    }
    private void Jump(InputAction.CallbackContext context)
    {
        if (isGrounded)
        {
        playerRB.velocity = new Vector2(playerRB.velocity.x, jumpForce);
        }
        else
        {
            if (canDoubleJump)
            {
                playerRB.velocity = new Vector2(playerRB.velocity.x, jumpForce);
                canDoubleJump = false;
            }
        }      
    }
}
