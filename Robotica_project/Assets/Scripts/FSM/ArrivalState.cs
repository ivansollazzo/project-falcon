using UnityEngine;

public class ArrivalState : State
{
    public ArrivalState(StateMachine stateMachine) : base(stateMachine) {}

    public override void EnterState()
    {
        Debug.Log("Arrivato alla destinazione! Stato di ARRIVAL.");
    }

    public override void ExecuteState()
    {
        // Get the robot controller
        RobotController robotController = stateMachine.gameObject.GetComponent<RobotController>();
        TTSManager ttsManager = robotController.GetTTSManager();

        // Speak the arrival message
        ttsManager.Speak("Sei arrivato a destinazione!");

        Debug.Log("Arrivato! Pronto per la prossima Destinazione");
        robotController.ClearDestination();
        stateMachine.SetState(new StandbyState(stateMachine));

        // Se ci sono path drawer, li rimuovo
        PathDrawer[] pathDrawers = stateMachine.gameObject.GetComponents<PathDrawer>();
        foreach (PathDrawer pathDrawer in pathDrawers)
        {
            pathDrawer.ClearPath();
            GameObject.Destroy(pathDrawer);
        }

        // Destroy the current grid
        GridManager.Instance.GenerateGrid();
        GridManager.Instance.DetectBlockedCells();
    }

    public override void ExitState()
    {
        Debug.Log("Uscendo dallo stato ARRIVAL.");
    }
}
