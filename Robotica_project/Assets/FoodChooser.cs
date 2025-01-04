using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class FoodChooser : MonoBehaviour
{
    [Header("Riferimento al MenuReading")]
    public MenuReading menuReading; // Riferimento allo script MenuReading

    public TTSManager ttsManager;

    public bool foodChoosen = false;

    public ThinkingCloudController cloudController; // Riferimento al controller della nuvoletta

    private List<string> responses = new List<string>()
    {
        "Mmmmmm.. un attimo che penso a cosa prendere amico!",
        "Fammi riflettere un momento... ok, ho deciso!",
        "Aspetta un secondo, devo fare una scelta...",
        "mmmmm... vediamo... cosa prenderò?",
        "Un po' indeciso... vediamo cosa c'è di buono!",
        "Voglio un momento per scegliere, non voglio sbagliare!"
    };

    private bool isThinking = false;  // Indica se stiamo "pensando"
    private float thinkingTime = 30f;  // Tempo di attesa per il "pensiero"
    private float thinkingTimer = 0f; // Timer per il "pensiero"

   
   private void Start()
{
    cloudController = GetComponent<ThinkingCloudController>();

    if (cloudController == null)
    {
        Debug.LogError("ThinkingCloudController non trovato! Assicurati che sia assegnato al personaggio.");
    }
}

    private void Update()
    {
        // Se il menu è stato letto e non è stata ancora fatta una scelta
        if (menuReading != null && menuReading.menuReaded && !foodChoosen)
        {
            if (!isThinking)
            {
                StartThinking();  // Inizia il pensiero
            }
            else
            {
                // Se stiamo pensando, aggiorniamo il timer
                thinkingTimer += Time.deltaTime;

                // Quando il timer raggiunge il tempo di "pensiero", scegliamo il cibo
                if (thinkingTimer >= thinkingTime)
                {
                    thinkingTimer = 0f;  // Reset del timer
                    ChooseFoodFromMenu();  // Scegli il cibo
                    foodChoosen = true;    // Segna che il cibo è stato scelto
                }
            }
        }
    }

    // Funzione per iniziare il "pensiero"
    private void StartThinking()
    {
        isThinking = true;
        string thinkingResponse = GetRandomResponse();

        cloudController?.ShowThinkingCloud();

        // Comunica la risposta
        if (ttsManager != null)
        {
            ttsManager.Speak(thinkingResponse, robotic_voice: false);
        }
        else
        {
            Debug.LogError("TextToSpeech non è collegato!");
        }

        Debug.Log(thinkingResponse);  // Log della risposta per il "pensiero"
    }

    private void ChooseFoodFromMenu()
    {
        // Ottieni il menu dal MenuReading
        Dictionary<string, string> menuItems = menuReading.menuItems;

        // Logga i cibi disponibili
        Debug.Log("Cibi disponibili: " + string.Join(", ", menuItems.Select(item => $"{item.Key} ({item.Value})")));

        // Filtra i cibi per tipo
        string mainChoice = menuItems.FirstOrDefault(item => item.Value == "Main").Key;
        string sideChoice = menuItems.FirstOrDefault(item => item.Value == "Side").Key;
        string drinkChoice = menuItems.FirstOrDefault(item => item.Value == "Drink").Key;

        // Controlla se tutte le scelte sono valide
        if (mainChoice == null || sideChoice == null || drinkChoice == null)
        {
            Debug.LogWarning("Non sono disponibili tutte le categorie richieste (Main, Side, Drink)!");
            return;
        }

        // Simula il pensiero come risposta casuale
        Debug.Log($"Cibo scelto: Main = {mainChoice}, Side = {sideChoice}, Drink = {drinkChoice}");

        string menuChoises = $"Dal menu ho scelto: {mainChoice}, {sideChoice}, {drinkChoice}.";

        cloudController?.HideThinkingCloud();

        // Comunica la scelta
        if (ttsManager != null)
        {
            ttsManager.Speak(menuChoises, robotic_voice: false);        
        }
        else
        {
            Debug.LogError("TextToSpeech non è collegato!");
        }
    }

    // Funzione che seleziona una risposta casuale e la restituisce
    public string GetRandomResponse()
    {
        int randomIndex = Random.Range(0, responses.Count);
        return responses[randomIndex];
    }
}
