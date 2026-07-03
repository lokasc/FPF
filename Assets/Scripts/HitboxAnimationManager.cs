using System.Collections.Generic;
using UnityEngine;

public class HitboxAnimationManager : MonoBehaviour
{
    public List<Hitbox> hitboxes;

    public void ActivateHitbox(int hitboxIndex)
    {
        hitboxes[hitboxIndex].EnableHitbox();
    }

    public void DeactivateHitbox(int hitboxIndex)
    {
        hitboxes[hitboxIndex].DisableHitbox();
    }
}
