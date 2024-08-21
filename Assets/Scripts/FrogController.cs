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
            gridManager.CollectGrapeAt(x, z);

            lastTilePosition = new Vector2Int(x, z);
            movementRoute.Add(lastTilePosition);

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
        int newX = x + facingDirection.x;
        int newZ = z + facingDirection.y;
    
        if (CheckAndMoveToGrape(newX, newZ))
        {
            return true;
        }
    
        // Check for an arrow on the current tile and adjust direction accordingly
        foreach (GameObject obj in gridArray[x, z])
        {
            Arrow arrow = obj.GetComponent<Arrow>();
            if (arrow != null)
            {
                facingDirection = arrow.direction;
                // Try moving in the new direction after adjusting for the arrow
                newX = x + facingDirection.x;
                newZ = z + facingDirection.y;
                if (CheckAndMoveToGrape(newX, newZ))
                {
                    return true;
                }
            }
        }

        return false;
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
                    // Change the frog's direction to the arrow's direction
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


    void FinalizeMovement()
    {
        if (movementRoute.Count > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, movementRoute.Count);
            Vector2Int spawnPosition = movementRoute[randomIndex];
            gridManager.SpawnFrogAt(spawnPosition.x, spawnPosition.y, frogColorID);
        }

        Destroy(gameObject);
    }

    void OnGrapeCollected(int x, int y, int collectedColorID)
    {
        // React to the grape being collected, if necessary
    }
}
