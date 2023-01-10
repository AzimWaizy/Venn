using System;

using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    #region Inspector
    
    [Header("Slope Movement")]
    
    [Min(0)]
    [SerializeField] private float pullDownForce = 5f;
    
    [SerializeField] private LayerMask layerMask;
    
    [Min(0)]
    [SerializeField] private float raycastLength = 0.5f;

    [Min(0)]
    [SerializeField] private float coyoteTime = 0.2f;
    
    [Header("Movement")]
    
    [Min(0)]
    [Tooltip("The maximum speed of the player in uu/s.")]
    [SerializeField] private float movementSpeed = 5f;
    
    [Min(0)]
    [Tooltip("How fast the movement speed is in-/decreasing.")]
    [SerializeField] private float speedChangeRate = 10f;
    
    [Min(0)]
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Camera")]

    [SerializeField] private Transform cameraTarget;
    [Range(-89f, 0f)]
    [SerializeField] private float verticalCameraRotationMin = -30f;
    [Range(0f, 89f)]
    [SerializeField] private float verticalCameraRotationMax = 70f;
    [Min(0)]
    [SerializeField] private float cameraHorizontalSpeed = 200f;
    [Min(0)]
    [SerializeField] private float cameraVerticalSpeed = 130f;
    
    [Header("Controller Settings")]

    [SerializeField] private Animator animator;

    
    #endregion

    private static readonly int MovementSpeed = Animator.StringToHash("MoveSpeed");
    private static readonly int Grounded = Animator.StringToHash("Grounded");
    private CharacterController characterController;
    private GameInput input;
    private InputAction moveAction;
    private Vector2 moveInput;
    private Vector3 lastMovement;
    private InputAction lookAction;
    private Vector2 lookInput;
    private Vector2 cameraRotation;
    private Quaternion characterTargetRotation = Quaternion.identity;
    private bool isGrounded = true;
    private float airTime;
    private Interactable selectedInteractable;
    
    #region UnityEventFunctions

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        input = new GameInput();
        moveAction = input.Player.Move;
        lookAction = input.Player.Look;

        input.Player.Interact.performed += Interact;
    }

    private void OnEnable()
    {
        EnableInput();
    }

    private void Update()
    {
        ReadInput();
        Rotate(moveInput);
        Move(moveInput);
        CheckGround();
        UpdateAnimator();
    }

    private void LateUpdate()
    {
        RotateCamera(lookInput);
    }

    private void OnDisable()
    {
        DisableInput();
    }

    private void OnDestroy()
    {
        input.Player.Interact.performed -= Interact;
    }
    
    #region Physics

    private void OnTriggerEnter(Collider other)
    {
        TrySelectInteractable(other);
    }
    private void OnTriggerExit(Collider other)
    {
        TryDeselectInteractable(other);
    }

    #endregion
    
    #endregion

    #region Movement

    private void Rotate(Vector2 moveInput)
    {
        if (moveInput != Vector2.zero)
        {
            Vector3 inputDirection = new Vector3(moveInput.x, 0f, moveInput.y).normalized;

            Vector3 worldInputDirection = cameraTarget.TransformDirection(inputDirection);
            worldInputDirection.y = 0f;
            
            characterTargetRotation = Quaternion.LookRotation(worldInputDirection);
        }

        if (Quaternion.Angle(transform.rotation, characterTargetRotation) > 0.1f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, characterTargetRotation, rotationSpeed * Time.deltaTime);
        }
        else
        {
            transform.rotation = characterTargetRotation;
        }
    }

    private void Move(Vector2 moveInput)
    {
        float targetSpeed = moveInput == Vector2.zero ? 0f : movementSpeed * moveInput.magnitude;

        Vector3 currentVelocity = lastMovement;
        currentVelocity.y = 0;
        float currentSpeed = currentVelocity.magnitude;

        if (Mathf.Abs(currentSpeed - targetSpeed) > 0.01f)
        {
            currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, speedChangeRate * Time.deltaTime);
        }
        else
        {
            currentSpeed = targetSpeed;
        }

        // Multiply the targetRotation Quaternion with the Vector3.forward (not commutative!)
        // to get a direction vector in the direction of the targetRotation.
        // In a sense "vectorize the quaternion" 
        Vector3 targetDirection = characterTargetRotation * Vector3.forward;

        Vector3 movement = targetDirection * currentSpeed;

        characterController.SimpleMove(movement);

        lastMovement = movement;

        if (Physics.Raycast(transform.position + Vector3.up * 0.01f, Vector3.down, out RaycastHit hit, raycastLength, layerMask, QueryTriggerInteraction.Ignore))
        {
            if (Vector3.ProjectOnPlane(movement, hit.normal).y < 0)
            {
                characterController.Move(Vector3.down * (pullDownForce * Time.deltaTime));
            }
        }
    }

    #endregion
    
    #region Ground Check

    private void CheckGround()
    {
        if (characterController.isGrounded)
        {
            airTime = 0;
        }
        else
        {
            airTime += Time.deltaTime;
        }

        isGrounded = airTime < coyoteTime;
    }
    
    #endregion
    
    #region Input

    public void EnableInput()
    {
        input.Enable();
    }

    public void DisableInput()
    {
        input.Disable();
    }

    private void ReadInput()
    {
        moveInput = moveAction.ReadValue<Vector2>();
        lookInput = lookAction.ReadValue<Vector2>();
    }
    
    #endregion
    
    #region Camera

    private void RotateCamera(Vector2 lookInput)
    {
        if (lookInput != Vector2.zero)
        {
            bool isMouseLook = IsMouseLook();

            float deltaTimeMultiplier = isMouseLook ? 1f : Time.deltaTime;

            float sensitivity = isMouseLook ? PlayerPrefs.GetFloat(SettingsMenu.MouseSensitivityKey, SettingsMenu.DefaultMouseSensitivity) : PlayerPrefs.GetFloat(SettingsMenu.ControllerSensitivityKey, SettingsMenu.DefaultControllerSensitivity);

            lookInput *= deltaTimeMultiplier * sensitivity;
            
            // Vertical camera rotation around the X-axis of the player!
            // Additionally multiply with -1 if we are using the controller AND we want to invert the Y input.
            bool invertY = !isMouseLook && SettingsMenu.GetBool(SettingsMenu.InvertYKey, SettingsMenu.DefaultInvertY);
            cameraRotation.x += lookInput.y * cameraVerticalSpeed * (invertY ? -1 : 1);
            // Horizontal camera rotation around the Y-axis of the player!
            cameraRotation.y += lookInput.x * cameraHorizontalSpeed;

            cameraRotation.x = NormalizeAngle(cameraRotation.x);
            cameraRotation.y = NormalizeAngle(cameraRotation.y);

            cameraRotation.x = Mathf.Clamp(cameraRotation.x, verticalCameraRotationMin, verticalCameraRotationMax);
        }
        
        // Important to always do even without input, so it is always steady and only move if we give input.
        // This prevents it from rotation with it´s parent Player object.
        cameraTarget.rotation = Quaternion.Euler(cameraRotation.x, cameraRotation.y, 0f);
    }

    private float NormalizeAngle(float angle)
    {
        // Limit the angle to (-360, 360)
        angle %= 360;

        if (angle < 0)
        {
            angle += 360;
        }

        if (angle > 180)
        {
            angle -= 360;
        }

        return angle;
    }

    private bool IsMouseLook()
    {
        if (lookAction.activeControl == null)
        {
            return true;
        }

        return lookAction.activeControl.name == "delta";
    }
    
    #endregion

    #region Interaction

    private void Interact(InputAction.CallbackContext _)
    {
        if (selectedInteractable != null)
        {
            selectedInteractable.Interact();
        }
    }

    private void TrySelectInteractable(Collider other)
    {
        Interactable interactable = other.GetComponent<Interactable>();

        if (interactable == null)
        {
            return;
        }

        if (selectedInteractable != null)
        {
            return;
        }

        selectedInteractable = interactable;
        selectedInteractable.Select();
    }
    private void TryDeselectInteractable(Collider other)
    {
        Interactable interactable = other.GetComponent<Interactable>();

        if (interactable == null)
        {
            return;
        }

        if (interactable == selectedInteractable)
        {
            selectedInteractable.Deselect();
            selectedInteractable = null;
        }
    }

    #endregion

    #region Animator

    private void UpdateAnimator()
    {
        Vector3 velocity = lastMovement;
        velocity.y = 0;
        float speed = velocity.magnitude;
        
        animator.SetFloat(MovementSpeed, speed);
        animator.SetBool(Grounded, isGrounded);
    }

    #endregion
}
