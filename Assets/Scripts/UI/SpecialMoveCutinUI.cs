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
        if (darkBackground) darkBackground.gameObject.SetActive(true);
        if (cutinImage) cutinImage.gameObject.SetActive(true);

        // Reset colors
        Color bgColor = darkBackground.color;
        bgColor.a = 0;
        if (darkBackground) darkBackground.color = bgColor;

        Color cutinColor = cutinImage.color;
        cutinColor.a = 1;
        cutinImage.color = cutinColor;

        // Slide in from right (assuming anchored to center or right)
        Vector2 startPos = new Vector2(Screen.width, 0); // Assuming Screen width is far enough
        Vector2 endPos = Vector2.zero; // Assuming 0 is the center position

        if (cutinTransform != null)
        {
            cutinTransform.anchoredPosition = startPos;
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
            
            if (darkBackground) {
                bgColor.a = Mathf.Lerp(0, 0.5f, ease); // Darken up to 0.5 alpha
                darkBackground.color = bgColor;
            }

            yield return null;
        }

        if (cutinTransform != null) cutinTransform.anchoredPosition = endPos;
        if (darkBackground) {
            bgColor.a = 0.5f;
            darkBackground.color = bgColor;
        }

        // Display
        yield return new WaitForSeconds(displayDuration);

        // Fade out
        t = 0;
        while (t < fadeOutDuration)
        {
            t += Time.deltaTime;
            float normalizedTime = t / fadeOutDuration;

            if (darkBackground) {
                bgColor.a = Mathf.Lerp(0.5f, 0, normalizedTime);
                darkBackground.color = bgColor;
            }

            cutinColor.a = Mathf.Lerp(1, 0, normalizedTime);
            cutinImage.color = cutinColor;

            yield return null;
        }

        if (darkBackground) darkBackground.gameObject.SetActive(false);
        if (cutinImage) cutinImage.gameObject.SetActive(false);
    }
}
