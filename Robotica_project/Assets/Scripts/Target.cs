using UnityEngine;
using UnityEngine.AI;

public class Target : MonoBehaviour
{
    private NavMeshAgent[] navAgents;  // Lista dei NavMeshAgent nella scena
    public Transform[] waypoints;     // Waypoint predefiniti per il movimento autonomo
    public Transform targetMarker;    // Indica visivamente l'obiettivo attuale
    public float verticalOffset = 10.0f;

    private int currentWaypointIndex = 0;

    [System.Obsolete]
    void Start()
    {
        // Trova tutti i NavMeshAgent nella scena
        navAgents = FindObjectsOfType<NavMeshAgent>();

        // Imposta la destinazione iniziale al primo waypoint
        if (waypoints.Length > 0)
        {
            UpdateTargets(waypoints[currentWaypointIndex].position);
        }
    }

    void Update()
    {
        // Controlla se tutti gli agenti hanno raggiunto il waypoint corrente
        if (AllAgentsReachedDestination())
        {
            // Passa al prossimo waypoint
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
            UpdateTargets(waypoints[currentWaypointIndex].position);

            // Sposta il marker visivo sul nuovo obiettivo
            if (targetMarker != null)
            {
                targetMarker.position = waypoints[currentWaypointIndex].position + new Vector3(0, verticalOffset, 0);
            }
        }

        // Evitamento dinamico degli ostacoli
        foreach (NavMeshAgent agent in navAgents)
        {
            AvoidObstacles(agent);
        }
    }

    void UpdateTargets(Vector3 targetPosition)
    {
        foreach (NavMeshAgent agent in navAgents)
        {
            agent.destination = targetPosition;
        }
    }

    bool AllAgentsReachedDestination()
    {
        // Verifica se tutti gli agenti sono vicini alla destinazione
        foreach (NavMeshAgent agent in navAgents)
        {
            if (agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
            {
                return false;
            }
        }
        return true;
    }

    void AvoidObstacles(NavMeshAgent agent)
    {
        RaycastHit hit;
        // Usa un Raycast per rilevare ostacoli davanti al robot
        if (Physics.Raycast(agent.transform.position, agent.transform.forward, out hit, 2.0f))
        {
            if (hit.collider.CompareTag("Obstacle"))
            {
                Debug.Log("Ostacolo rilevato! Cambiando direzione...");
                // Cambia temporaneamente direzione
                Vector3 avoidDirection = agent.transform.position + agent.transform.right * 2.0f;
                agent.SetDestination(avoidDirection);
            }
        }
    }
}
