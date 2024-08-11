using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Clouds : MonoBehaviour
{
    public GameObject[] cloudPrefabs;   // Array of cloud prefabs to spawn
    public float spawnInterval = 6f;    // Time between spawns
    public float minSpeed = 1f;         // Minimum speed of the clouds
    public float maxSpeed = 3f;         // Maximum speed of the clouds
    public int minClouds = 1;           // Minimum number of clouds to spawn each interval
    public int maxClouds = 3;           // Maximum number of clouds to spawn each interval

    private Camera mainCamera;
    private float minY;                 // Minimum Y position for cloud spawning
    private float maxY;                 // Maximum Y position for cloud spawning

    void Start()
    {
        mainCamera = Camera.main;

        // Calculate the top half of the camera's view dynamically
        float cameraHeight = mainCamera.orthographicSize;

        // Set the Y boundaries for spawning clouds in the top half of the screen
        minY = mainCamera.transform.position.y + cameraHeight / 4;
        maxY = mainCamera.transform.position.y + cameraHeight;

        StartCoroutine(SpawnClouds());
    }

    IEnumerator SpawnClouds()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            int cloudCount = Random.Range(minClouds, maxClouds + 1); // Random number of clouds to spawn
            for (int i = 0; i < cloudCount; i++)
            {
                SpawnCloud();
            }
        }
    }

    void SpawnCloud()
    {
        // Choose a random cloud prefab
        int randomIndex = Random.Range(0, cloudPrefabs.Length);
        GameObject cloud = Instantiate(cloudPrefabs[randomIndex]);

        // Randomly decide whether to spawn from the left or right
        bool startFromLeft = Random.Range(0, 2) == 0;

        // Calculate starting X position based on the side
        float randomX = startFromLeft
            ? mainCamera.ViewportToWorldPoint(new Vector3(-0.1f, 0, 0)).x // Start just off the left edge of the screen
            : mainCamera.ViewportToWorldPoint(new Vector3(1.1f, 0, 0)).x; // Start just off the right edge of the screen

        // Set random Y position within the top half of the screen
        float randomY = Random.Range(minY, maxY);

        cloud.transform.position = new Vector3(randomX, randomY, 0);

        // Get the CloudMovement component and set the movement if it exists
        CloudMove cloudMovement = cloud.GetComponent<CloudMove>();
        if (cloudMovement != null)
        {
            float speed = Random.Range(minSpeed, maxSpeed);
            cloudMovement.SetMovement(startFromLeft, speed);
        }
        else
        {
            Debug.LogError("CloudMovement component is missing on the cloud prefab.");
        }
    }
}
