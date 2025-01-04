using UnityEngine;
using System.Collections.Generic;

public class TTSManager : MonoBehaviour
{
    // We store a queue of voice objects
    private Queue<VoiceObject> voiceQueue = new Queue<VoiceObject>();

    // We store the current process
    private System.Diagnostics.Process currentProcess;

    // Method to add a voice object to the queue
    public void Speak(string text, bool robotic_voice = false)
    {
        // Add the voice object to the queue
        this.voiceQueue.Enqueue(new VoiceObject(text, robotic_voice));
    }

    // Method to process the queue
    public void ProcessQueue() {

        // Check if there's a current process running
        if (this.currentProcess != null)
        {
            // Check if the process has exited
            if (this.currentProcess.HasExited)
            {
                // Debug voice
                Debug.Log("Process ID: " + this.currentProcess.Id + " has exited");
                this.currentProcess = null;
            }
            else
            {
                // Return to avoid starting a new process
                return;
            }
        }

        // Check if the queue is not empty
        if (this.voiceQueue.Count > 0)
        {
            // Get the first voice object from the queue
            VoiceObject voiceObject = this.voiceQueue.Dequeue();
            
                // Check if operating system is Mac OS X
                if (SystemInfo.operatingSystemFamily == OperatingSystemFamily.MacOSX)
                {
                    if (voiceObject.GetRoboticVoice())
                    {
                        this.currentProcess = System.Diagnostics.Process.Start("say", "-v Cellos " + voiceObject.GetText());
                    }
                    else {
                        this.currentProcess = System.Diagnostics.Process.Start("say", voiceObject.GetText());
                    }
                    
                    // Debug voice
                    if (this.currentProcess != null) {
                        Debug.Log("Process ID: " + this.currentProcess.Id + " has been started");
                    }
                    else {
                        Debug.Log("Could not start voice process.");
                    }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Process the queue
        this.ProcessQueue();
    }

    // Called when the MonoBehaviour will be destroyed
    void OnDestroy()
    {
        // Check if process is not null
        if (this.currentProcess != null)
        {
            // Kill the process
            this.currentProcess.Kill();
            Debug.Log("Current process ID: " + this.currentProcess.Id + " has been killed");
        }
    }
}
