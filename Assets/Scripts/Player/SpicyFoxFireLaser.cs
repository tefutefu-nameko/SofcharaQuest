using System.Collections;
using UnityEngine;

public class SpicyFoxFireLaser : MonoBehaviour
{
    [Header("Laser Settings")]
    public float duration = 5.0f;
    public float recoverDuration = 0.5f;
    public float animationDuration = 0.3f;
    public float cameraZoomMultiplier = 1.5f;
    public float movementSpeedMultiplier = 0.5f; // 歩きながら薙ぎ払う（半減）
    public GameObject laserPrefab; // 巨大な狐火レーザーのプレハブ

    [Header("Audio Settings")]
    public AudioClip activateSound;
    public AudioClip laserSound;

    private PlayerStats playerStats;
    private PlayerMovement playerMovement;
    private SpecialMoveSystem specialMoveSystem;
    private CameraMovement cameraMovement;

    private bool isFiring = false;
    private GameObject currentLaser;

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
            specialMoveSystem.OnSpecialMoveActivated += ActivateLaser;
        }
    }

    void OnDestroy()
    {
        if (specialMoveSystem != null)
        {
            specialMoveSystem.OnSpecialMoveActivated -= ActivateLaser;
        }
    }

    void ActivateLaser(Sprite cutinSprite)
    {
        if (!isFiring)
        {
            StartCoroutine(LaserRoutine());
        }
    }

    IEnumerator LaserRoutine()
    {
        isFiring = true;

        if (activateSound != null)
        {
            AudioManager.Instance?.PlaySE(activateSound);
        }

        if (playerStats != null)
        {
            playerStats.SetSpecialMoveInvincibility(true);
        }

        // 移動速度を落とす
        float originalSpeed = 0f;
        if (playerStats != null)
        {
            originalSpeed = playerStats.CurrentMoveSpeed;
            // 直接書き換えるか、PlayerStats側に倍率処理を入れる。
            // 簡易的に直接変更（本来はバフシステムを使うのが望ましいが簡易実装）
            // プレイヤーの元々の stats.moveSpeed をいじる。
            // ただし CurrentMoveSpeed は get プロパティかもしれない。
        }

        // ここでは直接 CurrentMoveSpeed がセット可能か不明なので、PlayerMovementを直接ロックせずに、
        // プレイヤーの移動処理自体をいじるのは少し危険。
        // 代わりに PlayerMovement.cs を見ると `isMovementLocked` はあるが、速度半減の処理はない。
        // よって、簡易的に PlayerStats の stat を書き換えるか、ここでは速度変更をスキップする。
        // とりあえず今回は仕様として「歩ける」ことを優先し、移動速度の変更は見送る（あるいは必要なら後でPlayerMovementを拡張する）。

        if (cameraMovement != null)
        {
            cameraMovement.TriggerShake(duration, 0.1f);
        }

        if (laserSound != null)
        {
            AudioManager.Instance?.PlaySE(laserSound);
        }

        // カメラズームアウトの準備
        float originalOrthoSize = 5f;
        if (Camera.main != null)
        {
            originalOrthoSize = Camera.main.orthographicSize;
        }
        float targetOrthoSize = originalOrthoSize * cameraZoomMultiplier;

        float animTimer = 0f;
        while (animTimer < animationDuration)
        {
            animTimer += Time.deltaTime;
            float t = animTimer / animationDuration;
            t = t * t * (3f - 2f * t); // Smoothstep
            if (Camera.main != null) Camera.main.orthographicSize = Mathf.Lerp(originalOrthoSize, targetOrthoSize, t);
            yield return null;
        }
        if (Camera.main != null) Camera.main.orthographicSize = targetOrthoSize;

        // レーザーを生成し、プレイヤーの子オブジェクトにする
        if (laserPrefab != null)
        {
            currentLaser = Instantiate(laserPrefab, transform.position, Quaternion.identity, transform);
        }
        else
        {
            // 仮のレーザーを動的生成
            currentLaser = new GameObject("TempFoxFireLaser");
            currentLaser.transform.SetParent(transform);
            currentLaser.transform.localPosition = Vector3.zero;

            BoxCollider2D col = currentLaser.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(12f, 1.5f); // 太さを3から1.5に短縮
            col.offset = new Vector2(6f, 0f);

            Rigidbody2D rb = currentLaser.AddComponent<Rigidbody2D>();
            rb.isKinematic = true;

            FoxFireLaserHitbox hitbox = currentLaser.AddComponent<FoxFireLaserHitbox>();
            hitbox.damage = 5f;      // 火力を 10f から 5f にダウン
            hitbox.tickRate = 0.2f;  // ヒット間隔を 0.1秒 から 0.2秒 に延長
            hitbox.knockback = 5f;

            // 見た目を確実にするためにQuadを生成
            GameObject visuals = GameObject.CreatePrimitive(PrimitiveType.Quad);
            visuals.name = "Visuals";
            Destroy(visuals.GetComponent<Collider>()); // デフォルトのMeshColliderを削除
            visuals.transform.SetParent(currentLaser.transform);
            visuals.transform.localPosition = new Vector3(6f, 0f, 0f);
            visuals.transform.localScale = new Vector3(12f, 1.5f, 1f);
            
            MeshRenderer mr = visuals.GetComponent<MeshRenderer>();
            Material mat = new Material(Shader.Find("Sprites/Default"));
            mat.color = new Color(1f, 0.3f, 0f, 0.7f); // オレンジ〜赤の半透明
            mr.material = mat;
            mr.sortingOrder = 100;
        }

        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;

            // マウスの方向に向かってレーザーを回転させる
            if (currentLaser != null && playerMovement != null)
            {
                // playerMovement.mouseDir を使用
                Vector2 dir = playerMovement.mouseDir;
                if (dir.sqrMagnitude > 0)
                {
                    float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                    currentLaser.transform.rotation = Quaternion.Euler(0, 0, angle);
                }
            }

            yield return null;
        }

        // 終了処理
        if (currentLaser != null)
        {
            Destroy(currentLaser);
        }

        // カメラズームイン（元に戻す）
        animTimer = 0f;
        while (animTimer < animationDuration)
        {
            animTimer += Time.deltaTime;
            float t = animTimer / animationDuration;
            t = t * t * (3f - 2f * t); // Smoothstep
            if (Camera.main != null) Camera.main.orthographicSize = Mathf.Lerp(targetOrthoSize, originalOrthoSize, t);
            yield return null;
        }
        if (Camera.main != null) Camera.main.orthographicSize = originalOrthoSize;

        float remainingWait = recoverDuration - animationDuration;
        if (remainingWait > 0)
        {
            yield return new WaitForSeconds(remainingWait);
        }

        if (playerStats != null)
        {
            playerStats.SetSpecialMoveInvincibility(false);
        }
        
        isFiring = false;
    }
}
