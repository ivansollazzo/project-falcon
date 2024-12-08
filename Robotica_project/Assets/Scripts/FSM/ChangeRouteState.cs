using UnityEngine;

public class ChangeRouteState : State
{
    // Placeholder, please don't remove it at the moment
    private bool planningComplete = false;
    
    // Constructor
    public ChangeRouteState(StateMachine stateMachine) : base(stateMachine) { }

    public override void EnterState()
    {
        Debug.Log("Entered the CHANGE_ROUTE state! Robot is planning a route for the entered destination...");
    }

    public override void ExecuteState()
    {
        // If user entered a destination, set the planning state
        if (planningComplete)
        {
            //stateMachine.SetState(new NavigationState(stateMachine));
        }
    }

    public override void ExitState()
    {
        Debug.Log("Exited the CHANGE_ROUTE state!");
    }
}