using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class PlanningState : State
{
    // Attributes
    private RobotController robotController;
    private bool planningComplete = false;
    private bool planningFailed = false;
    private Vector3 destination;

    // Constructor
    public PlanningState(StateMachine stateMachine) : base(stateMachine) {
        this.robotController = stateMachine.gameObject.GetComponent<RobotController>();
        this.destination = robotController.GetDestination();
    }

    public override void EnterState()
    {
        Debug.Log("Entered the PLANNING state! Robot is planning a route for the entered destination...");
    }

    public override void ExecuteState()
    {
        if (!planningComplete)
        {
            // Calcola il percorso usando A*
            Vector3 startPosition = robotController.transform.position;
            List<GridManager.Cell> path = AStar(startPosition, destination);

            if (path != null)
            {
                Debug.Log("Percorso pianificato con successo!");
                //robotController.SetPath(path);
                planningComplete = true;
                //stateMachine.SetState(new NavigationState(stateMachine, path));
            }
            else
            {
                Debug.Log("Pianificazione fallita!");
                planningFailed = true;
            }
        }
    }

    public override void ExitState()
    {
        Debug.Log("Exited the PLANNING state!");
    }

private List<GridManager.Cell> AStar(Vector3 startPos, Vector3 targetPos)
{
    // Ottieni la griglia
    GridManager.Cell[,] grid = GridManager.Instance.GetGrid();

    Debug.Log("Posizione iniziale: " + startPos);
    Debug.Log("Posizione target: " + targetPos);

    // Converti le posizioni del mondo in celle della griglia
    GridManager.Cell startCell = GetCellFromWorldPosition(startPos, grid);
    GridManager.Cell targetCell = GetCellFromWorldPosition(targetPos, grid);

    Debug.Log("Cella iniziale: " + startCell);
    Debug.Log("Cella target: " + targetCell);


    if (startCell == null || targetCell == null || !targetCell.isWalkable)
    {
        Debug.LogError("Posizione iniziale o finale non valida.");
        return null;
    }

    List<GridManager.Cell> openSet = new List<GridManager.Cell>();
    HashSet<GridManager.Cell> closedSet = new HashSet<GridManager.Cell>();

    openSet.Add(startCell);

    while (openSet.Count > 0)
    {
        // Trova il nodo con il costo totale più basso (f = g + h)
        GridManager.Cell currentCell = openSet[0];
        for (int i = 1; i < openSet.Count; i++)
        {
            if (openSet[i].g + openSet[i].h < currentCell.g + currentCell.h)
                currentCell = openSet[i];
        }

        openSet.Remove(currentCell);
        closedSet.Add(currentCell);

        // Se abbiamo raggiunto il target
        if (currentCell == targetCell)
        {
            return RetracePath(startCell, targetCell);
        }

        // Controlla i vicini
        foreach (GridManager.Cell neighbor in GetNeighbors(currentCell, grid))
        {
            if (!neighbor.isWalkable || closedSet.Contains(neighbor))
                continue;

            float newCostToNeighbor = currentCell.g + Vector3.Distance(currentCell.position, neighbor.position);
            if (newCostToNeighbor < neighbor.g || !openSet.Contains(neighbor))
            {
                neighbor.g = newCostToNeighbor;
                neighbor.h = Vector3.Distance(neighbor.position, targetCell.position);
                neighbor.parent = currentCell;

                if (!openSet.Contains(neighbor))
                    openSet.Add(neighbor);
            }
        }
    }

    return null;  // Nessun percorso trovato
}

    private GridManager.Cell GetCellFromWorldPosition(Vector3 position, GridManager.Cell[,] grid)
    {
        // Trova la cella più vicina alla posizione nel mondo
        for (int x = 0; x < grid.GetLength(0); x++)
        {
            for (int y = 0; y < grid.GetLength(1); y++)
            {
                if (Vector3.Distance(grid[x, y].position, position) < GridManager.Instance.cellSize / 2)
                {
                    Debug.Log("Coordinate cella ottenute: " + grid[x, y]);
                    return grid[x, y];
                }
            }
        }
        return null;
    }

    private List<GridManager.Cell> GetNeighbors(GridManager.Cell cell, GridManager.Cell[,] grid)
    {
        List<GridManager.Cell> neighbors = new List<GridManager.Cell>();

        int x = cell.x;
        int y = cell.y;

        // Aggiungi tutte le celle adiacenti
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;

                int checkX = x + dx;
                int checkY = y + dy;

                if (checkX >= 0 && checkX < grid.GetLength(0) && checkY >= 0 && checkY < grid.GetLength(1))
                {
                    neighbors.Add(grid[checkX, checkY]);
                }
            }
        }
        return neighbors;
    }

    private List<GridManager.Cell> RetracePath(GridManager.Cell startCell, GridManager.Cell endCell)
    {
        List<GridManager.Cell> path = new List<GridManager.Cell>();
        GridManager.Cell currentCell = endCell;

        while (currentCell != startCell)
        {
            path.Add(currentCell);
            currentCell = currentCell.parent;
        }

        path.Reverse();
        return path;
    }


}
