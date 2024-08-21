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
        float currentLength = 0f;

        while (currentLength < maxTongueLength)
        {
            float step = tongueSpeed * Time.deltaTime;
            currentLength += step;

            // Move the tongue in the reversed direction the frog is facing
            tongue.transform.position += direction * step;

            yield return null;
        }

        // Once the tongue reaches its maximum length, check for a grape
        CheckForGrapeAtTarget(tongue.transform.position);
        
        // Destroy the tongue after reaching the target
        Destroy(tongue);
    }

    void CheckForGrapeAtTarget(Vector3 targetPosition)
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
                break;
            }
        }
    }

    public void SetInitialDirection(Vector2Int direction)
    {
        RotateFrogTowards(direction);
    }

    void RotateFrogTowards(Vector2Int direction)
    {
        float angle = 0f;

        if (direction == Vector2Int.down)
        {
            angle = 180f;  // Down corresponds to 180 degrees rotation
        }
        else if (direction == Vector2Int.up)
        {
            angle = 0f; // Up corresponds to 0 degrees rotation
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
}
