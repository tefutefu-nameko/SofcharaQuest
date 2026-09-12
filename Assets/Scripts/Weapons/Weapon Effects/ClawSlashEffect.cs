using UnityEngine;

/// <summary>
/// 爪による引き裂き（スラッシュ）演出を強化するアニメーション制御コンポーネント。
/// 画像を追加することなく、一瞬のスケール急拡大・前方スワイプ・滑らかなフェードアウトにより
/// 「空間をザシュッと引き裂く」迫力あるゲームフィールを実現します。
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class ClawSlashEffect : MonoBehaviour
{
    [Header("Animation Timings")]
    [Tooltip("アニメーション全体の持続時間（秒）")]
    public float duration = 0.18f;

    [Tooltip("手元から一気に振り抜くまでの時間（秒）")]
    public float slashTime = 0.05f;

    [Header("Scale Motion")]
    [Tooltip("エフェクト全体のサイズ倍率。1.5〜2倍など手軽に微調整できます")]
    public float sizeMultiplier = 1.8f;

    [Tooltip("出現瞬間のスケール倍率（手元での小ささ）")]
    public float startScaleMultiplier = 0.35f;

    [Tooltip("切り裂いた瞬間の最大スケール倍率")]
    public float maxScaleMultiplier = 1.25f;

    [Header("Swipe Motion")]
    [Tooltip("切り裂く際に前方に押し出すスワイプ移動距離")]
    public float swipeForwardDistance = 0.35f;

    private SpriteRenderer spriteRenderer;
    private Vector3 initialLocalScale;
    private Vector3 startPosition;
    private Color initialColor;
    private float elapsedTime = 0f;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        initialColor = spriteRenderer.color;
    }

    private void Start()
    {
        // sizeMultiplier を反映した基準スケール
        initialLocalScale = transform.localScale * sizeMultiplier;
        startPosition = transform.position;

        // 開始時は小さめのスケールで開始
        transform.localScale = initialLocalScale * startScaleMultiplier;
    }

    private void Update()
    {
        elapsedTime += Time.deltaTime;

        // 1. スワイプ＆スケール急拡大（発生から slashTime まで）
        if (elapsedTime <= slashTime)
        {
            float slashProgress = Mathf.Clamp01(elapsedTime / slashTime);
            // イージング（急激に飛び出す Quartic Out）
            float t = 1f - Mathf.Pow(1f - slashProgress, 4);

            // スケール急拡大
            float currentScale = Mathf.Lerp(startScaleMultiplier, maxScaleMultiplier, t);
            transform.localScale = initialLocalScale * currentScale;

            // 前方へのスワイプ移動
            transform.position = startPosition + transform.right * (swipeForwardDistance * t);
        }
        else
        {
            // 振り抜き後は最大スケールを維持
            float fadeProgress = Mathf.Clamp01((elapsedTime - slashTime) / (duration - slashTime));
            float currentScale = Mathf.Lerp(maxScaleMultiplier, maxScaleMultiplier * 1.05f, fadeProgress);
            transform.localScale = initialLocalScale * currentScale;

            // 2. アルファの滑らかなフェードアウト（余韻を残して消える）
            Color c = initialColor;
            c.a = Mathf.Lerp(initialColor.a, 0f, fadeProgress * fadeProgress); // 2乗カーブで綺麗に消える
            spriteRenderer.color = c;
        }

        // 完了したら消滅
        if (elapsedTime >= duration)
        {
            Destroy(gameObject);
        }
    }
}

