using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 強化武器「紅蓮の竜爪（Dragon Claw）」。
/// 右爪(claw_right)と左爪(claw_left)を交互に放つ2連撃を行い、
/// 水平方向（左右）または全方位（マウス方向）への攻撃を切り替え可能な設計。
/// </summary>
public class DragonClawWeapon : ProjectileWeapon
{
    public enum AimMode
    {
        Horizontal,  // 水平方向（左右：mouseDir.x の符号）
        Directional  // 全方位（マウスカーソルの360度方向）
    }

    [Header("Aim Settings")]
    [Tooltip("攻撃方向の制御モード。Horizontal=左右のみ、Directional=マウス全方位")]
    public AimMode aimMode = AimMode.Horizontal;

    [Header("Combo Visual Offsets")]
    [Tooltip("交差・切り裂き感を出すための角度オフセット（度）")]
    public float comboAngleOffset = 8f;
    [Tooltip("交差（X字）を綺麗に見せるためのY座標オフセット")]
    public float comboYOffset = 0.35f;

    [Header("Combo Timing (連撃間隔)")]
    [Tooltip("1撃目から2撃目までの間隔秒数。現在0.2秒（従来の2倍）")]
    public float comboInterval = 0.25f;

    [Header("Spawn Distance (前方出現距離)")]
    [Tooltip("1撃目（右爪）の前方距離。大きくするとキャラクターから離れます")]
    public float firstHitForwardOffset = 5.0f;

    [Tooltip("2撃目（左爪）の前方距離")]
    public float secondHitForwardOffset = 2.5f;

    [Header("Finisher Settings (3撃目フィニッシャー)")]
    [Tooltip("3撃目（炎の斬撃波）の前方出現距離")]
    public float finisherForwardOffset = 2.0f;

    [Tooltip("3撃目（炎の斬撃波）の飛翔速度")]
    public float finisherSpeed = 20.0f;

    [Tooltip("3撃目（炎の斬撃波）の生存時間（秒）。0以下の場合はStatsのlifespan")]
    public float finisherLifespan = 1.0f;

    [Tooltip("3撃目（炎の斬撃波）のサイズ倍率")]
    public float finisherSizeMultiplier = 1.5f;

    private int attackSequenceIndex = 0; // 0 = 1撃目, 1 = 2撃目, 2 = 3撃目(フィニッシャー)

    protected override void Start()
    {
        base.Start();

    }

    protected override bool Attack(int attackCount = 1)
    {
        if (!CanAttack()) return false;

        // クールダウン完了直後の最初の1発目ならシーケンスをリセット（0: 1撃目から開始）
        if (currentCooldown <= 0)
        {
            attackSequenceIndex = 0;
            // 3段コンボ（1撃目・2撃目・3撃目）を実行するため、attackCountを最低3回分確保
            if (attackCount < 3) attackCount = 3;
        }

        bool isFinisher = (attackSequenceIndex == 2);
        bool isFirstHit = (attackSequenceIndex == 0);

        // 3撃目の場合は finisherPrefab（未設定なら projectilePrefab にフォールバック）
        Projectile selectedPrefab = isFinisher && currentStats.finisherPrefab
            ? currentStats.finisherPrefab
            : currentStats.projectilePrefab;

        if (!selectedPrefab)
        {
            Debug.LogWarning(string.Format("Projectile prefab has not been set for {0}", name));
            currentCooldown = currentStats.cooldown;
            return false;
        }

        // 攻撃角度とオフセットの計算
        float spawnAngle = GetSpawnAngle();
        Vector2 spawnOffset = GetSpawnOffset(spawnAngle);

        // 弾丸の生成
        // 水平モードのフィニッシャー（Slash Projectile）は回転をつけず、左右反転（localScale.x）で向きを制御する（WhipWeapon準拠）
        Quaternion spawnRotation = (isFinisher && aimMode == AimMode.Horizontal) 
            ? Quaternion.identity 
            : Quaternion.Euler(0, 0, spawnAngle);

        Projectile spawned = Instantiate(
            selectedPrefab,
            owner.transform.position + (Vector3)spawnOffset,
            spawnRotation
        );

        spawned.weapon = this;
        spawned.owner = owner;

        if (isFinisher)
        {
            // === 3撃目：フィニッシャー（Slash 斬撃波）の制御 ===
            // 1. サイズ調整
            if (finisherSizeMultiplier != 1f)
            {
                spawned.transform.localScale *= finisherSizeMultiplier;
            }

            // 2. 水平モードで左向きの場合の左右反転（WhipWeapon準拠で localScale.x を負に反転。上下反転ではない）
            if (aimMode == AimMode.Horizontal)
            {
                float dirX = (movement.mouseDir.x != 0) ? Mathf.Sign(movement.mouseDir.x) : Mathf.Sign(movement.lastMovedVector.x);
                if (dirX < 0)
                {
                    Vector3 s = spawned.transform.localScale;
                    spawned.transform.localScale = new Vector3(-Mathf.Abs(s.x), s.y, s.z);
                }
            }

            // 3. 生存時間定義
            float life = finisherLifespan > 0 ? finisherLifespan : (currentStats.lifespan > 0 ? currentStats.lifespan : 0.8f);
            

            // 4. ゲームフィール演出（急拡大インパクト＆フェードアウト）の付与
            FinisherSlashEffect slashFx = spawned.GetComponent<FinisherSlashEffect>();
            if (!slashFx)
            {
                slashFx = spawned.gameObject.AddComponent<FinisherSlashEffect>();
            }
            slashFx.Initialize(life);

            // 5. 飛翔速度の付与（Projectile.Start() による速度0上書きを防ぐためコルーチンで適用）
            Vector2 flyDir = (aimMode == AimMode.Horizontal)
                ? (movement.mouseDir.x >= 0 ? Vector2.right : Vector2.left)
                : new Vector2(Mathf.Cos(spawnAngle * Mathf.Deg2Rad), Mathf.Sin(spawnAngle * Mathf.Deg2Rad));

            StartCoroutine(LaunchFinisher(spawned, flyDir, finisherSpeed));

            // 6. 生存時間
            Destroy(spawned.gameObject, life);

        }
        else
        {
            // === 1撃目・2撃目：爪（Claw）の制御 ===
            var slashAnim = spawned.GetComponent<ClawSlashEffect>();
            if (!slashAnim)
            {
                float life = currentStats.lifespan > 0 ? currentStats.lifespan : 0.18f;
                Destroy(spawned.gameObject, life);
            }

            // スケールの反転処理（Y軸を軸にした左右反転）:
            // 2撃目の場合はY軸を軸に反転（X反転）して対の爪を表現
            bool flipX = !isFirstHit;
            bool flipY = (aimMode == AimMode.Horizontal && Mathf.Abs(Mathf.DeltaAngle(spawnAngle, 180f)) < 45f);

            Vector3 s = spawned.transform.localScale;
            spawned.transform.localScale = new Vector3(flipX ? -s.x : s.x, flipY ? -s.y : s.y, s.z);

        }

        // クールダウン更新（0以下の場合は1.0秒で安全ガード）
        if (currentCooldown <= 0)
        {
            float baseCd = currentStats.cooldown > 0 ? currentStats.cooldown : 1.0f;
            currentCooldown += baseCd * (owner ? owner.CurrentCooldown : 1f);
        }

        attackCount--;
        attackSequenceIndex++;

        // 次の連撃（2発目 or 3発目）の準備
        if (attackCount > 0 && attackSequenceIndex < 3)
        {
            currentAttackCount = attackCount;
            // 攻撃間隔：1撃目→2撃目と同じ comboInterval をそのまま使用
            currentAttackInterval = comboInterval > 0 ? comboInterval : (currentStats.projectileInterval > 0 ? currentStats.projectileInterval : 0.2f);
        }
        else
        {
            // コンボ終了
            currentAttackCount = 0;
            currentAttackInterval = 0f;
        }

        return true;
    }

    protected override float GetSpawnAngle()
    {
        bool isFinisher = (attackSequenceIndex == 2);

        if (aimMode == AimMode.Directional)
        {
            // マウスカーソルの360度方向
            float baseAngle = Mathf.Atan2(movement.mouseDir.y, movement.mouseDir.x) * Mathf.Rad2Deg;
            if (isFinisher)
            {
                return baseAngle; // 3撃目（フィニッシャー）は正面まっすぐに発射
            }
            // 1発目と2発目でわずかに交差角度をつける
            float angleOffset = (attackSequenceIndex % 2 == 0) ? comboAngleOffset : -comboAngleOffset;
            return baseAngle + angleOffset;
        }
        else
        {
            // 水平方向（左右）
            float dirX = movement.mouseDir.x;
            if (dirX == 0) dirX = movement.lastMovedVector.x;
            float baseAngle = (dirX >= 0) ? 0f : 180f;

            if (isFinisher)
            {
                return baseAngle; // 3撃目（フィニッシャー）は水平正面まっすぐ
            }

            // 水平モード時もわずかに角度をつけるとX字に切り裂く演出になる
            float angleOffset = (attackSequenceIndex % 2 == 0) ? comboAngleOffset : -comboAngleOffset;
            if (dirX < 0) angleOffset = -angleOffset;

            return baseAngle + angleOffset;
        }
    }

    protected override Vector2 GetSpawnOffset(float spawnAngle = 0)
    {
        bool isFinisher = (attackSequenceIndex == 2);
        bool isFirstHit = (attackSequenceIndex == 0);

        float dist;
        float yOffset;

        if (isFinisher)
        {
            dist = finisherForwardOffset;
            yOffset = 0f; // 正面中央
        }
        else
        {
            // 1撃目（右爪）と2撃目（左爪）で前方距離を切り替え
            dist = isFirstHit ? firstHitForwardOffset : secondHitForwardOffset;
            yOffset = isFirstHit ? comboYOffset : -comboYOffset;
        }

        if (aimMode == AimMode.Directional)
        {
            // 角度に応じた全方位前方ベクトル
            Vector2 forwardDir = new Vector2(Mathf.Cos(spawnAngle * Mathf.Deg2Rad), Mathf.Sin(spawnAngle * Mathf.Deg2Rad));
            return forwardDir * dist;
        }
        else
        {
            // 水平モード：プレイヤーの向いている左右前方にオフセット
            float dirX = (movement.mouseDir.x != 0) ? Mathf.Sign(movement.mouseDir.x) : Mathf.Sign(movement.lastMovedVector.x);
            if (dirX == 0) dirX = 1f;
            return new Vector2(dirX * dist, yOffset);
        }
    }

    /// <summary>
    /// Projectile.Start() による速度初期化（0）が完了した直後に、フィニッシャー本来の飛翔速度をセットする
    /// </summary>
    private IEnumerator LaunchFinisher(Projectile proj, Vector2 flyDir, float speed)
    {
        // 生成フレームの初期化（Start() の実行完了）まで待機
        yield return null;

        if (proj != null)
        {
            Rigidbody2D rb = proj.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.gravityScale = 0f;
                rb.bodyType = RigidbodyType2D.Dynamic;
                rb.velocity = flyDir * speed;
            }
        }
    }
}
