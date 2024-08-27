using UnityEngine;

public class Arrow : MonoBehaviour
{
    public Vector2Int direction; 

    void Start()
    {
        SetDirectionBasedOnRotation();
    }

    void SetDirectionBasedOnRotation()
    {
        Vector3 eulerAngles = transform.eulerAngles;

        if (eulerAngles.y == 0f)
        {
            direction = Vector2Int.left; 
        }
        else if (eulerAngles.y == 90f)
        {
            direction = Vector2Int.up; 
        }
        else if (eulerAngles.y == 180f)
        {
            direction = Vector2Int.right; 
        }
        else if (eulerAngles.y == 270f)
        {
            direction = Vector2Int.down; 
        }
    }
}


