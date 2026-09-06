using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A GameObject that is spawned as an effect of a weapon firing, e.g. projectiles, auras, pulses.
/// </summary>
public abstract class WeaponEffect : MonoBehaviour
{

    [HideInInspector] public PlayerStats owner;
    [HideInInspector] public Weapon weapon;

    public float GetDamage()
    {
        return weapon.GetDamage();
    }

    public void OnDamageDealt(float baseAmount = 3f)
    {
        if (owner != null)
        {
            SpecialMoveSystem sms = owner.GetComponent<SpecialMoveSystem>();
            if (sms != null)
            {
                sms.AddGauge(baseAmount);
            }
        }
    }
}
