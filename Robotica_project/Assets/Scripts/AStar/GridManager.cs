using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("Grid settings")]
    public float cellSize = 0.72f;  // Dimensione della cella
    public int gridWidth = 150;  // Numero di celle per la larghezza della mappa
    public int gridHeight = 100;  // Numero di celle per l'altezza della mappa
    private Cell[,] grid;  // Griglia di celle

    // Singleton
    public static GridManager Instance;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    /* Start is called before the first frame update */
    void Start()
    {
        Debug.Log("Generazione della griglia iniziata...");
        GenerateGrid();
        Debug.Log("Griglia generata con successo.");
        DetectBlockedCells();
    }

    public Cell[,] GetGrid()
    {
        return this.grid;
    }

    public void GenerateGrid()
    {
        // Inizializza la griglia
        grid = new Cell[gridWidth, gridHeight];
        
        // Crea le celle della griglia
        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight; z++)
            {
                // Calcola la posizione della cella, partendo dalla posizione del GameObject
                Vector3 relativePosition = new Vector3(x * cellSize, 0, z * cellSize);
                Vector3 worldPosition = transform.position + RotatePosition(relativePosition);
                
                // Crea la cella
                grid[x, z] = new Cell(worldPosition, x, z, true);
            }
        }
    }

    // Funzione per ruotare una posizione rispetto alla rotazione del GameObject
    Vector3 RotatePosition(Vector3 position)
    {
        return transform.rotation * position;
    }

    void DetectBlockedCells()
    {
        // Trova tutti gli oggetti con il tag "BlockedCell" nella scena
        GameObject[] blockedObjects = GameObject.FindGameObjectsWithTag("BlockedCell");
        Debug.Log($"Trovati {blockedObjects.Length} oggetti con il tag 'BlockedCell'.");

        foreach (var obj in blockedObjects)
        {
            Collider objCollider = obj.GetComponent<Collider>();
            if (objCollider != null)
            {
                Bounds objBounds = objCollider.bounds;

                // Confronta le celle della griglia con l'oggetto bloccato
                for (int x = 0; x < gridWidth; x++)
                {
                    for (int z = 0; z < gridHeight; z++)
                    {
                        Vector3 cellPosition = grid[x, z].GetWorldPosition();

                        if (objBounds.Contains(cellPosition))
                        {
                            grid[x, z].SetWalkable(false);
                            grid[x,z].SetGCost(float.MaxValue);
                        }
                    }
                }
            }
        }
    }

    // Metodo per disegnare la griglia usando Gizmos
    private void OnDrawGizmos()
    {
        if (grid == null) return;

        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight; z++)
            {
                Vector3 cellPosition = grid[x, z].GetWorldPosition();
                Gizmos.color = grid[x, z].IsWalkable() ? Color.green : Color.red;
                Gizmos.DrawCube(cellPosition, new Vector3(cellSize, 0.1f, cellSize));
            }
        }
    }

    // Metodo per marcare una cella come bloccata
    public void MarkCellAsBlocked(Vector3 position)
    {
        Cell cell = GetCellFromWorldPosition(position);
        if (cell != null)
        {
            cell.SetWalkable(false);
            cell.SetGCost(float.MaxValue);
            Debug.Log("Cella marcata come bloccata: " + cell);
        }
        else {
            Debug.LogError("Nessuna cella trovata alla posizione specificata: " + position);
        }
    }

    public Cell GetCellFromWorldPosition(Vector3 position)
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