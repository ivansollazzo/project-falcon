 using Ink.Runtime;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System.Collections;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    private static DialogueManager instance;

    [Header("Dialogue UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;

    [SerializeField] private GameObject vocalChoice;
    private TextMeshProUGUI vocalChoiceText;

    private Story currentStory;

    public bool isDialogueActive;
    private Button currentChoiceButton;


    private void Awake()
    {
        if(instance != null)
        {
            Debug.LogWarning("Esiste già un'istanza di DialogueManager!");
        }
        instance = this;
    }

    public static DialogueManager GetInstance()
    {
        return instance;
    }

    private void Start()
    {
        isDialogueActive = false;
        dialoguePanel.SetActive(false);

        vocalChoiceText = vocalChoice.GetComponentInChildren<TextMeshProUGUI>();
    }

    private void Update()
    {
        //return right away if dialogue is not active
        if(!isDialogueActive)
        {
            return;
        }

        // Gestisce il passaggio alla prossima linea quando premi Enter o Spacebar
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            ContinueStory();
        }
        // Gestisce il click destro del mouse
        if (Input.GetMouseButtonDown(1))
        {
            SelectHighlightedChoice();
        }
    }

    public void EnterDialogueMode(TextAsset inkJSON)
    {

        currentStory = new Story(inkJSON.text);
        isDialogueActive = true;
        dialoguePanel.SetActive(true);
        
        ContinueStory();

    }
    
    public void ExitDialogueMode()
    {
        isDialogueActive = false;
        dialoguePanel.SetActive(false);
        dialogueText.text = "";
    }

    private void ContinueStory()
    {
        if ( currentStory.canContinue )
        {
            dialogueText.text = currentStory.Continue();

            //display choices if any for this dialogue line
            DisplayChoices();
        }

        else
        {
            ExitDialogueMode();
        }
    }

    private void DisplayChoices()
{
    Choice currentChoice = currentStory.currentChoices[0]; // Hai sempre e solo una scelta

    if (currentChoice == null)
    {
        Debug.LogWarning("Nessuna scelta trovata!");
        return;
    }

    // Attiva il bottone e mostra il testo
    vocalChoice.gameObject.SetActive(true);
    vocalChoiceText.text = currentChoice.text;

    // Ottieni il componente Button del vocalChoice
    currentChoiceButton = vocalChoice.GetComponent<Button>();

    if (currentChoiceButton != null)
{
    // Ottieni il componente OnClickScript associato al bottone
    OnClickScript onClickScript = currentChoiceButton.GetComponent<OnClickScript>();

    if (onClickScript != null)
    {
        // Aggiungi il listener per il metodo OnButtonClick dello script OnClickScript
        currentChoiceButton.onClick.RemoveAllListeners();
        currentChoiceButton.onClick.AddListener(() =>
        {
            onClickScript.OnButtonClick();
        });
    }
    else
    {
        Debug.LogWarning("OnClickScript non trovato sul bottone vocalChoice!");
    }
}


    // Seleziona la scelta nel sistema di input
    StartCoroutine(SelectChoice());
}


    private IEnumerator SelectChoice()
    {
        EventSystem.current.SetSelectedGameObject(null);
        yield return new WaitForEndOfFrame();
        EventSystem.current.SetSelectedGameObject(vocalChoice);
    }

    public void MakeChoice(int choiceIndex)
    {
        currentStory.ChooseChoiceIndex(choiceIndex);
    }

    private void SelectHighlightedChoice()
{
    if (currentStory.currentChoices.Count > 0)
    {
        // Fai la scelta con Ink
        MakeChoice(0);

        // Simula il click sul bottone associato (se esiste)
        if (currentChoiceButton != null)
        {
            currentChoiceButton.onClick.Invoke();
        }

        // Continua la storia
        ContinueStory();
    }
    else
    {
        Debug.LogWarning("Nessuna scelta selezionabile al momento!");
    }
}


}
