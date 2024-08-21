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

    void SpawnInitialFrogs()
    {
        for (int i = 0; i < gridWidth; i++)
        {
            Vector3 position = new Vector3(i * squareSize, 0f, (gridHeight - 1) * squareSize);
            SpawnFrog(position, i + 1);
        }
    }

    void PlaceTilesAndGrapes()
    {
        List<Vector2Int> availablePositions = GetAvailablePositions();
        foreach (Vector2Int position in availablePositions)
        {
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
            for (int z = 0; z < gridHeight - 1; z++)
            {
                availablePositions.Add(new Vector2Int(x, z));
            }
        }
        return ShuffleList(availablePositions);
    }

    void StackTilesAndGrapes(int x, int y, int stackSize)
    {
        for (int i = 0; i < stackSize; i++)
        {
            int colorID = UnityEngine.Random.Range(1, tileMaterials.Length + 1);
            Vector3 position = new Vector3(x * squareSize, i * 0.1f, y * squareSize);

            GameObject tile = Instantiate(tilePrefab, position, Quaternion.identity, this.transform);
            tile.GetComponent<Renderer>().material = tileMaterials[colorID - 1];
            tileArray[x, y].Add(tile);

            GameObject grape = Instantiate(grapePrefab, position + new Vector3(0, 0.2f, 0), Quaternion.identity, this.transform);
            grape.GetComponent<Renderer>().material = grapeMaterials[colorID - 1];
            grape.GetComponent<ColorID>().colorID = colorID;
            gridArray[x, y].Add(grape);

            tile.SetActive(i == stackSize - 1);  // Activate only the top tile
            grape.SetActive(i == stackSize - 1); // Activate only the top grape
        }
    }

    void PlaceArrow(int x, int y)
    {
        Vector3 position = new Vector3(x * squareSize, 0.1f, y * squareSize);
        GameObject arrow = Instantiate(arrowPrefab, position, Quaternion.identity, this.transform);

        // Randomly assign a direction to the arrow (up, down, left, right)
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        Vector2Int direction = directions[UnityEngine.Random.Range(0, directions.Length)];
        arrow.GetComponent<Arrow>().direction = direction;

        // Rotate the arrow according to its direction
        arrow.transform.rotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.y));

        // Add the arrow to the grid array for potential future use
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
                gridArray[x, y][gridArray[x, y].Count - 1].SetActive(true);
                tileArray[x, y][gridArray[x, y].Count - 1].GetComponent<Renderer>().material =
                    gridArray[x, y][gridArray[x, y].Count - 1].GetComponent<Renderer>().material;
            }
            else
            {
                OnGrapeCollected?.Invoke(x, y, collectedColorID);
            }
        }
    }

    public void SpawnFrogAt(int x, int y, int frogColorID)
    {
        RemoveAllGrapesAt(x, y);
        Vector3 position = new Vector3(x * squareSize, tileArray[x, y].Count * 0.1f, y * squareSize);
        SpawnFrog(position, frogColorID);
        OnFrogSpawned?.Invoke(x, y, frogColorID);
    }

    void SpawnFrog(Vector3 position, int colorID)
    {
        GameObject frog = Instantiate(frogPrefab, position, Quaternion.identity, this.transform);
        frog.GetComponentInChildren<SkinnedMeshRenderer>().material = frogMaterials[colorID - 1];
        frog.GetComponentInChildren<ColorID>().colorID = colorID;
    }

    void RemoveAllGrapesAt(int x, int y)
    {
        foreach (var grape in gridArray[x, y])
        {
            Destroy(grape);
        }
        gridArray[x, y].Clear();
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
