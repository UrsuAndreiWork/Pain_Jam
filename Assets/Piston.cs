using UnityEngine;

public class Piston : MonoBehaviour
{
    public Transform playerTransform;  // Reference to the player's transform
    public float triggerDistance = 5f; // Distance at which the object starts moving
    public float moveSpeed = 10f;      // Speed at which the object moves
    public float returnSpeed = 2f;     // Speed at which the object returns to its initial position
    public float maximumDistance = 20f; // Maximum distance the object can move
    public Vector3 initialPosition;    // The initial position of the object

    public bool moveLeft = true;       // Checkbox to decide the direction of movement

    private bool isReturning = false;  // Flag to check if the object is returning

    void Start()
    {
        initialPosition = transform.position; // Store the initial position
    }

    void Update()
    {
        // Determine if we should consider the player based on direction
        if (!isReturning)
        {
            if (ShouldMove())
            {
                // Player is close on the correct side, move rapidly
                Move();
            }
            else
            {
                // Player is not close, return to the initial position
                StartReturning();
            }
        }
        else
        {
            ReturnToInitialPosition();
        }
    }

    bool ShouldMove()
    {
        // Calculate the horizontal distance between the player and the object
        float horizontalDistance = playerTransform.position.x - transform.position.x;

        // Check if the player is within the trigger distance and on the correct side
        if (moveLeft && horizontalDistance <= 0 && Mathf.Abs(horizontalDistance) <= triggerDistance)
        {
            return true; // Move left and player is to the left within range
        }
        else if (!moveLeft && horizontalDistance >= 0 && Mathf.Abs(horizontalDistance) <= triggerDistance)
        {
            return true; // Move right and player is to the right within range
        }

        return false; // Player is not in the relevant direction or out of range
    }

    void Move()
    {
        // Decide the direction based on the moveLeft boolean
        Vector3 direction = moveLeft ? Vector3.left : Vector3.right;

        // Move the object but limit the distance
        float distanceMoved = Vector3.Distance(initialPosition, transform.position);
        if (distanceMoved < maximumDistance)
        {
            transform.position += direction * moveSpeed * Time.deltaTime;
        }
        else
        {
            StartReturning();
        }
    }

    void StartReturning()
    {
        // Begin the return process
        isReturning = true;
    }

    void ReturnToInitialPosition()
    {
        // Return the object to its initial position slowly
        if (transform.position != initialPosition)
        {
            transform.position = Vector3.MoveTowards(transform.position, initialPosition, returnSpeed * Time.deltaTime);
        }
        else
        {
            // Once returned, stop returning and allow interaction again
            isReturning = false;
        }
    }
}
