using UnityEngine;

public class CubeButtonCity : MonoBehaviour
{
    public Camera myCamera; // Assegna la telecamera manualmente nel Inspector
    private Material buttonMaterial; // Riferimento al materiale del bottone
    private Color originalColor; // Colore originale del bottone
    public Color hoverColor = Color.red; // Colore da applicare quando il mouse è sopra


    [Header("Dialogue UI")]
    [SerializeField] private TextAsset inkJSON;

    private void Start()
    {
        // Ottieni il materiale del bottone
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            buttonMaterial = renderer.material;
            originalColor = buttonMaterial.color; // Salva il colore originale
        }
        else
        {
            Debug.LogError("Renderer non trovato sul bottone!");
        }
    }

    public void OnButtonClicked()
    {
        // Azione da eseguire quando il bottone viene cliccato
        Debug.Log("Bottone cliccato!");
        DialogueManagerCity.GetInstance().EnterDialogueMode(inkJSON);
        TTSManager.Instance.Speak("Benvenuto a UAITF City! Io sono UAITF Robot ed il mio obiettivo è quello di portarti al sicuro e nel minor tempo possibile alla destinazione scelta da te. Seleziona il bottone Vocal choice e scegli una delle possibili destinazioni fra Bar,Negozio,Casa,Ufficio,Supermercato.");
    }

    private void OnMouseEnter()
    {
        // Cambia il colore quando il cursore è sopra il bottone
        if (buttonMaterial != null)
        {
            buttonMaterial.color = hoverColor;
        }
    }

    private void OnMouseExit()
    {
        // Ripristina il colore originale quando il cursore esce dal bottone
        if (buttonMaterial != null)
        {
            buttonMaterial.color = originalColor;
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // 0 è il tasto sinistro del mouse
        {
            if (myCamera == null)
            {
                Debug.LogError("La telecamera non è assegnata!");
                return;
            }

            // Use RaycastAll
            Ray ray = myCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit[] hits = Physics.RaycastAll(ray);

            foreach (RaycastHit hit in hits)
            {
                if (hit.collider == GetComponent<Collider>())
                {
                    Debug.Log("Log di onbuttonclicked!");
                    OnButtonClicked();
                    break;
                }
            }

        }
    }
}
