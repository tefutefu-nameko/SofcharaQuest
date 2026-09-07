using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StackOverflowGiant : MonoBehaviour
{
    [Header("Giant Settings")]
    public float giantScale = 5f;
    public float giantDuration = 5.0f;
    public float recoverDuration = 0.5f;
    public float animationDuration = 0.3f;
    public float cameraZoomMultiplier = 1.5f;
    
    [Header("Projectile Settings")]
    public GameObject giantProjectilePrefab;
    public int projectileCount = 100;
    public float fireRate = 0.05f; // Duration / projectileCount -> 5.0 / 100 = 0.05
    public float projectileSpeed = 10f;
    
    [Header("Damage Settings")]
    public float bodyDamage = 100f;
    public float bodyKnockback = 10f;

    [Header("Audio Settings")]
    public AudioClip giantBurstSound;
    public AudioClip fireSound;
    public AudioClip cooldownSound;

    private PlayerStats playerStats;
    private PlayerMovement playerMovement;
    private SpecialMoveSystem specialMoveSystem;
    private CircleCollider2D giantCollider;
    private CameraMovement cameraMovement;
    
    private bool isGiant = false;
    private float originalScale = 1f;

    void Start()
    {
        playerStats = GetComponent<PlayerStats>();
        playerMovement = GetComponent<PlayerMovement>();
        specialMoveSystem = GetComponent<SpecialMoveSystem>();
        
        if (Camera.main != null)
        {
            cameraMovement = Camera.main.GetComponent<CameraMovement>();
        }

        if (specialMoveSystem != null)
        {
            specialMoveSystem.OnSpecialMoveActivated += ActivateGiant;
        }

        // Add a trigger collider for body damage
        giantCollider = gameObject.AddComponent<CircleCollider2D>();
        giantCollider.isTrigger = true;
        giantCollider.enabled = false;
    }

    void OnDestroy()
    {
        if (specialMoveSystem != null)
        {
            specialMoveSystem.OnSpecialMoveActivated -= ActivateGiant;
        }
    }

    void ActivateGiant(Sprite cutinSprite)
    {
        if (!isGiant)
        {
            StartCoroutine(GiantRoutine());
        }
    }

    IEnumerator GiantRoutine()
    {
        isGiant = true;
        
        // State 1: Overflow & Giant
        originalScale = transform.localScale.x;
        float targetScale = giantScale;
        
        float originalOrthoSize = 5f;
        if (Camera.main != null)
        {
            originalOrthoSize = Camera.main.orthographicSize;
        }
        float targetOrthoSize = originalOrthoSize * cameraZoomMultiplier;
        
        if (playerStats != null)
        {
            playerStats.SetSpecialMoveInvincibility(true);
        }

        giantCollider.radius = 0.5f; // Adjust based on base sprite size
        giantCollider.enabled = true;

        if (cameraMovement != null)
        {
            cameraMovement.TriggerShake(giantDuration, 0.2f);
        }

        if (giantBurstSound != null)
        {
            AudioManager.Instance?.PlaySE(giantBurstSound);
        }

        // Animate Growth
        float animTimer = 0f;
        while (animTimer < animationDuration)
        {
            animTimer += Time.deltaTime;
            float t = animTimer / animationDuration;
            t = t * t * (3f - 2f * t); // Smoothstep
            transform.localScale = Vector3.one * Mathf.Lerp(originalScale, targetScale, t);
            if (Camera.main != null) Camera.main.orthographicSize = Mathf.Lerp(originalOrthoSize, targetOrthoSize, t);
            yield return null;
        }
        transform.localScale = Vector3.one * targetScale;
        if (Camera.main != null) Camera.main.orthographicSize = targetOrthoSize;

        // Weaponアタッチ
        GiantBarrageWeapon barrageWeapon = null;
        if (giantProjectilePrefab != null)
        {
            Projectile projComponent = giantProjectilePrefab.GetComponent<Projectile>();
            if (projComponent != null)
            {
                barrageWeapon = gameObject.AddComponent<GiantBarrageWeapon>();
                barrageWeapon.Setup(projComponent, playerStats, fireRate);
            }
        }

        // 巨大化時間の待機
        yield return new WaitForSeconds(giantDuration);

        // State 2: Recover
        if (barrageWeapon != null)
        {
            Destroy(barrageWeapon);
        }

        giantCollider.enabled = false;

        if (playerMovement != null)
        {
            playerMovement.SetMovementLock(true);
        }
        
        if (cooldownSound != null)
        {
            AudioManager.Instance?.PlaySE(cooldownSound);
        }

        // Animate Shrink
        animTimer = 0f;
        while (animTimer < animationDuration)
        {
            animTimer += Time.deltaTime;
            float t = animTimer / animationDuration;
            t = t * t * (3f - 2f * t); // Smoothstep
            transform.localScale = Vector3.one * Mathf.Lerp(targetScale, originalScale, t);
            if (Camera.main != null) Camera.main.orthographicSize = Mathf.Lerp(targetOrthoSize, originalOrthoSize, t);
            yield return null;
        }
        transform.localScale = Vector3.one * originalScale;
        if (Camera.main != null) Camera.main.orthographicSize = originalOrthoSize;

        float remainingWait = recoverDuration - animationDuration;
        if (remainingWait > 0)
        {
            yield return new WaitForSeconds(remainingWait);
        }

        // Reset
        if (playerMovement != null)
        {
            playerMovement.SetMovementLock(false);
        }
        if (playerStats != null)
        {
            playerStats.SetSpecialMoveInvincibility(false);
        }
        
        isGiant = false;
    }



    void OnTriggerStay2D(Collider2D other)
    {
        if (isGiant && other.CompareTag("Enemy"))
        {
            EnemyStats es = other.GetComponent<EnemyStats>();
            if (es != null)
            {
                es.TakeDamage(bodyDamage, transform.position, bodyKnockback);
            }
        }
    }
}
