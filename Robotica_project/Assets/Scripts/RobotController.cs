using UnityEngine;
using UnityEngine.AI;
public class RobotController : MonoBehaviour
{
    private StateMachine stateMachine;
    private Vector3 destination;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Add NavMeshAgent and StateMachine components
        stateMachine = this.gameObject.AddComponent<StateMachine>();

        // Check if components are assigned
        if (stateMachine == null)
        {
            Debug.LogError("StateMachine is not assigned!");
        }

        // Impostazione dello stato iniziale
        if (stateMachine != null)
        {
            stateMachine.SetState(new StandbyState(stateMachine));  // Inizia con lo stato Standby
        }
    }

    public void SetDestination(Vector3 dest) {
        this.destination = dest;
        Debug.Log("Destination has been set as: " + this.destination);
    }

    public Vector3 GetDestination() {
        return this.destination;
    }
}
