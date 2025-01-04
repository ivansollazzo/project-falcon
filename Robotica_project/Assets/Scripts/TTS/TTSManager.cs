using UnityEngine;

public class TTSManager : MonoBehaviour
{
    private System.Diagnostics.Process process;
    public void Speak(string text, bool robotic_voice = false)
    {
        // Check if operating system is Mac OS X
        if (SystemInfo.operatingSystemFamily == OperatingSystemFamily.MacOSX)
        {
            if (robotic_voice)
            {
                this.process = System.Diagnostics.Process.Start("say", "-v Cellos " + text);
            }
            else {
                this.process = System.Diagnostics.Process.Start("say", text);
            }
            
            // Debug voice
            if (this.process != null) {
                Debug.Log("Process ID: " + this.process.Id + " has been started");
            }
            else {
                Debug.Log("Could not start voice process.");
            }
        }
    }

    // Called when the MonoBehaviour will be destroyed
    void OnDestroy()
    {
        // Check if process is not null
        if (this.process != null)
        {
            // Kill the process
            this.process.Kill();
            Debug.Log("Process ID: " + this.process.Id + " has been killed");
        }
    }
}
