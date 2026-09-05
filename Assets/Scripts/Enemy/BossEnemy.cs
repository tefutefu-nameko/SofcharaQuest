using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossEnemy : MonoBehaviour
{
    PlayerStats player;

    // PlayerStatsを継続的に更新
    void Update()
    {
        player = FindObjectOfType<PlayerStats>();
    }

    // Boss撃破でゲーム終了
    private void OnDestroy()
    {
        Debug.LogWarning("Bossはシンだよ");
        player.Kill();
    }
}
