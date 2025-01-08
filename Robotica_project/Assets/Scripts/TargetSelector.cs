using UnityEngine;

public class TargetSelector : MonoBehaviour
{
    private RobotController robotController;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        robotController = GetComponent<RobotController>();
    }

    // Update is called once per frame
    void Update()
    {
        // We're according to select a point according to mouse click
        if (Input.GetMouseButtonDown(0))
        {
            // Create a ray from the camera to the mouse position
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // If the ray hits something
            if (Physics.Raycast(ray, out hit))
            {
                // Get the hit point
                Vector3 targetPoint = hit.point;

                // Set the target point
                robotController.SetDestination(targetPoint);
            }
        }
    }
}
