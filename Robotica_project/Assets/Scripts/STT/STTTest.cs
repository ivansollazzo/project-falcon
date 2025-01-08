using UnityEngine;
using System.Threading.Tasks;
using Unity.Collections;
using System.IO.Compression;

public class STTTest : MonoBehaviour
{
    private TextAsset inkJSON;
    private GameObject robot;
    private RobotController robotController;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
        // Get robot game object name
        GameObject robot = GameObject.Find("Robot");
        STTManager sttManager = robot.GetComponent<STTManager>();
        robotController = robot.GetComponent<RobotController>();
        inkJSON = Resources.Load<TextAsset>("InkFiles/NonHoCapito");
    }

    public async void Speaking() // Cambiato a public
    {
        // Get robot game object name
        Debug.Log("Speaking from button click GUI");

         
        GameObject robot = GameObject.Find("Robot");
        robotController = robot.GetComponent<RobotController>();
        STTManager sttManager = robot.GetComponent<STTManager>();
        inkJSON = Resources.Load<TextAsset>("InkFiles/NonHoCapito");

        // Start the speech-to-text engine and get the output asynchronously
        string output = await sttManager.Speak();

        //transform output in lower case. Also strip the output of any leading or trailing white spaces
        output = output.Trim().ToLower();

        Debug.Log("Output: " + output);

        if (output == "ordina" || output == "voglio ordinare")
        {
            //bring the robot to the "Checkout screens" tag object
            /*
            coordinate per il checkout
            x = -0.48f;
            y = 0.13f;
            z = 9.98f;*/

            Debug.Log("Robot is moving to checkout");
            robotController.SetDestination(new Vector3(-0.48f, 0.13f, 9.98f));
        
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

            robotController.SetDestination(new Vector3(-9.914f, 0.11999999f, 1.938766f));
            Debug.Log("Robot is moving to entrance");

        }
        else
        {
            DialogueManager.GetInstance().EnterDialogueMode(inkJSON);
            Debug.Log("I cannot understand what you said");
        }

        

    }
}
