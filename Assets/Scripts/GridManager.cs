using System;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public GameObject tilePrefab;
    public GameObject frogPrefab;
    public GameObject grapePrefab;
    public GameObject arrowPrefab;
    public int gridWidth = 5;
    public int gridHeight = 5;
    public float squareSize = 1f;
    public float arrowSpawnChance = 0.2f;

    public Material[] frogMaterials;
    public Material[] grapeMaterials;
    public Material[] tileMaterials;

    private List<GameObject>[,] gridArray;
    private List<GameObject>[,] tileArray;

    public event Action<int, int, int> OnGrapeCollected;
    public event Action<int, int, int> OnFrogSpawned;

    void Start()
    {
        InitializeGrid();
        SpawnInitialFrogs();
        PlaceTilesAndGrapes();
    }
    Vector3 CreateTile(int x, int y)
    {
        Vector3 tilePosition = new Vector3(x * squareSize, 0f, y * squareSize);

        int colorID = UnityEngine.Random.Range(1, tileMaterials.Length + 1);
        GameObject tile = Instantiate(tilePrefab, tilePosition, Quaternion.identity, this.transform);
        tile.GetComponent<Renderer>().material = tileMaterials[colorID - 1];
        tileArray[x, y].Add(tile);

        return tilePosition + new Vector3(0, 0.1f, 0);
    }

    void InitializeGrid()
    {
        gridArray = new List<GameObject>[gridWidth, gridHeight];
        tileArray = new List<GameObject>[gridWidth, gridHeight];

        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight; z++)
            {
                gridArray[x, z] = new List<GameObject>();
                tileArray[x, z] = new List<GameObject>();
            }
        }
    }

    private List<Vector2Int> frogPositions;

    void SpawnInitialFrogs()
    {
        List<Vector2Int> availablePositions = GetAvailablePositions();

        availablePositions = ShuffleList(availablePositions);

        frogPositions = new List<Vector2Int>();

        for (int i = 0; i < frogMaterials.Length; i++)
        {
            Vector2Int position = availablePositions[i];

            SpawnFrog(new Vector3(position.x * squareSize, 0f, position.y * squareSize), i + 1);

            frogPositions.Add(position);
        }
    }


    void PlaceTilesAndGrapes()
    {
        List<Vector2Int> availablePositions = GetAvailablePositions();

        foreach (Vector2Int position in availablePositions)
        {
            CreateTile(position.x, position.y);

            if (frogPositions.Contains(position))
            {
                continue;
            }

            if (UnityEngine.Random.value < arrowSpawnChance)
            {
                PlaceArrow(position.x, position.y);
            }
            else
            {
                StackTilesAndGrapes(position.x, position.y, 3);
            }
        }
    }


    List<Vector2Int> GetAvailablePositions()
    {
        List<Vector2Int> availablePositions = new List<Vector2Int>();
        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight; z++)
            {
                availablePositions.Add(new Vector2Int(x, z));
            }
        }
        return availablePositions;
    }

    void StackTilesAndGrapes(int x, int y, int stackSize)
    {
        int colorID = UnityEngine.Random.Range(1, grapeMaterials.Length + 1);

        for (int i = 0; i < stackSize; i++)
        {
            Vector3 position = CreateTile(x, y);

            GameObject tile = tileArray[x, y][i];
            tile.GetComponent<Renderer>().material = tileMaterials[colorID - 1];
            tile.GetComponent<ColorID>().colorID = colorID;

            GameObject grape = Instantiate(grapePrefab, position + new Vector3(0, 0.2f * i, 0), Quaternion.identity, this.transform);
            grape.GetComponent<Renderer>().material = grapeMaterials[colorID - 1];
            grape.GetComponent<ColorID>().colorID = colorID;

            gridArray[x, y].Add(grape);

            tile.SetActive(i == stackSize - 1);
            grape.SetActive(i == stackSize - 1);
        }
    }



    void PlaceArrow(int x, int y)
    {
        Vector3 arrowPosition = CreateTile(x, y);

        GameObject arrow = Instantiate(arrowPrefab, arrowPosition, Quaternion.identity, this.transform);

        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        Vector2Int direction = directions[UnityEngine.Random.Range(0, directions.Length)];
        arrow.GetComponent<Arrow>().direction = direction;

        arrow.transform.rotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.y));

        gridArray[x, y].Add(arrow);
    }



   public void CollectGrapeAt(int x, int y)
{
    if (gridArray[x, y].Count > 0)
    {
        GameObject topGrape = gridArray[x, y][gridArray[x, y].Count - 1];
        int collectedColorID = topGrape.GetComponent<ColorID>().colorID;

        gridArray[x, y].RemoveAt(gridArray[x, y].Count - 1);
        Destroy(topGrape);

        if (gridArray[x, y].Count > 0)
        {
            GameObject nextGrape = gridArray[x, y][gridArray[x, y].Count - 1];
            nextGrape.SetActive(true);

            int newColorID = nextGrape.GetComponent<ColorID>().colorID;

            GameObject tile = tileArray[x, y][gridArray[x, y].Count - 1];
            tile.GetComponent<Renderer>().material = tileMaterials[newColorID - 1];
            tile.GetComponent<ColorID>().colorID = newColorID;
        }
        else
        {
            OnGrapeCollected?.Invoke(x, y, collectedColorID);

            int newColorID = UnityEngine.Random.Range(1, grapeMaterials.Length + 1);

            Vector3 position = CreateTile(x, y); 

            GameObject newGrape = Instantiate(grapePrefab, position, Quaternion.identity, this.transform);
            newGrape.GetComponent<Renderer>().material = grapeMaterials[newColorID - 1];
            newGrape.GetComponent<ColorID>().colorID = newColorID;

            GameObject tile = tileArray[x, y][0]; 
            tile.GetComponent<Renderer>().material = tileMaterials[newColorID - 1];
            tile.GetComponent<ColorID>().colorID = newColorID;

            gridArray[x, y].Add(newGrape);
            tile.SetActive(true);
            newGrape.SetActive(true);
        }
    }
}
   
    void SpawnFrog(Vector3 position, int colorID)
    {
        GameObject frog = Instantiate(frogPrefab, position, Quaternion.identity, this.transform);
        frog.GetComponentInChildren<SkinnedMeshRenderer>().material = frogMaterials[colorID - 1];
        frog.GetComponentInChildren<ColorID>().colorID = colorID;

        Vector2Int gridPosition = new Vector2Int(Mathf.RoundToInt(position.x / squareSize), Mathf.RoundToInt(position.z / squareSize));
        Vector2Int direction = GetForcedValidDirection(gridPosition);

        FrogController frogController = frog.GetComponent<FrogController>();
        frogController.SetInitialDirection(direction);

       
    }



    Vector2Int GetForcedValidDirection(Vector2Int position)
    {
        // Bottom-left corner
        if (position.x == 0 && position.y == 0)
        {
            return Vector2Int.up;
        }
        // Bottom-right corner
        else if (position.x == gridWidth - 1 && position.y == 0)
        {
            return Vector2Int.up;
        }
        // Top-left corner
        else if (position.x == 0 && position.y == gridHeight - 1)
        {
            return Vector2Int.down;
        }
        // Top-right corner
        else if (position.x == gridWidth - 1 && position.y == gridHeight - 1)
        {
            return Vector2Int.down;
        }
        // Left edge (not corners)
        else if (position.x == 0)
        {
            return Vector2Int.right;
        }
        // Right edge (not corners)
        else if (position.x == gridWidth - 1)
        {
            return Vector2Int.left;
        }
        // Bottom edge (not corners)
        else if (position.y == 0)
        {
            return Vector2Int.up;
        }
        // Top edge (not corners)
        else if (position.y == gridHeight - 1)
        {
            return Vector2Int.down;
        }
        // Anywhere else in the grid
        else
        {
            return Vector2Int.up; 
        }
    }
    
    List<Vector2Int> ShuffleList(List<Vector2Int> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            Vector2Int temp = list[i];
            int randomIndex = UnityEngine.Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
        return list;
    }


    public List<GameObject>[,] GetGridArray() => gridArray;
    public List<GameObject>[,] GetTileArray() => tileArray;
}
