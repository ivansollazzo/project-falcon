using UnityEngine;
using Neo4j.Driver;

public class BlindedPerson : MonoBehaviour
{
    private IDriver driver;

    [Header("Disabled person properties")]
    public int age;
    public float weight;
    public int height;

    async void Start()
    {
        string uri = "bolt://localhost:7687";
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

        // Recupera le proprietà del soggetto disabile
        RetrieveProperties();
    }

    private async void RetrieveProperties()
    {
        Debug.Log("Recupero dei dati del soggetto disabile...");

        try
        {
            using (var session = driver.AsyncSession())
            {
                var result = await session.ExecuteReadAsync(async tx =>
                {
                    // Query per recuperare solo i cibi disponibili
                    var queryResult = await tx.RunAsync(@"
                        // Recupera i dati della persona
                        match (persona:PERSONA_DISABILE {name: 'Persona Cieca'}) return persona.peso as weight, persona.eta as age, persona.altezza as height
                    ");
                    return await queryResult.ToListAsync();
                });

                foreach (var record in result)
                {
                    // Return the retrieved data
                    this.age = record["age"].As<int>();
                    this.weight = record["weight"].As<float>();
                    this.height = record["height"].As<int>();
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Errore durante il recupero delle caratteristiche del soggetto disabile: " + ex.Message);
        }
    }

    public int GetBMR() {
        // Since this is a man, we use the Harris-Benedict formula
        return (int)(88.362 + (13.397 * this.weight) + (4.799 * this.height) - (5.677 * this.age));
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
