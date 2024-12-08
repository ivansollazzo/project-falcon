using UnityEngine;

public class WaitingState : State
{
    // Placeholder, please don't remove it at the moment
    private bool waitComplete = false;
    
    // Constructor
    public WaitingState(StateMachine stateMachine) : base(stateMachine) { }

    public override void EnterState()
    {
        Debug.Log("Entered the WAITING state! Robot is waiting for the obstacle going away...");
    }

    public override void ExecuteState()
    {
        // If user entered a destination, set the navigation state
        if (waitComplete)
        {
            //stateMachine.SetState(new NavigationState(stateMachine));
        }
    }

    public override void ExitState()
    {
        Debug.Log("Exited the WAITING state!");
    }
}