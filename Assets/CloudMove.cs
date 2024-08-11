using UnityEngine;

public class CloudMove : MonoBehaviour
{
    private float speed;
    private bool moveToRight;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        // Move the cloud horizontally based on its direction
        Vector3 direction = moveToRight ? Vector3.right : Vector3.left;
        transform.Translate(direction * speed * Time.deltaTime);

        // Destroy the cloud when it goes off screen to the opposite side
        if ((moveToRight && transform.position.x > mainCamera.ViewportToWorldPoint(new Vector3(1.1f, 0, 0)).x) ||
            (!moveToRight && transform.position.x < mainCamera.ViewportToWorldPoint(new Vector3(-0.1f, 0, 0)).x))
        {
            Destroy(gameObject);
        }
    }

    public void SetMovement(bool startFromLeft, float newSpeed)
    {
        moveToRight = startFromLeft; // Move right if starting from left, and vice versa
        speed = newSpeed;
    }
}
