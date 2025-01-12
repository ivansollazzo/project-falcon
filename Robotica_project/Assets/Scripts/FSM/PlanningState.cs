using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class PlanningState : State
{
    private RobotController robotController;
    private bool planningComplete = false;

    private TTSManager ttsManager;
    private Vector3 destination;

    private Cell[,] grid;

    public List<Cell> path;
    private CameraController cameraController; // Riferimento alla CameraController

    private CameraControllerFF cameraControllerFF;


    public PlanningState(StateMachine stateMachine) : base(stateMachine)
    {
        this.robotController = stateMachine.gameObject.GetComponent<RobotController>();
        this.destination = robotController.GetDestination();
        this.grid = GridManager.Instance.GetGrid();
        this.ttsManager = TTSManager.Instance;

        // Ottenere la CameraController dalla scena (modifica se necessario)
        this.cameraController = Camera.main.GetComponent<CameraController>();
        this.cameraControllerFF = Camera.main.GetComponent<CameraControllerFF>();
    }

    public override void EnterState()
    {
        Debug.Log("Stato PLANNING: Pianificazione del percorso verso la destinazione...");
        Debug.Log("Destinazione: " + destination);

        // Se ci sono path drawer, li rimuovo
        PathDrawer[] pathDrawers = stateMachine.gameObject.GetComponents<PathDrawer>();
        foreach (PathDrawer pathDrawer in pathDrawers)
        {
            pathDrawer.ClearPath();
            GameObject.Destroy(pathDrawer);
        }

        // Start the coroutine to execute the planning process
        stateMachine.StartCoroutine(ExecutePlanning());
    }

    // Coroutine che aggiunge un delay prima di iniziare la pianificazione
    private IEnumerator WaitForPlanning()
    {
        // Aspetta un po' prima di iniziare la pianificazione, ad esempio 3 secondi
        yield return new WaitForSeconds(3);

        // Clear the queue
        ttsManager.ClearQueue();

        // Dopo il delay, inizia la pianificazione del percorso
        stateMachine.StartCoroutine(ExecutePlanning());
    }

    public override void ExecuteState()
    {
        // Managed by coroutine
    }

    private IEnumerator ExecutePlanning()
    {
        // Otteniamo la posizione corrente del robot
        Vector3 robotPosition = stateMachine.gameObject.transform.position;

        // Otteniamo la cella di partenza del robot
        Cell startCell = GridManager.Instance.GetCellFromWorldPosition(robotPosition);
        Cell endCell = GridManager.Instance.GetCellFromWorldPosition(destination);

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
            pathDrawer.DrawPath(path);

            /* Verifica se il primo elemento del percorso è vicino al robot, se sì lo rimuove */
            if (Vector3.Distance(robotPosition, path[0].GetWorldPosition()) < 1.0f)
            {
                path.RemoveAt(0);
            }

            if(SceneManager.GetActiveScene().name == "City")
            {
                // Richiamo la cam per visualizzare la destinazione
                cameraController.UpdateDestination(destination);
            }
            else if(SceneManager.GetActiveScene().name == "FastFood")
            {
                // Richiamo la cam FF per visualizzare la destinazione
                cameraControllerFF.UpdateDestination(destination);
            }

            // Feedback vocale
            ttsManager.Speak("Grazie ai miei calcoli ho trovato il percorso più sicuro e veloce. Sto per iniziare la navigazione. Règgiti forte!");


            yield return new WaitForSeconds(8);

            // Torna alla visuale del robot
            if(SceneManager.GetActiveScene().name == "City")
            {
                if (cameraController != null)
                {
                    cameraController.FollowRobot();
                }
            }
            if(SceneManager.GetActiveScene().name == "FastFood")
            {
                    if (cameraControllerFF != null)
                {
                    cameraControllerFF.FollowRobot();
                }
            }
            // Passa allo stato di navigazione
            stateMachine.SetState(new NavigationState(stateMachine, path));
        }
        else
        {
            // Feedback vocale
            ttsManager.Speak("Percorso non trovato. Mi dispiace. Riprova per favore.");
            yield return new WaitForSeconds(2);
            Debug.Log("Percorso non trovato!");

            // Torna alla visuale del robot in caso di errore
            if(SceneManager.GetActiveScene().name == "City")
            {
                if (cameraController != null)
                {
                    cameraController.FollowRobot();
                }
            }
            if(SceneManager.GetActiveScene().name == "FastFood")
            {
                    if (cameraControllerFF != null)
                {
                    cameraControllerFF.FollowRobot();
                }
            }
            stateMachine.SetState(new StandbyState(stateMachine));
        }
    }

    public override void ExitState()
    {
        Debug.Log("Uscito dallo stato PLANNING.");
    }
}