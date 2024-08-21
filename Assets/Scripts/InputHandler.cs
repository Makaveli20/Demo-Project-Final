using UnityEngine;

public class InputManager : MonoBehaviour
{
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleMouseClick();
        }
    }

    void HandleMouseClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider != null && hit.collider.gameObject.CompareTag("Frog"))
            {
                FrogController frogController = hit.collider.gameObject.GetComponent<FrogController>();
                if (frogController != null)
                {
                    Debug.Log("Frog clicked: " + hit.collider.gameObject.name);
                    frogController.OnFrogClicked();
                }
            }
        }
    }
}