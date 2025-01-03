using UnityEngine;

public class DisabileFollowRobot : MonoBehaviour
{
    public Transform robot;           // Riferimento al robot
    public float followDistance = 2f; // Distanza di seguimento (dietro al robot)
    public float speed = 2.5f;        // Velocità di movimento del disabile
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // Calcola la direzione opposta del robot per mantenere il disabile dietro di esso
        Vector3 direction = robot.position - transform.position;
        direction.y = 0;  // Ignora il movimento verticale

        // Verifica se il robot si sta muovendo
        if (direction.magnitude > followDistance)
        {
            // Calcola la posizione del disabile dietro al robot
            Vector3 behindPosition = robot.position - robot.forward * followDistance;

            // Muovi il disabile verso la posizione dietro al robot
            transform.position = Vector3.MoveTowards(transform.position, behindPosition, speed * Time.deltaTime);

            // Se il robot si sta muovendo, attiva l'animazione "Walking"
            animator.SetBool("IsWalking", true);
        }
        else
        {
            // Il disabile è vicino al robot, fermalo
            animator.SetBool("IsWalking", false);
        }
    }
}
