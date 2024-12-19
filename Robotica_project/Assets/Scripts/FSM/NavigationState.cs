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

    
    // Constructor
    public NavigationState(StateMachine stateMachine) : base(stateMachine) {
        this.robotController = stateMachine.gameObject.GetComponent<RobotController>();
        this.currentCornerIndex = 0;
    }

    public override void EnterState()
    {
        Debug.Log("Entered the NAVIGATION state! Robot is moving to specified destination...");
    }

    public override void ExecuteState()
    {
    }

    public override void ExitState()
    {
        Debug.Log("Exited the NAVIGATION state!");
    }
}