using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class NavigationState : State
{
    // Attributes
    private RobotController robotController;    
    private bool destinationReached = false;
    private int currentCornerIndex = 0;

    private bool obstaclesDetectionEnabled = false;

    private List<Cell> path;
    

    public NavigationState(StateMachine stateMachine,List<Cell>path) : base(stateMachine)
    {
        this.robotController = stateMachine.gameObject.GetComponent<RobotController>();
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
            // Controllo se il robot ha ostacoli davanti a se utilizzando il filtro di kalman
            robotController.StartCoroutine(robotController.CheckObstacle((obstacleMove) =>
            {
                if (obstacleMove)
                {
                    Debug.Log("Ostacolo fisso rilevato. Ripianificazione in corso...");

                    // Transizione allo stato di pianificazione
                    stateMachine.SetState(new PlanningState(stateMachine));
                    return;
                }

                if (rotatedToTarget)
                {
                    bool movedToTarget = robotController.MoveToTarget(targetPosition);

                    if (movedToTarget) {
                        Debug.Log("Posizione punto " + currentCornerIndex + ": " + targetPosition + " raggiunta.");
                        obstaclesDetectionEnabled = false;
                        currentCornerIndex++;
                    }
                }

                if (currentCornerIndex >= path.Count)
                {
                    this.destinationReached = true;
                }
                if (destinationReached)
                {
                    stateMachine.SetState(new ArrivalState(stateMachine));
                }
            }));
        }
    }

    public override void ExitState()
    {
        Debug.Log("Exited the NAVIGATION state!");
    }
}