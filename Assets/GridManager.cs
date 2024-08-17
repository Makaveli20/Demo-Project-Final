using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class SimpleGrid : MonoBehaviour
{
    public GameObject tilePrefab;      // The prefab for the grid tiles
    public GameObject frogPrefab;
    public GameObject grapePrefab;
    public int gridWidth = 5;
    public int gridHeight = 5;
    public float squareSize = 1f;

    public Material[] frogMaterials;   // Array to hold the frog materials
    public Material[] grapeMaterials;  // Array to hold the grape materials
    public Material[] tileMaterials;   // Array to hold the tile materials, matching grape colors

    private List<GameObject>[,] gridArray; // List to hold stacked grapes
    private List<GameObject>[,] tileArray; // List to hold stacked tiles
    private GameObject[] frogs;

    void Start()
    {
        CreateGrid();
        SpawnFrogs();
        PlaceTilesAndGrapes();
    }

    void Update()
    {
        // Detect mouse click input
        if (Input.GetMouseButtonDown(0)) // Left-click
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                // Check if the clicked object is a frog
                if (hit.collider != null && hit.collider.gameObject.CompareTag("Frog"))
                {
                    FrogMovement frogMovement = hit.collider.gameObject.GetComponent<FrogMovement>();
                    if (frogMovement != null)
                    {
                        frogMovement.OnFrogClicked(); // Trigger frog movement
                    }
                }
            }
        }
    }

    void CreateGrid()
    {
        gridArray = new List<GameObject>[gridWidth, gridHeight];
        tileArray = new List<GameObject>[gridWidth, gridHeight];

        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight; z++)
            {
                gridArray[x, z] = new List<GameObject>(); // Initialize the list for grapes
                tileArray[x, z] = new List<GameObject>(); // Initialize the list for tiles
            }
        }
    }

    void SpawnFrogs()
    {
        frogs = new GameObject[gridWidth];

        for (int i = 0; i < gridWidth; i++)
        {
            Vector3 position = new Vector3(i * squareSize, 0f, (gridHeight - 1) * squareSize);
            GameObject frog = Instantiate(frogPrefab, position, Quaternion.identity, this.transform);

            // Assign a specific material and ID to the frog
            frog.GetComponentInChildren<SkinnedMeshRenderer>().material = frogMaterials[i];
            frog.GetComponentInChildren<ColorID>().colorID = i + 1; // IDs from 1 to 5

            frogs[i] = frog;
        }
    }

    void PlaceTilesAndGrapes()
    {
        List<Vector2Int> availablePositions = new List<Vector2Int>();

        // Collect all positions except the bottom row
        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight - 1; z++)  // Exclude the bottom row where the frogs are placed
            {
                availablePositions.Add(new Vector2Int(x, z)); // Use Vector2Int for grid coordinates
            }
        }

        // Shuffle the positions randomly
        availablePositions = ShuffleList(availablePositions);

        // Ensure that each frog has a matching neighbor tile and grape
        for (int i = 0; i < frogs.Length; i++)
        {
            Vector3 frogPosition = frogs[i].transform.position;
            int frogX = Mathf.RoundToInt(frogPosition.x / squareSize);
            int frogZ = Mathf.RoundToInt(frogPosition.z / squareSize) - 1; // Check the tile above the frog

            if (frogZ >= 0)
            {
                Vector2Int matchingPosition = new Vector2Int(frogX, frogZ);
                StackTilesAndGrapes(matchingPosition.x, matchingPosition.y, i + 1, 3); // Stack 3 tiles and grapes
                availablePositions.Remove(matchingPosition); // Remove the used position
            }
        }

        // Stack the remaining tiles and grapes randomly
        int id = 1;
        foreach (var pos in availablePositions)
        {
            StackTilesAndGrapes(pos.x, pos.y, id, 3); // Stack 3 tiles and grapes per tile
            id++;
            if (id > 5) id = 1;
        }
    }

    void StackTilesAndGrapes(int x, int y, int id, int stackSize)
    {
        for (int i = 0; i < stackSize; i++)
        {
            // Position the tile and grape
            Vector3 position = new Vector3(x * squareSize, i * 0.1f, y * squareSize);

            // Instantiate and stack the tile
            GameObject tile = Instantiate(tilePrefab, position, Quaternion.identity, this.transform);
            tile.GetComponent<Renderer>().material = tileMaterials[id - 1];
            tileArray[x, y].Add(tile);

            // Instantiate and stack the grape on the tile
            GameObject grape = Instantiate(grapePrefab, position + new Vector3(0, 0.2f, 0), Quaternion.identity, this.transform);
            grape.GetComponent<Renderer>().material = grapeMaterials[id - 1];
            grape.GetComponent<ColorID>().colorID = id;
            gridArray[x, y].Add(grape);

            // Initially hide all but the top grape and tile
            if (i < stackSize - 1)
            {
                tile.SetActive(false);
                grape.SetActive(false);
            }
        }
    }

    public void CollectGrapeAt(int x, int y)
    {
        if (gridArray[x, y].Count > 0 && tileArray[x, y].Count > 0)
        {
            // Remove the top grape
            GameObject topGrape = gridArray[x, y][gridArray[x, y].Count - 1];
            gridArray[x, y].RemoveAt(gridArray[x, y].Count - 1);
            Destroy(topGrape);

            // Remove the top tile
            GameObject topTile = tileArray[x, y][tileArray[x, y].Count - 1];
            tileArray[x, y].RemoveAt(tileArray[x, y].Count - 1);
            Destroy(topTile);

            // Show the next tile and grape if they exist
            if (gridArray[x, y].Count > 0 && tileArray[x, y].Count > 0)
            {
                gridArray[x, y][gridArray[x, y].Count - 1].SetActive(true);
                tileArray[x, y][tileArray[x, y].Count - 1].SetActive(true);
            }
        }
    }

    public void SpawnGrapeAt(int x, int z, int frogColorID)
    {
        // Ensure a grape is only spawned if there is a corresponding tile
        if (tileArray[x, z].Count == 0)
        {
            return; // Do not spawn a grape if there is no tile
        }

        // Ensure that a grape does not spawn on the frog's current position
        foreach (var frog in frogs)
        {
            Vector3 frogPosition = frog.transform.position;
            int frogX = Mathf.RoundToInt(frogPosition.x / squareSize);
            int frogZ = Mathf.RoundToInt(frogPosition.z / squareSize);

            if (x == frogX && z == frogZ)
            {
                return; // Don't spawn a grape here
            }
        }

        // Calculate the position on the grid
        Vector3 position = new Vector3(x * squareSize, tileArray[x, z].Count * 0.1f, z * squareSize);

        // Instantiate the grape prefab at the calculated position
        GameObject grape = Instantiate(grapePrefab, position + new Vector3(0, 0.2f, 0), Quaternion.identity, this.transform);

        // Assign the correct material and color ID to the grape
        grape.GetComponent<Renderer>().material = grapeMaterials[frogColorID - 1];
        grape.GetComponent<ColorID>().colorID = frogColorID;

        // Add the grape to the top of the stack for the grid position
        gridArray[x, z].Add(grape);

        // Ensure only the top grape is visible
        HideAllButTopGrape(x, z);
    }

    private void HideAllButTopGrape(int x, int z)
    {
        for (int i = 0; i < gridArray[x, z].Count - 1; i++)
        {
            gridArray[x, z][i].SetActive(false);
        }
        gridArray[x, z][gridArray[x, z].Count - 1].SetActive(true);
    }

    List<Vector2Int> ShuffleList(List<Vector2Int> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            Vector2Int temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
        return list;
    }

    public List<GameObject>[,] GetGridArray()
    {
        return gridArray;
    }

    public List<GameObject>[,] GetTileArray()
    {
        return tileArray;
    }
}
