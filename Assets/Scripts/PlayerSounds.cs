using FMODUnity;
using UnityEngine;
using UnityEngine.Events;

public class PlayerSounds : MonoBehaviour
{
    #region Inspector

    [SerializeField] private StudioEventEmitter stepSound;
    [SerializeField] private StudioEventEmitter landSound;

    [Header("Unity Events")]
    [SerializeField] private UnityEvent onStep;
    [SerializeField] private UnityEvent onLand;

    #endregion

    #region Animation Events

    public void PlaySound(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight < 0.5f)
        {
            return;
        }
        
        switch (animationEvent.stringParameter.ToLowerInvariant())
        {
            case "step":
                stepSound.Play();
                onStep.Invoke();
                break;
            case "land":
                landSound.Play();
                onLand.Invoke();
                break;
            default:
                Debug.LogWarning($"Unknown sound parameter: ´{animationEvent.stringParameter}´.", this);
                break;
        }
    }

    #endregion
}
