using UnityEngine;

public class StandbyState : State
{
    // Placeholder, please don't remove it at the moment
    private bool userEnteredDestination = false;
    
    // Constructor
    public StandbyState(StateMachine stateMachine) : base(stateMachine) { }

    public override void EnterState()
    {
        Debug.Log("Entered the STANDBY state! Robot is waiting for input by user...");
    }

    public override void ExecuteState()
    {
        // If user entered a destination, set the planning state
        if (userEnteredDestination)
        {
            stateMachine.SetState(new PlanningState(stateMachine));
        }
    }

    public override void ExitState()
    {
        Debug.Log("Exited the STANDBY state!");
    }
}
