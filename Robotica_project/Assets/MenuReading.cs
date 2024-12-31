using UnityEngine;
using Neo4j.Driver;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;

public class MenuReading : MonoBehaviour
{
    [Header("Rilevamento Checkout Screens")]
    public float detectionDistance = 10f; // Distanza massima per rilevare i checkout screens
    public float coneAngle = 30f; // Angolo del cono per rilevamento
    public int numRays = 20; // Numero di raggi nel cono

    [Header("Riferimento al TextToSpeech")]
    public TextToSpeech textToSpeech; // Riferimento alla classe TextToSpeech

    private bool checkoutDetected = false; // Stato di rilevamento
    Dictionary<string, bool> menuItems = new Dictionary<string, bool>();

    private IDriver driver;
    private IAsyncSession session;

    async void Start()
    {
        string uri = "bolt://localhost:7689";
        string user = "neo4j";
        string password = "PeraCotta10$";

        Debug.Log("Inizializzazione del driver Neo4j...");

        try
        {
            driver = GraphDatabase.Driver(uri, AuthTokens.Basic(user, password));
            session = driver.AsyncSession();

            Debug.Log("Connessione al database Neo4j in corso...");
            var result = await session.RunAsync("RETURN 1 AS test");
            Debug.Log("Connessione al database Neo4j riuscita!");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Errore nella connessione al database Neo4j: " + ex.Message);
        }
    }

    private async Task<Dictionary<string, bool>> RetrieveMenu()
    {
        Debug.Log("Recupero dei cibi dal database...");

        Dictionary<string, bool> menuItems = new Dictionary<string, bool>();

        try
        {
            var result = await session.ExecuteReadAsync(async tx =>
            {
                var queryResult = await tx.RunAsync("MATCH (item:MENU_ITEM) RETURN item.name AS name, item.available AS available");
                return await queryResult.ToListAsync();
            });

            Debug.Log($"Trovati {result.Count} cibi nel database.");

            foreach (var record in result)
            {
                string name = record["name"].As<string>();
                bool isAvailable = record["available"].As<bool>();
                menuItems[name] = isAvailable;

                Debug.Log($"Cibo recuperato: {name}, Disponibile (originale): {isAvailable}");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Errore durante il recupero dei cibi: " + ex.Message);
        }

        return menuItems;
    }

    void Update()
    {
        // Controlla se ci sono "Checkout Screens" vicini
        checkoutDetected = CheckForCheckoutScreens();

        if (checkoutDetected)
        {
            Debug.Log("Checkout Screen rilevato! Inizio interazione...");
            StartCoroutine(RetrieveMenuCoroutine());
        }
    }
private IEnumerator RetrieveMenuCoroutine()
{
    var retrieveTask = RetrieveMenu();
    yield return new WaitUntil(() => retrieveTask.IsCompleted);

    if (retrieveTask.Exception == null)
    {
        menuItems = retrieveTask.Result;
        Debug.Log("Menu ricevuto con successo!");

        // Trasforma il menu in una stringa
        string menuText = "Menu aggiornato: ";
        foreach (var item in menuItems)
        {
            menuText += $"{item.Key} - {(item.Value ? "Disponibile" : "Non disponibile")}. ";
        }

        // Chiama TextToSpeech per pronunciare il menu
        if (textToSpeech != null)
        {
            textToSpeech.StartSpeech(menuText);
        }
        else
        {
            Debug.LogError("TextToSpeech non è collegato!");
        }
    }
    else
    {
        Debug.LogError("Errore durante l'aggiornamento del menu: " + retrieveTask.Exception.Message);
    }
}

    private bool CheckForCheckoutScreens()
    {
        Vector3 rayOrigin = transform.position;
        Vector3 rayDirection = transform.forward;

        bool screenDetected = false;

        float halfAngle = coneAngle / 2f;

        for (int i = 0; i < numRays; i++)
        {
            // Calcola l'angolo per ogni raggio
            float angle = Mathf.Lerp(-halfAngle, halfAngle, (float)i / (numRays - 1));
            Vector3 dir = Quaternion.Euler(0, angle, 0) * rayDirection;

            RaycastHit hit;
            if (Physics.Raycast(rayOrigin, dir, out hit, detectionDistance))
            {
                if (hit.collider.CompareTag("Checkout Screens"))
                {
                    Debug.Log("Checkout Screen rilevato: " + hit.collider.name);
                    Debug.DrawRay(rayOrigin, dir * detectionDistance, Color.blue);
                    screenDetected = true;
                }
                else
                {
                    Debug.DrawRay(rayOrigin, dir * detectionDistance, Color.green);
                }
            }
            else
            {
                Debug.DrawRay(rayOrigin, dir * detectionDistance, Color.red);
            }
        }

        return screenDetected;
    }
}
