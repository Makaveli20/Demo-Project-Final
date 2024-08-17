using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class FrogMovement : MonoBehaviour
{
    public float moveSpeed = 5f;  // Speed at which the frog moves
    private Vector3 targetPosition; // The target position for the frog
    private List<GameObject>[,] gridArray; // Reference to the grid array in SimpleGrid
    private List<GameObject>[,] tileArray; // Reference to the tile array in SimpleGrid

    private int frogColorID; // The frog's color ID
    private bool isMoving = false; // Track whether the frog is currently moving
    private SimpleGrid simpleGrid; // Reference to the SimpleGrid script for spawning grapes

    private float squareSize; // Store the square size

    void Start()
    {
        targetPosition = transform.position;

        // Get the ColorID component to find the frog's color ID
        frogColorID = GetComponentInChildren<ColorID>().colorID;

        // Get the reference to the grid and tile arrays from the SimpleGrid script
        simpleGrid = GameObject.FindObjectOfType<SimpleGrid>();
        gridArray = simpleGrid.GetGridArray();
        tileArray = simpleGrid.GetTileArray();

        // Get the square size from SimpleGrid
        squareSize = simpleGrid.squareSize;
    }

    void Update()
    {
        if (isMoving)
        {
            // Move towards the target position
            if (transform.position != targetPosition)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            }
            else
            {
                // Stop moving once the target position is reached
                isMoving = false;

                // Collect the grape at the current position
                int x = Mathf.RoundToInt(transform.position.x / squareSize);
                int z = Mathf.RoundToInt(transform.position.z / squareSize);
                simpleGrid.CollectGrapeAt(x, z);

                // Continue moving to the next matching grape
                MoveToMatchingGrape(x, z);
            }
        }
    }

    public void OnFrogClicked()
    {
        if (!isMoving) // Only start moving if the frog is not already moving
        {
            int x = Mathf.RoundToInt(transform.position.x / squareSize);
            int z = Mathf.RoundToInt(transform.position.z / squareSize);
            MoveToMatchingGrape(x, z);
        }
    }

    void MoveToMatchingGrape(int x, int z)
    {
        // Try to move in each direction; prioritize the first valid direction found
        if (CheckAndMoveToSameColoredGrape(x, z - 1)) return;  // Up (Z+)
        if (CheckAndMoveToSameColoredGrape(x, z + 1)) return;  // Down (Z-)
        if (CheckAndMoveToSameColoredGrape(x - 1, z)) return;  // Left (X-)
        if (CheckAndMoveToSameColoredGrape(x + 1, z)) return;  // Right (X+)

        // If no valid moves, stop the frog
        isMoving = false;

        // Optionally, you can call simpleGrid.SpawnNewGrapes() here if you want to spawn new grapes after movement ends.
    }

    bool CheckAndMoveToSameColoredGrape(int x, int z)
    {
        // Ensure we're within bounds
        if (x >= 0 && x < gridArray.GetLength(0) && z >= 0 && z < gridArray.GetLength(1))
        {
            if (gridArray[x, z].Count > 0)
            {
                GameObject targetGrape = gridArray[x, z][gridArray[x, z].Count - 1];
                ColorID targetColorID = targetGrape.GetComponent<ColorID>();
                if (targetColorID != null && targetColorID.colorID == frogColorID)
                {
                    // Set the new target position and start moving
                    targetPosition = targetGrape.transform.position;
                    isMoving = true;
                    return true;
                }
            }
        }
        return false;
    }
}