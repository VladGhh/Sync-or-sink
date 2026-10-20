using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public float airControl = 5f;
    public float interactRange = 1f;
    public Animator animator;

    private Rigidbody2D rb;
    private bool isPlayer1;

    public Transform groundCheck;
    public float groundCheckRadius = 0.5f;
    public LayerMask whatIsGround;

    private bool isGrounded;
    private bool canJump = true;
    private float moveHorizontal;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        if (gameObject.name == "Player1")
        {
            isPlayer1 = true;
        }
        else if (gameObject.name == "Player2")
        {
            isPlayer1 = false;
        }
        else
        {
            Debug.LogError("hello????");
        }
        //make sure which player is which
    }

    void Update()
    {
        if (isPlayer1)
        {
            moveHorizontal = Input.GetAxisRaw("Horizontal1");
        }
        else
        {
            moveHorizontal = Input.GetAxisRaw("Horizontal2");
        }

        if (moveHorizontal > 0)
        {
            animator.SetBool("IsWalking", true);
            transform.localScale = new Vector3(.5f, .5f, 1f);
        }
        else if (moveHorizontal < 0)
        {
            animator.SetBool("IsWalking", true);
            transform.localScale = new Vector3(-0.5f, .5f, 1f);
        }
        else
        {
            animator.SetBool("IsWalking", false);
        }
        //animations for moving on horizontal axes

        isGrounded = CheckIfGrounded();

        if (isGrounded && rb.linearVelocity.y <= 0.01f)
        {
            canJump = true;
        }

        if (canJump && ((isPlayer1 && Input.GetKeyDown(KeyCode.W)) || (!isPlayer1 && Input.GetKeyDown(KeyCode.UpArrow))))
        {
            Jump();
        }

        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, interactRange);
        foreach (Collider2D collider in colliders)
        {
            Interactable interactable = collider.GetComponent<Interactable>();
            if (interactable != null)
            {
                interactable.ShowPrompt();

                if ((isPlayer1 && Input.GetKeyDown(KeyCode.E)) || (!isPlayer1 && Input.GetKeyDown(KeyCode.RightShift)))
                {
                    interactable.Interact();
                }
            }
        }
    }

    void FixedUpdate()
    {
        float targetSpeed = moveHorizontal * moveSpeed;

        if (isGrounded)
        {
            rb.linearVelocity = new Vector2(targetSpeed, rb.linearVelocity.y);
        }
        else
        {
            float newX = Mathf.Lerp(rb.linearVelocity.x, targetSpeed, airControl * Time.fixedDeltaTime);
            rb.linearVelocity = new Vector2(newX, rb.linearVelocity.y);
        }

        if (rb.linearVelocity.y < -0.01f && !animator.GetBool("IsWalking"))
        {
            animator.SetBool("IsJumping", false);
            animator.SetBool("IsFalling", true);
        }
        else if (rb.linearVelocity.y > 0.01f && !animator.GetBool("IsWalking"))
        {
            animator.SetBool("IsFalling", false);
            animator.SetBool("IsJumping", true);
        }
        else
        {
            animator.SetBool("IsFalling", false);
            animator.SetBool("IsJumping", false);
        }
    }

    private bool CheckIfGrounded()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(groundCheck.position, groundCheckRadius, whatIsGround);
        foreach (Collider2D col in colliders)
        {
            if (col.gameObject != gameObject)
            {
                return true;
            }
        }
        return false;
    }

    private void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        canJump = false;
    }
}