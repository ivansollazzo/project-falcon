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
        // Ruotiamo il robot verso il target e calcoliamo l'errore ogni volta. Questo errore è la differenza tra la rotazione attuale e la rotazione desiderata. Se l'errore è minore di un certo valore, ritorniamo true, altrimenti false.
        Vector3 direction = (targetPosition - transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        float error = Quaternion.Angle(transform.rotation, targetRotation);

        if (error > 0.1f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5.0f);
            return false;
        }

        return true;
    }

    public bool MoveToTarget(Vector3 targetPosition)
    {
        // Muoviamo il robot verso il target e calcoliamo l'errore ogni volta. Se l'errore è minore di un certo valore, ritorniamo true, altrimenti false.
        float error = Vector3.Distance(transform.position, targetPosition);
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * 5.0f);

        if (error > 0.1f)
        {
            return false;
        }

        return true;
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