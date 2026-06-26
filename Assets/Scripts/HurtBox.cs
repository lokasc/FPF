using FishNet.Object;
using UnityEngine;

public class Hurtbox : NetworkBehaviour
{
    public Player player;

    public void Initialize(Player owner)
    {
        player = owner;
    }

    
    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageRPCtoServer(int amount)
    {
        // Debug.LogWarning("I've been hit!");
        if (Owner == null) return;
        Debug.LogWarning("I've been hit 2!");
        
        player.TakeDamage(amount);
    }
}