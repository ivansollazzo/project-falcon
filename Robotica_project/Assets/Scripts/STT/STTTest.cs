using UnityEngine;
using System.Threading.Tasks;
using Unity.Collections;
using System.IO.Compression;
using System.Collections.Generic;
using System;
using System.Text.RegularExpressions;
using System.Linq;

public class STTTest : MonoBehaviour
{
    [SerializeField]
    private TextAsset inkJSON;

    [SerializeField]
    private TextAsset inkJSON2;

    private GameObject robot;
    private RobotController robotController;

    [SerializeField] private GameObject disabledPerson; // Riferimento al GameObject della persona disabile

    public Dictionary<string, List<(string Name, int Calories)>> menuItems = new Dictionary<string, List<(string Name, int Calories)>>();

    public ThinkingCloudController cloudController; // Riferimento al controller della nuvoletta

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Get robot game object name
        GameObject robot = GameObject.Find("Robot");
        STTManager sttManager = robot.GetComponent<STTManager>();
        robotController = robot.GetComponent<RobotController>();
        cloudController = GetComponent<ThinkingCloudController>();

        if (cloudController == null)
        {
            Debug.LogError("ThinkingCloudController non trovato! Assicurati che sia assegnato al personaggio.");
        }

    }

    public async void Speaking() // Cambiato a public
    {
        // Get robot game object name
        Debug.Log("Speaking from button click GUI");


        GameObject robot = GameObject.Find("Robot");
        robotController = robot.GetComponent<RobotController>();
        STTManager sttManager = robot.GetComponent<STTManager>();

        // Start the speech-to-text engine and get the output asynchronously
        string output = await sttManager.Speak();

        //transform output in lower case. Also strip the output of any leading or trailing white spaces
        output = output.Trim().ToLower();

        Debug.Log("Output: " + output);

        if (output == "ordina" || output == "voglio ordinare" || output == "ordino" || output == "ordinare")
        {
            //bring the robot to the "Ordering screens" tag object
            /*
            coordinate per il tavolo

            x = 3.35f;
            y = 0.11999999f;
            z = 3.26f;

            x = -17.63f;
            y = 0.11999999f;
            z = -0.37f;
        {
            //bring the robot to the "Checkout screens" tag object
            /*
            coordinate per il checkout
            x = -0.48f;
            y = 0.13f;
            z = 9.98f;*/

            Debug.Log("Robot is moving to checkout");
            robotController.SetDestination(new Vector3(0.081f, -0.009f, 10.175f));
            DialogueManager.GetInstance().ExitDialogueMode();


        }
        else if (output == "entra" || output == "voglio entrare" || output == "entrare")
        {
            //bring the robot to the "Entrance" tag object

            /*
            coordinate per il tavolo

            x = 3.35f;
            y = 0.11999999f;
            z = 3.26f;

            x = -17.63f;
            y = 0.11999999f;
            z = -0.37f;


            x = -9.914f;
            y = 0.11999999f;
            z = 1.938766f;*/

            robotController.SetDestination(new Vector3(-1.64f, 0.13f, 7.21f));
            DialogueManager.GetInstance().ExitDialogueMode();
            Debug.Log("Robot is moving to entrance");

        }
        else
        {
            DialogueManager dialogueManager = DialogueManager.GetInstance();
            dialogueManager.EnterDialogueMode(inkJSON);
            TTSManager.Instance.Speak("Mi dispiace, non ho capito cosa hai detto. Ripeti una delle opzioni disponibili: voglio entrare o voglio ordinare.");
            Debug.Log("I cannot understand what you said");
        }



    }


    public async void Ordering() // Cambiato a public
    {
        // Get robot game object name
        Debug.Log("Speaking from button click GUI");

        GameObject robot = GameObject.Find("Robot");
        robotController = robot.GetComponent<RobotController>();
        STTManager sttManager = robot.GetComponent<STTManager>();
        TTSManager ttsManager = TTSManager.Instance;

        // Start the speech-to-text engine and get the output asynchronously
        string output = await sttManager.Speak();

        // Pre-process the output
        output = output.ToString().ToLower();

        // Regex pattern to handle splitting
        string pattern = @"\b(?:e|,|;|\s+)\b";


        // Split the string usando il pattern
        List<string> outputList = Regex.Split(output, pattern)
            .Select(word => word.Trim())
            .Where(word => !string.IsNullOrEmpty(word))
            .ToList();

        Debug.Log("Output: " + output);

        // Get dhe BMF of the disabled person
        BlindedPerson blindedPerson = disabledPerson.GetComponent<BlindedPerson>();
        int bmr = blindedPerson.GetBMR();

        // Check if the output is a valid menu item
        List<string> foodList = new List<string>();

        // Get keys of the menuItems["Prioritario"] dictionary
        List<string> prioritarioKeys = menuItems["Prioritario"].Select(item => item.Name.ToLower()).ToList();

        // Get keys of the menuItems["Normale"] dictionary
        List<string> normaleKeys = menuItems["Normale"].Select(item => item.Name.ToLower()).ToList();

        foreach (var item in outputList)
        {
            if (prioritarioKeys.Contains(item))
            {
                foodList.Add(item);
            }
            else if (normaleKeys.Contains(item))
            {
                foodList.Add(item);
            }
        }

        // Now that we have the list of valid menu items, we can check if the sum of their calories is less than the BMR
        int totalCalories = 0;

        foreach (var item in foodList)
        {
            if (prioritarioKeys.Contains(item))
            {
                totalCalories += menuItems["Prioritario"].Where(menuItem => menuItem.Name.ToLower() == item).Select(menuItem => menuItem.Calories).First();
            }
            else if (normaleKeys.Contains(item))
            {
                totalCalories += menuItems["Normale"].Where(menuItem => menuItem.Name.ToLower() == item).Select(menuItem => menuItem.Calories).First();
            }
        }

        // Se il numero di calorie supera il fabbisogno calorico, avvisa l'utente
        if (totalCalories > bmr)
        {
            ttsManager.Speak("Mi dispiace, ma hai superato il tuo fabbisogno calorico giornaliero. Il mio obiettivo è quello di proporti dei pasti bilanciati per tenerti in salute, quindi Ti consiglio di rivedere il tuo ordine.");
            DialogueManagerOrder dialogueManager = DialogueManagerOrder.GetInstance();
            dialogueManager.EnterDialogueMode(inkJSON2);
        }
        else
        {
            // Check if the output contains any of the menu items   
            string dialogue = "Sto inserendo nel tuo ordine i cibi che hai scelto: ";

            // Add the food items to the dialogue. If list is a single item, add it without a comma
            if (foodList.Count == 1)
            {
                dialogue += foodList[0] + ".";
            }
            else
            {
                foreach (var item in foodList)
                {
                    // If it's the last item, add a period
                    if (item == foodList.Last())
                    {
                        dialogue += "e ";
                        dialogue += item + ".";
                    }
                    else
                    {
                        dialogue += item + ", ";
                    }
                }
            }

            // Let the robot speak the dialogue
            ttsManager.Speak(dialogue);

            // Aspetta che il robot abbia finito di parlare
            await Task.Delay(TimeSpan.FromSeconds(3));

            cloudController?.HideCloud();

            //lo porta al tavolo
            robotController.SetDestination(new Vector3(-8, 0, 4));
            DialogueManagerOrder.GetInstance().ExitDialogueMode();
            ttsManager.Speak("Il tuo ordine è stato preso in carico. Andiamo ad accomodarci al tavolo.");

            // Aspetta che il robot abbia finito di parlare
            await Task.Delay(TimeSpan.FromSeconds(6));

        }
    }

    public void setMenuItems(Dictionary<string, List<(string, int)>> menuItemsSource)
    {
        menuItems = menuItemsSource;
    }
}