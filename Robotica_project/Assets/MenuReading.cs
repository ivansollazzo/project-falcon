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
    private bool isRetrievingMenu = false; // Stato per il recupero del menu
    private float lastMenuUpdateTime = 0f; // Tempo dell'ultimo aggiornamento del menu

    [SerializeField]
    private float menuUpdateCooldown = 600f; // Tempo minimo tra due recuperi consecutivi (in secondi)

    private bool firstRetrieve = false;
    Dictionary<string, bool> menuItems = new Dictionary<string, bool>();

    private IDriver driver;

    async void Start()
    {
        string uri = "bolt://localhost:7689";
        string user = "neo4j";
        string password = "PeraCotta10$";

        Debug.Log("Inizializzazione del driver Neo4j...");

        try
        {
            driver = GraphDatabase.Driver(uri, AuthTokens.Basic(user, password));

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
            using (var session = driver.AsyncSession())
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
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Errore durante il recupero dei cibi: " + ex.Message);
        }

        return menuItems;
    }

    void Update()
    {
        if (!isRetrievingMenu && (!firstRetrieve || Time.time - lastMenuUpdateTime > menuUpdateCooldown))
        {
            Debug.Log("ATTENZIONE RETRIEVING AVVIATO: TEMPO PASSATO:"+Time.time+ " LAST UPDATE:"+lastMenuUpdateTime+" COOLDOWN:"+menuUpdateCooldown);
            checkoutDetected = CheckForCheckoutScreens();
            if (checkoutDetected){
                Debug.Log("Checkout Screen rilevato! Inizio interazione...");
                StartCoroutine(RetrieveMenuCoroutine());
            }
        }
    }

    private IEnumerator RetrieveMenuCoroutine()
    {
        firstRetrieve = true;
        isRetrievingMenu = true; // Imposta lo stato come "recupero in corso"
        var retrieveTask = RetrieveMenu();
        yield return new WaitUntil(() => retrieveTask.IsCompleted);

        if (retrieveTask.Exception == null)
        {
            menuItems = retrieveTask.Result;

            string menuText = "CIAO AMICO, il menu aggiornato è il seguente: ";
            foreach (var item in menuItems)
            {
                menuText += $"{item.Key} - {(item.Value ? "Disponibile" : "Non disponibile")}. ";
            }

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

        isRetrievingMenu = false; // Recupero terminato
        lastMenuUpdateTime = Time.time; // Aggiorna il tempo dell'ultimo recupero
    }

    private bool CheckForCheckoutScreens()
    {
        Vector3 rayOrigin = transform.position;
        Vector3 rayDirection = transform.forward;

        bool screenDetected = false;

        float halfAngle = coneAngle / 2f;

        for (int i = 0; i < numRays; i++)
        {
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

    async void OnApplicationQuit()
    {
        Debug.Log("Chiusura dell'applicazione. Pulizia delle risorse...");

        try
        {
            if (driver != null)
            {
                await driver.DisposeAsync();
                Debug.Log("Driver chiuso.");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Errore durante la chiusura delle risorse: " + ex.Message);
        }
    }
}
