using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LineToCollider : MonoBehaviour
{
    public float wallHeight = 2f; // Altezza del muro
    public float wallThickness = 0.1f; // Spessore del muro

    void Start()
    {
        LineRenderer lineRenderer = GetComponent<LineRenderer>();
        int positionsCount = lineRenderer.positionCount;

        for (int i = 0; i < positionsCount - 1; i++)
        {
            Vector3 startPoint = lineRenderer.GetPosition(i);
            Vector3 endPoint = lineRenderer.GetPosition(i + 1);

            // Calcola la posizione centrale e la lunghezza del segmento
            Vector3 midpoint = (startPoint + endPoint) / 2;
            float segmentLength = Vector3.Distance(startPoint, endPoint);

            // Crea il muro
            GameObject wallSegment = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wallSegment.transform.position = midpoint;
            wallSegment.transform.LookAt(endPoint); // Allinea il muro al segmento
            wallSegment.transform.localScale = new Vector3(wallThickness, wallHeight, segmentLength);

            // Rendi il muro invisibile se necessario
            wallSegment.GetComponent<Renderer>().enabled = false; // Disabilita la visibilità
        }
    }
}
