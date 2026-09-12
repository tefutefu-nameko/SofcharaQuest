using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 竜爪フィニッシャー（3撃目の大斬撃波）のゲームフィール演出用コンポーネント。
/// 放たれた瞬間の急拡大（Quartic Out イージング）と、
/// 消滅前の滑らかなフェードアウトにより「必殺技としての重みと鋭さ」を演出します。
/// </summary>
public class FinisherSlashEffect : MonoBehaviour
{
    [Header("Sprite Animation (連番アニメーション)")]
    [Tooltip("パラパラアニメーションさせるスプライト配列（5枚）")]
    public Sprite[] animationFrames;

    [Tooltip("1コマあたりの表示秒数（例: 0.06秒で約16fps）")]
    public float frameRate = 0.06f;

    [Tooltip("アニメーションをループ再生するか")]
    public bool loopAnimation = true;

    [Header("Impact Timing (発生のタメ・弾け)")]
    [Tooltip("手元から一気に大斬撃へ急拡大する時間（秒）")]
    public float burstTime = 0.08f;

    [Tooltip("出現瞬間のスケール倍率")]
    public float startScaleMultiplier = 0.4f;

    [Tooltip("弾けた瞬間の最大スケール倍率")]
    public float maxScaleMultiplier = 1.25f;

    [Header("Fade Out (消滅演出)")]
    [Tooltip("寿命終了前のフェードアウト時間（秒）")]
    public float fadeDuration = 0.15f;

    private SpriteRenderer spriteRenderer;
    private Vector3 targetBaseScale;
    private Color initialColor;
    private float totalLifespan;
    private float elapsedTime = 0f;
    private float animTimer = 0f;
    private int currentFrameIndex = 0;
    private bool isInitialized = false;

    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }

    /// <summary>
    /// フィニッシャー生成時に寿命などのパラメータを初期化
    /// </summary>
    public void Initialize(float lifespan)
    {
        totalLifespan = lifespan;
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        if (spriteRenderer != null)
        {
            initialColor = spriteRenderer.color;
        }

        // 現在設定されているスケールを基準サイズとして記憶
        targetBaseScale = transform.localScale;

        // 出現時は小さく構える
        transform.localScale = targetBaseScale * startScaleMultiplier;

        // 連番の初期フレームをセット
        if (animationFrames != null && animationFrames.Length > 0 && spriteRenderer != null)
        {
            currentFrameIndex = 0;
            spriteRenderer.sprite = animationFrames[0];
        }

        isInitialized = true;
    }

    private void Update()
    {
        if (!isInitialized) return;

        elapsedTime += Time.deltaTime;

        // 1. 連番スプライトのコマ送りアニメーション
        if (animationFrames != null && animationFrames.Length > 1 && spriteRenderer != null)
        {
            animTimer += Time.deltaTime;
            if (animTimer >= frameRate)
            {
                animTimer -= frameRate;
                currentFrameIndex++;

                if (currentFrameIndex >= animationFrames.Length)
                {
                    currentFrameIndex = loopAnimation ? 0 : animationFrames.Length - 1;
                }

                spriteRenderer.sprite = animationFrames[currentFrameIndex];
            }
        }

        // 2. 出現瞬間の急拡大（発生から burstTime まで）
        if (elapsedTime <= burstTime)
        {
            float progress = Mathf.Clamp01(elapsedTime / burstTime);
            // 急激に飛び出す Quartic Out イージング
            float t = 1f - Mathf.Pow(1f - progress, 4);

            float currentScale = Mathf.Lerp(startScaleMultiplier, maxScaleMultiplier, t);
            transform.localScale = targetBaseScale * currentScale;
        }
        else
        {
            // 弾けた後は安定サイズへわずかに収束しながら飛翔
            float tAfter = Mathf.Clamp01((elapsedTime - burstTime) / 0.15f);
            float currentScale = Mathf.Lerp(maxScaleMultiplier, 1.0f, tAfter);
            transform.localScale = targetBaseScale * currentScale;
        }

        // 3. 寿命直前の滑らかなフェードアウト
        if (spriteRenderer != null && totalLifespan > fadeDuration)
        {
            float fadeStartTime = totalLifespan - fadeDuration;
            if (elapsedTime >= fadeStartTime)
            {
                float fadeProgress = Mathf.Clamp01((elapsedTime - fadeStartTime) / fadeDuration);
                Color c = initialColor;
                c.a = Mathf.Lerp(initialColor.a, 0f, fadeProgress * fadeProgress);
                spriteRenderer.color = c;
            }
        }
    }
}
