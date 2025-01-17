using System.Xml.Serialization;
using UnityEngine;

public class ObstacleSensor : MonoBehaviour
{
    [Header("Cono superiore")]
    public float upperConeAngle = 30f;
    public float upperConeRange = 1.0f;
    public float upperRayLength = 0.4f;
    public int upperRayCount = 10;
    public float upperConeOffsetY = 0.5f;
    public float upperConeMinimumDistance = 0.4f;

    [Header("Cono centrale")]
    public float middleConeAngle = 30f;
    public float middleConeRange = 1.0f;
    public float middleRayLength = 0.4f;
    public int middleRayCount = 10;
    public float middleConeOffsetY = 0.25f;
    public float middleConeMinimumDistance = 0.4f;

    [Header("Cono inferiore")]
    public float lowerConeAngle = 30f;
    public float lowerConeRange = 1.0f;
    public float lowerRayLength = 0.4f;
    public int lowerRayCount = 10;
    public float lowerConeOffsetY = 0.0f;
    public float lowerConeMinimumDistance = 0.4f;

    private Collider detectedObstacle;
    private bool sensorsEnabled = false;

    private TTSManager ttsManager;

    void Start()
    {
        ttsManager = TTSManager.Instance;
    }

    void Update()
    {
        // Gestione dei tre coni
        if (sensorsEnabled)
        {
            LaunchCone(Vector3.forward, upperConeAngle, upperConeRange, upperRayCount, upperConeOffsetY, upperConeMinimumDistance, upperRayLength);
            LaunchCone(Vector3.forward, middleConeAngle, middleConeRange, middleRayCount, middleConeOffsetY, middleConeMinimumDistance, middleRayLength);
            LaunchCone(Vector3.forward, lowerConeAngle, lowerConeRange, lowerRayCount, lowerConeOffsetY, lowerConeMinimumDistance, lowerRayLength);
        }
    }

    public Collider CheckForObstacles()
    {
        return this.detectedObstacle;
    }

    public void EnableSensor(bool enabled)
    {
        this.sensorsEnabled = enabled;
    }

    public bool IsSensorEnabled()
    {
        return this.sensorsEnabled;
    }

    private void LaunchCone(Vector3 direction, float angle, float range, int rayCount, float offsetY, float minimumDistance, float rayLength)
    {
        Vector3 coneOrigin = transform.position + transform.up * offsetY;
        Quaternion baseRotation = Quaternion.LookRotation(transform.forward);
        float halfAngle = angle / 2;

        bool foundObstacle = false;

        for (int i = 0; i < rayCount; i++)
        {
            float stepAngle = i / (float)(rayCount - 1) * angle - halfAngle;
            Quaternion rayRotation = baseRotation * Quaternion.Euler(0, stepAngle, 0);
            Vector3 rayDirection = rayRotation * Vector3.forward;

            if (Physics.Raycast(coneOrigin, rayDirection, out RaycastHit hit, range, LayerMask.GetMask("Obstacle", "Pedestrians", "Cars")))
            {
                float minDist = 0;

                // If it's a pedestrian, the minimum distance must be higher of a 0.8 factor
                if (LayerMask.LayerToName(hit.collider.gameObject.layer) == "Pedestrians")
                {
                    minDist = 1.0f;
                }
                else {
                    minDist = minimumDistance;
                }

                // Now we can check if the distance is less than the minimum distance
                if (hit.distance < minDist)
                {
                    if (!foundObstacle)
                    {
                        string tagName = hit.collider.gameObject.GetComponent<ObjectName>().objectName;

                        // Gestione del TTS in base al tag dell'oggetto
                        if (tagName == "Remy") ttsManager.Speak("Ho trovato un ragazzo che cammina");
                        else if (tagName == "James") ttsManager.Speak("Ho trovato un pedone che corre");
                        else if (tagName == "Bench") ttsManager.Speak("Siamo davanti a una panchina");
                        else if (tagName == "Kate") ttsManager.Speak("Ho trovato un pedone che cammina");
                        else if (tagName == "TrashCan") ttsManager.Speak("Siamo vicino ad un cestino della spazzatura");
                        else if (tagName == "Cars") ttsManager.Speak("Fermo sta passando un auto");
                        else if (tagName == "Semaforo") ttsManager.Speak("Siamo davanti ad un Semaforo lo devo aggirare");
                        else if (tagName == "PaloLuce") ttsManager.Speak("Ho trovato un Palo della luce sul nostro percorso");
                        else ttsManager.Speak("Ho trovato un Ostacolo sul nostro percorso");

                        Debug.Log($"Ostacolo rilevato a: {hit.point}");
                        this.detectedObstacle = hit.collider;
                        foundObstacle = true;
                    }
                }
            }
        }

        if (!foundObstacle)
        {
            this.detectedObstacle = null;
        }
    }

    // Funzione per disegnare i raggi come Gizmos nell'Editor
    void OnDrawGizmos()
    {
        if (Camera.current == Camera.main) return;

        if (sensorsEnabled)
        {
            DrawCone(Vector3.forward, upperConeAngle, upperConeRange, upperRayCount, upperConeOffsetY, upperRayLength);
            DrawCone(Vector3.forward, middleConeAngle, middleConeRange, middleRayCount, middleConeOffsetY, middleRayLength);
            DrawCone(Vector3.forward, lowerConeAngle, lowerConeRange, lowerRayCount, lowerConeOffsetY, lowerRayLength);
        }
    }

    private void DrawCone(Vector3 direction, float angle, float range, int rayCount, float offsetY, float rayLength)
    {
        Vector3 coneOrigin = transform.position + transform.up * offsetY;
        Quaternion baseRotation = Quaternion.LookRotation(transform.forward);
        float halfAngle = angle / 2;

        for (int i = 0; i < rayCount; i++)
        {
            float stepAngle = i / (float)(rayCount - 1) * angle - halfAngle;
            Quaternion rayRotation = baseRotation * Quaternion.Euler(0, stepAngle, 0);
            Vector3 rayDirection = rayRotation * Vector3.forward;

            Gizmos.color = Color.red;
            Gizmos.DrawRay(coneOrigin, rayDirection * rayLength);
        }
    }
}