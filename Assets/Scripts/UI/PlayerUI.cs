using System;
using UnityEngine;
using FishNet;
using FishNet.Object;
using TMPro;
using Unity.VisualScripting;

public class PlayerUI : NetworkBehaviour
{
    [SerializeField] private GameObject floatingUI;
    [SerializeField] private TMP_Text floatingText;
    [SerializeField] private TMP_Text healthText;
    private Player player;
    private void Start()
    {
        player = transform.parent.GetComponent<Player>();
    }

    public override void OnStartClient()
    {
        if (IsOwner)
        {
            floatingUI.SetActive(false);
        }
    }

    void Update()
    {
        floatingText.text = player.health.Value.ToString();
    }

    public void Subscribe(Player player)
    {
        player.health.OnChange += OnHealthChanged;
    }

    private void OnHealthChanged(float prev, float next, bool asServer)
    {
        healthText.text = next.ToString();
       
    }
}
