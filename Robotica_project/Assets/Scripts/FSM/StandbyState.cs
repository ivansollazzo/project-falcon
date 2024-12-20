using UnityEngine;

public class StandbyState : State
{
    // Attributes
    private RobotController robotController;
    private bool destinationSet;

    // Constructor
    public StandbyState(StateMachine stateMachine) : base(stateMachine)
    {
        robotController = stateMachine.gameObject.GetComponent<RobotController>();
        destinationSet = false;
    }

    public override void EnterState()
    {
        Debug.Log("Entered the STANDBY state! Robot is waiting for input by user...");
    }

    public override void ExecuteState()
    {
        // Imposta destinazione
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 destination = GetDestinationFromMouseClick();
            Vector3 gridDestination = new Vector3(destination.x, -0.01f, destination.z);
            robotController.SetDestination(gridDestination);
            destinationSet = true;
        }

        // Passa a PlanningState
        if (destinationSet)
        {
            stateMachine.SetState(new PlanningState(stateMachine));
        }
    }


    public override void ExitState()
    {
        Debug.Log("Exited the STANDBY state!");
    }

    // Get destination from click
    private Vector3 GetDestinationFromMouseClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            return hit.point;
        }

        return Vector3.zero;
    }
}
