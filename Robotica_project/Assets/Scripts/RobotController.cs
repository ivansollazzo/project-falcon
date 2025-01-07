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

    private ObstacleSensor obstacleSensor;

    private bool destinationSet = false;
    
    void Start()
    {
        stateMachine = this.gameObject.AddComponent<StateMachine>();
        particleFilter = this.gameObject.GetComponent<ParticleFilter>();
        ttsManager = this.gameObject.GetComponent<TTSManager>();
        disabledPerson = GameObject.Find("DisabledPerson");
        obstacleSensor = this.gameObject.GetComponent<ObstacleSensor>();

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

    public ObstacleSensor GetObstacleSensor()
    {
        return this.obstacleSensor;
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
            
            // Calculate the direction to the target
            Vector3 direction = targetPosition - transform.position;
            direction.Normalize();

            // Calculate the error
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            float error = Quaternion.Angle(transform.rotation, targetRotation);

            if (error >= 0.01f)
            {
                float rotationSpeed = Time.deltaTime * 360.0f;
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
                return false;
            }
            
            return true;
        }
        
        return false;
    }

    public bool IsDestinationSet()
    {
        return this.destinationSet;
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
        Vector3 gridDestination = new Vector3(destination.x, -0.01f, destination.z);
        this.destination = gridDestination;
        destinationSet = true;
        Debug.Log("Destinazione impostata: " + destination);
    }

    public void ClearDestination()
    {
        this.destination = Vector3.zero;
        destinationSet = false;
    }

    public Vector3 GetDestination()
    {
        return this.destination;
    }

    public StateMachine GetStateMachine()
    {
        return this.stateMachine;
    }

}