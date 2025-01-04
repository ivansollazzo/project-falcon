using UnityEngine;

public class TTSTest : MonoBehaviour
{
    private TTSManager ttsManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        this.gameObject.AddComponent<TTSManager>();
        this.gameObject.GetComponent<TTSManager>().Speak("This is something very cool very cool very cool this is something very cool than every Mac and do", robotic_voice: true);
        this.gameObject.GetComponent<TTSManager>().Speak("Tu mi fai girar, tu mi fai girar, come se fossi una bambola, poi mi butti giù, poi mi butti giù.", robotic_voice: false);
    }
}
