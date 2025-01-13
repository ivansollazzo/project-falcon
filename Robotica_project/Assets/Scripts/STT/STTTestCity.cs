using UnityEngine;
using System.Threading.Tasks;
using Unity.Collections;
using System.IO.Compression;

public class STTTestCity : MonoBehaviour
{
    [SerializeField]
    private TextAsset inkJSON;     private GameObject robot;
    private RobotController robotController;

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

        if (output == "casa" || output == "portami a casa")
        {
            Debug.Log("Robot is moving to go home");
            robotController.SetDestination(new Vector3(-4.09f, 0f, 25.16f));
            DialogueManagerCity.GetInstance().ExitDialogueMode();
 
        }
        else if (output == "ufficio" || output == "portami in ufficio")
        {

            robotController.SetDestination(new Vector3(27.27f, 0f, 34.21f));
            Debug.Log("Robot is moving to go to the office");
            DialogueManagerCity.GetInstance().ExitDialogueMode();


        }
        else if(output == "bar" || output == "portami al bar")
        {
            robotController.SetDestination(new Vector3(0f, 0f, -16.92f));
            Debug.Log("Robot is moving to go to the bar");
            DialogueManagerCity.GetInstance().ExitDialogueMode();

        }
        else if(output == "supermercato" || output == "portami al supermercato")
        {
            robotController.SetDestination(new Vector3(12.39f, 0f, 17.21f));
            Debug.Log("Robot is moving to go to the supermarket");
            DialogueManagerCity.GetInstance().ExitDialogueMode();

        }
        else if (output == "negozio" || output == "portami al negozio")
        {
            robotController.SetDestination(new Vector3(23.64f, 0f, -12.744f));
            Debug.Log("Robot is moving to go to the store");
            DialogueManagerCity.GetInstance().ExitDialogueMode();

        }     
        else
        {
            TTSManager.Instance.Speak("Mi dispiace, non ho capito cosa hai detto. Ripeti una delle opzioni disponibili: casa, ufficio,bar,supermercato e negozio");
            DialogueManagerCity.GetInstance().EnterDialogueMode(inkJSON);
            Debug.Log("I cannot understand what you said");
        }

    }
}
