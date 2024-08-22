using UnityEngine;

public class Arrow : MonoBehaviour
{
    public Vector2Int direction; // Direction the arrow points (e.g., (1,0) for right, (-1,0) for left)

    void Start()
    {
        SetDirectionBasedOnRotation();
    }

    void SetDirectionBasedOnRotation()
    {
        Vector3 eulerAngles = transform.eulerAngles;

        // Correct the direction based on the rotation
        if (eulerAngles.y == 0f)
        {
            direction = Vector2Int.left; // Facing down visually, sends right
        }
        else if (eulerAngles.y == 90f)
        {
            direction = Vector2Int.up; // Facing left visually, sends down
        }
        else if (eulerAngles.y == 180f)
        {
            direction = Vector2Int.right; // Facing up visually, sends left
        }
        else if (eulerAngles.y == 270f)
        {
            direction = Vector2Int.down; // Facing right visually, sends up
        }
    }
}


