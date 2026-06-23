using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object.Synchronizing;
using FishNet.Object;
using TMPro;
using FishNet.CodeGenerating;
using UnityEngine.InputSystem;

public class PlayerHealth : NetworkBehaviour
{

    [SerializeField] private TMP_Text healthText;

    [AllowMutableSyncType]
    public SyncVar<int> health = new(10);
       

    private void Start()
    {
       // healthText = GameObject.FindGameObjectWithTag("HealthText").GetComponent<TextMeshProUGUI>();
        
    }

    private void Update()
    {

        healthText.text = health.Value.ToString();

        if (!base.IsOwner)
            return;

        

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            ServerLowerHealth(-2);
        }

        
    }



    [ServerRpc]
    private void ServerLowerHealth(int hit)
    {
        health.Value += hit;
    }
}