using UnityEngine;

public class WaitingState : State {
    private RobotController robotController;

    private NavigationState currentNavigationState;

    private TTSManager ttsManager;
    private float waitingTime;
    private float elapsedTime = 0.0f;

    public WaitingState(StateMachine stateMachine, float waitingTime, NavigationState navigationState) : base(stateMachine) {
        this.robotController = stateMachine.gameObject.GetComponent<RobotController>();
        this.waitingTime = waitingTime;
        this.currentNavigationState = navigationState;
        robotController.SetMoving(false);
    }

    public override void EnterState() {
        Debug.Log("Entered the WAITING state! Robot is waiting...");
        ttsManager = robotController.GetTTSManager();
        ttsManager.Speak("Ho rilevato un ostacolo. Aspetta un attimo per favore!");
    }

    public override void ExecuteState() {
        // Check if the obstacle has been removed
        Collider detectedObstacle = stateMachine.gameObject.GetComponent<ObstacleSensor>().CheckForObstacles();

        elapsedTime += Time.deltaTime;

        if (elapsedTime >= waitingTime) {
            ttsManager.Speak("Tempo di attesa scaduto. Ripianifico il percorso.");

            // We calculate the min and max cells occupied by the obstacle
            float obstacleMinX = detectedObstacle.bounds.min.x;
            float obstacleMaxX = detectedObstacle.bounds.max.x;
            float obstacleMinZ = detectedObstacle.bounds.min.z;
            float obstacleMaxZ = detectedObstacle.bounds.max.z;

            // We must calculate now the points of the diagonal of the obstacle
            Vector3 diagMin = new Vector3(obstacleMinX, 0, obstacleMinZ);
            Vector3 diagMax = new Vector3(obstacleMaxX, 0, obstacleMaxZ);

            // Now we get cells of point1 and point4 (diagonals)
            Cell minCell = GridManager.Instance.GetCellFromWorldPosition(diagMin);
            Cell maxCell = GridManager.Instance.GetCellFromWorldPosition(diagMax);

            Debug.Log("Min cell: " + minCell);
            Debug.Log("Max cell: " + maxCell);

            // Now we calculate min X and Z and max X and Z
            int minX, minZ, maxX, maxZ;

            if (minCell.GetX() < maxCell.GetX()) {
                minX = minCell.GetX();
                maxX = maxCell.GetX();
            } else {
                minX = maxCell.GetX();
                maxX = minCell.GetX();
            }

            if (minCell.GetZ() < maxCell.GetZ()) {
                minZ = minCell.GetZ();
                maxZ = maxCell.GetZ();
            } else {
                minZ = maxCell.GetZ();
                maxZ = minCell.GetZ();
            }

            // Now we set the cells as blocked from min to max of that diagonal
            for (int z = minZ; z <= maxZ; z++) {
                for (int x = minX; x <= maxX; x++) {

                    // Get the cell
                    Cell cell = GridManager.Instance.GetGrid()[x, z];

                    // Check if it's inside the obstacle bounds
                    if (detectedObstacle.bounds.Contains(cell.GetWorldPosition())) {
                        // Mark the cell as blocked
                        Debug.Log("Marking cell as blocked: " + cell);
                        GridManager.Instance.MarkCellAsBlocked(cell);
                    }
                }
            }

            stateMachine.SetState(new PlanningState(stateMachine));
        }

        if (detectedObstacle == null) {
            ttsManager.Speak("Ostacolo rimosso. Riprendo la navigazione...");
            robotController.SetMoving(true);
            stateMachine.SetState(currentNavigationState);
        }
    }

    public override void ExitState() {
        Debug.Log("Exited the WAITING state! Robot is moving to standby position...");
    }
}