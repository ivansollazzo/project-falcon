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
    private float closeEnoughDistance = 0.5f;

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

        // Controlla se ci sono altri punti target nella stessa direzione e vai direttamente a quel punto
        for (int i = currentCornerIndex + 1; i < path.Count; i++)
        {
            Vector3 nextPosition = path[i].GetWorldPosition();
            Vector3 direction = nextPosition - targetPosition;
            direction.Normalize();
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            // Se la direzione è maggiore di una certa soglia, allora non andare direttamente a quel punto
            if (Quaternion.Angle(robotController.transform.rotation, targetRotation) > 0.25f)
            {
                break;
            }

            targetPosition = nextPosition;
        }
        
        // Aggiorna la posizione del robot
        bool rotatedToTarget = robotController.RotateToTarget(targetPosition);

        if (rotatedToTarget)
        {
            bool movedToTarget = robotController.MoveToTarget(targetPosition);

            if (movedToTarget) {
                Debug.Log("Posizione punto " + currentCornerIndex + ": " + targetPosition + " raggiunta.");
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
    }

    public override void ExitState()
    {
        Debug.Log("Exited the NAVIGATION state!");
    }
}