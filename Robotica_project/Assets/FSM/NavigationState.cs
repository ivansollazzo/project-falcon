using UnityEngine;
using UnityEngine.AI;

public class NavigationState : State
{
    private NavMeshAgent agent;
    private Transform destination;

    public NavigationState(StateMachine stateMachine) : base(stateMachine) { }

    public override void EnterState()
    {
        Debug.Log("Entered the NAVIGATION state! Robot is moving....");
        
        agent = stateMachine.GetComponent<NavMeshAgent>();
        destination = GameObject.Find("Destination").transform;

        if (destination != null)
        {
            agent.SetDestination(destination.position);
        }
    }

    public override void ExecuteState()
    {
        // Controlla ostacoli durante la navigazione
        DetectObstacles();

        // Verifica se la destinazione è stata raggiunta
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
            {
                stateMachine.SetState(new ArrivalState(stateMachine));
            }
        }
    }

    public override void ExitState()
    {
        Debug.Log("Uscito dallo stato di Navigazione.");
    }

    private void DetectObstacles()
    {
        RaycastHit hit;
        if (Physics.Raycast(stateMachine.transform.position, stateMachine.transform.forward, out hit, 2f))
        {
            Debug.Log("Ostacolo rilevato: " + hit.collider.name);
            stateMachine.SetState(new ObstacleDetectedState(stateMachine));
        }
    }
}
