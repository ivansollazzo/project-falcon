using UnityEngine;

public class ThinkingCloudController : MonoBehaviour
{
    public GameObject thinkingCloud; // Riferimento alla nuvoletta
    private Animator cloudAnimator; // Riferimento all'Animator

    void Start()
    {
        // Ottieni il componente Animator dalla nuvoletta
        if (thinkingCloud != null)
        {
            cloudAnimator = thinkingCloud.GetComponent<Animator>();
        }
    }

    public void ShowThinkingCloud()
    {
        if (thinkingCloud != null)
        {
            thinkingCloud.SetActive(true); // Attiva la nuvoletta
            if (cloudAnimator != null)
            {
                cloudAnimator.SetTrigger("Show"); // Avvia l'animazione di comparsa
            }
        }
    }

    public void HideThinkingCloud()
    {
        if (thinkingCloud != null)
        {
            if (cloudAnimator != null)
            {
                cloudAnimator.SetTrigger("Hide"); // Avvia l'animazione di scomparsa
            }
            Invoke(nameof(DisableCloud), 1f); // Disattiva dopo l'animazione
        }
    }

    private void DisableCloud()
    {
        thinkingCloud.SetActive(false); // Disattiva la nuvoletta
    }
}
