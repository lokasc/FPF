using FishNet.Object;
using UnityEngine;

public class Hurtbox : NetworkBehaviour
{
    public Player Owner;

    public void Initialize(Player owner)
    {
        Owner = owner;
    }

    [ServerRpc(RequireOwnership = false)]
    public void TakeDamage(int amount)
    {
        // Debug.LogWarning("I've been hit!");
        if (Owner == null) return;
        Debug.LogWarning("I've been hit 2!");
        
        Owner.TakeDamage(amount);
    }
}