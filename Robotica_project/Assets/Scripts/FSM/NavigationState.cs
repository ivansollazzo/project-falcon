using System.Collections.Generic;
using UnityEngine;

public class NavigationState : State
{
    // Attributes
    private RobotController robotController;    
    private bool destinationReached = false;
    private int currentCornerIndex = 0;

    private bool obstaclesDetectionEnabled = false;

    private ObstacleSensor obstacleSensor;

    private List<Cell> path;
    

    public NavigationState(StateMachine stateMachine,List<Cell>path) : base(stateMachine)
    {
        this.robotController = stateMachine.gameObject.GetComponent<RobotController>();
        this.obstacleSensor = stateMachine.gameObject.GetComponent<ObstacleSensor>();
        this.path = path;
    }

    public override void EnterState()
    {
        Debug.Log("Entered the NAVIGATION state! Robot is moving to specified destination...");

        // Ottengo tutte le posizioni del percorso
        foreach (Cell cell in path)
        {
            Debug.Log(cell.GetWorldPosition());
        }

        // Imposta il comando di navigazione
        robotController.SetMoving(true);
    }

    public override void ExecuteState()
    {
        // Ottieni la prossima posizione target dal percorso
        Vector3 targetPosition = path[currentCornerIndex].GetWorldPosition();
        
        // Aggiorna la posizione del robot
        bool rotatedToTarget = robotController.RotateToTarget(targetPosition);

        if (rotatedToTarget) {
            // Enable obstacle detection
            obstaclesDetectionEnabled = true;
        }

        if (obstaclesDetectionEnabled)
        {
            Vector3? obstaclePosition = obstacleSensor.CheckForObstacles();
            
            if (obstaclePosition != null) {
                Debug.Log("Ostacolo rilevato. Passaggio al waiting state...");

                stateMachine.SetState(new WaitingState(stateMachine, 5.0f, this));
            }
        }

        if (rotatedToTarget) {
            
            bool movedToTarget = robotController.MoveToTarget(targetPosition);

            if (movedToTarget) {
                Debug.Log("Posizione punto " + currentCornerIndex + ": " + targetPosition + " raggiunta.");
                        
                obstaclesDetectionEnabled = false;

                // Move to the next corner
                currentCornerIndex++;
            }
        }
        if (currentCornerIndex >= path.Count) {
            this.destinationReached = true;
        }
        if (destinationReached)
        {
            stateMachine.SetState(new ArrivalState(stateMachine));
        }
    }

    public override void ExitState()
    {
        // Disabilita il movimento
        robotController.SetMoving(false);

        Debug.Log("Exited the NAVIGATION state!");
    }
}