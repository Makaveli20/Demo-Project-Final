using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrogController : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Vector3 targetPosition;
    private List<GameObject>[,] gridArray;
    private List<Vector2Int> movementRoute;

    private int frogColorID;
    private bool isMoving = false;
    private GridManager gridManager;

    private float squareSize;
    private Vector2Int lastTilePosition;
    private Vector2Int facingDirection;

    void Start()
    {
        Initialize();
    }

    void Initialize()
    {
        targetPosition = transform.position;
        frogColorID = GetComponentInChildren<ColorID>().colorID;
        gridManager = FindObjectOfType<GridManager>();
        gridArray = gridManager.GetGridArray();
        squareSize = gridManager.squareSize;
        movementRoute = new List<Vector2Int>();

        facingDirection = Vector2Int.down; // Frog initially faces upward

        gridManager.OnGrapeCollected += OnGrapeCollected;
    }

    void OnDestroy()
    {
        gridManager.OnGrapeCollected -= OnGrapeCollected;
    }

    void Update()
    {
        if (isMoving)
        {
            MoveFrog();
        }
    }

    void MoveFrog()
    {
        if (transform.position != targetPosition)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
        }
        else
        {
            isMoving = false;

            int x = Mathf.RoundToInt(transform.position.x / squareSize);
            int z = Mathf.RoundToInt(transform.position.z / squareSize);

            // Check if there is an arrow at this position and rotate the frog
            foreach (GameObject obj in gridArray[x, z])
            {
                Arrow arrow = obj.GetComponent<Arrow>();
                if (arrow != null)
                {
                    RotateFrogTowards(arrow.direction);
                    facingDirection = arrow.direction;

                    // After rotating, check if there is a matching grape in the new direction
                    if (TryMoveToMatchingGrape(x, z))
                    {
                        return; // Start moving towards the matching grape
                    }
                    break;
                }
            }

            // Collect the grape at the current position
            gridManager.CollectGrapeAt(x, z);

            lastTilePosition = new Vector2Int(x, z);
            movementRoute.Add(lastTilePosition);

            // Attempt to move to the next matching grape
            if (!TryMoveToMatchingGrape(x, z))
            {
                FinalizeMovement();
            }
        }
    }


    public void OnFrogClicked()
    {
        if (!isMoving)
        {
            int x = Mathf.RoundToInt(transform.position.x / squareSize);
            int z = Mathf.RoundToInt(transform.position.z / squareSize);
            TryMoveToMatchingGrape(x, z);
        }
    }

    bool TryMoveToMatchingGrape(int x, int z)
    {
        // Adjust x and z by the facing direction
        int newX = x + facingDirection.x;
        int newZ = z + facingDirection.y;

        // Check for the arrow on the current tile first
        foreach (GameObject obj in gridArray[x, z])
        {
            Arrow arrow = obj.GetComponent<Arrow>();
            if (arrow != null)
            {
                // Rotate the frog to match the arrow's direction
                RotateFrogTowards(arrow.direction);
                facingDirection = arrow.direction;

                // Adjust newX and newZ after direction change
                newX = x + facingDirection.x;
                newZ = z + facingDirection.y;

                // Move the frog in the new direction
                if (CheckAndMoveToGrape(newX, newZ))
                {
                    return true;
                }
            }
        }

        // Check for grapes in the new position
        return CheckAndMoveToGrape(newX, newZ);
    }



    bool CheckAndMoveToGrape(int x, int z)
    {
        if (x >= 0 && x < gridArray.GetLength(0) && z >= 0 && z < gridArray.GetLength(1))
        {
            // Check for an arrow on this tile first
            foreach (GameObject obj in gridArray[x, z])
            {
                Arrow arrow = obj.GetComponent<Arrow>();
                if (arrow != null)
                {
                    // Rotate the frog to match the arrow's direction
                    RotateFrogTowards(arrow.direction);
                    facingDirection = arrow.direction;

                    // Set the frog's target position to this arrow tile
                    targetPosition = new Vector3(x * squareSize, transform.position.y, z * squareSize);
                    isMoving = true;
                    return true;
                }
            }

            // Check for a grape on this tile
            if (gridArray[x, z].Count > 0)
            {
                GameObject targetGrape = gridArray[x, z][gridArray[x, z].Count - 1];
                ColorID targetColorID = targetGrape.GetComponent<ColorID>();
                if (targetColorID != null && targetColorID.colorID == frogColorID)
                {
                    targetPosition = targetGrape.transform.position;
                    isMoving = true;
                    return true;
                }
            }
        }
        return false;
    }



    void RotateFrogTowards(Vector2Int direction)
    {
        float angle = 0f;

        // Determine the angle to rotate based on the direction
        if (direction == Vector2Int.down)
        {
            angle = 0f;  // Down corresponds to 0 degrees rotation (initial direction)
        }
        else if (direction == Vector2Int.up)
        {
            angle = 180f; // Up corresponds to 180 degrees rotation
        }
        else if (direction == Vector2Int.right)
        {
            angle = 90f; // Right corresponds to 90 degrees rotation
        }
        else if (direction == Vector2Int.left)
        {
            angle = 270f; // Left corresponds to 270 degrees rotation
        }

        // Apply the rotation to the frog's transform
        transform.rotation = Quaternion.Euler(0f, angle, 0f);
    }



    public void SetInitialDirection(Vector2Int direction)
    {
        facingDirection = direction;
        RotateFrogTowards(direction);
    }




    void FinalizeMovement()
    {
        if (movementRoute.Count > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, movementRoute.Count);
            Vector2Int spawnPosition = movementRoute[randomIndex];

            // Pass the current facing direction to the new frog
            gridManager.SpawnFrogAt(spawnPosition.x, spawnPosition.y, frogColorID, facingDirection);

            // Remove the spawn position from the movement route so no other object spawns there
            movementRoute.RemoveAt(randomIndex);
        }

        // Spawn either a grape or an arrow on the remaining tiles in the movement route
        foreach (var position in movementRoute)
        {
            if (UnityEngine.Random.value > 0.5f)
            {
                gridManager.SpawnGrapeAt(position.x, position.y);
            }
            else
            {
                gridManager.SpawnArrowAt(position.x, position.y);
            }
        }

        Destroy(gameObject);
    }




    void OnGrapeCollected(int x, int y, int collectedColorID)
    {
        // React to the grape being collected, if necessary
    }
}
