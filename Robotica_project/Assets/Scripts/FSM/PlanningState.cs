using UnityEngine;
using System.Collections.Generic;

public class PlanningState : State
{
    private RobotController robotController; // Controller del robot
    private bool planningComplete = false;  // Pianificazione completata
    private Vector3 destination;            // Destinazione finale

    public PlanningState(StateMachine stateMachine) : base(stateMachine)
    {
        robotController = stateMachine.gameObject.GetComponent<RobotController>();
        destination = robotController.GetDestination();
    }

    public override void EnterState()
    {
        Debug.Log("Stato PLANNING: Pianificazione del percorso verso la destinazione...");
    }

    public override void ExecuteState()
    {
    }

    public override void ExitState()
    {
        Debug.Log("Uscito dallo stato PLANNING.");
    }
}
