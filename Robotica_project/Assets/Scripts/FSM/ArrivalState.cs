using UnityEngine;

public class ArrivalState : State
{
    public ArrivalState(StateMachine sm) : base(sm) { }

    public override void EnterState()
    {
        Debug.Log("Robot entered ARRIVAL state!");
    }

    public override void ExecuteState()
    {
        Debug.Log("Robot has reached the target!");
        stateMachine.SetState(new StandbyState(stateMachine));
    }

    public override void ExitState()
    {
        Debug.Log("Robot is exiting ARRIVAL state!");
    }
}
