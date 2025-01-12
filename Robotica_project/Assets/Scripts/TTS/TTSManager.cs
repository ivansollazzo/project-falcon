using UnityEngine;
using System.Collections.Generic;

public class TTSManager : MonoBehaviour
{
    private Queue<VoiceObject> voiceQueue = new Queue<VoiceObject>();
    private Dictionary<string, float> spokenTextsTimestamps = new Dictionary<string, float>(); // Traccia i timestamp
    private System.Diagnostics.Process currentProcess;

    public static TTSManager Instance;

    // Intervallo di tempo minimo tra due ripetizioni dello stesso testo (in secondi)
    public float repeatCooldown = 20f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Speak(string text, bool robotic_voice = false)
    {
        float currentTime = Time.time;

        // Controlla se il testo è già stato detto di recente
        if (spokenTextsTimestamps.TryGetValue(text, out float lastSpokenTime))
        {
            if (currentTime - lastSpokenTime < repeatCooldown)
            {
                Debug.Log($"Text '{text}' was spoken recently. Skipping...");
                return;
            }
        }

        // Aggiungi il testo alla coda e aggiorna il timestamp
        Debug.Log($"Adding new voice object to queue: {text}");
        voiceQueue.Enqueue(new VoiceObject(text, robotic_voice));
        spokenTextsTimestamps[text] = currentTime;
    }

    void Update()
    {
        // Process the queue
        ProcessQueue();
    }

    public void ClearQueue() {
        // Clear the queue
        voiceQueue.Clear();

        // Clear the spoken texts timestamps
        spokenTextsTimestamps.Clear();
    }

    public void ProcessQueue()
    {
        try
        {
            if (currentProcess != null && !currentProcess.HasExited)
                return;

            if (voiceQueue.Count > 0)
            {
                VoiceObject voiceObject = voiceQueue.Dequeue();

                if (SystemInfo.operatingSystemFamily == OperatingSystemFamily.MacOSX)
                {
                    string command = voiceObject.GetRoboticVoice()
                        ? $"-v Cellos {EscapeText(voiceObject.GetText())}"
                        : EscapeText(voiceObject.GetText());

                    currentProcess = System.Diagnostics.Process.Start("say", command);
                    Debug.Log($"Started process with ID: {currentProcess?.Id}");
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error in ProcessQueue: {ex.Message}");
            currentProcess = null;
        }
    }

    private string EscapeText(string text)
    {
        return "\"" + text.Replace("\"", "\\\"") + "\"";
    }

    void OnDestroy()
    {
        if (currentProcess != null && !currentProcess.HasExited)
        {
            currentProcess.Kill();
            Debug.Log($"Current process ID: {currentProcess.Id} has been killed");
        }
    }
}