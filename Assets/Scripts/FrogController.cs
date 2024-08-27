using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrogController : MonoBehaviour
{
    public GameObject tonguePrefab;  // Reference to the tongue prefab
    public float tongueSpeed = 10f;  // Speed at which the tongue extends
    public float maxTongueLength = 1f;  // Maximum length the tongue can extend

    private GridManager gridManager;

    void Start()
    {
        Initialize();
    }

    void Initialize()
    {
        gridManager = FindObjectOfType<GridManager>();
        if (gridManager == null)
        {
            Debug.LogError("GridManager not found. Ensure it is in the scene.");
        }
    }

    public void OnFrogClicked()
    {
        // Extend the tongue in the direction the frog is facing
        ExtendTongue();
    }

    void ExtendTongue()
    {
        // Instantiate the tongue at the frog's position
        GameObject tongue = Instantiate(tonguePrefab, transform.position, Quaternion.identity);
        
        // Determine the direction the frog is facing based on its rotation and reverse it
        Vector3 direction = -transform.forward;  // Reverse the frog's forward direction

        StartCoroutine(ExtendTongueCoroutine(tongue, direction));
    }

    IEnumerator ExtendTongueCoroutine(GameObject tongue, Vector3 direction)
{
    Vector3 startPosition = transform.position;
    Vector3 currentPosition = startPosition;
    float currentLength = 0f;

    while (true)
    {
        float step = tongueSpeed * Time.deltaTime;
        currentLength += step;

        // Correctly scale and position the tongue depending on the direction
        if (direction == Vector3.forward || direction == Vector3.back)
        {
            // Vertical extension (up or down)
            tongue.transform.localScale = new Vector3(tongue.transform.localScale.x, tongue.transform.localScale.y, currentLength);
            tongue.transform.position = startPosition + direction * (currentLength / 2);
        }
        else if (direction == Vector3.right || direction == Vector3.left)
        {
            // Horizontal extension (left or right)
            tongue.transform.localScale = new Vector3(currentLength, tongue.transform.localScale.y, tongue.transform.localScale.z);
            tongue.transform.position = startPosition + direction * (currentLength / 2);
        }

        yield return null;

        // Check if the tongue has reached the next tile
        if (currentLength >= gridManager.squareSize)
        {
            // Update the current position to the next tile
            currentPosition += direction * gridManager.squareSize;
            currentLength = 0f;  // Reset the length for the next segment

            // Check if the current position is within grid boundaries
            if (!IsPositionWithinGrid(currentPosition))
            {
                break;  // Stop extending if the next position is outside the grid
            }

            // Check for an arrow on the current tile and adjust the direction
            if (CheckAndChangeDirectionIfArrow(currentPosition, ref direction))
            {
                // Adjust the start position for the next segment after changing direction
                startPosition = currentPosition;
                continue;  // Skip to the next loop iteration to start extending in the new direction
            }

            // Check for matching grapes in the current tile
            if (!CheckAndCollectGrapeAtTarget(currentPosition))
            {
                break;  // Stop extending if no matching grape is found
            }

            // Adjust the start position for the next segment
            startPosition = currentPosition;
        }
    }

    // Destroy the tongue after reaching the target
    Destroy(tongue);
}

    bool CheckAndChangeDirectionIfArrow(Vector3 position, ref Vector3 direction)
    {
        int x = Mathf.RoundToInt(position.x / gridManager.squareSize);
        int z = Mathf.RoundToInt(position.z / gridManager.squareSize);

        // Check for arrows on the target tile
        foreach (GameObject obj in gridManager.GetGridArray()[x, z])
        {
            Arrow arrow = obj.GetComponent<Arrow>();
            if (arrow != null)
            {
                // Change the direction based on the arrow's direction
                direction = new Vector3(arrow.direction.x, 0, arrow.direction.y);
                return true;  // Indicate that the direction has changed
            }
        }
        return false;  // No arrow found, direction remains the same
    }



    bool IsPositionWithinGrid(Vector3 position)
    {
        int x = Mathf.RoundToInt(position.x / gridManager.squareSize);
        int z = Mathf.RoundToInt(position.z / gridManager.squareSize);

        return x >= 0 && x < gridManager.gridWidth && z >= 0 && z < gridManager.gridHeight;
    }


bool CheckAndCollectGrapeAtTarget(Vector3 targetPosition)
{
    int x = Mathf.RoundToInt(targetPosition.x / gridManager.squareSize);
    int z = Mathf.RoundToInt(targetPosition.z / gridManager.squareSize);

    // Check for grapes on the target tile
    foreach (GameObject obj in gridManager.GetGridArray()[x, z])
    {
        ColorID grapeColorID = obj.GetComponent<ColorID>();
        if (grapeColorID != null && grapeColorID.colorID == GetComponentInChildren<ColorID>().colorID)
        {
            // Collect the grape if the colors match
            gridManager.CollectGrapeAt(x, z);
            return true;
        }
    }
    return false;  // Return false if no matching grape is found
}


    public void SetInitialDirection(Vector2Int direction)
    {
        if (direction == Vector2Int.down)
        {
            transform.rotation = Quaternion.Euler(0f, 0f, 0f); // Down corresponds to 0 degrees rotation
        }
        else if (direction == Vector2Int.up)
        {
            transform.rotation = Quaternion.Euler(0f, 180f, 0f); // Up corresponds to 180 degrees rotation
        }
        else if (direction == Vector2Int.right)
        {
            transform.rotation = Quaternion.Euler(0f, 270f, 0f); // Right corresponds to 90 degrees rotation
        }
        else if (direction == Vector2Int.left)
        {
            transform.rotation = Quaternion.Euler(0f, 90f, 0f); // Left corresponds to 270 degrees rotation
        }
    }


   
}
