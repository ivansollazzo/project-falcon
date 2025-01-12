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

    private Vector3 previousPosition;
    private float movementThreshold = 0.1f; // Soglia per aggiornare il filtro particellare

    void Start()
    {
        stateMachine = this.gameObject.AddComponent<StateMachine>();
        particleFilter = this.gameObject.GetComponent<ParticleFilter>();
        ttsManager = TTSManager.Instance;
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
        else
        {
            disabledPersonAnimator = disabledPerson.GetComponent<Animator>();
            disabledPersonAnimator.SetFloat("Speed", 0.0f);
        }

        if (stateMachine != null)
        {
            stateMachine.SetState(new StandbyState(stateMachine));
        }

        previousPosition = transform.position; // Inizializza la posizione precedente
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

            Vector3 estimatedPosition = particleFilter.EstimatePosition();
            Vector3 targetDirection = targetPosition - transform.position;
            targetDirection.Normalize();
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

            Vector3 correctedDirection = (targetPosition - estimatedPosition).normalized;
            Quaternion correctedRotation = Quaternion.LookRotation(correctedDirection);

            float rotationSpeed = 90.0f;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, correctedRotation, rotationSpeed * Time.deltaTime);

            Vector3 controlInput = Vector3.zero;
            Vector3 measurement = transform.position;
            particleFilter.UpdateParticles(controlInput, measurement);

            Debug.DrawLine(transform.position, estimatedPosition, Color.yellow);
            Debug.DrawLine(estimatedPosition, targetPosition, Color.cyan);
            Debug.DrawRay(transform.position, transform.forward * 2.0f, Color.red);

            float error = Quaternion.Angle(transform.rotation, targetRotation);
            return error < 1.0f;
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

            Vector3 estimatedPosition = particleFilter.EstimatePosition();
            Vector3 directionToTarget = targetPosition - estimatedPosition;
            float distanceToTarget = directionToTarget.magnitude;

            // Velocità dinamica in base alla distanza
            float speed = Mathf.Lerp(1.0f, 3.0f, distanceToTarget / 10f);
            Vector3 desiredMovement = directionToTarget.normalized * speed/2 * Time.deltaTime;

            transform.position += desiredMovement;

            // Aggiorna il filtro particellare solo se il movimento è significativo
            if (Vector3.Distance(transform.position, previousPosition) > movementThreshold)
            {
                Vector3 controlInput = desiredMovement;
                Vector3 measurement = transform.position;
                particleFilter.UpdateParticles(controlInput, measurement);

                previousPosition = transform.position; // Aggiorna la posizione precedente
            }

            Debug.DrawLine(transform.position, estimatedPosition, Color.yellow);
            Debug.DrawLine(estimatedPosition, targetPosition, Color.cyan);

            if (distanceToTarget <= 0.25f)
            {
                if (this.disabledPersonAnimator != null)
                {
                    this.disabledPersonAnimator.SetFloat("Speed", 0.0f);
                }
                return true; // Raggiunto il target
            }

            return false; // Non ancora arrivati
        }

        return false;
    }

    public bool IsDestinationSet()
    {
        return this.destinationSet;
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