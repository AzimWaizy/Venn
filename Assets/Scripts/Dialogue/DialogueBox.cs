using System;
using System.Collections;
using System.Collections.Generic;

using Ink.Runtime;

using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueBox : MonoBehaviour
{
    public static event Action<DialogueBox> DialogueContinued;
    public static event Action<DialogueBox, int> ChoiceSelected;

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
        
        DisplayButton(dialogueLine.choices);
    }

    private void DisplayButton(List<Choice> choices)
    {
        Selectable newSelection;
        
        if (choices == null || choices.Count == 0)
        {
            ShowContinueButton(true);
            ShowChoices(false);
            newSelection = continueButton;
        }
        else
        {
            ClearChoices();
            List<Button> choiceButtons = GenerateChoices(choices);
            ShowContinueButton(false);
            ShowChoices(true);
            newSelection = choiceButtons[0];
        }

        StartCoroutine(DelayedSelect(newSelection));
    }

    private void ClearChoices()
    {
        foreach (Transform child in choiceContainer)
        {
            Destroy(child.gameObject);
        }
    }

    private List<Button> GenerateChoices(List<Choice> choices)
    {
        List<Button> choiceButtons = new List<Button>(choices.Count);

        for (int i = 0; i < choices.Count; i++)
        {
            Choice choice = choices[i];

            Button button = Instantiate(choiceButtonPrefab, choiceContainer);

            int index = i;
            button.onClick.AddListener(
                () =>
                {
                    ChoiceSelected?.Invoke(this, index);
                }
            );

            TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
            buttonText.SetText(choice.text);
            button.name = choice.text;
            
            choiceButtons.Add(button);
        }

        return choiceButtons;
    }

    private void ShowContinueButton(bool show)
    {
        continueButton.gameObject.SetActive(show);
    }

    private void ShowChoices(bool show)
    {
        choiceContainer.gameObject.SetActive(show);
    }

    private IEnumerator DelayedSelect(Selectable selectable)
    {
        yield return new WaitForSeconds(1);
        
        selectable.Select();
    }
}



