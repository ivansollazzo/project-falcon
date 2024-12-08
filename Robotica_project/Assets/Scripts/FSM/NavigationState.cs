using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class NavigationState : State
{
    // Attributes
    private RobotController robotController;
    private NavMeshAgent navMeshAgent;
    private bool destinationReached = false;

    private int currentCornerIndex;
    private float closeEnoughDistance = 0.5f;

    private NavMeshPath plannedPath;
    
    // Constructor
    public NavigationState(StateMachine stateMachine, NavMeshPath path) : base(stateMachine) {
        this.robotController = stateMachine.gameObject.GetComponent<RobotController>();
        this.navMeshAgent = robotController.GetNavMeshAgent();
        this.plannedPath = path;
        this.currentCornerIndex = 0;
    }

    public override void EnterState()
    {
        Debug.Log("Entered the NAVIGATION state! Robot is moving to specified destination...");

        // Check if there's a planned path
        if (this.plannedPath != null && plannedPath.corners.Length > 0) {
            Debug.Log("There's a planned path! Robot is starting navigation...");
        }
    }

    public override void ExecuteState()
    {
        // Set robot destination
        Vector3 currentTarget = plannedPath.corners[currentCornerIndex];
        navMeshAgent.SetDestination(currentTarget);

        Debug.Log("Moving to point: " + plannedPath.corners[currentCornerIndex]);


        if (Vector3.Distance(navMeshAgent.transform.position, currentTarget) <= this.closeEnoughDistance) {
            
            // Go to next point
            this.currentCornerIndex++;
            
            if (this.currentCornerIndex >= plannedPath.corners.Length) {
                this.destinationReached = true;
            }
        }

        if (this.destinationReached) {
            stateMachine.SetState(new ArrivalState(stateMachine));
        }
    }

    public override void ExitState()
    {
        Debug.Log("Exited the NAVIGATION state!");
    }
}