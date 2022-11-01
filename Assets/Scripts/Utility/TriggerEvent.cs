using UnityEngine;
using UnityEngine.Events;

public class TriggerEvent : MonoBehaviour
{
    private const string NoTag = "Untagged";
    private const string PlayerTag = "Player";
    
    #region Inspector

    [SerializeField] private UnityEvent<Collider> onTriggerEnter;
    [SerializeField] private UnityEvent<Collider> onTriggerExit;
    [SerializeField] private bool filterOnTag = true;
    [SerializeField] private string reactOn = PlayerTag;
    
    #endregion
    
    #region Unity Event Functions

    //Called when a value is changed in the inspector.
    private void OnValidate()
    {
        //Replaces am ´empty´ reactOn field with "Untagged".
        if (string.IsNullOrWhiteSpace(reactOn))
        {
            reactOn = NoTag;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (filterOnTag && !other.CompareTag(reactOn))
        {
            return;
        }
        
        onTriggerEnter.Invoke(other);
    }

    private void OnTriggerExit(Collider other)
    {
        
        if (filterOnTag && !other.CompareTag(reactOn))
        {
            return;
        }
        
        onTriggerExit.Invoke(other);
    }
    
    #endregion
}
