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
        // Verifica se la cella di destinazione è percorribile
        if (!endCell.IsWalkable())
        {
            Debug.LogError("La cella di destinazione non è percorribile.");
            return null;
        }
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
    
        // Ottieni la posizione corrente della cella
        int currentX = (int)cell.GetGridPosition().x;
        int currentZ = (int)cell.GetGridPosition().z;
    
        // Direzioni ortogonali (N, S, W, E)
        (int dx, int dz)[] straightDirections =
        {
            (-1, 0), // Nord
            (1, 0),  // Sud
            (0, -1), // Ovest
            (0, 1)   // Est
        };
    
        // Direzioni diagonali
        (int dx, int dz, (int adjX, int adjZ) first, (int adjX, int adjZ) second)[] diagonalDirections =
        {
            (-1, -1, (-1, 0), (0, -1)), // Nord-Ovest
            (-1, 1, (-1, 0), (0, 1)),  // Nord-Est
            (1, -1, (1, 0), (0, -1)),  // Sud-Ovest
            (1, 1, (1, 0), (0, 1))     // Sud-Est
        };
    
        // Aggiungi prima le celle ortogonali
        foreach (var (dx, dz) in straightDirections)
        {
            int neighborX = currentX + dx;
            int neighborZ = currentZ + dz;
    
            if (IsValidCell(neighborX, neighborZ))
            {
                neighbors.Add(grid[neighborX, neighborZ]);
            }
        }
    
        // Aggiungi le celle diagonali solo se entrambe le celle adiacenti sono percorribili
        foreach (var (dx, dz, first, second) in diagonalDirections)
        {
            int neighborX = currentX + dx;
            int neighborZ = currentZ + dz;
    
            int adjX1 = currentX + first.adjX;
            int adjZ1 = currentZ + first.adjZ;
            int adjX2 = currentX + second.adjX;
            int adjZ2 = currentZ + second.adjZ;
    
            // Verifica che la cella diagonale sia valida e che entrambe le celle   adiacenti siano percorribili
            if (IsValidCell(neighborX, neighborZ) &&
                IsValidCell(adjX1, adjZ1) &&
                IsValidCell(adjX2, adjZ2))
            {
                neighbors.Add(grid[neighborX, neighborZ]);
            }
        }
    
        return neighbors;
    }
    
    // Verifica se una cella è valida (dentro i limiti e percorribile)
    private bool IsValidCell(int x, int z)
    {
        return x >= 0 && x < grid.GetLength(0) &&
               z >= 0 && z < grid.GetLength(1) &&
               grid[x, z].IsWalkable();
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