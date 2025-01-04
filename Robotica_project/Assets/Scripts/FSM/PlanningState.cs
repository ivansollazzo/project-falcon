using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlanningState : State
{
    private RobotController robotController;
    private bool planningComplete = false;

    private TTSManager ttsManager;
    private Vector3 destination;

    private Cell[,] grid;

    public List<Cell> path;

    public PlanningState(StateMachine stateMachine) : base(stateMachine)
    {
        this.robotController = stateMachine.gameObject.GetComponent<RobotController>();
        this.destination = robotController.GetDestination();
        this.grid = GridManager.Instance.GetGrid();
        this.ttsManager = robotController.GetTTSManager();
    }

    public override void EnterState()
    {
        Debug.Log("Stato PLANNING: Pianificazione del percorso verso la destinazione...");
        Debug.Log("Destinazione: " + destination);

        // Se ci sono path drawer, li rimuovo
        PathDrawer[] pathDrawers = stateMachine.gameObject.GetComponents<PathDrawer>();
        foreach (PathDrawer pathDrawer in pathDrawers)
        {
            GameObject.Destroy(pathDrawer);
        }

        // Start the coroutine to execute the planning process
        stateMachine.StartCoroutine(ExecutePlanning());
    }

    // Converting ExecuteState to IEnumerator (Coroutine)
    public override void ExecuteState()
    {
        // Managed by coroutine
    }

    private IEnumerator ExecutePlanning()
    {
        // Ottengo la posizione corrente del robot
        Vector3 robotPosition = stateMachine.gameObject.transform.position;

        // Ottengo la cella di partenza corrispondente del robot. Itero tutte le celle per ottenere la posizione corrispondente con riferimento alla distanza più vicina.
        Cell startCell = GridManager.Instance.GetCellFromWorldPosition(robotPosition);
        Cell endCell = GridManager.Instance.GetCellFromWorldPosition(destination);

        // Stampiamo tutto
        Debug.Log("Cella di partenza: " + startCell);
        Debug.Log("Cella di destinazione: " + endCell);

        // Creiamo un'istanza di AStar
        AStar aStar = new AStar(grid, startCell, endCell);

        // Feedback vocale
        ttsManager.Speak("Pianificazione del percorso migliore. Attendi per favore...");

        // Wait for 5 seconds
        yield return new WaitForSeconds(5);

        // Cerchiamo il percorso
        path = aStar.FindPath();

        if (path != null)
        {
            Debug.Log("Percorso trovato!");
            Debug.Log(path.Count + " celle nel percorso.");
            Debug.Log(path);
            planningComplete = true;

            // Disegna il percorso
            PathDrawer pathDrawer = stateMachine.gameObject.AddComponent<PathDrawer>();
            pathDrawer.path = path;

            // Verifichiamo quanto il primo elemento del percorso sia vicino al robot. Nel caso sia troppo vicino, lo rimuoviamo.
            if (Vector3.Distance(robotPosition, path[0].GetWorldPosition()) < 0.5f)
            {
                path.RemoveAt(0);
            }

            // Feedback vocale
            ttsManager.Speak("Pianificazione completata. Sto per iniziare la navigazione. Règgiti forte!");

            yield return new WaitForSeconds(5);

            // Passa allo stato di navigazione
            stateMachine.SetState(new NavigationState(stateMachine, path));
        }
        else
        {
            // Feedback vocale
            ttsManager.Speak("Percorso non trovato. Mi dispiace. Riprova per favore.");

            yield return new WaitForSeconds(2);
            
            Debug.Log("Percorso non trovato!");
            stateMachine.SetState(new StandbyState(stateMachine));
        }
    }

    public override void ExitState()
    {
        Debug.Log("Uscito dallo stato PLANNING.");
    }
}