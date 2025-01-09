using UnityEngine;
using System.Collections;

public class CameraControllerFF : MonoBehaviour
{
    private Transform robotTransform; // Riferimento al robot
    private Vector3 destination; // Destinazione corrente
    private bool isFollowingRobot = true; // Flag per determinare se seguire il robot

    [Header("Posizione iniziale della camera")]
    public Vector3 initialOffset = new Vector3(0, 10, -10); // Offset iniziale rispetto al robot
    public float initialTransitionSpeed = 2.0f; // Velocità di transizione verso la posizione iniziale

    [Header("Posizione sulla destinazione")]
    public Vector3 destinationOffset = new Vector3(0, 15, -5); // Offset rispetto alla destinazione
    public float destinationTransitionSpeed = 2.0f; // Velocità di transizione verso la destinazione

    [Header("Posizione durante il follow del robot")]
    public Vector3 followOffset = new Vector3(0, 8, -12); // Offset durante il follow
    public float followTransitionSpeed = 2.5f; // Velocità di transizione durante il follow

    private void Start()
    {
        // Trova il robot nella scena tramite tag
        robotTransform = GameObject.FindGameObjectWithTag("Player").transform;

        if (robotTransform == null)
        {
            Debug.LogError("Robot non trovato! Assicurati che il robot abbia il tag 'Robot'.");
        }

        // Inizia la visualizzazione dalla posizione iniziale
        MoveToPosition(robotTransform.position + initialOffset, initialTransitionSpeed);
    }

    private void LateUpdate()
    {
        if (isFollowingRobot && robotTransform != null)
        {
            // Segui il robot mantenendo l'offset
            Vector3 targetPosition = robotTransform.position + followOffset;
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * followTransitionSpeed);
            transform.LookAt(robotTransform); // Mantieni lo sguardo sul robot
        }
    }

    /// <summary>
    /// Aggiorna la destinazione e focalizza la telecamera su di essa.
    /// </summary>
    /// <param name="newDestination">La nuova destinazione da visualizzare.</param>
    public void UpdateDestination(Vector3 newDestination)
    {
        isFollowingRobot = false; // Interrompe il follow del robot
        destination = newDestination;

        // Muovi la telecamera verso la destinazione
        StopAllCoroutines(); // Ferma altre transizioni in corso
        MoveToPosition(destination + destinationOffset, destinationTransitionSpeed, robotTransform);
    }

    /// <summary>
    /// Torna a seguire il robot.
    /// </summary>
    public void FollowRobot()
    {
        isFollowingRobot = true; // Riprendi il follow del robot
    }

    /// <summary>
    /// Muove la telecamera verso una posizione target.
    /// </summary>
    /// <param name="targetPosition">Posizione target.</param>
    /// <param name="speed">Velocità di transizione.</param>
    /// <param name="lookTarget">Il target da guardare.</param>
    private void MoveToPosition(Vector3 targetPosition, float speed, Transform lookTarget = null)
    {
        StartCoroutine(TransitionToPosition(targetPosition, speed, lookTarget));
    }

    /// <summary>
    /// Coroutine per una transizione graduale verso una posizione target.
    /// </summary>
    private IEnumerator TransitionToPosition(Vector3 targetPosition, float speed, Transform lookTarget = null)
    {
        while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * speed);

            // Se è stato fornito un target, guarda verso di esso
            if (lookTarget != null)
            {
                transform.LookAt(lookTarget);
            }

            yield return null;
        }
    }
}
