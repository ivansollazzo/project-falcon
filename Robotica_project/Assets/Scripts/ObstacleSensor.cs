using UnityEngine;

public class ObstacleSensor : MonoBehaviour
{
    public float detectionDistance = 0.4f; // Distanza massima di rilevamento
    public float detectionAngle = 45.0f; // Angolo del cono di rilevamento
    public int raysCount = 10; // Numero di raggi
    public LayerMask obstacleLayer; // Layer da cui rilevare gli ostacoli

    public Vector3? CheckForObstacles()
    {
        // Direzione base (davanti all'oggetto)
        Vector3 baseDirection = transform.forward;

        // Variabili per il raycast più vicino
        Collider closestCollider = null;
        float closestDistance = Mathf.Infinity;

        // Genera raggi distribuiti uniformemente all'interno del cono
        for (int i = 0; i < raysCount; i++)
        {
            // Genera un angolo casuale all'interno del cono
            float angle = Random.Range(-detectionAngle / 2, detectionAngle / 2);
            float elevation = Random.Range(-detectionAngle / 2, detectionAngle / 2);

            // Calcola la direzione del raggio
            Quaternion rotation = Quaternion.Euler(elevation, angle, 0);
            Vector3 rayDirection = rotation * baseDirection;

            // Lancia il raycast con il LayerMask (supporta più layer)
            if (Physics.Raycast(transform.position, rayDirection, out RaycastHit hit, detectionDistance, obstacleLayer))
            {
                Debug.DrawRay(transform.position, rayDirection * detectionDistance, Color.red);

                // Controlla se il collider colpito è il più vicino
                if (hit.distance < closestDistance)
                {
                    closestCollider = hit.collider;
                    closestDistance = hit.distance;
                }
            }
        }

        // Se è stato trovato un collider, lo restituisce
        if (closestCollider != null)
        {
            Debug.Log($"Ostacolo più vicino rilevato: {closestCollider.name} alla posizione {closestCollider.transform.position}");
            return closestCollider.transform.position;
        }

        // Nessun ostacolo rilevato
        Debug.Log("Nessun ostacolo rilevato.");
        return null;
    }

    // Per fare il test direttamente nella scena
    private void OnDrawGizmos()
    {
        // Disegna la direzione dei raggi per debug
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.forward * detectionDistance); // Raggio principale
    }
}