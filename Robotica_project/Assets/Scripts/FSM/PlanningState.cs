using UnityEngine;
using UnityEngine.AI;

public class PlanningState : State
{
    // Attributes
    private RobotController robotController;
    private NavMeshAgent navMeshAgent;
    private bool planningComplete = false;
    private bool planningFailed = false;
    private Vector3 destination;

    private NavMeshPath plannedPath;

    // Constructor
    public PlanningState(StateMachine stateMachine) : base(stateMachine) {
        this.robotController = stateMachine.gameObject.GetComponent<RobotController>();
        this.navMeshAgent = this.robotController.GetNavMeshAgent();
        this.destination = robotController.GetDestination();
    }

    public override void EnterState()
    {
        Debug.Log("Entered the PLANNING state! Robot is planning a route for the entered destination...");
    }

    public override void ExecuteState()
    {
        // Set destination and calculate path
        if (this.navMeshAgent != null)
        {
            this.plannedPath = new NavMeshPath();
            bool pathFound = this.navMeshAgent.CalculatePath(destination, this.plannedPath);

            if (pathFound && this.plannedPath.status == NavMeshPathStatus.PathComplete)
            {
                this.planningComplete = true;
                Debug.Log("Path planning complete!");
            }
            else
            {
                Debug.LogError("Path planning failed!");
                this.planningFailed = true;
            }
        }
        else
        {
            Debug.LogError("NavMeshAgent is null!");
        }

        if (this.planningComplete)
        {
            stateMachine.SetState(new NavigationState(stateMachine, plannedPath));
        }
        
        if (this.planningFailed) {
            stateMachine.SetState(new StandbyState(stateMachine));
        }
    }

    public override void ExitState()
    {
        Debug.Log("Exited the PLANNING state!");
    }
}
