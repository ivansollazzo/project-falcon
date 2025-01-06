 using Ink.Runtime;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System.Collections;

public class DialogueManager : MonoBehaviour
{
    private static DialogueManager instance;

    [Header("Dialogue UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;

    [SerializeField] private GameObject vocalChoice;
    private TextMeshProUGUI vocalChoiceText;

    private Story currentStory;

    private bool isDialogueActive;

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
    }

    public void EnterDialogueMode(TextAsset inkJSON)
    {

        currentStory = new Story(inkJSON.text);
        isDialogueActive = true;
        dialoguePanel.SetActive(true);
        
        ContinueStory();

    }
    
    private void ExitDialogueMode()
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
        Choice currentChoise = currentStory.currentChoices[0];

        if(currentChoise == null)
        {
            Debug.LogWarning("Nessuna scelta trovata!");
            return;
        }
        
        vocalChoice.gameObject.SetActive(true);
        vocalChoiceText.text = currentChoise.text;

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
}
