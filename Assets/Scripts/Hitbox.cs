using System.Collections.Generic;
using FishNet;
using FishNet.Object;
using UnityEngine;

public class Hitbox : NetworkBehaviour
{
    [Header("Hit Settings")]
    [SerializeField] private int damage = 10;
    [SerializeField] private Collider hitboxCollider;

    private readonly HashSet<Hurtbox> _alreadyHit = new HashSet<Hurtbox>();
    public Player _owner;

    private void Awake()
    {
        if (hitboxCollider == null)
            hitboxCollider = GetComponent<Collider>();

        if (hitboxCollider != null)
            hitboxCollider.isTrigger = true;

        // if (hitboxCollider != null)
        //     hitboxCollider.enabled = false;
    }
    
    public void Initialize(Player owner)
    {
        _owner = owner;
    }

    [Server]
    public void EnableHitbox()
    {
        _alreadyHit.Clear();

        if (hitboxCollider != null)
            hitboxCollider.enabled = true;
    }

    [Server]
    public void DisableHitbox()
    {
        if (hitboxCollider != null)
            hitboxCollider.enabled = false;

        _alreadyHit.Clear();
    }
    

    // Client Authority Hitboxes
    private void OnTriggerEnter(Collider other)
    {
        if (!IsOwner)
        {
            return; 
        }
        
        Hurtbox hurtbox = other.GetComponent<Hurtbox>();

        if (hurtbox == null)
            hurtbox = other.GetComponentInParent<Hurtbox>();

        if (hurtbox == null)
            return;

        if (hurtbox.Owner == null)
            return;

        // Ignore hitting yourself.
        if (_owner != null && hurtbox.player == _owner)
            return;

        Debug.Log(other.name);
        // // Ignore duplicate hits during one swing.
        // if (_alreadyHit.Contains(hurtbox))
        //     return;

        // _alreadyHit.Add(hurtbox);
        hurtbox.TakeDamageRPCtoServer(damage);
    }
}