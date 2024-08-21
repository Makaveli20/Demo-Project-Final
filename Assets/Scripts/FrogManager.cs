using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrogManager : MonoBehaviour
{
    public GameObject frogPrefab;
    public Material[] frogMaterials;
    private GameObject[] frogs;

    public void InitializeFrogs(int count, float squareSize, int gridHeight)
    {
        frogs = new GameObject[count];
        for (int i = 0; i < count; i++)
        {
            Vector3 position = new Vector3(i * squareSize, 0f, (gridHeight - 1) * squareSize);
            GameObject frog = Instantiate(frogPrefab, position, Quaternion.identity, this.transform);
            frog.GetComponentInChildren<SkinnedMeshRenderer>().material = frogMaterials[i];
            frog.GetComponentInChildren<ColorID>().colorID = i + 1;
            frogs[i] = frog;
        }
    }

    public void SpawnFrogAt(int x, int y, int frogColorID)
    {
        // Logic for spawning a frog
    }
}
