using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrogController : MonoBehaviour
{
    public GameObject tonguePrefab;  
    public float tongueSpeed = 10f;  
    public float maxTongueLength = 1f;  

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
       
        ExtendTongue();
    }

    void ExtendTongue()
    {
        
        GameObject tongue = Instantiate(tonguePrefab, transform.position, Quaternion.identity);
        
       
        Vector3 direction = -transform.forward;  

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

        
        if (direction == Vector3.forward || direction == Vector3.back)
        {
            tongue.transform.localScale = new Vector3(tongue.transform.localScale.x, tongue.transform.localScale.y, currentLength);
            tongue.transform.position = startPosition + direction * (currentLength / 2);
        }
        else if (direction == Vector3.right || direction == Vector3.left)
        {
            tongue.transform.localScale = new Vector3(currentLength, tongue.transform.localScale.y, tongue.transform.localScale.z);
            tongue.transform.position = startPosition + direction * (currentLength / 2);
        }

        yield return null;

        if (currentLength >= gridManager.squareSize)
        {
            currentPosition += direction * gridManager.squareSize;
            currentLength = 0f;  

            if (!IsPositionWithinGrid(currentPosition))
            {
                break; 
            }

            if (CheckAndChangeDirectionIfArrow(currentPosition, ref direction))
            {
                startPosition = currentPosition;
                continue;  
            }

            if (!CheckAndCollectGrapeAtTarget(currentPosition))
            {
                break; 
            }

            startPosition = currentPosition;
        }
    }

    Destroy(tongue);
}

    bool CheckAndChangeDirectionIfArrow(Vector3 position, ref Vector3 direction)
    {
        int x = Mathf.RoundToInt(position.x / gridManager.squareSize);
        int z = Mathf.RoundToInt(position.z / gridManager.squareSize);

        foreach (GameObject obj in gridManager.GetGridArray()[x, z])
        {
            Arrow arrow = obj.GetComponent<Arrow>();
            if (arrow != null)
            {
                
                direction = new Vector3(arrow.direction.x, 0, arrow.direction.y);
                return true;
            }
        }
        return false;  
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

    
    foreach (GameObject obj in gridManager.GetGridArray()[x, z])
    {
        ColorID grapeColorID = obj.GetComponent<ColorID>();
        if (grapeColorID != null && grapeColorID.colorID == GetComponentInChildren<ColorID>().colorID)
        {
           
            gridManager.CollectGrapeAt(x, z);
            return true;
        }
    }
    return false;  
}


    public void SetInitialDirection(Vector2Int direction)
    {
        if (direction == Vector2Int.down)
        {
            transform.rotation = Quaternion.Euler(0f, 0f, 0f); 
        }
        else if (direction == Vector2Int.up)
        {
            transform.rotation = Quaternion.Euler(0f, 180f, 0f); 
        }
        else if (direction == Vector2Int.right)
        {
            transform.rotation = Quaternion.Euler(0f, 270f, 0f); 
        }
        else if (direction == Vector2Int.left)
        {
            transform.rotation = Quaternion.Euler(0f, 90f, 0f); 
        }
    }


   
}
