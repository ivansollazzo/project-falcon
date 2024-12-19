using UnityEngine;
using System.Collections.Generic;

public class PlanningState : State
{
    private RobotController robotController;
    private bool planningComplete = false;
    private Vector3 destination;

    private Cell[,] grid;

    private List<Cell> path;

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

        // Se ci sono path drawer, li rimuovo
        PathDrawer[] pathDrawers = stateMachine.gameObject.GetComponents<PathDrawer>();
        foreach (PathDrawer pathDrawer in pathDrawers)
        {
            GameObject.Destroy(pathDrawer);
        }
    }

    public override void ExecuteState()
    {
        // Ottengo la posizione corrente del robot
        Vector3 robotPosition = stateMachine.gameObject.transform.position;

        // Ottengo la cella di partenza corrispondente del robot. Itero tutte le celle per ottenere la posizione corrispondente con riferimento alla distanza più vicina.
        Cell startCell = GetClosestCell(robotPosition);
        Cell endCell = GetClosestCell(destination);

        // Stampiamo tutto
        Debug.Log("Cella di partenza: " + startCell);
        Debug.Log("Cella di destinazione: " + endCell);

        // Creiamo un'istanza di AStar
        AStar aStar = new AStar(grid, startCell, endCell);

        // Cerchiamo il percorso
        path = aStar.FindPath();

        if (path != null)
        {
            Debug.Log("Percorso trovato!");
            Debug.Log(path.Count + " celle nel percorso.");
            Debug.Log(path);
            planningComplete = true;

            // Disegna il percorso
            PathDrawer pathDrawer = stateMachine.gameObject.AddComponent<PathDrawer>();
            pathDrawer.path = path;
        }
        else
        {
            Debug.Log("Percorso non trovato!");
        }

        // Passa allo stato di standby
        stateMachine.SetState(new StandbyState(stateMachine));
    }

    public override void ExitState()
    {
        Debug.Log("Uscito dallo stato PLANNING.");
    }

    // Metodo per ottenere la cella più vicina
    public Cell GetClosestCell(Vector3 position)
    {
        Cell closestCell = null;
        float minDistance = float.MaxValue;
        foreach (Cell cell in grid)
        {
            float distance = Vector3.Distance(cell.GetWorldPosition(), position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestCell = cell;
            }
        }

        return closestCell;
    }
}
