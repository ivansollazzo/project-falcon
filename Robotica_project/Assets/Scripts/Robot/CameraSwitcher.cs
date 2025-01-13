using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    private Camera mainCamera;
    private Camera robotCamera;

    public static CameraSwitcher Instance;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        // Trova le camere
        mainCamera = Camera.main;
        robotCamera = GameObject.Find("RobotCamera")?.GetComponent<Camera>();

        // Controlla se le telecamere esistono
        if (mainCamera == null)
        {
            Debug.LogError("Camera principale non trovata!");
        }
        if (robotCamera == null)
        {
            Debug.LogError("Robot Camera non trovata!");
        }

        // Imposta la telecamera principale abilitata e la telecamera robot disabilitata (inizialmente)
        if (mainCamera != null && robotCamera != null)
        {
            mainCamera.enabled = true;
            robotCamera.enabled = false;
        }
    }

    void Update()
    {
        // Se premi "K" cambia tra la camera principale e la camera robotica
        if (Input.GetKeyDown(KeyCode.K))
        {
            if (robotCamera.enabled) {
                robotCamera.enabled = false;
            }
            else {
                robotCamera.enabled = true;
            }
        }
    }
}