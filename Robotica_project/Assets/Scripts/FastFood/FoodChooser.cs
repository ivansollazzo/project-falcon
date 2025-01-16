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
    private float thinkingTime = 15f; // Tempo di attesa per il "pensiero"
    private float thinkingTimer = 0f; // Timer per il "pensiero"

    private List<string> responses = new List<string>
    {
        "Cosa scegli dal menu?",
        "Cosa prenderai?",
        "Fai la tua scelta, io ti aspetto!",
        "Hai già deciso cosa prendere?",
        "Hai già scelto cosa mangiare?",
        "Mamma mia, che fame! Cosa sceglierai?"
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
        Debug.Log("CHOOSE FROM MENU PARTITO");
        STTTest sttTestScript = sttTestObject.GetComponent<STTTest>();

        Dictionary<string, List<(string Name, int Calories)>> menuItems = menuReading.menuItems;
        Debug.Log("Cibi disponibili FOODCHOSER: " + string.Join(", ", menuItems.Select(item => $"{item.Key} ({string.Join(", ", item.Value)})")));

        sttTestScript.setMenuItems(menuItems);

        try
        {
            Debug.Log("DIALOGUE MANAGER ORDER");
            DialogueManagerOrder.GetInstance().EnterDialogueMode(baseInkJsonFile);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Errore durante EnterDialogueMode: " + ex.Message);
        }

    }

    private string GetRandomResponse()
    {
        return responses[Random.Range(0, responses.Count)];
    }


}
