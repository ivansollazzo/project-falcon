using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFinder : MonoBehaviour
{
    public GridManager gridManager;  // Riferimento al GridManager per accedere alla griglia

    private Vector3 targetPosition;  // Posizione di destinazione
    private List<GridManager.Cell> path;  // Lista delle celle che costituiscono il percorso
    private float moveSpeed = 1f;  // Velocità di movimento del GameObject

    public GameObject startMarkerPrefab;  // Prefab per il punto di partenza
    public GameObject endMarkerPrefab;    // Prefab per il punto di arrivo

    void Start()
    {
        // Imposta il targetPosition (ad esempio, 7.2 unità a destra e 7.2 unità sopra)
        targetPosition = new Vector3(transform.position.x + 7.2f, transform.position.y, transform.position.z + 7.2f);

        // Calcola il percorso (assicurati che questa funzione ritorni un percorso valido)
        path = FindPath(transform.position, targetPosition);

        // Verifica che il percorso sia stato trovato
        if (path != null && path.Count > 0)
        {
            Debug.Log("Percorso trovato, numero di punti: " + path.Count);
        }
        else
        {
            Debug.LogWarning("Nessun percorso trovato!");
        }

        // Crea un oggetto visivo per il punto di partenza
        GameObject startMarker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        startMarker.transform.position = transform.position;
        startMarker.GetComponent<Renderer>().material.color = Color.green;

        // Crea un oggetto visivo per il punto di arrivo
        GameObject endMarker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        endMarker.transform.position = targetPosition;
        endMarker.GetComponent<Renderer>().material.color = Color.red;

        // Avvia il movimento lungo il percorso
        StartCoroutine(FollowPath());
    }


    // Trova il percorso dalla posizione corrente alla destinazione
        List<GridManager.Cell> FindPath(Vector3 startPos, Vector3 targetPos)
    {
        // Esegui la logica di ricerca del percorso (ad esempio BFS o A*)
        List<GridManager.Cell> pathList = new List<GridManager.Cell>();

        // Cerca la cella di partenza e la cella di arrivo nella griglia
        GridManager.Cell startCell = gridManager.GetCellFromWorldPosition(startPos);
        GridManager.Cell endCell = gridManager.GetCellFromWorldPosition(targetPos);

        // Esegui la ricerca del percorso dalla cella di partenza alla cella di arrivo
        // Qui puoi inserire il codice per trovare il percorso, ad esempio con A* o BFS

        // Supponiamo che tu stia aggiungendo delle celle al percorso
        pathList.Add(startCell);  // Aggiungi la cella di partenza
        pathList.Add(endCell);    // Aggiungi la cella di arrivo

        // Restituisci la lista delle celle del percorso
        return pathList;
    }


    // Implementazione dell'algoritmo A*
    private List<GridManager.Cell> AStar(GridManager.Cell[,] grid, int startX, int startY, int endX, int endY)
    {
        List<GridManager.Cell> openList = new List<GridManager.Cell>();
        HashSet<GridManager.Cell> closedList = new HashSet<GridManager.Cell>();

        GridManager.Cell startCell = grid[startX, startY];
        GridManager.Cell endCell = grid[endX, endY];

        openList.Add(startCell);

        while (openList.Count > 0)
        {
            // Trova la cella con il valore f (costo totale più stima)
            GridManager.Cell currentCell = openList[0];
            foreach (var cell in openList)
            {
                if (GetF(cell) < GetF(currentCell))
                {
                    currentCell = cell;
                }
            }

            openList.Remove(currentCell);
            closedList.Add(currentCell);

            // Se abbiamo raggiunto la destinazione, ricostruisci il percorso
            if (currentCell == endCell)
            {
                return ReconstructPath(currentCell);
            }

            // Esamina i vicini
            List<GridManager.Cell> neighbors = GetNeighbors(currentCell, grid);
            foreach (var neighbor in neighbors)
            {
                if (closedList.Contains(neighbor) || !neighbor.isWalkable) continue;

                float tentativeG = currentCell.g + GetDistance(currentCell, neighbor);

                if (!openList.Contains(neighbor) || tentativeG < neighbor.g)
                {
                    neighbor.g = tentativeG;
                    neighbor.h = GetDistance(neighbor, endCell);
                    neighbor.parent = currentCell;

                    if (!openList.Contains(neighbor))
                    {
                        openList.Add(neighbor);
                    }
                }
            }
        }

        return new List<GridManager.Cell>();  // Se non c'è un percorso
    }

    // Ricostruisci il percorso da fine a inizio
    private List<GridManager.Cell> ReconstructPath(GridManager.Cell endCell)
    {
        List<GridManager.Cell> path = new List<GridManager.Cell>();
        GridManager.Cell current = endCell;

        while (current != null)
        {
            path.Add(current);
            current = current.parent;
        }

        path.Reverse();
        return path;
    }

    // Ottieni i vicini di una cella
    private List<GridManager.Cell> GetNeighbors(GridManager.Cell cell, GridManager.Cell[,] grid)
    {
        List<GridManager.Cell> neighbors = new List<GridManager.Cell>();
        int[] dx = { -1, 1, 0, 0 }; // Offset per gli spostamenti su X
        int[] dy = { 0, 0, -1, 1 }; // Offset per gli spostamenti su Y

        for (int i = 0; i < 4; i++) // Cambia in 8 per includere diagonali
        {
            int nx = cell.x + dx[i];
            int ny = cell.y + dy[i];

            if (nx >= 0 && nx < grid.GetLength(0) && ny >= 0 && ny < grid.GetLength(1))
            {
                neighbors.Add(grid[nx, ny]);
            }
        }

        return neighbors;
    }

    // Calcola la distanza tra due celle (per il costo di spostamento)
    private float GetDistance(GridManager.Cell a, GridManager.Cell b)
    {
        int dx = Mathf.Abs(a.x - b.x);
        int dy = Mathf.Abs(a.y - b.y);
        return dx + dy;
    }

    // Calcola il valore f (g + h)
    private float GetF(GridManager.Cell cell)
    {
        return cell.g + cell.h;
    }

    // Segui il percorso trovato
    private IEnumerator FollowPath()
    {
        foreach (var cell in path)
        {
            Vector3 target = cell.position;
            while (transform.position != target)
            {
                transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
                yield return null;
            }
        }
    }

    // Disegna il percorso con Gizmos
    private void OnDrawGizmos()
    {
        // Controlla se il gridManager è valido
        if (gridManager != null)
        {
            // Se il percorso esiste, disegnalo
            if (path != null && path.Count > 0)
            {
                // Visualizza il punto di partenza con una sfera verde
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(path[0].position, 0.3f);  // Punti iniziali

                // Visualizza il punto di arrivo con una sfera rossa
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(path[path.Count - 1].position, 0.3f);  // Punti finali

                // Visualizza il percorso con una linea blu
                Gizmos.color = Color.blue;
                for (int i = 0; i < path.Count - 1; i++)
                {
                    Vector3 currentPos = path[i].position;
                    Vector3 nextPos = path[i + 1].position;
                    Gizmos.DrawLine(currentPos, nextPos);  // Linea tra i punti del percorso
                }
            }
        }
    }


}
