using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Neo4j.Driver;
using System.Threading.Tasks;

public class FoodAvailability : MonoBehaviour
{
    private IDriver driver;

    async void Start()
    {
        string uri = "bolt://192.168.31.100:7687";
        string user = "neo4j";
        string password = "fr0stus3r";

        Debug.Log("Inizializzazione del driver Neo4j...");

        try
        {
            driver = GraphDatabase.Driver(uri, AuthTokens.Basic(user, password));

            Debug.Log("Connessione al database Neo4j riuscita!");

            // Avvia la coroutine per aggiornare la disponibilità dei cibi
            StartCoroutine(UpdateFoodAvailability());
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Errore nella connessione al database Neo4j: " + ex.Message);
        }
    }

    private IEnumerator UpdateFoodAvailability()
    {
        Debug.Log("Coroutine di aggiornamento avviata...");

        while (true)
        {
            Debug.Log("Attesa di 3 minuti prima del prossimo aggiornamento...");
            yield return new WaitForSeconds(180f);

            Debug.Log("Avvio dell'aggiornamento della disponibilità dei cibi...");

            // Recupera i cibi dal database
            var task = RetrieveAndGenerateMenuItemsAvailability();
            while (!task.IsCompleted)
            {
                yield return null;
            }

            if (task.Exception != null)
            {
                Debug.LogError("Errore durante il recupero dei cibi: " + task.Exception.InnerException?.Message);
                continue;
            }

            Dictionary<string, bool> menuItems = task.Result;

            // Passa i dati raccolti al task per l'aggiornamento
            var updateTask = UpdateMenuItemsAvailability(menuItems);
            while (!updateTask.IsCompleted)
            {
                yield return null;
            }

            if (updateTask.Exception != null)
            {
                Debug.LogError("Errore durante l'aggiornamento della disponibilità dei cibi: " + updateTask.Exception.InnerException?.Message);
            }
        }
    }

    private async Task<Dictionary<string, bool>> RetrieveAndGenerateMenuItemsAvailability()
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
                    
                    // Cambia la probabilità: solo il 20% di probabilità di essere false
                    bool isAvailable = Random.value > 0.2f; // Genera true (80%) o false (20%)
                    
                    menuItems[name] = isAvailable;

                    Debug.Log($"Cibo recuperato: {name}, Disponibile (originale): {record["available"].As<bool>()}, Nuova Disponibilità: {isAvailable}");
                }

            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Errore durante il recupero dei cibi: " + ex.Message);
        }

        return menuItems;
    }

    private async Task UpdateMenuItemsAvailability(Dictionary<string, bool> menuItems)
    {
        Debug.Log("Inizio aggiornamento della disponibilità dei cibi...");

        try
        {
            using (var session = driver.AsyncSession())
            {
                await session.ExecuteWriteAsync(async tx =>
                {
                    foreach (var item in menuItems)
                    {
                        string name = item.Key;
                        bool isAvailable = item.Value;

                        Debug.Log($"Aggiornamento di '{name}' con disponibilità: {isAvailable}");

                        await tx.RunAsync(
                            "MATCH (item:MENU_ITEM {name: $name}) SET item.available = $available",
                            new { name, available = isAvailable }
                        );

                        Debug.Log($"Cibo aggiornato: {name}, Disponibile: {isAvailable}");
                    }
                });
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Errore nella transazione di aggiornamento: " + ex.Message);
        }

        Debug.Log("Aggiornamento della disponibilità completato.");
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
