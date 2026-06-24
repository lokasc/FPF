using UnityEngine;
using FishNet;
using FishNet.Object;
using TMPro;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private TMP_Text healthText;
    public void Subscribe(Player player)
    {
        player.health.OnChange += OnHealthChanged;
    }

    private void OnHealthChanged(float prev, float next, bool asServer)
    {
        healthText.text = next.ToString();
    }
}
