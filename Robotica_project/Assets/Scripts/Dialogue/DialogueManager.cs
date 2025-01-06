using Ink.Runtime;
using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    private static DialogueManager instance;

    [Header("Dialogue UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;

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
    }

    private void Update()
    {
        //return right away if dialogue is not active
        if(!isDialogueActive)
        {
            return;
        }

        //handle continuing to the next line in the dialogue when subit is pressed
        if(InputManager.GetInstance().GetSubmitPressed())
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
        }

        else
        {
            ExitDialogueMode();
        }
    }
}
