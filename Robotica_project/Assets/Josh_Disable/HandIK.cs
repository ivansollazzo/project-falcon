using UnityEngine;

public class HandIK : MonoBehaviour
{
    public Animator animator;
    public Transform handleTarget; // La posizione della maniglia
    public Transform robot; // Il robot da seguire

    void OnAnimatorIK(int layerIndex)
    {
        if (animator)
        {
            // Abilita IK per la mano destra
            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
            animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);

            // Posiziona la mano destra sul target
            animator.SetIKPosition(AvatarIKGoal.RightHand, handleTarget.position);
            animator.SetIKRotation(AvatarIKGoal.RightHand, handleTarget.rotation);

            // Sincronizza la posizione generale del player con il robot
            Vector3 offset = new Vector3(0, 0, -0.5f); // Offset per mantenere la distanza
            transform.position = robot.position + offset;
        }
    }
}
