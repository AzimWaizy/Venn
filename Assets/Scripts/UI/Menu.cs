using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    #region Inspector

    [SerializeField] private Selectable selectOnOpen;

    [SerializeField] private bool selectPreviousOnClosed = true;
    
    [SerializeField] private bool disableOnAwake;

    #endregion

    private Selectable selectOnClosed;

    #region Unity Event Functions

    private void Awake()
    {
        if (disableOnAwake)
        {
            gameObject.SetActive(false);
        }
        else
        {
            Open(true);
        }
    }

    #endregion

    public void Open(bool instant = false)
    {
        gameObject.SetActive(true);

        if (selectPreviousOnClosed)
        {
            GameObject previousSelection = EventSystem.current.currentSelectedGameObject;
            if (previousSelection != null)
            {
                selectOnClosed = previousSelection.GetComponent<Selectable>();
            }
        }

        StartCoroutine(DelayedSelect(selectOnOpen));
    }

    public void Close()
    {
        if (selectPreviousOnClosed && selectOnClosed != null)
        {
            selectOnClosed.StartCoroutine(DelayedSelect(selectOnClosed));
        }
        
        gameObject.SetActive(false);
    }

    public void Show()
    {
        //TODO DOTween
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        //TODO DOTween
        gameObject.SetActive(false);
    }
    
    private IEnumerator DelayedSelect(Selectable newSelection)
    {
        yield return null;
        Select(newSelection);
    }

    private void Select(Selectable newSelection)
    {
        if (newSelection == null)
        {
            return;
        }
        newSelection.Select();
    }
}
