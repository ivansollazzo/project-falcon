using UnityEngine;

public class CharacterAnimatorController : MonoBehaviour
{
    public Animator animator;  // Riferimento all'Animator del disabile
    public Transform robotHandle;  // La maniglia del robot

    private bool isWalking = false;  // Stato di camminata

    void Update()
    {
        // Controlla se il robot si sta muovendo
        isWalking = robotHandle.GetComponent<Rigidbody>().linearVelocity.magnitude > 0;

        // Imposta la velocità dell'animazione in base al movimento del robot
        animator.SetFloat("Speed", isWalking ? 1.0f : 0);  // Cambia la velocità per attivare o disattivare la camminata
        animator.SetBool("IsWalking", isWalking);  // Stato animazione camminata
    }
}
