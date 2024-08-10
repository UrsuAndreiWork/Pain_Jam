using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arc_Movement : MonoBehaviour
{
    public float startX = -5f;  // Starting X position (left side)
    public float endX = 5f;     // Ending X position (right side)
    public float height = 8f;   // Maximum height of the arc (how high the object goes)
    public float speed = 3f;    // Speed of the movement in units per second
    public Vector3 fixedPoint = new Vector3(0f, 0f, 0f); // Fixed point for the line

    private float journeyLength;
    private float startTime;
    private bool movingRight = true;  // Track the direction of movement
    private LineRenderer lineRenderer;
    private Renderer objectRenderer;

    void Start()
    {
        // Set the initial position of the object
        transform.position = new Vector3(startX, CalculateY(startX), transform.position.z);

        // Calculate the total journey length between the start and end points
        journeyLength = Mathf.Abs(endX - startX);

        // Record the time when the movement started
        startTime = Time.time;

        // Get the LineRenderer component attached to this GameObject
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            Debug.LogError("LineRenderer component missing from the GameObject.");
            return;
        }

        // Get the Renderer component to calculate the bounds of the object
        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer == null)
        {
            Debug.LogError("Renderer component missing from the GameObject.");
            return;
        }

        // Set the line color to black
        lineRenderer.startColor = Color.black;
        lineRenderer.endColor = Color.black;

        // Set the line width to be thinner
        lineRenderer.startWidth = 0.05f;  // Adjust as needed for desired thickness
        lineRenderer.endWidth = 0.05f;    // Adjust as needed for desired thickness

        // Set the initial line positions
        lineRenderer.positionCount = 2;
        UpdateLinePosition();
    }

    void Update()
    {
        // Calculate the time since the movement started
        float timeElapsed = (Time.time - startTime) * speed;

        // Calculate the fraction of the journey completed based on time
        float fractionOfJourney = timeElapsed / journeyLength;

        // Determine the current X position based on direction
        float newX;
        if (movingRight)
        {
            newX = Mathf.Lerp(startX, endX, fractionOfJourney);
        }
        else
        {
            newX = Mathf.Lerp(endX, startX, fractionOfJourney);
        }

        // Calculate the new Y position based on the arc equation
        float newY = CalculateY(newX);

        // Move the object to the new position
        transform.position = new Vector3(newX, newY, transform.position.z);

        // Update the line position
        UpdateLinePosition();

        // Reverse direction when reaching the end points
        if (fractionOfJourney >= 1f)
        {
            movingRight = !movingRight;  // Switch direction
            startTime = Time.time;  // Reset the start time for the new journey
        }
    }

    float CalculateY(float x)
    {
        // A simple parabolic function to create an arc
        float normalizedX = (x - startX) / (endX - startX);

        // Create a downward parabola: -(normalizedX * (1 - normalizedX))
        return height * (-(normalizedX * (1 - normalizedX)));
    }

    void UpdateLinePosition()
    {
        // Set the fixed endpoint of the line
        lineRenderer.SetPosition(0, fixedPoint);

        // Calculate the center of the object using its Renderer component
        Vector3 objectCenter = objectRenderer.bounds.center;

        // Set the following endpoint of the line to the center of the object
        lineRenderer.SetPosition(1, objectCenter);
    }
}
