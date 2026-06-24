using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object.Synchronizing;
using FishNet.Object;
using TMPro;
using FishNet.CodeGenerating;
using UnityEngine.Events;
using UnityEngine.InputSystem;

// Central Class for managing UI and health.
public class Player : NetworkBehaviour
{
    [SerializeField] private PlayerUI playerUI;
    // We will need to refactor this 
    [SerializeField] private float maxHealth;
    [AllowMutableSyncType]
    public SyncVar<float> health = new(10);

    public override void OnStartClient()
    {
        if (!IsOwner) return;
        playerUI.gameObject.SetActive(true);
        playerUI.Subscribe(this);
    }

    public override void OnStartServer()
    {
        // Initialize health on server
        health.Value = maxHealth;
    }

    private void Update()
    {
        if (!base.IsOwner) return;

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            ServerTakeDamage(2);
        }
    }

    // This is executed on the server, health is autosync-ed so the OnTakeDamage Function is called. 
    [ServerRpc(RequireOwnership = false)]
    private void ServerTakeDamage(float hit)
    {
        health.Value -= hit; 
    }
}