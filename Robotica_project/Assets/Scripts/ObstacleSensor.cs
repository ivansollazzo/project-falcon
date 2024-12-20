using UnityEngine;

public class ObstacleSensor : MonoBehaviour
{
    public float detectionRange = 5.0f;
    public float detectionAngle = 45.0f; // Angolo del cono di rilevamento
    public int raysCount = 10; // Numero di raggi nel cono
    public int verticalLayers = 3; // Numero di livelli verticali
    public float verticalSpacing = 0.5f; // Spaziatura tra i livelli verticali
    public LayerMask obstacleLayer;

    public Vector3? DetectObstacle()
    {
        RaycastHit hit;
        Vector3 direction = transform.forward; // Direzione del sensore

        for (int layer = 1; layer < verticalLayers; layer++)
        {
            float heightOffset = layer * verticalSpacing;

            for (int i = 0; i < raysCount; i++)
            {
                float angle = Mathf.Lerp(-detectionAngle / 2, detectionAngle / 2, i / (raysCount - 1f));
                Vector3 rayDirection = Quaternion.Euler(0, angle, 0) * direction;
                Vector3 rayOrigin = transform.position + Vector3.up * heightOffset;

                if (Physics.Raycast(rayOrigin, rayDirection, out hit, detectionRange, obstacleLayer))
                {
                    Debug.DrawRay(rayOrigin, rayDirection * detectionRange, Color.magenta, 0.1f); // Visualizza i raggi in viola se rilevano un ostacolo
                    Debug.Log($"Ostacolo rilevato a distanza: {hit.distance} a altezza: {heightOffset}");
                    return hit.point;
                }
                else
                {
                    Debug.DrawRay(rayOrigin, rayDirection * detectionRange, Color.red, 0.1f); // Visualizza i raggi in rosso se non rilevano un ostacolo
                }
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

        for (int layer = 1; layer < verticalLayers; layer++)
        {
            float heightOffset = layer * verticalSpacing;

            for (int i = 0; i < raysCount; i++)
            {
                float angle = Mathf.Lerp(-detectionAngle / 2, detectionAngle / 2, i / (raysCount - 1f));
                Vector3 rayDirection = Quaternion.Euler(0, angle, 0) * direction;
                Vector3 rayOrigin = transform.position + Vector3.up * heightOffset;
                Gizmos.DrawLine(rayOrigin, rayOrigin + rayDirection * detectionRange);
            }
        }
    }
}
