using System;

using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueBox : MonoBehaviour
{
    public static event Action<DialogueBox> DialogueContinued;

    #region Inspector

    [SerializeField] private TextMeshProUGUI dialogueSpeaker;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Button continueButton;

    [Header("Choices")]
    [SerializeField] private Transform choiceContainer;
    [SerializeField] private Button choiceButtonPrefab;

    #endregion

    #region Unity Event Functions

    private void Awake()
    {
        continueButton.onClick.AddListener(
            () =>
            {
                DialogueContinued?.Invoke(this);
            }
        );
    }

    private void OnEnable()
    {
        dialogueSpeaker.SetText(string.Empty);
        dialogueText.SetText(string.Empty);
    }

    #endregion

    public void DisplayText(DialogueLine dialogueLine)
    {
        if (dialogueLine.speaker != null)
        {
            dialogueSpeaker.SetText(dialogueLine.speaker);
        }
        
        dialogueText.SetText(dialogueLine.text);
    }
}
