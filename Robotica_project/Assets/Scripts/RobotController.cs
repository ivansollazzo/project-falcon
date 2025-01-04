using System.Collections;
using UnityEngine;

public class RobotController : MonoBehaviour
{
    private StateMachine stateMachine;
    private Vector3 destination;
    private bool isMoving = false;
    private ParticleFilter particleFilter;

    private GameObject disabledPerson;

    private Animator disabledPersonAnimator;

    private TTSManager ttsManager;
    
    void Start()
    {
        stateMachine = this.gameObject.AddComponent<StateMachine>();
        particleFilter = this.gameObject.GetComponent<ParticleFilter>();
        ttsManager = this.gameObject.GetComponent<TTSManager>();
        disabledPerson = GameObject.Find("DisabledPerson");

        if (stateMachine == null)
        {
            Debug.LogError("StateMachine is not assigned!");
        }

        if (particleFilter == null)
        {
            Debug.LogError("ParticleFilter is not assigned!");
        }

        if (ttsManager == null)
        {
            Debug.LogError("TTSManager is not assigned!");
        }

        if (disabledPerson == null)
        {
            Debug.LogError("DisabledPerson is not assigned!");
        }
        else {
            // Get the animator
            disabledPersonAnimator = disabledPerson.GetComponent<Animator>();
            disabledPersonAnimator.SetFloat("Speed", 0.0f);
        }

        if (stateMachine != null)
        {
            stateMachine.SetState(new StandbyState(stateMachine));
        }
    }

    public void SetMoving(bool moving)
    {
        this.isMoving = moving;
    }

    public bool IsMoving()
    {
        return this.isMoving;
    }

    public TTSManager GetTTSManager()
    {
        return this.ttsManager;
    }

    public bool RotateToTarget(Vector3 targetPosition)
    {
        if (isMoving)
        {
            if (this.disabledPersonAnimator != null)
            {
                this.disabledPersonAnimator.SetFloat("Speed", 0.0f);
            }
            
            Debug.Log("Ruotando verso il target: " + targetPosition);
            Vector3 direction = targetPosition - transform.position;
    
            if (direction.sqrMagnitude < 0.1f)
                return true;

            direction.Normalize();
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            float error = Quaternion.Angle(transform.rotation, targetRotation);

            if (error >= 0.1f)
            {
                float rotationSpeed = Time.deltaTime * 360.0f;
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
                return false;
            }

            transform.rotation = targetRotation;
            return true;
        }
        return false;
    }

    public bool MoveToTarget(Vector3 targetPosition) 
    {
        if (isMoving) 
        {
            if (this.disabledPersonAnimator != null)
            {
                this.disabledPersonAnimator.SetFloat("Speed", 1.0f);
            }

            // Calcola la direzione e la distanza verso il target (con l'asse z rivolto verso il target)
            Vector3 directionToTarget = targetPosition - transform.position;
            float distanceToTarget = directionToTarget.magnitude;

            // Calcola il controllo di input (movimento desiderato)
            Vector3 controlInput = transform.position + directionToTarget.normalized * Time.deltaTime;

            // Aggiorna il filtro particellare con il controllo di input e la posizione attuale
            particleFilter.UpdateParticles(controlInput, transform.position);

            // Ottieni la posizione stimata dal filtro particellare
            Vector3 estimatedPosition = particleFilter.EstimatePosition();

            // Calcola la direzione corretta usando la posizione stimata
            Vector3 correctedDirection = (targetPosition - estimatedPosition).normalized;
            
            // Muovi il robot
            transform.position += correctedDirection * Time.deltaTime;

            Debug.DrawLine(transform.position, estimatedPosition, Color.yellow);
            Debug.DrawLine(estimatedPosition, targetPosition, Color.cyan);

            // Controlla se siamo abbastanza vicini alla destinazione
            if (distanceToTarget <= 0.25f) 
            {
                if (this.disabledPersonAnimator != null)
                {
                    this.disabledPersonAnimator.SetFloat("Speed", 0.0f);
                }
                
                return true;
            }
            return false;
        }

        return false;
    }

    public void SetDestination(Vector3 destination)
    {
        this.destination = destination;
        Debug.Log("Destinazione impostata: " + destination);
    }

    public Vector3 GetDestination()
    {
        return this.destination;
    }

    public IEnumerator CheckObstacle(System.Action<bool> callback)
    {
        ObstacleSensor obstacleSensor = GetComponent<ObstacleSensor>();
        Vector3? obstaclePosition = obstacleSensor.CheckForObstacles();
        
        if (obstaclePosition != null)
        {
            // Disable the disabled person animator
            if (this.disabledPersonAnimator != null)
            {
                this.disabledPersonAnimator.SetFloat("Speed", 0.0f);
            }

            ttsManager.Speak("Ho rilevato un ostacolo. Aspetta un attimo per favore!");
            
            Debug.Log("Ostacolo rilevato! Posizione: " + obstaclePosition);
            Debug.Log("Aspetto 5 secondi per vedere se l'ostacolo si muove...");
            
            yield return new WaitForSeconds(5);
            
            Debug.Log("Controllo se l'ostacolo si è mosso...");
            obstaclePosition = obstacleSensor.CheckForObstacles();
            
            if (obstaclePosition != null)
            {
                Debug.Log("Ostacolo fisso rilevato: " + obstaclePosition.Value);
                GridManager.Instance.MarkCellAsBlocked(obstaclePosition.Value);

                yield return new WaitForSeconds(5);

                ttsManager.Speak("L'ostacolo non si è mosso e blocca il mio percorso. Mi dispiace, aspetta che ripianifico!");

                callback(true);
                
                yield break;
            }
        }
        
        // Enable the disabled person animator
        if (this.disabledPersonAnimator != null)
        {
            this.disabledPersonAnimator.SetFloat("Speed", 1.0f);
        }

        callback(false);
    
    }
}