using UnityEngine;

public class TTSTest : MonoBehaviour
{
    private TTSManager ttsManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Get the TTSManager component
        this.ttsManager = TTSManager.Instance;

        // Add voice objects to the queue
        ttsManager.Speak("Cough! Cough!");
        //ttsManager.Speak("This is something very cool very cool very cool this is something very cool than every Mac and do.", robotic_voice: true);
        ttsManager.Speak("Tu mi fai girar, tu mi fai girar, come fossi una bambola, poi mi butti giù, poi mi butti giù.", robotic_voice: false);
        ttsManager.Speak("Ah ah, mi piaci. Ah ah ah, mi piaci. Tanto, tanto, ah.", robotic_voice: false);
    }
}
