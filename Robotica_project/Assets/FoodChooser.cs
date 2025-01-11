using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Newtonsoft.Json;

public class FoodChooser : MonoBehaviour
{
    [Header("Riferimento al MenuReading")]
    public MenuReading menuReading; // Riferimento allo script MenuReading
    public TTSManager ttsManager;
    public ThinkingCloudController cloudController; // Riferimento al controller della nuvoletta
    public TextAsset baseInkJsonFile; // Collegare il file .inkjson dall'Inspector

    public GameObject sttTestObject; // Riferimento al GameObject con lo script STTTest

    public bool foodChoosen = false;
    private bool isThinking = false;
    private float thinkingTime = 30f; // Tempo di attesa per il "pensiero"
    private float thinkingTimer = 0f; // Timer per il "pensiero"

    private List<string> responses = new List<string>
    {
        ".........Mmmmmm.. un attimo che penso a cosa prendere amico!",
        ".........Fammi riflettere un momento... ok, ho deciso!",
        ".........Aspetta un secondo, devo fare una scelta...",
        ".........mmmmm... vediamo... cosa prenderò?",
        ".........Un po' indeciso... vediamo cosa c'è di buono!",
        ".........Voglio un momento per scegliere, non voglio sbagliare!"
    };

    private void Start()
    {
        // Get the TTS Instance
        ttsManager = TTSManager.Instance;

        cloudController = GetComponent<ThinkingCloudController>();

        if (cloudController == null)
        {
            Debug.LogError("ThinkingCloudController non trovato! Assicurati che sia assegnato al personaggio.");
        }

        if (baseInkJsonFile == null)
        {
            Debug.LogError("File InkJSON non assegnato! Collegalo dall'Inspector.");
        }
    }

    private void Update()
    {
        if (menuReading != null && menuReading.menuReaded && !foodChoosen)
        {
            if (!isThinking)
            {
                StartThinking();
            }
            else
            {
                thinkingTimer += Time.deltaTime;

                if (thinkingTimer >= thinkingTime)
                {
                    thinkingTimer = 0f;
                    ChooseFoodFromMenu();
                    foodChoosen = true;
                }
            }
        }
    }

    private void StartThinking()
    {
        isThinking = true;
        string thinkingResponse = GetRandomResponse();

        cloudController?.ShowCloud();

        if (ttsManager != null)
        {
            ttsManager.Speak(thinkingResponse, robotic_voice: false);
        }
        else
        {
            Debug.LogError("TextToSpeech non è collegato!");
        }

        Debug.Log(thinkingResponse);
    }

    private void ChooseFoodFromMenu()
    {
        Debug.LogError("CHOOSE FROM MENU PARTITOOOOOO");
        STTTest sttTestScript = sttTestObject.GetComponent<STTTest>();
    
        Dictionary<string, List<(string Name, int Calories)>> menuItems = menuReading.menuItems;
        Debug.Log("Cibi disponibili FOODOODCHOOSEERR: " + string.Join(", ", menuItems.Select(item => $"{item.Key} ({string.Join(", ", item.Value)})")));

        sttTestScript.setMenuItems(menuItems);
        
        // Percorso del file aggiornato
        string updatedFilePath = Path.Combine(Application.persistentDataPath, "UpdatedMenu.inkjson");

        // Aggiorna il file JSON
        UpdateInkJson(menuItems, updatedFilePath);

        if (File.Exists(updatedFilePath))
        {
            TextAsset updatedInkJsonFile = new TextAsset(File.ReadAllText(updatedFilePath));

            try
            {
                DialogueManager dialogueManager = DialogueManager.GetInstance();
                dialogueManager.setSource("inside");
                dialogueManager.EnterDialogueMode(updatedInkJsonFile);
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Errore durante EnterDialogueMode: " + ex.Message);
            }
        }
        else
        {
            Debug.LogError("Il file JSON aggiornato non è stato trovato!");
        }
    }

    private string GetRandomResponse()
    {
        int randomIndex = Random.Range(0, responses.Count);
        return responses[randomIndex];
    }

    private void UpdateInkJson(Dictionary<string, List<(string, int)>> menuItems, string filePath)
    {
        if (baseInkJsonFile == null)
        {
            Debug.LogError("File InkJSON di base non assegnato.");
            return;
        }

        // Leggi il contenuto del file di base
        string baseContent = baseInkJsonFile.text;
        var inkJson = JsonConvert.DeserializeObject<Dictionary<string, object>>(baseContent);

        // Modifica la sezione `menu_section`
        var menuSection = new List<object>();
        foreach (var category in menuItems)
        {
            menuSection.Add(new Dictionary<string, object>
            {
                { "category", category.Key },
                { "items", category.Value }
            });
        }

        if (inkJson.ContainsKey("menu_section"))
        {
            inkJson["menu_section"] = menuSection;
        }
        else
        {
            var root = inkJson["root"] as List<object>;
            if (root != null)
            {
                root.Add(new Dictionary<string, object>
                {
                    { "menu_section", menuSection }
                });
            }
        }

        // Serializza e salva il contenuto aggiornato
        string updatedContent = JsonConvert.SerializeObject(inkJson, Formatting.Indented);
        File.WriteAllText(filePath, updatedContent);

        Debug.Log($"File InkJSON aggiornato: {filePath}");
    }
}
