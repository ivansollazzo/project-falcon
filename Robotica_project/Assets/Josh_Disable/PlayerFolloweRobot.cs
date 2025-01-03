using UnityEngine;

public class DisabileFollow : MonoBehaviour
{
    public Transform robot;        // Riferimento al robot
    public float followDistance = 2f;  // Distanza di seguimento
    public float speed = 2.5f;     // Velocità di movimento del disabile
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // Calcola la direzione verso il robot
        Vector3 direction = robot.position - transform.position;
        direction.y = 0;  // Mantieni solo il movimento orizzontale

        // Se la distanza dal robot è maggiore della distanza di seguimento
        if (direction.magnitude > followDistance)
        {
            // Muovi il disabile verso il robot
            transform.Translate(direction.normalized * speed * Time.deltaTime, Space.World);

            // Imposta la velocità dell'animazione
            animator.SetFloat("Speed", direction.magnitude);  // Walking
        }
        else
        {
            // Il disabile è abbastanza vicino al robot, metti in idle
            animator.SetFloat("Speed", 0);  // Idle
        }
    }
}
