using UnityEngine;
using System.Collections.Generic;

public class PlanningState : State
{
    private RobotController robotController;
    private bool planningComplete = false;
    private Vector3 destination;

    private Cell[,] grid;

    public PlanningState(StateMachine stateMachine) : base(stateMachine)
    {
        this.robotController = stateMachine.gameObject.GetComponent<RobotController>();
        this.destination = robotController.GetDestination();
        this.grid = GridManager.Instance.GetGrid();
    }

    public override void EnterState()
    {
        Debug.Log("Stato PLANNING: Pianificazione del percorso verso la destinazione...");
        Debug.Log("Destinazione: " + destination);
    }

    public override void ExecuteState()
    {
        // Ottengo la posizione corrente del robot
        Vector3 robotPosition = stateMachine.gameObject.transform.position;

        // Ottengo la cella di partenza corrispondente del robot. Itero tutte le celle per ottenere la posizione corrispondente con riferimento alla distanza più vicina.
        Cell startCell = null;
        float minDistance = float.MaxValue;
        foreach (Cell cell in grid)
        {
            float distance = Vector3.Distance(cell.GetWorldPosition(), robotPosition);
            if (distance < minDistance)
            {
                minDistance = distance;
                startCell = cell;
            }
        }

        // Ottengo la cella di destinazione corrispondente alla destinazione del robot. Itero tutte le celle per ottenere la posizione corrispondente con riferimento alla distanza più vicina.
        Cell endCell = null;
        minDistance = float.MaxValue;
        foreach (Cell cell in grid)
        {
            float distance = Vector3.Distance(cell.GetWorldPosition(), destination);
            if (distance < minDistance)
            {
                minDistance = distance;
                endCell = cell;
            }
        }

        // Stampiamo tutto
        Debug.Log("Cella di partenza: " + startCell);
        Debug.Log("Cella di destinazione: " + endCell);
        
    }

    public override void ExitState()
    {
        Debug.Log("Uscito dallo stato PLANNING.");
    }
}
