using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public float cellSize = 0.72f;  // Dimensione della cella
    public int gridWidth = 150;  // Numero di celle per la larghezza della mappa
    public int gridHeight = 100;  // Numero di celle per l'altezza della mappa
    private Cell[,] grid;  // Griglia di celle

    public static GridManager Instance;

    private void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        Debug.Log("Generazione della griglia iniziata...");
        GenerateGrid();
        Debug.Log("Griglia generata con successo.");
    }

    public Cell[,] GetGrid() {
        return this.grid;
    }

    void GenerateGrid()
    {
        // Inizializza la griglia
        grid = new Cell[gridWidth, gridHeight];
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                // Calcola la posizione della cella, partendo dalla posizione del GameObject
                Vector3 relativePosition = new Vector3(x * cellSize, 0, y * cellSize);
                Vector3 worldPosition = transform.position + RotatePosition(relativePosition);
                grid[x, y] = new Cell(worldPosition, true); // Imposta tutte le celle come percorribili di default
            }
        }

        // Rileva le aree non percorribili
        DetectBlockedCells();
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
                    for (int y = 0; y < gridHeight; y++)
                    {
                        Vector3 cellPosition = grid[x, y].position;

                        if (objBounds.Contains(cellPosition))
                        {
                            grid[x, y].isWalkable = false; // Se la cella è nel collider, non è percorribile
                            Debug.Log($"Cella a posizione ({x},{y}) non percorribile.");
                        }
                    }
                }
            }
        }
    }

    // Restituisce la cella nella posizione specificata
    public Cell GetCellAtPosition(Vector3 worldPosition)
    {
        int x = Mathf.FloorToInt((worldPosition.x - transform.position.x) / cellSize);
        int y = Mathf.FloorToInt((worldPosition.z - transform.position.z) / cellSize);

        if (x >= 0 && x < gridWidth && y >= 0 && y < gridHeight)
        {
            return grid[x, y];
        }

        Debug.LogWarning($"Posizione ({worldPosition}) fuori dai limiti della griglia.");
        return null;
    }

    // Definisce la cella (per l'algoritmo A*)
    public class Cell
    {
        public Vector3 position; // Posizione globale della cella
        public bool isWalkable; // Se la cella è percorribile
        public int x, y; // Indici della cella
        public float g, h; // Distanza dal punto di partenza (g) e stima della distanza al punto di arrivo (h)
        public Cell parent; // Cella precedente nel percorso
        private Vector3 cellWorldPosition;

        public Cell(Vector3 position, bool isWalkable)
        {
            this.position = position;
            this.isWalkable = isWalkable;
        }

        public Cell(Vector3 cellWorldPosition, int x, int y)
        {
            this.cellWorldPosition = cellWorldPosition;
            this.x = x;
            this.y = y;
        }

        public override string ToString()
        {
            return $"Cell [x: {x}, y: {y}, pos: {position}, walkable: {isWalkable}]";
        }

        public Cell(Vector3 pos, int x, int y, bool walkable)
        {
            this.position = pos;
            this.x = x;
            this.y = y;
            this.isWalkable = walkable;
            this.g = float.MaxValue; // Inizialmente impostato a infinito
            this.h = 0;
            this.parent = null;
        }
    }

    // Metodo per disegnare la griglia usando Gizmos
    private void OnDrawGizmos()
    {
        if (grid == null) return;

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector3 cellPosition = grid[x, y].position;
                Gizmos.color = grid[x, y].isWalkable ? Color.green : Color.red;
                Gizmos.DrawCube(cellPosition, new Vector3(cellSize, 0.1f, cellSize));
            }
        }
    }

     // Metodo per ottenere la cella dalla posizione nel mondo
    public Cell GetCellFromWorldPosition(Vector3 worldPosition)
    {
        // Supponiamo che la griglia abbia origine nell'angolo inferiore sinistro e che la posizione della cella sia un multiplo della dimensione della cella
        int x = Mathf.FloorToInt(worldPosition.x / cellSize);
        int y = Mathf.FloorToInt(worldPosition.z / cellSize);
        
        // Assicurati che le coordinate siano all'interno dei limiti della griglia
        x = Mathf.Clamp(x, 0, gridWidth - 1);
        y = Mathf.Clamp(y, 0, gridHeight - 1);

        // Calcola la posizione mondiale della cella
        Vector3 cellWorldPosition = new Vector3(x * cellSize, 0, y * cellSize);
        
        // Restituisci la cella
        return new Cell(cellWorldPosition, x, y);
    }

}
