using UnityEngine;
using System.Threading.Tasks;

public class STTTest : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
        // Get robot game objectname
        GameObject robot = GameObject.Find("Robot");
        STTManager sttManager = robot.GetComponent<STTManager>();

        // Start the speech-to-text engine and get the output asynchronously
        string output = await sttManager.Speak();

        // Use the output
        Debug.Log("STTTest: " + output);
    }
}