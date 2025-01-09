using UnityEngine;

public class ObstacleSensor : MonoBehaviour
{
    [Header("Cono superiore")]
    public float upperConeAngle = 30f; // Angolo del cono
    public float upperConeRange = 1.0f; // Portata del raggio
    public float upperRayLength = 0.4f; // Lunghezza visiva del raggio
    public int upperRayCount = 10;    // Numero di raggi
    public float upperConeOffsetY = 0.5f; // Altezza del cono superiore
    public float upperConeMinimumDistance = 0.4f; // Distanza minima per rilevare un ostacolo

    [Header("Cono centrale")]
    public float middleConeAngle = 30f;
    public float middleConeRange = 1.0f;
    public float middleRayLength = 0.4f;
    public int middleRayCount = 10;
    public float middleConeOffsetY = 0.25f; // Altezza del cono centrale
    public float middleConeMinimumDistance = 0.4f; // Distanza minima per rilevare un ostacolo

    [Header("Cono inferiore")]
    public float lowerConeAngle = 30f;
    public float lowerConeRange = 1.0f;
    public float lowerRayLength = 0.4f;
    public int lowerRayCount = 10;
    public float lowerConeOffsetY = 0.0f; // Altezza del cono inferiore
    public float lowerConeMinimumDistance = 0.4f; // Distanza minima per rilevare un ostacolo

    [Header("Debug")]
    public bool showRays = true;

    private Collider detectedObstacle;
    private bool sensorsEnabled = false;

    void Update()
    {
        // Gestione dei tre coni
        LaunchCone(Vector3.forward, upperConeAngle, upperConeRange, upperRayCount, upperConeOffsetY, upperConeMinimumDistance, upperRayLength);
        LaunchCone(Vector3.forward, middleConeAngle, middleConeRange, middleRayCount, middleConeOffsetY, middleConeMinimumDistance, middleRayLength);
        LaunchCone(Vector3.forward, lowerConeAngle, lowerConeRange, lowerRayCount, lowerConeOffsetY, lowerConeMinimumDistance, lowerRayLength);
    }

    public Collider CheckForObstacles()
    {
        return this.detectedObstacle;
    }

    // Function to enable sensors
    public void EnableSensor(bool enabled)
    {
        this.sensorsEnabled = enabled;
    }

    // Function to get if sensors are enabled
    public bool IsSensorEnabled()
    {
        return this.sensorsEnabled;
    }

    // Funzione per lanciare i raggi del cono
    private void LaunchCone(Vector3 direction, float angle, float range, int rayCount, float offsetY, float minimumDistance, float rayLength)
    {
        // Calcola la posizione di partenza del cono, tenendo conto della rotazione del GameObject
        Vector3 coneOrigin = transform.position + transform.up * offsetY;

        // Calcola la direzione iniziale del cono (in avanti rispetto alla rotazione del GameObject)
        Quaternion baseRotation = Quaternion.LookRotation(transform.forward);
        float halfAngle = angle / 2;

        bool foundObstacle = false;

        // Lancia i raggi
        for (int i = 0; i < rayCount; i++)
        {
            // Calcolo della direzione per ogni raggio
            float stepAngle = i / (float)(rayCount - 1) * angle - halfAngle; // Distribuzione angolare
            Quaternion rayRotation = baseRotation * Quaternion.Euler(0, stepAngle, 0); // Ruota attorno all'asse Y
            Vector3 rayDirection = rayRotation * Vector3.forward;

            // Lancia il raggio
            if (Physics.Raycast(coneOrigin, rayDirection, out RaycastHit hit, range, LayerMask.GetMask("Obstacle","Pedestrians","Cars")))
            {
                if (hit.distance < minimumDistance)
                {
                    // Rilevato un ostacolo, salva il collider e interrompi il ciclo
                    if (!foundObstacle)
                    {
                        Debug.Log($"Ostacolo rilevato a: {hit.point}");
                        this.detectedObstacle = hit.collider;
                        foundObstacle = true; // Imposta il flag per impedire la ricerca di altri ostacoli
                    }
                }
            }

            // Disegna i raggi per debugging usando la lunghezza rayLength
            if (showRays)
                Debug.DrawRay(coneOrigin, rayDirection * rayLength, Color.red);
        }

        // Se non è stato trovato nessun ostacolo, resetta il collider
        if (!foundObstacle)
        {
            this.detectedObstacle = null;
        }
    }
}