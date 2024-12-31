using System.Runtime.InteropServices;
using UnityEngine;

public sealed class TextToSpeech : MonoBehaviour
{
    // Metodo per avviare il discorso con un testo specifico
    public void StartSpeech(string text)
    {
        if (!string.IsNullOrEmpty(text))
        {
            ttsrust_say(text);
        }
        else
        {
            Debug.LogError("Il testo fornito per il discorso è nullo o vuoto.");
        }
    }

    #if !UNITY_EDITOR && (UNITY_IOS || UNITY_WEBGL)
    const string _dll = "__Internal";
    #else
    const string _dll = "ttsrust";
    #endif

    [DllImport(_dll)] 
    static extern void ttsrust_say(string text);
}
