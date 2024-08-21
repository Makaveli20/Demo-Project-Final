using UnityEngine;

public class Arrow : MonoBehaviour
{
    public Vector2Int direction; // Direction the arrow points (e.g., (1,0) for right, (-1,0) for left)

    void Start()
    {
        // Optionally, you could set the direction based on the arrow's rotation or a specific input
        SetDirectionBasedOnRotation();
    }

    void SetDirectionBasedOnRotation()
    {
        Vector3 eulerAngles = transform.eulerAngles;
        if (eulerAngles.y == 180)
        {
            direction = Vector2Int.up;
        }
        else if (eulerAngles.y == 90)
        {
            direction = Vector2Int.right; 
        }
        else if (eulerAngles.y == 0)
        {
            direction = Vector2Int.down; 
        }
        else if (eulerAngles.y == 270)
        {
            direction = Vector2Int.left; 
        }
    }

}