using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Animator))]
public class Player : MonoBehaviour
{
    private PlayerInputActions _inputActions;
    private Animator _playerAnimator;
   
    private readonly Vector2 _xPosLimits = new Vector2(-8.5f, 8.5f);
    private readonly Vector2 _zPosLimits = new Vector2(-5f, 4f);
    private readonly Vector2 _rotationLimits = new Vector2(-75f, 75f);
    private Vector2 _inputMovement;
    
    private bool _parryAttackCooldown = false;
    private float _rotationSpeed;
    private float _currentYRotation;
    public bool IsParryable { get; set; }
    public List<Projectile> projectilesInArea = new List<Projectile>();
    
    private void Awake()
    {
        _inputActions = new PlayerInputActions();
        _playerAnimator = GetComponent<Animator>();
    }
    
    private void OnEnable()
    {
        _inputActions.Enable();
        _inputActions.Player.ParryAttack.performed += OnParryAttack;
        _inputActions.Player.Move.performed += OnMove;
        _inputActions.Player.Move.canceled += OnMoveCanceled;
    }

    private void OnDisable()
    {
        _inputActions.Disable();
        _inputActions.Player.ParryAttack.performed -= OnParryAttack;
        _inputActions.Player.Move.performed -= OnMove;
        _inputActions.Player.Move.canceled -= OnMoveCanceled;
    }

    private void Update()
    {
        // Get the delta position from the input action
        Vector2 deltaPosition = _inputMovement;

        // Calculate reversed X movement and normal Z movement
        float reversedX = deltaPosition.x * 2f * Time.deltaTime;
        float zMovement = deltaPosition.y * 2f * Time.deltaTime;
        
        // Apply movement on the X-Z plane
        Vector3 movement = new Vector3(reversedX, 0, zMovement);
        transform.Translate(movement, Space.World);
        
        transform.position = new Vector3(
            Mathf.Clamp(transform.position.x, _xPosLimits.x, _xPosLimits.y),
            2f,
            Mathf.Clamp(transform.position.z, _zPosLimits.x, _zPosLimits.y));
        
        float yRotation = -deltaPosition.x * 50f * Time.deltaTime;
        float zExtraRotation = Mathf.Abs(-deltaPosition.y) * 20f * Time.deltaTime;
        
        _currentYRotation += yRotation;
        switch (_currentYRotation)
        {
            case > 0:
                _currentYRotation -= zExtraRotation;
                break;
            case < 0:
                _currentYRotation += zExtraRotation;
                break;
        }
        
        _currentYRotation = Mathf.Clamp(_currentYRotation, _rotationLimits.x, _rotationLimits.y);
        
        transform.rotation = Quaternion.Euler(0, _currentYRotation, 0);
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        _inputMovement = context.ReadValue<Vector2>();
    }
    
    private void OnMoveCanceled(InputAction.CallbackContext obj)
    {
        _inputMovement = Vector2.zero;
    }

    private void OnParryAttack(InputAction.CallbackContext context)
    {
        if (!_parryAttackCooldown)
        {
            _parryAttackCooldown = true;
            _playerAnimator.Play("Sword_Swing");
            Task.Run(() => CooldownReset());
        }
        
        if (!IsParryable) return;
        
        Debug.Log("Parried");
        
        foreach (var projectile in projectilesInArea)
        {
            projectile.Speed *= -1;

            projectile.transform.localScale *= 1.2f;
        }
        
    }

    private async UniTaskVoid CooldownReset()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(3), ignoreTimeScale: false);

        _parryAttackCooldown = false;
    }
}
