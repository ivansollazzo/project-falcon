using UnityEngine;

public class ObstacleDetectedState : State
{
    public ObstacleDetectedState(StateMachine stateMachine) : base(stateMachine) { }

    public override void EnterState()
    {
        Debug.Log("Gestione ostacolo: Evitamento automatico.");
        PlayAudio("Sto evitando l'ostacolo.");
        stateMachine.StartCoroutine(HandleObstacle());
    }

    public override void ExecuteState() { }

    public override void ExitState()
    {
        Debug.Log("Ostacolo evitato. Tornando alla navigazione.");
    }

    private System.Collections.IEnumerator HandleObstacle()
    {
        // Simula l'evitamento dell'ostacolo con una rotazione e un avanzamento
        stateMachine.transform.Rotate(0, 90, 0);
        yield return new WaitForSeconds(1);
        stateMachine.transform.Translate(Vector3.forward * 2);
        yield return new WaitForSeconds(1);

        stateMachine.SetState(new NavigationState(stateMachine));
    }

    private void PlayAudio(string message)
    {
        Debug.Log("Audio: " + message);
    }
}
