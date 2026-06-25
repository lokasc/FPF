using FishNet.Object;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombatController : NetworkBehaviour
{
    [Header("Punch Settings")]
    [SerializeField] private float punchCooldown = 0.75f;

    [Header("References")]
    private Animator animator;

    private float punchTimer;
    private PlayerInput _playerInput;
    private InputAction _attackAction;
    private Player _player;

    private void Awake()
    {
        _player = GetComponent<Player>();
        _playerInput = GetComponent<PlayerInput>();

        var map = _playerInput.actions.FindActionMap("Player", throwIfNotFound: true);
        _attackAction = map.FindAction("Attack", throwIfNotFound: true);
    }

    private void Start()
    {
        animator = _player.playerModel.fpsAnimator;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!IsOwner)
        {
            if (_playerInput != null)
                _playerInput.enabled = false;
            return;
        }

        _playerInput.enabled = true;
        _attackAction.performed += HandleAttack;
        _attackAction.Enable();
    }

    public override void OnStopClient()
    {
        base.OnStopClient();

        if (_attackAction != null)
        {
            _attackAction.performed -= HandleAttack;
            _attackAction.Disable();
        }
    }

    private void Update()
    {
        if (!IsOwner)
            return;

        if (punchTimer > 0f)
        {
            punchTimer -= Time.deltaTime;

            if (punchTimer < 0f)
                punchTimer = 0f;
        }
    }

    private void HandleAttack(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed)
            return;

        TryPunch();
    }

    private void TryPunch()
    {
        if (punchTimer > 0f)
            return;

        if (animator == null)
            return;

        punchTimer = punchCooldown;

        animator.ResetTrigger("PUNCH");
        animator.SetTrigger("PUNCH");
    }
}