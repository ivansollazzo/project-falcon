using UnityEngine;

public class PedestrianController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float movementSpeed = 3.0f; // Velocità del movimento
    public float rotationSpeed = 200.0f; // Velocità di rotazione
    public float stopDistance = 0.5f; // Distanza per considerare la destinazione raggiunta

    [Header("Destination Info")]
    public Vector3 destination; // Destinazione corrente
    public bool reachedDestination = false; // Stato di destinazione raggiunta

    private Vector3 lastPosition; // Per calcolare la velocità
    private Vector3 velocity; // Direzione del movimento

    void Start()
    {
        lastPosition = transform.position; // Inizializza la posizione iniziale
    }

    void Update()
    {
        // Controlla se non ha ancora raggiunto la destinazione
        if (Vector3.Distance(transform.position, destination) > stopDistance)
        {
            reachedDestination = false;

            // Direzione verso la destinazione
            Vector3 destinationDirection = destination - transform.position;
            destinationDirection.y = 0; // Ignora variazioni sull'asse Y

            // Rotazione verso la destinazione
            Quaternion targetRotation = Quaternion.LookRotation(destinationDirection);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            // Movimento in avanti
            transform.Translate(Vector3.forward * movementSpeed * Time.deltaTime);
        }
        else
        {
            // Destinazione raggiunta
            reachedDestination = true;
        }

        // Calcola la velocità del movimento
        velocity = (transform.position - lastPosition) / Time.deltaTime;
        velocity.y = 0; // Ignora variazioni sull'asse Y

        // Velocità normalizzata
        float velocityMagnitude = velocity.magnitude;
        velocity = velocity.normalized;

        // Dot Product per controllare la direzione
        float forwardDotProduct = Vector3.Dot(transform.forward, velocity);
        float rightDotProduct = Vector3.Dot(transform.right, velocity);

        // Aggiorna la posizione precedente
        lastPosition = transform.position;

        // Debug (Opzionale)
        Debug.Log($"Velocity Magnitude: {velocityMagnitude}, Forward Dot: {forwardDotProduct}, Right Dot: {rightDotProduct}");
    }

    // Metodo per impostare una nuova destinazione
    public void SetDestination(Vector3 newDestination)
    {
        destination = newDestination;
        reachedDestination = false;
    }
}
