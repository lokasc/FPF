using FishNet.Object;
using UnityEngine;

public class Hurtbox : NetworkBehaviour
{
    public Player Owner;

    public void Initialize(Player owner)
    {
        Owner = owner;
    }

    [Server]
    public void TakeDamage(int amount)
    {
        if (Owner == null)
            return;

        Owner.health.Value -= amount;
    }
}