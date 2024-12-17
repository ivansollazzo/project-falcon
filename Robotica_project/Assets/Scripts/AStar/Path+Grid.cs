using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathfindingRobot : MonoBehaviour
{
    [Header("Grid Settings")]
    public float cellSize = 0.72f;  // Dimensione della cella
    public int gridWidth = 150;  // Larghezza della griglia
    public int gridHeight = 100;  // Altezza della griglia
    public LayerMask blockedLayer;  // Layer per rilevare celle bloccate

    private Cell[,] grid;  // La griglia di celle
    private List<Cell> path;  // Il percorso calcolato
    private Vector3 startPosition;  // Posizione di partenza della griglia

    [Header("A* Settings")]
    public Vector3 goalPosition;  // Posizione obiettivo del robot
    private Cell startCell, goalCell;  // Celle di partenza e obiettivo
    private List<Cell> openList, closedList;  // Liste per A*

    void Start()
    {
        startPosition = transform.position;
        GenerateGrid();
        FindPath();
    }

    // Generazione della griglia
    void GenerateGrid()
    {
        grid = new Cell[gridWidth, gridHeight];
        Debug.Log("Generazione griglia...");

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector3 worldPosition = startPosition + new Vector3(x * cellSize, 0, y * cellSize);

                // Verifica se la cella è bloccata
                bool isWalkable = !Physics.CheckSphere(worldPosition, cellSize / 2, blockedLayer);
                grid[x, y] = new Cell(worldPosition, isWalkable);
            }
        }
        Debug.Log("Griglia generata con successo.");
    }

    // Calcolo del percorso con A*
    void FindPath()
    {
        startCell = GetCellAtPosition(transform.position);
        goalCell = GetCellAtPosition(goalPosition);

        if (startCell == null || goalCell == null || !goalCell.isWalkable)
        {
            Debug.LogError("Cella di partenza o obiettivo non valida!");
            return;
        }

        openList = new List<Cell>();
        closedList = new List<Cell>();
        path = new List<Cell>();

        openList.Add(startCell);

        while (openList.Count > 0)
        {
            Cell currentCell = GetCellWithLowestF();

            if (currentCell == goalCell)
            {
                ReconstructPath(currentCell);
                return;
            }

            openList.Remove(currentCell);
            closedList.Add(currentCell);

            foreach (var neighbor in GetNeighbors(currentCell))
            {
                if (closedList.Contains(neighbor) || !neighbor.isWalkable)
                    continue;

                float tentativeG = currentCell.g + Vector3.Distance(currentCell.position, neighbor.position);

                if (!openList.Contains(neighbor) || tentativeG < neighbor.g)
                {
                    neighbor.g = tentativeG;
                    neighbor.h = Vector3.Distance(neighbor.position, goalCell.position);
                    neighbor.f = neighbor.g + neighbor.h;
                    neighbor.parent = currentCell;

                    if (!openList.Contains(neighbor))
                        openList.Add(neighbor);
                }
            }
        }

        Debug.LogError("Nessun percorso trovato!");
    }

    // Restituisce la cella con il punteggio F più basso
    Cell GetCellWithLowestF()
    {
        Cell lowestFCell = openList[0];
        foreach (var cell in openList)
        {
            if (cell.f < lowestFCell.f)
                lowestFCell = cell;
        }
        return lowestFCell;
    }

    // Ricostruisce il percorso
    void ReconstructPath(Cell currentCell)
    {
        path.Clear();
        while (currentCell != null)
        {
            path.Add(currentCell);
            currentCell = currentCell.parent;
        }
        path.Reverse();
        Debug.Log("Percorso ricostruito!");
    }

    // Ottiene la cella alla posizione specificata
    Cell GetCellAtPosition(Vector3 worldPosition)
    {
        int x = Mathf.FloorToInt((worldPosition.x - startPosition.x) / cellSize);
        int y = Mathf.FloorToInt((worldPosition.z - startPosition.z) / cellSize);

        if (x >= 0 && x < gridWidth && y >= 0 && y < gridHeight)
        {
            return grid[x, y];
        }
        return null;
    }

    // Restituisce i vicini di una cella
    List<Cell> GetNeighbors(Cell cell)
    {
        List<Cell> neighbors = new List<Cell>();
        int x = Mathf.FloorToInt((cell.position.x - startPosition.x) / cellSize);
        int y = Mathf.FloorToInt((cell.position.z - startPosition.z) / cellSize);

        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;

                int nx = x + dx;
                int ny = y + dy;

                if (nx >= 0 && nx < gridWidth && ny >= 0 && ny < gridHeight)
                {
                    neighbors.Add(grid[nx, ny]);
                }
            }
        }
        return neighbors;
    }

    // Disegna la griglia e il percorso con i Gizmos
    private void OnDrawGizmos()
    {
        if (grid != null)
        {
            foreach (var cell in grid)
            {
                Gizmos.color = cell.isWalkable ? Color.green : Color.red;
                Gizmos.DrawCube(cell.position, Vector3.one * (cellSize - 0.1f));
            }
        }

        if (path != null)
        {
            Gizmos.color = Color.blue;
            foreach (var cell in path)
            {
                Gizmos.DrawSphere(cell.position, cellSize / 4);
            }
        }
    }

    // Classe per rappresentare una cella nella griglia
    public class Cell
    {
        public Vector3 position;
        public bool isWalkable;
        public float g, h, f;
        public Cell parent;

        public Cell(Vector3 position, bool isWalkable)
        {
            this.position = position;
            this.isWalkable = isWalkable;
            this.g = this.h = this.f = 0;
            this.parent = null;
        }
    }
}
