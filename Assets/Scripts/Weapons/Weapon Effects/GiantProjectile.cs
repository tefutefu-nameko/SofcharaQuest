using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class GiantProjectile : MonoBehaviour
{
    public float speed = 10f;
    public float damage = 50f;
    public float knockback = 5f;
    public int piercing = 99;
    public float angularVelocity = 0f;
    
    public Sprite[] sprites;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (sprites != null && sprites.Length > 0 && spriteRenderer != null)
        {
            spriteRenderer.sprite = sprites[Random.Range(0, sprites.Length)];
            spriteRenderer.sortingOrder = 100; // 前面に表示
        }
        transform.localScale = Vector3.one * 0.5f; // 見えやすいようにスケールを小さめに調整
        
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.velocity = transform.right * speed;
        rb.angularVelocity = angularVelocity;

        // 画面外判定の誤動作を防ぐため、一定時間（例えば5秒）経過で自動的に消滅させる
        Destroy(gameObject, 5f);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            EnemyStats es = other.GetComponent<EnemyStats>();
            if (es != null)
            {
                es.TakeDamage(damage, transform.position, knockback);
                piercing--;
                if (piercing <= 0)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}
