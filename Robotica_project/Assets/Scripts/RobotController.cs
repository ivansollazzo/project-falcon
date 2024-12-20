using UnityEngine;
public class RobotController : MonoBehaviour
{
    private StateMachine stateMachine;
    private Vector3 destination;
    private bool isMoving = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Aggiunta del componente StateMachine
        stateMachine = this.gameObject.AddComponent<StateMachine>();

        // Check if components are assigned
        if (stateMachine == null)
        {
            Debug.LogError("StateMachine is not assigned!");
        }

        // Impostazione dello stato iniziale
        if (stateMachine != null)
        {
            stateMachine.SetState(new StandbyState(stateMachine));  // Inizia con lo stato Standby
        }
    }

    // Funzione per muovere il robot verso la destinazione
    public void SetMoving(bool moving)
    {
        this.isMoving = moving;
    }

    public bool IsMoving()
    {
        return this.isMoving;
    }

    // Metodo per ruotare verso il target
    public bool RotateToTarget(Vector3 targetPosition)
    {
        // Verifica che il comando di movimento si attivo
        if (isMoving)
        {
            Vector3 direction = targetPosition - transform.position;
    
            if (direction.sqrMagnitude < 0.001f) // Già allineato
                return true;

            direction.Normalize();
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            float error = Quaternion.Angle(transform.rotation, targetRotation);

            if (error > 1.0f) // Aumenta la tolleranza
            {
                // Calcola la velocità di rotazione in base all'errore. Adatta la velocità di rotazione in base all'errore.
                float rotationSpeed = Time.deltaTime * 180.0f;

                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
                return false;
            }

            transform.rotation = targetRotation; // Allinea forzatamente
            return true;
        }

        return false;
    }

    public bool MoveToTarget(Vector3 targetPosition)
    {
        // Verifica che il comando di movimento sia attivo
        if (isMoving)
        {
            // Muoviamo il robot verso il target e calcoliamo l'errore ogni volta. Se l'errore è minore di un certo valore, ritorniamo true, altrimenti false.
            float error = Vector3.Distance(transform.position, targetPosition);

            // Calcola il fattore di interpolazione basato sulla velocità e sulla distanza
            float t = Mathf.Clamp01(Time.deltaTime / error);

            // Muove il robot verso il target
            transform.position = Vector3.Lerp(transform.position, targetPosition, t);

            if (error > 0.1f)
            {
                return false;
            }

            return true;
        }

        return false;
    }

    // Funzione per impostare la destinazione del robot
    public void SetDestination(Vector3 destination)
    {
        this.destination = destination;
        Debug.Log("Destinazione impostata: " + destination);
    }

    // Funzione per ottenere la destinazione del robot
    public Vector3 GetDestination()
    {
        return this.destination;
    }
}