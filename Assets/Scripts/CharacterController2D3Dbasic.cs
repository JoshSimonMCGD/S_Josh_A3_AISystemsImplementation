using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerControllerBasic : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;

    [Range(0f, 1f)]
    public float airControl = 0.35f;

    [Header("Jump / Gravity")]
    public float jumpHeight = 1.5f;
    public float gravity = -25f;
    private Vector3 _externalVelocity;
    public float externalDrag = 12f;

    private CharacterController _cc;
    private PlayerInput _input;

    private InputAction _moveAction;
    private InputAction _jumpAction;

    private Vector2 _moveInput;
    private Vector3 _horizontalVelocity;
    private float _verticalVelocity;

    private InputAction _abilityAction;
    private PlayerIgniteAbility _igniteAbility;
    private Animator _animator;

    private void Awake()
    {
        _cc = GetComponent<CharacterController>();
        _input = GetComponent<PlayerInput>();
        _igniteAbility = GetComponent<PlayerIgniteAbility>();
        _moveAction = _input.actions["Move"];
        _jumpAction = _input.actions["Jump"];
        _abilityAction = _input.actions["Ability"];
        _animator = GetComponentInChildren<Animator>();
    }

    private void OnEnable()
    {
        _moveAction.Enable();
        _jumpAction.Enable();
        _abilityAction.Enable();
    }

    private void OnDisable()
    {
        _moveAction.Disable();
        _jumpAction.Disable();
        _abilityAction.Disable();
    }

    private void Update()
    {
        ReadInput();
        HandleHorizontalMovement();
        HandleJumpAndGravity();
        ApplyMovement();
        FaceMoveDirection();
        HandleExternalVelocity();
        UpdateAnimator();
        
        // Player AOE Ability check
        if (_abilityAction.WasPressedThisFrame() && _igniteAbility != null)
        {
            _igniteAbility.TryActivate();
        }
    }

    private void ReadInput()
    {
        _moveInput = _moveAction.ReadValue<Vector2>();
    }

    // Horizontal movement control including air control (serialzied)
    private void HandleHorizontalMovement()
    {
        Vector3 inputDirection = new Vector3(_moveInput.x, 0f, _moveInput.y);
        inputDirection = Vector3.ClampMagnitude(inputDirection, 1f);

        Vector3 targetVelocity = inputDirection * moveSpeed;

        float control = _cc.isGrounded ? 1f : airControl;
        _horizontalVelocity = Vector3.Lerp(_horizontalVelocity, targetVelocity, control);
    }

    private void HandleJumpAndGravity()
    {
        if (_cc.isGrounded)
        {
            if (_verticalVelocity < 0f)
                _verticalVelocity = -2f;

            if (_jumpAction.WasPressedThisFrame())
            {
                _verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
        }
        else
        {
            _verticalVelocity += gravity * Time.deltaTime;
        }
    }

    private void ApplyMovement()
    {
        Vector3 finalMove = new Vector3(
            _horizontalVelocity.x + _externalVelocity.x,
            _verticalVelocity,
            _horizontalVelocity.z + _externalVelocity.z
        );

        _cc.Move(finalMove * Time.deltaTime);
    }

    // Faces the player the direction of movement
    private void FaceMoveDirection()
    {
        Vector3 flatMove = new Vector3(_horizontalVelocity.x, 0f, _horizontalVelocity.z);

        if (flatMove.sqrMagnitude > 0.001f)
        {
            transform.forward = flatMove.normalized;
        }
    }

    // Modular knockback. Used here for barrel explosions
    public void ApplyKnockback(Vector3 force)
    {
        _externalVelocity += force;
    }

    private void HandleExternalVelocity()
    {
        _externalVelocity = Vector3.Lerp(_externalVelocity, Vector3.zero, externalDrag * Time.deltaTime);
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody rb = hit.collider.attachedRigidbody;

        if (rb == null || rb.isKinematic)
            return;

        Vector3 pushDirection = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);

        float pushForce = 2f; // Pushback Force Multiplier

        rb.AddForce(pushDirection * pushForce, ForceMode.Impulse);
    }

    private void UpdateAnimator()
    {
        if (_animator == null)
            return;

        Vector3 flatVelocity = new Vector3(_horizontalVelocity.x, 0f, _horizontalVelocity.z);

        _animator.SetFloat("Speed", flatVelocity.magnitude);
        _animator.SetBool("IsGrounded", _cc.isGrounded);
    }
}
