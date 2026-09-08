using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class EnemyStats : EnemyBase
{
    public EnemyScriptableObject enemyData;
    //Current stats
    [HideInInspector]
    public float currentMoveSpeed;
    [HideInInspector]
    public float currentHealth;
    [HideInInspector]
    public float currentDamage;
    EnemyMovement movement;

    void Awake()
    {
        base.Awake();
        //Assign the vaiables
        currentMoveSpeed = enemyData.MoveSpeed;
        currentHealth = enemyData.MaxHealth;
        currentDamage = enemyData.Damage;
    }
    void Start()
    {
        base.Start();
        // Get a reference to the enemy movement script.
        movement = GetComponent<EnemyMovement>();
    }

    void Update()
    {
        if (ShouldDespawn()) ReturnEnemy();
    }

    // This function always needs at least 2 values, the amount of damage dealt <dmg>, as well as where the damage is
    // coming from, which is passed as <sourcePosition>. The <sourcePosition> is necessary because it is used to calculate
    // the direction of the knockback.
    public override void TakeDamage(float dmg)
    {
        TakeDamage(dmg, transform.position, 0f, 0f);
    }

    public void TakeDamage(float dmg, Vector2 sourcePosition, float knockbackForce = 5f, float knockbackDuration = 0.2f)
    {
        currentHealth -= dmg;
        StartCoroutine(DamageFlashCoroutine());

        // Create the text popup when enemy takes damage.
        if (dmg > 0)
            GameManager.GenerateFloatingText(Mathf.FloorToInt(dmg).ToString(), transform);

        // Apply knockback if it is not zero.
        if (knockbackForce > 0)
        {
            // Gets the direction of knockback.
            Vector2 dir = (Vector2)transform.position - sourcePosition;
            movement.Knockback(dir.normalized * knockbackForce, knockbackDuration);
        }

        // Kills the enemy if the health drops below zero.
        if (currentHealth <= 0)
        {
            Kill();
        }
    }

    // Applies a stun effect to the enemy.
    public void ApplyStun(float duration)
    {
        if (movement) movement.ApplyStun(duration);
    }

    // This is a Coroutine function that makes the enemy flash when taking damage.
    public void Kill()
    {
        Destroy(gameObject);
        StartCoroutine(KillFade());
    }

    // This is a Coroutine function that fades the enemy away slowly.
    IEnumerator KillFade()
    {
        // Waits for a single frame.
        WaitForEndOfFrame w = new WaitForEndOfFrame();
        float t = 0, origAlpha = spriteRenderer ? spriteRenderer.color.a : 1f;

        // This is a loop that fires every frame.
        while (t < deathFadeTime)
        {
            yield return w;
            t += Time.deltaTime;

            // Set the colour for this frame.
            if (spriteRenderer)
            {
                spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, (1 - t / deathFadeTime) * origAlpha);
            }
        }

        Destroy(gameObject);
    }

    void OnCollisionStay2D(Collision2D col)
    {
        //Reference the script from the collided collider and deal damage using TakeDamage()
        if (col.gameObject.CompareTag("Player"))
        {
            PlayerStats player = col.gameObject.GetComponent<PlayerStats>();
            player.TakeDamage(currentDamage);    //Make sure to use currentDamage instead of weaponData.Damage in case any damage multipliers in the future
            player.TakeDamage(currentDamage); // Make sure to use currentDamage instead of weaponData.Damage in case any damage multipliers in the future
        }
    }

    private void OnDestroy()
    {
        EnemySpawner es = FindObjectOfType<EnemySpawner>();
        if (es != null)
        {
            es.OnEnemyKilled();
        }
    }
    void ReturnEnemy()
    {
        // EnemySpawner es = FindObjectOfType<EnemySpawner>();
        // if (es == null || player == null) return;
        // transform.position = player.position + es.relativeSpawnPoints[Random.Range(0, es.relativeSpawnPoints.Count)].position;
    }

    protected override void Die()
    {
        Kill();
    }
}
