using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SpecialMoveCutinUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] GameObject panelRoot; // The root object holding the dark background and cutin image
    [SerializeField] Image darkBackground;
    [SerializeField] Image cutinImage;
    [SerializeField] RectTransform cutinTransform;

    [Header("Animation Settings")]
    [SerializeField] float slideInDuration = 0.3f;
    [SerializeField] float displayDuration = 1.0f;
    [SerializeField] float fadeOutDuration = 0.5f;
    [SerializeField] float cutinScale = 0.5f; // カットインのスケール
    [SerializeField] Vector2 cutinOffset = new Vector2(300f, -200f); // カットインの表示位置のオフセット（中央から右下など）

    SpecialMoveSystem specialMoveSystem;
    Coroutine currentAnimation;

    void Start()
    {
        if (darkBackground)
        {
            darkBackground.gameObject.SetActive(false);
            darkBackground.raycastTarget = false; // Prevent blocking clicks
        }
        if (cutinImage)
        {
            cutinImage.gameObject.SetActive(false);
            cutinImage.raycastTarget = false; // Prevent blocking clicks
        }
    }

    void OnEnable()
    {
        if (!specialMoveSystem) specialMoveSystem = FindObjectOfType<SpecialMoveSystem>();
        if (specialMoveSystem)
        {
            specialMoveSystem.OnSpecialMoveActivated += ShowCutin;
        }
    }

    void OnDisable()
    {
        if (specialMoveSystem)
        {
            specialMoveSystem.OnSpecialMoveActivated -= ShowCutin;
        }
    }

    public void ShowCutin(Sprite cutinSprite)
    {
        Debug.Log("ShowCutin called!");
        if (cutinSprite == null) Debug.LogError("Cutin failed: cutinSprite is null! (CharacterDataに顔画像が設定されていません)");
        if (cutinImage == null) Debug.LogError("Cutin failed: cutinImage is null! (インスペクターでCutin Image枠が空です)");
        if (panelRoot == null) Debug.LogError("Cutin failed: panelRoot is null! (インスペクターでPanel Root枠が空です)");

        if (cutinSprite == null || cutinImage == null || panelRoot == null) return;
        
        Debug.Log("Cutin starting animation...");

        cutinImage.sprite = cutinSprite;

        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
        }
        currentAnimation = StartCoroutine(CutinAnimationCoroutine());
    }

    IEnumerator CutinAnimationCoroutine()
    {
        // 背景暗転は無効化するため、非表示のままにする
        if (darkBackground) darkBackground.gameObject.SetActive(false);
        if (cutinImage) cutinImage.gameObject.SetActive(true);

        // Reset colors
        Color cutinColor = cutinImage.color;
        cutinColor.a = 1;
        cutinImage.color = cutinColor;

        // Slide in from right
        Vector2 startPos = new Vector2(Screen.width * 1.5f, cutinOffset.y); 
        Vector2 endPos = cutinOffset; 

        if (cutinTransform != null)
        {
            cutinTransform.anchoredPosition = startPos;
            cutinTransform.localScale = Vector3.one * cutinScale; // スケールを適用
        }

        float t = 0;
        while (t < slideInDuration)
        {
            t += Time.deltaTime;
            float normalizedTime = t / slideInDuration;
            
            // Ease out cubic
            float ease = 1 - Mathf.Pow(1 - normalizedTime, 3);

            if (cutinTransform != null)
            {
                cutinTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, ease);
            }

            yield return null;
        }

        if (cutinTransform != null) cutinTransform.anchoredPosition = endPos;

        // Display
        yield return new WaitForSeconds(displayDuration);

        // Fade out
        t = 0;
        while (t < fadeOutDuration)
        {
            t += Time.deltaTime;
            float normalizedTime = t / fadeOutDuration;

            cutinColor.a = Mathf.Lerp(1, 0, normalizedTime);
            cutinImage.color = cutinColor;

            yield return null;
        }

        if (darkBackground) darkBackground.gameObject.SetActive(false);
        if (cutinImage) cutinImage.gameObject.SetActive(false);
    }
}
