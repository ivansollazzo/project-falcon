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
        Vector3? obstaclePosition = stateMachine.gameObject.GetComponent<ObstacleSensor>().CheckForObstacles();

        elapsedTime += Time.deltaTime;

        if (elapsedTime >= waitingTime) {
            ttsManager.Speak("Tempo di attesa scaduto. Ripianifico il percorso.");

            // Mark the cell as occupied
            GridManager.Instance.MarkCellAsBlocked(obstaclePosition.Value);            

            stateMachine.SetState(new PlanningState(stateMachine));
        }

        if (obstaclePosition == null) {
            ttsManager.Speak("Ostacolo rimosso. Riprendo la navigazione...");
            robotController.SetMoving(true);
            stateMachine.SetState(currentNavigationState);
        }
    }

    public override void ExitState() {
        Debug.Log("Exited the WAITING state! Robot is moving to standby position...");
    }
}