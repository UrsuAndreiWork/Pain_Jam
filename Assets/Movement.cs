using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float maxJumpForce = 10f;
    public float chargeSpeed = 0.5f;
    public float minJumpForce = 0.5f;
    public float jumpCooldown = 0.5f;
    public LayerMask groundLayer;

    public LineRenderer lineRenderer;
    public int trajectoryResolution = 100;

    private Rigidbody2D rb;
    private BoxCollider2D boxCollider;
    private bool isCharging = false;
    private bool isGrounded = false;
    private bool canJump = true;
    private float currentJumpForce = 0f;
    private int jumpDirection = 0;

    private bool onSlipperySurface = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = trajectoryResolution;
        lineRenderer.enabled = false; // Hide the line renderer initially
    }

    void Update()
    {
        isGrounded = IsGrounded();

        if (isGrounded && canJump)
        {
            if (Input.GetKey(KeyCode.Space))
            {
                StartCharging();
                DrawJumpArc();
            }

            if (Input.GetKeyUp(KeyCode.Space))
            {
                Jump();
                lineRenderer.enabled = false;
            }

            if (isCharging)
            {
                ChargeJump();
                SetJumpDirection();
            }
            else if (!onSlipperySurface)
            {
                HandleMovement();
            }
        }

        // Reset charging if player touches the ground
        if (isGrounded && !isCharging)
        {
            ResetJumpState();
        }
    }

    private bool IsGrounded()
    {
        RaycastHit2D hit = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0f, Vector2.down, 0.1f, groundLayer);
        return hit.collider != null;
    }

    private void StartCharging()
    {
        if (!isCharging)
        {
            isCharging = true;
            currentJumpForce = minJumpForce;
            lineRenderer.enabled = true;  // Show the arc when charging starts
        }
    }

    private void ChargeJump()
    {
        if (currentJumpForce < maxJumpForce)
        {
            currentJumpForce += chargeSpeed * Time.deltaTime;
        }
    }

    private void SetJumpDirection()
    {
        if (Input.GetKey(KeyCode.A))
        {
            jumpDirection = -1;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            jumpDirection = 1;
        }
        else
        {
            jumpDirection = 0;
        }
    }

    private void Jump()
    {
        isCharging = false;
        canJump = false;

        Vector2 jumpVector = Vector2.up * currentJumpForce;

        if (jumpDirection != 0)
        {
            float horizontalForceMultiplier = 0.5f;
            jumpVector += Vector2.right * jumpDirection * (currentJumpForce * horizontalForceMultiplier);
        }

        rb.velocity = jumpVector;

        StartCoroutine(JumpCooldown());
    }

    private void HandleMovement()
    {
        float moveInput = Input.GetAxis("Horizontal");

        if (onSlipperySurface)
        {
            moveInput = 0f;  // Disable horizontal movement on slippery surface
        }

        // Move the character
        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);

        // Flip the sprite based on the direction of movement
        if (moveInput > 0) // Moving right
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
        else if (moveInput < 0) // Moving left
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
    }

    private IEnumerator JumpCooldown()
    {
        yield return new WaitForSeconds(jumpCooldown);
        canJump = true;
    }

    private void DrawJumpArc()
    {
        Vector2 startPosition = transform.position;
        Vector2 startVelocity = Vector2.up * currentJumpForce;

        if (jumpDirection != 0)
        {
            float horizontalForceMultiplier = 0.5f;
            startVelocity += Vector2.right * jumpDirection * (currentJumpForce * horizontalForceMultiplier);
        }

        for (int i = 0; i < trajectoryResolution; i++)
        {
            float time = i / (float)trajectoryResolution;
            Vector2 position = CalculatePositionAtTime(startPosition, startVelocity, time);
            lineRenderer.SetPosition(i, position);
        }
    }

    private Vector2 CalculatePositionAtTime(Vector2 startPosition, Vector2 startVelocity, float time)
    {
        return startPosition + startVelocity * time + 0.5f * Physics2D.gravity * (time * time);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("SlipperySurface"))
        {
            onSlipperySurface = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("SlipperySurface"))
        {
            onSlipperySurface = false;
        }
    }

    private void ResetJumpState()
    {
        isCharging = false;
        jumpDirection = 0;
        currentJumpForce = 0f;
        lineRenderer.enabled = false;  // Hide the arc when on the ground
    }
}
