using UnityEngine;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

public class STTManager : MonoBehaviour
{
    private Process process;
    private string sttEngineCommand;
    private StringBuilder sttOutput;

    // Metodo asincrono per eseguire il processo senza bloccare il thread principale
    public async Task<string> Speak() 
    {
        // Verifica che la piattaforma sia macOS
        if (SystemInfo.operatingSystemFamily == OperatingSystemFamily.MacOSX)
        {
            this.sttEngineCommand = "run UAITFSTTManager";

            // Inizializza lo StringBuilder per raccogliere l'output
            sttOutput = new StringBuilder();

            // Verifica se il processo è già in esecuzione
            if (this.process == null || this.process.HasExited)
            {
                // Crea una nuova istanza del processo solo se non è già in esecuzione
                this.process = new Process();
                this.process.StartInfo.FileName = "/usr/bin/shortcuts";
                this.process.StartInfo.Arguments = this.sttEngineCommand;
                this.process.StartInfo.RedirectStandardOutput = true;
                this.process.StartInfo.RedirectStandardError = true;
                this.process.StartInfo.UseShellExecute = false;
                this.process.StartInfo.CreateNoWindow = true;

                // Abbonati agli eventi di output per leggere il flusso in modo asincrono
                this.process.OutputDataReceived += new DataReceivedEventHandler(ProcessOutputHandler);
                this.process.ErrorDataReceived += new DataReceivedEventHandler(ProcessErrorHandler);

                // Avvia il processo
                this.process.Start();
                this.process.BeginOutputReadLine(); // Inizia a leggere l'output in modo asincrono
                this.process.BeginErrorReadLine();  // Inizia a leggere gli errori in modo asincrono

                // Usa un Task per attendere il completamento del processo
                await Task.Run(() => this.process.WaitForExit()); // Asincrono, non blocca Unity

                UnityEngine.Debug.Log("STT Engine has finished");
            }
            else
            {
                UnityEngine.Debug.Log("STT Engine is already running.");
            }
        }
        else
        {
            UnityEngine.Debug.LogError("This functionality is only supported on macOS.");
        }

        // Restituisci l'output come stringa
        return sttOutput.ToString();
    }

    // Gestore dell'output del processo
    private void ProcessOutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
    {
        if (outLine.Data != null)
        {
            sttOutput.Append(outLine.Data + "\n");
            UnityEngine.Debug.Log("STT Engine Output: " + outLine.Data);
        }
    }

    // Gestore degli errori del processo
    private void ProcessErrorHandler(object sendingProcess, DataReceivedEventArgs outLine)
    {
        if (outLine.Data != null)
        {
            UnityEngine.Debug.LogError("STT Engine Error: " + outLine.Data);
        }
    }

    // Assicurati che il processo venga chiuso quando l'applicazione termina
    private void OnApplicationQuit()
    {
        if (this.process != null && !this.process.HasExited)
        {
            this.process.Kill();
            this.process.Dispose();
            UnityEngine.Debug.Log("STT Engine process has been terminated.");
        }
    }
}