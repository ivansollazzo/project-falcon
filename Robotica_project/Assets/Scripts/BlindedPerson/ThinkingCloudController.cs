using UnityEngine;

public class ThinkingCloudController : MonoBehaviour
{
    public GameObject thinkingCloud; // Riferimento al GameObject della nuvoletta
    private Animator cloudAnimator; // Riferimento all'Animator

    private void Start()
    {
        if (thinkingCloud == null)
        {
            Debug.LogError("ThinkingCloud non assegnato! Per favore assegna il GameObject della nuvoletta.");
            return;
        }

        cloudAnimator = thinkingCloud.GetComponent<Animator>();

        if (cloudAnimator == null)
        {
            Debug.LogError("Nessun componente Animator trovato su ThinkingCloud!");
        }

        // Nascondi la nuvoletta all'inizio
        thinkingCloud.SetActive(false);
    }

    // Mostra la nuvoletta
    public void ShowCloud()
    {
        if (cloudAnimator != null)
        {
            thinkingCloud.SetActive(true); // Attiva il GameObject
            cloudAnimator.SetTrigger("Show"); // Attiva il trigger Show
        }
    }

    // Nascondi la nuvoletta
    public void HideCloud()
    {
        if (cloudAnimator != null)
        {
            cloudAnimator.SetTrigger("Hide"); // Attiva il trigger Hide
            Invoke(nameof(DeactivateCloud), 0.5f); // Disattiva dopo la durata dell'animazione

            // Destroy the GameObject after the animation duration
            Destroy(thinkingCloud, 0.5f);
        }
    }

    // Disattiva il GameObject della nuvoletta
    private void DeactivateCloud()
    {
        thinkingCloud.SetActive(false);
    }
}
