using UnityEngine;
using System.Collections.Generic;

public class PathDrawer : MonoBehaviour
{
    public List<Cell> path; // La lista del percorso

    // Metodo per disegnare il percorso in Unity tramite Gizmos
    private void OnDrawGizmos()
    {
        if (path == null || path.Count == 0)
            return;

        // Imposta il colore dei Gizmos per visualizzare il percorso
        Gizmos.color = Color.magenta;

        // Disegna le linee tra le celle del percorso
        for (int i = 0; i < path.Count - 1; i++)
        {
            Vector3 start = path[i].GetWorldPosition();
            Vector3 end = path[i + 1].GetWorldPosition();
            Gizmos.DrawLine(start, end);
        }

        // Se vuoi visualizzare ogni cella del percorso con una sfera:
        Gizmos.color = Color.blue;
        foreach (var cell in path)
        {
            Gizmos.DrawSphere(cell.GetWorldPosition(), 0.2f);  // Disegna una sfera per ogni cella
        }
    }
}
