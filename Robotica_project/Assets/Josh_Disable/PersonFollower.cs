using UnityEngine;

public class PersonFollower : MonoBehaviour
{
    public Animator animator;  // Riferimento all'Animator
    public Transform robotHandle;  // La maniglia del robot
    public float followSpeed = 3.0f; // Velocità di inseguimento
    public float minDistance = 0.5f; // Distanza minima tra il personaggio e la maniglia

    void Update()
    {
        // Calcola la distanza dal robot
        float currentDistance = Vector3.Distance(transform.position, robotHandle.position);

        // Aggiorna l'animazione basata sulla distanza
        if (currentDistance > minDistance)
        {
            animator.SetFloat("Speed", followSpeed);  // Camminata
            // Muovi il personaggio verso la maniglia
            Vector3 targetPosition = robotHandle.position - (robotHandle.forward * minDistance);
            transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);

            // Aggiorna la rotazione verso la direzione di movimento
            Vector3 direction = targetPosition - transform.position;
            if (direction.magnitude > 0.1f)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }
        }
        else
        {
            animator.SetFloat("Speed", 0);  // Idle
        }
    }
}
