using System;
using FishNet.Object;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombatController : NetworkBehaviour
{
    [Header("Punch Settings")]
    [SerializeField] private float punchCooldown = 0.75f;

    [Header("References")] private Animator animator;

    private float nextPunchTime;
    private PlayerInput _playerInput;
    private InputAction _attackAction;
    
    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        var map      = _playerInput.actions.FindActionMap("Player", throwIfNotFound: true);
        _attackAction = map.FindAction("Attack", throwIfNotFound: true);

        //_attackAction.started += OnAttack;
    }

    public override void OnStartClient()
    {
        if (!IsOwner) return;
        _playerInput.enabled = true;
        _attackAction.performed += HandleAttack;
    }
    

    private void Start()
    {
        animator = GetComponent<Player>().playerModel.fpsAnimator;
    }
    

    private void TryPunch()
    {
        if (Time.time < nextPunchTime)
            return;

        nextPunchTime = Time.time + punchCooldown;

        animator.ResetTrigger("PUNCH");
        animator.SetTrigger("PUNCH");
    }

    private void HandleAttack(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed)
            return;

        TryPunch();
    }
}