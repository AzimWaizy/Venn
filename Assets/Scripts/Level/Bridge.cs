using DG.Tweening;

using UnityEditor;

using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class Bridge : MonoBehaviour
{
    #region Inspector

    [SerializeField] private Transform platform;
    [SerializeField] private Vector3 retractedPosition;
    [SerializeField] private Vector3 extendedPosition;
    [SerializeField] private bool startExtended;

    [Header("Animation")]
    [SerializeField] private float moveDuration = 1f;

    [SerializeField] private Ease ease = DOTween.defaultEaseType;

    #endregion

    private bool extended;

    #region Unity Event Functions

    private void Awake()
    {
        extended = startExtended;
        platform.localPosition = startExtended ? extendedPosition : retractedPosition;
    }

    #endregion

    public void Toggle()
    {
        if (extended)
        {
            Retract();
        }
        else
        {
            Extend();
        }
    }

    public void Extend()
    {
        extended = true;
        MovePlatform(extendedPosition);
    }

    public void Retract()
    {
        extended = false;
        MovePlatform(retractedPosition);
    }

    private void MovePlatform(Vector3 targetPosition)
    {
        float speed = (retractedPosition - extendedPosition).magnitude / moveDuration;
        
        platform.DOKill();
        platform.DOLocalMove(targetPosition, speed)
                .SetSpeedBased()
                .SetEase(ease);

        Debug.Log("Start of bridge movement.");
    }
}
