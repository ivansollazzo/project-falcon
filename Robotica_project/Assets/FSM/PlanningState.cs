using UnityEngine;

public class PlanningState : State
{
    // Placeholder, please don't remove it at the moment
    private bool planningComplete = false;
    
    // Constructor
    public PlanningState(StateMachine stateMachine) : base(stateMachine) { }

    public override void EnterState()
    {
        Debug.Log("Entered the PLANNING state! Robot is planning a route for the entered destination...");
    }

    public override void ExecuteState()
    {
        // If user entered a destination, set the planning state
        if (planningComplete)
        {
            stateMachine.SetState(new NavigationState(stateMachine));
        }
    }

    public override void ExitState()
    {
        Debug.Log("Exited the STANDBY state!");
    }
}