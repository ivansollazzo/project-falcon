using System;
using System.Collections.Generic;
using UnityEngine;

public class AStar
{

    // Lista contenente le celle aperte
    private List<Cell> openList;

    // Lista contentenen le celle chiuse
    private HashSet<Cell> closedList;

    // Griglia di celle
    private Cell[,] grid;

    // Cella iniziale
    private Cell startCell;

    // Cella di destinazione
    private Cell endCell;

    // Costruttore

    public AStar(Cell[,] grid, Cell startCell, Cell endCell)
    {
        this.grid = grid;
        this.startCell = startCell;
        this.endCell = endCell;
        openList = new List<Cell>();
        closedList = new HashSet<Cell>();
    }

    // Metodo principale per eseguire l'algoritmo A*
    public List<Cell> FindPath()
    {
        // Inizializzazione delle liste
        openList.Clear();
        closedList.Clear();

        // Aggiunta della cella di partenza alla open list
        openList.Add(startCell);

        while (openList.Count > 0)
        {
            // Trova la cella con il costo f più basso
            Cell currentCell = GetCellWithLowestFCost(openList);

            // Se la destinazione è raggiunta, fermati
            if (currentCell == endCell)
            {
                return RetracePath();
            }

            // Muovi la cella corrente dalla open list alla closed list
            openList.Remove(currentCell);
            closedList.Add(currentCell);

            // Esplora i vicini
            foreach (Cell neighbor in GetNeighbors(currentCell))
            {
                // Se la cella non è percorribile o è già nella closed list, salta
                if (!neighbor.IsWalkable() || closedList.Contains(neighbor))
                    continue;

                // Calcola il nuovo gCost per il vicino
                float tentativeGCost = currentCell.GetGCost() + GetManhattanDistance(currentCell.GetWorldPosition(), neighbor.GetWorldPosition());

                // Se il vicino non è nella open list o se abbiamo trovato un percorso migliore
                if (!openList.Contains(neighbor) || tentativeGCost < neighbor.GetGCost())
                {
                    // Imposta il gCost, hCost e il parent del vicino
                    neighbor.SetGCost(tentativeGCost);
                    neighbor.CalculateHCost(endCell);
                    neighbor.SetParent(currentCell);

                    // Aggiungi il vicino alla open list
                    if (!openList.Contains(neighbor))
                        openList.Add(neighbor);
                }
            }
        }

        // Se siamo usciti dal ciclo, non esiste un percorso
        Debug.Log("Non esiste un percorso valido!");
        return null;
    }

    // Metodo per ottenere la distanza Manhattan tra due vector 3
    private float GetManhattanDistance(Vector3 a, Vector3 b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.z - b.z);
    }
    

    // Trova la cella con il costo f più basso
    private Cell GetCellWithLowestFCost(List<Cell> cells)
    {
        Cell lowestFCostCell = cells[0];
        foreach (Cell cell in cells)
        {
            if (cell.GetFCost() < lowestFCostCell.GetFCost())
                lowestFCostCell = cell;
        }
        return lowestFCostCell;
    }

    // Ottieni i vicini di una cella
    private List<Cell> GetNeighbors(Cell cell)
    {
        List<Cell> neighbors = new List<Cell>();

        // Controlla le celle adiacenti (N, S, E, W)
        int[] directions = { -1, 1 };

        foreach (int dx in directions)
        {
            foreach (int dz in directions)
            {
                if (dx == 0 && dz == 0)
                    continue;

                int checkX = Convert.ToInt32(cell.GetGridPosition().x) + dx;
                int checkZ = Convert.ToInt32(cell.GetGridPosition().z) + dz;

                // Verifica se il vicino è dentro i limiti della griglia
                if (checkX >= 0 && checkX < grid.GetLength(0) && checkZ >= 0 && checkZ < grid.GetLength(1))
                {
                    neighbors.Add(grid[checkX, checkZ]);
                }
            }
        }

        return neighbors;
    }

    // Traccia il percorso (retrocedi dalla destinazione alla partenza)
    private List<Cell> RetracePath()
    {
        List<Cell> path = new List<Cell>();
        Cell currentCell = endCell;

        while (currentCell != startCell)
        {
            path.Add(currentCell);
            currentCell = currentCell.GetParent();  // Supponiamo che ogni cella abbia un riferimento al "genitore" (parent)
        }

        path.Add(startCell);
        path.Reverse();
        return path;
    }
}