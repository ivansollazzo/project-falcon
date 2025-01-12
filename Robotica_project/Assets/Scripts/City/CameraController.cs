using System.Collections;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform robot;                // Il robot da seguire
    public Vector3 initialOffsetPosition;  // Offset iniziale della posizione
    public Vector3 initialOffsetRotation;  // Rotazione iniziale relativa (Euler angles)
    public float followDistance = 1.5f;      // Distanza desiderata dietro il robot
    public float heightOffset = 1.5f;        // Offset verticale per la telecamera
    public float transitionSpeed = 2f;     // Velocità di transizione

    private Transform destination;         // Destinazione dinamica
    private Vector3 initialRobotPosition;  // Posizione iniziale del robot
    private enum CameraState { StaticPosition, FollowRobot, LookAtDestination, LookAtRobot }; // Stati della telecamera
    private CameraState currentState = CameraState.StaticPosition; // Stato iniziale
    private bool returningToRobot = false; // Flag per determinare se la telecamera sta tornando al robot

    private float waitTime = 8.0f; // Tempo di attesa in secondi
    private bool isWaiting = false;

    void Start()
    {
        // Imposta la posizione iniziale della telecamera (statica)
        transform.position = robot.position + initialOffsetPosition;

        // Imposta la rotazione iniziale della telecamera
        transform.rotation = Quaternion.Euler(initialOffsetRotation);

        // La telecamera guarda sempre il robot inizialmente
        transform.LookAt(robot);

        // Memorizza la posizione iniziale del robot
        initialRobotPosition = robot.position;
    }

    private IEnumerator WaitAndReturnToRobot()
    {
        isWaiting = true;
        yield return new WaitForSeconds(waitTime);
        returningToRobot = false;
        currentState = CameraState.FollowRobot;
        isWaiting = false;
    }
    void LateUpdate()
    {
        // Verifica se il robot si è spostato dalla sua posizione iniziale
        if (currentState == CameraState.StaticPosition && Vector3.Distance(robot.position, initialRobotPosition) > 0.1f)
        {
            // Se il robot si è spostato, cambia lo stato della telecamera per iniziare a seguirlo
            currentState = CameraState.FollowRobot;
        }

        switch (currentState)
        {
            case CameraState.StaticPosition:
                // Mantieni la telecamera nella posizione iniziale senza movimento
                break;

            case CameraState.FollowRobot:
                // Calcola la posizione dietro al robot, tenendo conto della rotazione del robot
                Vector3 behindPosition = robot.position - robot.forward * followDistance;
                behindPosition.y += heightOffset;  // Aggiungi l'offset verticale

                // Transizione della posizione della telecamera
                transform.position = Vector3.Lerp(transform.position, behindPosition, Time.deltaTime * transitionSpeed);

                // La telecamera guarda sempre il robot
                transform.LookAt(robot);
                break;

            case CameraState.LookAtDestination:
                if (destination != null)
                {
                    // Calcola la posizione tutta la mappa dalla partenza alla destinazione
                    Vector3 targetPosition = destination.position + initialOffsetPosition;
                    targetPosition.y += 60;  // Aggiungi l'offset verticale
                    transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * transitionSpeed);
                    // La telecamera guarda la destinazione e sta ferma per un po'
                    transform.LookAt(destination);

                    // Verifica se la telecamera è abbastanza vicina alla destinazione per tornare al robot
                    if (returningToRobot && Vector3.Distance(transform.position, targetPosition) < 0.5f && !isWaiting)
                    {
                        StartCoroutine(WaitAndReturnToRobot());
                    }
                }
                break;
            
            case CameraState.LookAtRobot:
                // La telecamera guarda il robot con gli offset iniziali
                LookAtRobot();
                break;
        }
    }

    public void UpdateDestination(Transform newDestination)
    {
        // Aggiorna la destinazione con un oggetto di tipo Transform
        destination = newDestination;
        currentState = CameraState.LookAtDestination;
        returningToRobot = true;
    }

    public void UpdateDestination(Vector3 destinationPosition)
    {
        // Crea un oggetto temporaneo per rappresentare la destinazione
        if (destination == null)
        {
            GameObject tempDestination = new GameObject("TempDestination");
            destination = tempDestination.transform;
        }

        destination.position = destinationPosition;
        currentState = CameraState.LookAtDestination;
        returningToRobot = true;
    }

    public void FollowRobot()
    {
        // Torna a seguire il robot
        currentState = CameraState.FollowRobot;
    }

    public void StartFollowingRobot()
    {
        // Cambia stato per iniziare a seguire il robot
        currentState = CameraState.FollowRobot;
    }

    // Metodo per guardare il robot con gli offset iniziali
    public void LookAtRobot()
    {
        currentState = CameraState.LookAtRobot;

        // Calcola la posizione davanti al robot con degli offset che stabilisco io
        Vector3 offsetPosition = robot.position + robot.forward * initialOffsetPosition.z + robot.right * initialOffsetPosition.x + robot.up * initialOffsetPosition.y;  
        //La cam si sposta gradualmente verso la posizione calcolata
        transform.position = Vector3.Lerp(transform.position, offsetPosition, Time.deltaTime * transitionSpeed);    

        //La cam guarda il robot
        transform.LookAt(robot);

    }
}
