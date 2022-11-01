using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    #region Inspector

    [SerializeField] private UnityEvent onInteracted;
    [SerializeField] private UnityEvent onSelected;
    [SerializeField] private UnityEvent onDeselect;
    
    #endregion
    
    #region Unity Event Functions

    private void Start()
    {
        List<Interaction> interactions = GetComponentsInChildren<Interaction>(true).ToList();

        if (interactions.Count > 0)
        {
            interactions[0].gameObject.SetActive(true);
        }
    }

    #endregion

    public void Interact()
    {
        Interaction interaction = FindActiveInteraction();

        if (interaction != null)
        {
            interaction.Execute();
        }
        
        Debug.Log("Interact");
        onInteracted.Invoke();
    }

    public void Select()
    {
        Debug.Log("Select");
        onSelected.Invoke();
    }

    public void Deselect()
    {
        Debug.Log("Deselect");
        onDeselect.Invoke();
    }

    private Interaction FindActiveInteraction()
    {
        return GetComponentInChildren<Interaction>(false);
    }
}
