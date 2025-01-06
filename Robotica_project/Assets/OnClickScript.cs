using UnityEngine;

public class OnClickScript : MonoBehaviour
{
    public GameObject sttTestObject; // Riferimento al GameObject con lo script STTTest

    void Start()
    {
        if (sttTestObject == null)
        {
            Debug.LogError("GameObject STTTest non è assegnato! Assicurati di assegnarlo nel Inspector.");
        }
    }

    // Metodo chiamato quando il bottone viene cliccato
    public void OnButtonClick()
    {
        if (sttTestObject != null)
        {
            // Ottieni il componente STTTest dal GameObject
            STTTest sttTestScript = sttTestObject.GetComponent<STTTest>();

            if (sttTestScript != null)
            {
                // Chiama una funzione nello script STTTest
                sttTestScript.Speaking(); // Sostituisci "YourFunction" con il nome del metodo da chiamare
            }
            else
            {
                Debug.LogError("Il componente STTTest non è presente nel GameObject STTTest.");
            }
        }
    }
}
