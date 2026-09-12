using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class FoxFireLaserHitbox : MonoBehaviour
{
    public float damage = 50f;
    public float knockback = 5f;
    public float tickRate = 0.1f; // 0.1秒ごとにダメージ

    private Dictionary<EnemyStats, float> affectedTargets = new Dictionary<EnemyStats, float>();
    private List<EnemyStats> targetsToUnaffect = new List<EnemyStats>();

    void Update()
    {
        Dictionary<EnemyStats, float> affectedTargsCopy = new Dictionary<EnemyStats, float>(affectedTargets);

        foreach (KeyValuePair<EnemyStats, float> pair in affectedTargsCopy)
        {
            affectedTargets[pair.Key] -= Time.deltaTime;
            if (pair.Value <= 0)
            {
                if (targetsToUnaffect.Contains(pair.Key))
                {
                    affectedTargets.Remove(pair.Key);
                    targetsToUnaffect.Remove(pair.Key);
                }
                else
                {
                    affectedTargets[pair.Key] = tickRate;
                    pair.Key.TakeDamage(damage, transform.position, knockback);
                }
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out EnemyStats es))
        {
            if (!affectedTargets.ContainsKey(es))
            {
                affectedTargets.Add(es, 0);
            }
            else
            {
                if (targetsToUnaffect.Contains(es))
                {
                    targetsToUnaffect.Remove(es);
                }
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent(out EnemyStats es))
        {
            if (affectedTargets.ContainsKey(es))
            {
                targetsToUnaffect.Add(es);
            }
        }
    }

    void OnDisable()
    {
        affectedTargets.Clear();
        targetsToUnaffect.Clear();
    }
}
