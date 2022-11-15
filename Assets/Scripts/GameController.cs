using System;
using System.Security;

using UnityEngine;
using UnityEngine.TestTools;

public class GameController : MonoBehaviour
{
    private PlayerController player;
    private DialogueController dialogueController;
    #region  Unity Event Functions

    private void Awake()
    {
        player = FindObjectOfType<PlayerController>();

        if (player == null)
        {
            Debug.LogError("No player found in the scene", this);
        }
        
        dialogueController = FindObjectOfType<DialogueController>();

        if (dialogueController == null)
        {
            Debug.LogError("No dialogueController found in the scene", this);
        }
    }

    private void OnEnable()
    {
        DialogueController.DialogueClosed += EndDialogue;
    }

    private void OnDisable()
    {
        DialogueController.DialogueClosed -= EndDialogue;
    }

    private void Start()
    {
        EnterPlayMode();
    }

    #endregion

    #region Modes

    private void EnterPlayMode()
    {
        Cursor.lockState = CursorLockMode.Locked;
        player.EnableInput();
    }

    private void EnterDialogueMode()
    {
        Cursor.lockState = CursorLockMode.None;
        player.DisableInput();
    }

    #endregion

    public void StartDialogue(string dialoguePath)
    {
        EnterDialogueMode();
        dialogueController.StartDialogue(dialoguePath);
    }

    private void EndDialogue()
    {
        EnterPlayMode();
    }
}
