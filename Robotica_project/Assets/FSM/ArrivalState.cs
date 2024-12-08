using UnityEngine;

public class ArrivalState : State
{
    public ArrivalState(StateMachine sm) : base(sm) { }

    public override void EnterState()
    {
        Debug.Log("Robot entered ARRIVAL state!");
    }

    public override void ExecuteState()
    {
        // Riavvia automaticamente il ciclo dopo un breve intervallo
        stateMachine.StartCoroutine(WaitAndRestart());
    }

    public override void ExitState()
    {
        Debug.Log("Robot is exiting ARRIVAL state!");
    }

    private System.Collections.IEnumerator WaitAndRestart()
    {
        yield return new WaitForSeconds(5);
        stateMachine.SetState(new StandbyState(stateMachine));
    }

    private void PlayAudio(string message)
    {
        Debug.Log("Audio: " + message);
    }
}
