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
    public TTSManager ttsManager;

    private bool checkoutDetected = false; // Stato di rilevamento
    private bool isRetrievingMenu = false; // Stato per il recupero del menu
    private float lastMenuUpdateTime = 0f; // Tempo dell'ultimo aggiornamento del menu

    [SerializeField]
    private float menuUpdateCooldown = 600f; // Tempo minimo tra due recuperi consecutivi (in secondi)

    public bool menuReaded = false;
    public bool firstRetrieve = false;
    public Dictionary<string, List<string>> menuItems = new Dictionary<string, List<string>>();

    private IDriver driver;

    public FoodChooser foodChooser;

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

    private async Task<Dictionary<string, List<string>>> RetrieveMenu()
    {
        Debug.Log("Recupero dei cibi con priorità dal database...");

        Dictionary<string, List<string>> menuItems = new Dictionary<string, List<string>>
        {
            { "Prioritario", new List<string>() },
            { "Normale", new List<string>() }
        };

        try
        {
            using (var session = driver.AsyncSession())
            {
                var result = await session.ExecuteReadAsync(async tx =>
                {
                    // Query per recuperare solo i cibi disponibili
                    var queryResult = await tx.RunAsync(@"
                        // Recupera i cibi prioritari
                        MATCH (robot:ROBOT {name: 'Assistente Ordini'})-[:PROPONE_PRIORITARIO]->(item:MENU_ITEM)
                        WHERE item.available = true
                        RETURN item.name AS name, item.type AS type, 'Prioritario' AS priority
                        UNION
                        // Recupera i cibi normali escludendo quelli prioritari
                        MATCH (robot:ROBOT {name: 'Assistente Ordini'})-[:PROPONE]->(item:MENU_ITEM)
                        WHERE item.available = true
                        AND NOT (robot)-[:PROPONE_PRIORITARIO]->(item)
                        RETURN item.name AS name, item.type AS type, 'Normale' AS priority
                    ");
                    return await queryResult.ToListAsync();
                });

                Debug.Log($"Trovati {result.Count} cibi disponibili nel database con priorità.");

                foreach (var record in result)
                {
                    string name = record["name"].As<string>();
                    string type = record["type"].As<string>();
                    string priority = record["priority"].As<string>();

                    if (priority == "Prioritario")
                    {
                        menuItems["Prioritario"].Add($"{name}");
                    }
                    else if (priority == "Normale")
                    {
                        menuItems["Normale"].Add($"{name}");
                    }

                    Debug.Log($"Cibo recuperato: {name},  Priorità: {priority}");
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
        if (!isRetrievingMenu && (!firstRetrieve || Time.time - lastMenuUpdateTime > menuUpdateCooldown) && !foodChooser.foodChoosen)
        {
            //Debug.Log("ATTENZIONE RETRIEVING AVVIATO: TEMPO PASSATO:"+Time.time+ " LAST UPDATE:"+lastMenuUpdateTime+" COOLDOWN:"+menuUpdateCooldown);
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
            string menuText = "";
            string menuText1 = "";

            if (menuItems["Prioritario"].Count > 0) // Controlla se ci sono cibi prioritari
            {
                Debug.Log("CIBI PRIOTIARI TROVATI VAI TTS PARLA");
                menuText = "....................In base alle tue preferenze , nel menu aggiornato ci sono: ";
                menuText += string.Join(", ", menuItems["Prioritario"]) + "...........";

                if (ttsManager != null)
                {
                    ttsManager.Speak(menuText, robotic_voice: false);
                }
            }

            if (menuItems["Normale"].Count > 0) // Solo cibi con priorità normale
            {
                Debug.Log("CIBI NORMALE TROVATI VAI TTS PARLA");
                menuText1 = "...........In base alle tue intolleranze, nel menu aggiornato ci sono questi cibi, anche se non sono i tuoi preferiti: ";
                menuText1 += string.Join(", ", menuItems["Normale"]) + ".";
            }
            else
            {
                menuText1 = "Spiacente, al momento non ci sono cibi disponibili per te.";
            }

            if (ttsManager != null)
            {
                ttsManager.Speak(menuText1, robotic_voice: false);
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
        menuReaded = true;
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
