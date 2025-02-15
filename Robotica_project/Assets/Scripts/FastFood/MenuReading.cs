using UnityEngine;
using Neo4j.Driver;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

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
    public Dictionary<string, List<(string Name, int Calories)>> menuItems = new Dictionary<string, List<(string Name, int Calories)>>();

    private IDriver driver;

    public FoodChooser foodChooser;

    private List<Vector3> rayOrigins = new List<Vector3>(); // Aggiungi una lista per memorizzare gli origini dei raggi
    private List<Vector3> rayDirections = new List<Vector3>(); // Aggiungi una lista per memorizzare le direzioni dei raggi

    async void Start()
    {
        // Get the TTS Instance
        ttsManager = TTSManager.Instance;

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

    private async Task<Dictionary<string, List<(string, int)>>> RetrieveMenu()
    {
        Debug.Log("Recupero dei cibi con priorità dal database...");

        Dictionary<string, List<(string, int)>> menuItems = new Dictionary<string, List<(string, int)>>
        {
            { "Prioritario", new List<(string, int)>() },
            { "Normale", new List<(string, int)>() }
        };

        try
        {
            using (var session = driver.AsyncSession())
            {
                var result = await session.ExecuteReadAsync(async tx =>
                {
                    // Query per recuperare solo i cibi disponibili con le calorie
                    var queryResult = await tx.RunAsync(@"
                        // Recupera i cibi prioritari
                        MATCH (robot:ROBOT {name: 'Assistente Ordini'})-[:PROPONE_PRIORITARIO]->(item:MENU_ITEM)
                        WHERE item.available = true
                        RETURN item.name AS name, item.type AS type, item.calories AS calories, 'Prioritario' AS priority
                        UNION
                        // Recupera i cibi normali escludendo quelli prioritari
                        MATCH (robot:ROBOT {name: 'Assistente Ordini'})-[:PROPONE]->(item:MENU_ITEM)
                        WHERE item.available = true
                        AND NOT (robot)-[:PROPONE_PRIORITARIO]->(item)
                        RETURN item.name AS name, item.type AS type, item.calories AS calories, 'Normale' AS priority
                    ");
                    return await queryResult.ToListAsync();
                });

                Debug.Log($"Trovati {result.Count} cibi disponibili nel database con priorità.");

                foreach (var record in result)
                {
                    string name = record["name"].As<string>();
                    string type = record["type"].As<string>();
                    int calories = record["calories"].As<int>();
                    string priority = record["priority"].As<string>();

                    if (priority == "Prioritario")
                    {
                        menuItems["Prioritario"].Add((name, calories));
                    }
                    else if (priority == "Normale")
                    {
                        menuItems["Normale"].Add((name, calories));
                    }

                    Debug.Log($"Cibo recuperato: {name}, Calorie: {calories}, Priorità: {priority}");
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
            if (checkoutDetected)
            {
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

        // Aspetto 8 secondi
        yield return new WaitForSeconds(8);

        // Clear the TTS queue
        ttsManager.ClearQueue();

        if (retrieveTask.Exception == null)
        {
            menuItems = retrieveTask.Result;
            string menuText = "";
            string menuText1 = "";

            // We have to shrink the list of normal items if there are more than 10 items. The list must contain only 10 items.
            if (menuItems["Normale"].Count > 10)
            {
                menuItems["Normale"] = menuItems["Normale"].GetRange(0, 10);
            }

            // Now we can read the menu
            if (menuItems["Prioritario"].Count > 1) // Controlla se ci sono cibi prioritari
            {
                menuText = "In base alle tue preferenze alimentari, nel menu sono disponibili questi cibi: ";

                foreach (var item in menuItems["Prioritario"])
                {
                    // If it's the last item
                    if (item == menuItems["Prioritario"][menuItems["Prioritario"].Count - 1])
                    {
                        menuText += " e ";
                        menuText += item.Name + ", con " + item.Calories + " calorie.";
                    }
                    else
                    {
                        menuText += item.Name + ", con " + item.Calories + " calorie, ";
                    }
                }
            }
            else if (menuItems["Prioritario"].Count == 1)
            {
                menuText = "In base alle tue preferenze alimentari, nel menu è disponibile solo questo cibo: ";
                menuText += menuItems["Prioritario"][0].Name + ", con " + menuItems["Prioritario"][0].Calories + " calorie.";
            }
            else
            {
                menuText = "Spiacente, al momento non ci sono cibi disponibili in base alle tue preferenze.";
            }

            if (ttsManager != null)
            {
                ttsManager.Speak(menuText, robotic_voice: false);
            }

            if (menuItems["Normale"].Count > 1) // Solo cibi con priorità normale
            {
                menuText1 = "In base alle tue intolleranze, il menu contiene questi cibi, anche se non sono tra i tuoi preferiti: ";

                foreach (var item in menuItems["Normale"])
                {
                    // If it's the last item
                    if (item == menuItems["Normale"][menuItems["Normale"].Count - 1])
                    {
                        menuText1 += " e ";
                        menuText1 += item.Name + ", con " + item.Calories + " calorie.";
                    }
                    else
                    {
                        menuText1 += item.Name + ", con " + item.Calories + " calorie, ";
                    }
                }
            }
            else if (menuItems["Normale"].Count == 1)
            {
                menuText1 = "In base alle tue intolleranze, il menu contiene solo questo cibo, anche se non è tra i tuoi preferiti: ";
                menuText1 += menuItems["Normale"][0].Name + " con " + menuItems["Normale"][0].Calories + " calorie.";
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

        rayOrigins.Clear();
        rayDirections.Clear();

        for (int i = 0; i < numRays; i++)
        {
            float angle = Mathf.Lerp(-halfAngle, halfAngle, (float)i / (numRays - 1));
            Vector3 dir = Quaternion.Euler(0, angle, 0) * rayDirection;

            rayOrigins.Add(rayOrigin);
            rayDirections.Add(dir);

            RaycastHit hit;
            if (Physics.Raycast(rayOrigin, dir, out hit, detectionDistance))
            {
                if (hit.collider.CompareTag("Checkout Screens"))
                {
                    Debug.Log("Checkout Screen rilevato: " + hit.collider.name);
                    screenDetected = true;
                }
            }
        }

        return screenDetected;
    }

    void OnDrawGizmos()
    {
        if (Camera.current == Camera.main) return;

        if (rayOrigins.Count == 0 || rayDirections.Count == 0) return;

        Gizmos.color = Color.blue;

        for (int i = 0; i < rayOrigins.Count; i++)
        {
            Gizmos.DrawRay(rayOrigins[i], rayDirections[i] * detectionDistance);
        }
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