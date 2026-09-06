using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Character Data", menuName = "2D Top-down Rogue-like/Character Data")]
public class CharacterData : ScriptableObject
{
    [SerializeField]
    Sprite icon;

    public RuntimeAnimatorController animator;

    public Sprite Icon { get => icon; private set => icon = value; }

    [SerializeField]
    new string name;
    public string Name { get => name; private set => name = value; }

    [SerializeField]
    WeaponData startingWeapon;
    public WeaponData StartingWeapon { get => startingWeapon; private set => startingWeapon = value; }

    [Header("Special Move")]
    [SerializeField]
    Sprite specialMoveCutin;
    public Sprite SpecialMoveCutin { get => specialMoveCutin; private set => specialMoveCutin = value; }

    [SerializeField]
    float specialMoveGaugeMultiplier = 1f;
    public float SpecialMoveGaugeMultiplier { get => specialMoveGaugeMultiplier; private set => specialMoveGaugeMultiplier = value; }

    [System.Serializable]
    public struct Stats
    {
        public float maxHealth, recovery, moveSpeed;
        public float might, speed, magnet, cooldown;

        public Stats(float maxHealth = 1000, float recovery = 0, float moveSpeed = 1f, float might = 1f, float speed = 1f, float magnet = 30f, float cooldown = 1f)
        {
            this.maxHealth = maxHealth;
            this.recovery = recovery;
            this.moveSpeed = moveSpeed;
            this.might = might;
            this.speed = speed;
            this.magnet = magnet;
            this.cooldown = cooldown;
        }

        public static Stats operator +(Stats s1, Stats s2)
        {
            s1.maxHealth += s2.maxHealth;
            s1.recovery += s2.recovery;
            s1.moveSpeed += s2.moveSpeed;
            s1.might += s2.might;
            s1.speed += s2.speed;
            s1.magnet += s2.magnet;
            s1.cooldown += s2.cooldown;
            return s1;
        }
    }
    public Stats stats = new Stats(1000);
}
