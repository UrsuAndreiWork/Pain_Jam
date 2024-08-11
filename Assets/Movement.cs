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

    private Vector2 lastPosition;
    private float stuckTimer = 0f;
    private float stuckThreshold = 4f;  // Time in seconds to consider player stuck

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = trajectoryResolution;
        lineRenderer.enabled = false; // Hide the line renderer initially

        lastPosition = transform.position;
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

        // Check if the player is stuck
        CheckIfStuck();

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

        // Ensure the jump vector is upwards initially
        Vector2 jumpVector = new Vector2(0, currentJumpForce);

        if (jumpDirection != 0)
        {
            float horizontalForceMultiplier = 0.5f;
            jumpVector.x = jumpDirection * (currentJumpForce * horizontalForceMultiplier);
        }

        // Apply the jump vector as velocity
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

        // Flip the sprite based on the direction of movement without changing the dimensions
        if (moveInput > 0) // Moving right
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else if (moveInput < 0) // Moving left
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
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
        // Detect if the player has landed on a ground layer surface
        if (((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            ResetJumpState();
        }

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

    private void CheckIfStuck()
    {
        // If the player is not grounded and position hasn't changed significantly
        if (!isGrounded && (Vector2.Distance(transform.position, lastPosition) < 0.01f))
        {
            stuckTimer += Time.deltaTime;

            // If the player has been stuck for more than the threshold
            if (stuckTimer >= stuckThreshold)
            {
                TeleportToClosestObject();
                stuckTimer = 0f;  // Reset the stuck timer
            }
        }
        else
        {
            stuckTimer = 0f;  // Reset the stuck timer if the player is moving or grounded
        }

        lastPosition = transform.position;
    }

    private void TeleportToClosestObject()
    {
        // Find all colliders in the scene
        Collider2D[] allColliders = FindObjectsOfType<Collider2D>();
        Collider2D closestCollider = null;
        float closestDistance = Mathf.Infinity;

        foreach (Collider2D collider in allColliders)
        {
            // Ignore the player's own collider
            if (collider == boxCollider) continue;

            // Calculate the distance to this collider
            float distance = Vector2.Distance(transform.position, collider.bounds.center);

            // Check if this is the closest one so far
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestCollider = collider;
            }
        }

        if (closestCollider != null)
        {
            // Teleport the player to the top center of the closest object
            Vector2 teleportPosition = new Vector2(closestCollider.bounds.center.x, closestCollider.bounds.max.y + boxCollider.bounds.extents.y);
            transform.position = teleportPosition;
        }
    }
}
