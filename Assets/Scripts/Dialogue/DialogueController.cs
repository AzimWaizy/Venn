using System;
using System.Collections.Generic;
using System.Linq;

using Ink;
using Ink.Runtime;
using UnityEngine;
using UnityEngine.EventSystems;

public class DialogueController : MonoBehaviour
{
    private const string SpeakerSeparator = ":";
    private const string EscapeColonPlaceholder = "$";
    private const string EscapeColon = "::";

    public static event Action DialogueOpened;
    public static event Action DialogueClosed;
    public static event Action<string> InkEvent; 

    #region Inspector

    [Header("Ink")]
    [SerializeField] private TextAsset inkAsset;

    [Header("UI")]
    [SerializeField] private DialogueBox dialogueBox;
    
    #endregion

    private Story inkStory;
    private GameState gameState;

    #region Unity Event Funktions

    private void Awake()
    {
        gameState = FindObjectOfType<GameState>();
        inkStory = new Story(inkAsset.text);
        inkStory.onError += OnInkError;
        inkStory.BindExternalFunction<string>("Unity_Event", Unity_Event);
        inkStory.BindExternalFunction<string>("Get_State", Get_State);
        inkStory.BindExternalFunction<string, int>("Add_State", Add_State);
    }

    private void OnEnable()
    {
        DialogueBox.DialogueContinued += OnDialogueContinued;
        DialogueBox.ChoiceSelected += OnChoiceSelected;
    }

    private void Start()
    {
        dialogueBox.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        DialogueBox.DialogueContinued -= OnDialogueContinued;
        DialogueBox.ChoiceSelected -= OnChoiceSelected;
    }

    private void OnDestroy()
    {
        inkStory.onError -= OnInkError;
    }

    #endregion

    #region Dialogue Lifecycle

    public void StartDialogue(string dialoguePath)
    {
        OpenDialogue();
        
        inkStory.ChoosePathString(dialoguePath);
        ContinueDialogue();
    }

    private void OpenDialogue()
    {
        dialogueBox.gameObject.SetActive(true);
        
        DialogueOpened?.Invoke();
    }

    private void CloseDialogue()
    {
        dialogueBox.gameObject.SetActive(false);
        
        EventSystem.current.SetSelectedGameObject(null);
        
        DialogueClosed?.Invoke();
    }

    private void ContinueDialogue()
    {
        if (IsAtEnd())
        {
            CloseDialogue();
            return;
        }

        DialogueLine line;
        if (CanContinue())
        {
            string inkLine = inkStory.Continue();
            if (string.IsNullOrWhiteSpace(inkLine))
            {
                ContinueDialogue();
            }

            line = ParseText(inkLine, inkStory.currentTags);
        }
        else
        {
            line = new DialogueLine();
        }

        line.choices = inkStory.currentChoices;
        
        dialogueBox.DisplayText(line);
    }

    private void SelectChoice(int choiceIndex)
    {
        inkStory.ChooseChoiceIndex(choiceIndex);
        ContinueDialogue();
    }

    private void OnDialogueContinued(DialogueBox _)
    {
        ContinueDialogue();
    }

    private void OnChoiceSelected(DialogueBox _, int choiceIndex)
    {
        SelectChoice(choiceIndex);
    }
    
    #endregion

    #region Ink

    private DialogueLine ParseText(string inkLine, List<string> inkStoryCurrentTags)
    {
        inkLine = inkLine.Replace(EscapeColon, EscapeColonPlaceholder);
        
        List<string> parts = inkLine.Split(SpeakerSeparator).ToList();

        string speaker;
        string text;

        switch (parts.Count)
        {
            case 1:
                speaker = null;
                text = parts[0];
                break;
            case 2:
                speaker = parts[0];
                text = parts[1];
                break;
            default:
                Debug.LogWarning($"Ink dialogue line split at more {SpeakerSeparator} than expected. Please make sure to use {EscapeColon} for {SpeakerSeparator} inside text.");
                goto case 2;
        }

        DialogueLine line = new DialogueLine();
        line.speaker = speaker?.Trim();
        line.text = text.Replace(EscapeColon, EscapeColonPlaceholder).Trim();

        if (inkStoryCurrentTags.Contains("thought"))
        {
            line.text = $"<i>{line.text}</i>";
        }

        return line;
    }

    private bool CanContinue()
    {
        return inkStory.canContinue;
    }

    private bool HasChoices()
    {
        return inkStory.currentChoices.Count > 0;
    }

    private bool IsAtEnd()
    {
        return !CanContinue() && !HasChoices();
    }

    private void OnInkError(string message,ErrorType type)
    {
        switch (type)
        {
            case ErrorType.Author:
                break;
            case ErrorType.Warning:
                Debug.LogWarning(message);
                break;
            case ErrorType.Error:
                Debug.LogError(message);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
            
    }

    private void Unity_Event(string eventName)
    {
        InkEvent?.Invoke(eventName);
    }

    private object Get_State(string id)
    {
        State state = gameState.Get(id);
        return state != null ? state.amount : 0;
    }

    private void Add_State(string id, int amount)
    {
        gameState.Add(id, amount);
    }

    #endregion
}

public struct DialogueLine
{
    public string speaker;
    public string text;
    public List<Choice> choices;
}
