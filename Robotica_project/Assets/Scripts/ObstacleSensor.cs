using UnityEngine;

public class ObstacleSensor : MonoBehaviour
{
    public float detectionRange = 5.0f;
    public float detectionAngle = 45.0f; // Angolo del cono di rilevamento
    public int raysCount = 10; // Numero di raggi nel cono
    public LayerMask obstacleLayer;

    public Vector3? DetectObstacle()
    {
        RaycastHit hit;
        Vector3 direction = transform.forward; // Direzione del sensore

        for (int i = 0; i < raysCount; i++)
        {
            float angle = Mathf.Lerp(-detectionAngle / 2, detectionAngle / 2, i / (raysCount - 1f));
            Vector3 rayDirection = Quaternion.Euler(0, angle, 0) * direction;

            if (Physics.Raycast(transform.position, rayDirection, out hit, detectionRange, obstacleLayer))
            {
                Debug.Log("Ostacolo rilevato a distanza: " + hit.distance);
                return hit.point;
            }
        }

        Debug.Log("Nessun ostacolo rilevato.");
        return null;
    }

    private void OnDrawGizmos()
    {
        // Visualizza il cono di rilevamento nel Scene View
        Gizmos.color = Color.red;
        Vector3 direction = transform.forward;

        for (int i = 0; i < raysCount; i++)
        {
            float angle = Mathf.Lerp(-detectionAngle / 2, detectionAngle / 2, i / (raysCount - 1f));
            Vector3 rayDirection = Quaternion.Euler(0, angle, 0) * direction;
            Gizmos.DrawLine(transform.position, transform.position + rayDirection * detectionRange);
        }
    }
}
