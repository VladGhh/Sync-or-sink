using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public float airControl = 5f;
    public float interactRange = 1f;
    public float pushSpeed = 4f;            // cat de repede merge cand impinge cutia
    public float pushCheckDistance = .7f;  // cat de departe se uita dupa cutie
    public Animator animator;

    private Rigidbody2D rb;
    private bool isPlayer1;

    public Transform groundCheck;
    public float groundCheckRadius = 0.05f;
    public LayerMask whatIsGround;

    public bool isCarryingItem = false;
    
    private bool isGrounded;
    private bool isBoxInFront;
    private bool canJump = true;
    private bool isPushing;
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
        isBoxInFront = IsBoxInFront();
        isPushing = IsBoxInFront()&&isGrounded;
        animator.SetBool("IsPushing", isPushing);

        if (moveHorizontal > 0 )
        {
            animator.SetBool("IsWalking", true);
            transform.localScale = new Vector3(.5f, .5f, 1f);
        }
        else if (moveHorizontal < 0 )
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
        
        if (isCarryingItem && ((isPlayer1 && Input.GetKeyDown(KeyCode.E)) || (!isPlayer1 && Input.GetKeyDown(KeyCode.RightShift))))
        {
            PickableItem heldItem = GetComponentInChildren<PickableItem>();
            if (heldItem != null)
            {
                heldItem.Drop();
            }
        }
        else
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, interactRange);
            foreach (Collider2D collider in colliders)
            {
                Interactable interactable = collider.GetComponent<Interactable>();
                if (interactable != null)
                {
                    interactable.ShowPrompt();

                    if ((isPlayer1 && Input.GetKeyDown(KeyCode.E)) || (!isPlayer1 && Input.GetKeyDown(KeyCode.RightShift)))
                    {
                        interactable.Interact(this);
                        break;
                    }
                }
            }
        }
        
    }

    void FixedUpdate()
    {
        //daca care un item si incearca sa impinga o cutie, sta pe loc
        if (isCarryingItem && isPushing)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }
       

        float targetSpeed = moveHorizontal * (isPushing ? pushSpeed : moveSpeed);

        //daca e in aer nu poate sa impinga cutii
        if (!isGrounded && isBoxInFront)
        {
            targetSpeed = 0f;
        }

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

    // true daca are o cutie in fata
    private bool IsBoxInFront()
    {
        if ( moveHorizontal == 0) return false;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, new Vector2(moveHorizontal, 0f), pushCheckDistance, LayerMask.GetMask("Movable"));
        return hit.collider != null && hit.collider.GetComponent<Box>() != null;
    }

    private void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        canJump = false;
    }
}