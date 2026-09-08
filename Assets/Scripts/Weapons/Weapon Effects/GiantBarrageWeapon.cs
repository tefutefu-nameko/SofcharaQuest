using UnityEngine;

public class GiantBarrageWeapon : ProjectileWeapon
{
    protected override void Awake()
    {
        // 既存のWeaponのAwake（WeaponDataの参照）をスキップ
    }

    protected override void Start()
    {
        // 既存のWeaponのStart（Initialise）をスキップ
    }

    public void Setup(Projectile projectilePrefab, PlayerStats ownerStats, float fireRate)
    {
        this.owner = ownerStats;
        this.movement = ownerStats.GetComponent<PlayerMovement>();
        
        // 必殺技専用のダミーステータスを設定
        currentStats = new Weapon.Stats();
        currentStats.projectilePrefab = projectilePrefab;
        currentStats.cooldown = fireRate;
        currentStats.damage = 50f;     // 必殺技の固定ダメージ
        currentStats.speed = 10f;      // 弾速
        currentStats.piercing = 99;    // 貫通力（ほぼ無限）
        currentStats.area = 1f;        // 弾の大きさ
        currentStats.number = 1;       // 1回の発射数
        currentStats.lifespan = 5f;    // 5秒で消滅

        currentCooldown = 0f;
    }

    // 全方位（0〜360度）ランダムに発射
    protected override float GetSpawnAngle()
    {
        return Random.Range(0f, 360f);
    }

    // 中心（プレイヤー自身）から発射
    protected override Vector2 GetSpawnOffset(float spawnAngle = 0)
    {
        return Vector2.zero;
    }
}
