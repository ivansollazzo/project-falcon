using System.Collections;
using UnityEngine;

public class TimerHelper : MonoBehaviour
{
    // Funzione generica che accetta una durata e una callback da eseguire dopo il ritardo
    public static IEnumerator WaitAndExecute(float waitTime, System.Action callback)
    {
        // Aspetta il tempo specificato
        yield return new WaitForSeconds(waitTime);

        // Esegui la callback passata
        callback?.Invoke();
    }
}