using UnityEngine;
using System.Collections;

public class SpawnSpot : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject enemyPrefab;   // 湧かせる敵プレハブ
    public int spawnCount = 3;       // 一度に湧かせる数
    public float cooldown = 10f;     // クールタイム秒

    [Header("Donut Range Settings (player-centered)")]
    public float innerRadius = 8f;   // プレイヤーから近すぎる範囲を除外
    public float outerRadius = 15f;  // プレイヤーからの最大範囲

    private Transform player;
    private bool isOnCooldown = false;

    void Start()
    {
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogError("[SpawnSpot] Playerタグが見つかりません。プレイヤーに\"Player\"タグを付けてください。");
        }
    }

    void Update()
    {
        if (player == null || isOnCooldown) return;

        // プレイヤー中心の距離判定
        float dist = Vector3.Distance(player.position, transform.position);

        if (dist >= innerRadius && dist <= outerRadius)
        {
            StartCoroutine(SpawnAndCooldown());
        }
    }

    IEnumerator SpawnAndCooldown()
    {
        // 敵を一定数湧かせる
        for (int i = 0; i < spawnCount; i++)
        {
            Vector3 offset = Random.insideUnitSphere * 2f; // 半径2のランダム位置
            offset.y = 0; // 地面に沿わせる
            Instantiate(enemyPrefab, transform.position + offset, Quaternion.identity);
        }


        // クールタイム開始
        isOnCooldown = true;
        yield return new WaitForSeconds(cooldown);
        isOnCooldown = false;
    }

    // エディタで範囲を可視化（プレイヤー中心ではなくスポット位置に描かれるので注意）
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, outerRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, innerRadius);
    }
}