using UnityEngine;
using System.Threading.Tasks;
using Unity.Collections;
using System.IO.Compression;
using System.Collections.Generic;
using System;
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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Get robot game object name
        GameObject robot = GameObject.Find("Robot");
        STTManager sttManager = robot.GetComponent<STTManager>();
        robotController = robot.GetComponent<RobotController>();
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

        if (output == "ordina" || output == "voglio ordinare" || output == "ordino")
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
            robotController.SetDestination(new Vector3(-0.48f, 0.13f, 9.98f));
            DialogueManager.GetInstance().ExitDialogueMode();

        
        }
        else if (output == "entra" || output == "voglio entrare")
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

            robotController.SetDestination(new Vector3(-9.914f, 0.199999f, 1.938766f));
            DialogueManager.GetInstance().ExitDialogueMode();
            Debug.Log("Robot is moving to entrance");

        }
        else
        {
            DialogueManager dialogueManager = DialogueManager.GetInstance();
            dialogueManager.EnterDialogueMode(inkJSON);
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

        output = output.ToString();


        // Transform output to lower case, trim whitespace, split it into a list, and clean up each word
        List<string> outputList = output.Trim().ToLower().Split(new[] { ' ', ',', ';' }, StringSplitOptions.RemoveEmptyEntries).Select(word => word.Trim()).ToList();

        Debug.Log("Output: " + output);
        
        // Get dhe BMF of the disabled person
        BlindedPerson blindedPerson = disabledPerson.GetComponent<BlindedPerson>();
        int bmr = blindedPerson.GetBMR();

        // Check if the output contains any of the menu items   
        string dialogue = "....................Dopo aver riflettuto ho scelto di prendere dal menu: ";
        // Somma delle calorie
        int totalCalories = 0;

        foreach (var item in menuItems["Prioritario"])
        {
            if (outputList.Contains(item.Name.ToLower()))
            {
                dialogue += item.Name + ", ";
                totalCalories += item.Calories;
            }
        }
        // Se non sono stati trovati cibi prioritari, controlla i normali
        if (totalCalories < bmr)
        {
            foreach (var item in menuItems["Normale"])
            {
                if (outputList.Contains(item.Name.ToLower()))
                {
                    dialogue += item.Name + ", ";
                    totalCalories += item.Calories;
                }
            }
        }

         if (totalCalories > bmr)
        {
            ttsManager.Speak("Mi dispiace, ma hai superato il tuo fabbisogno calorico giornaliero. Ti consiglio di scegliere qualcos'altro.");
            DialogueManagerOrder dialogueManager = DialogueManagerOrder.GetInstance();
            dialogueManager.EnterDialogueMode(inkJSON2);
        }
        else
        {
            if (dialogue.EndsWith(", "))
        {
            dialogue = dialogue.TrimEnd(',', ' ') + ".";
            ttsManager.Speak(dialogue);

            //lo portaq al tavolo
            robotController.SetDestination(new Vector3(-8, 0, 4));
            DialogueManagerOrder.GetInstance().ExitDialogueMode();
            ttsManager.Speak("Il tuo ordine è stato preso in carico. Andiamo ad accomodarci al tavolo.");

        }
        }

    }

    public void setMenuItems(Dictionary<string, List<(string, int)>> menuItemsSource)
    {
        menuItems = menuItemsSource;
    }
}
