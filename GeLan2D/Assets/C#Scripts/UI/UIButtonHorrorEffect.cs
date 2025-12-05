using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.UI;

public class UIButtonHorrorEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private RectTransform rect;
    private Vector3 originalScale;
    private Vector3 originalPos;
    private Tween shakeTween;
    private Tween scaleDistortTween;
    private Tween flickerTween;

    private CanvasGroup cg; // 模糊闪动用 (透明度模拟)

    void Start()
    {
        rect = GetComponent<RectTransform>();
        cg = gameObject.GetComponent<CanvasGroup>();
        if (cg == null) cg = gameObject.AddComponent<CanvasGroup>();

        originalScale = rect.localScale;
        originalPos = rect.anchoredPosition;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        StartShake();
        StartScaleDistort();
        StartFlicker();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StopShake();
        StopDistort();
        StopFlicker();
    }

    // 位置抖动 
    void StartShake()
    {
        shakeTween?.Kill();

        shakeTween = rect.DOShakeAnchorPos(
            duration: 1f,
            strength: new Vector2(3f, 3f),
            vibrato: 30,
            randomness: 120,
            snapping: false,
            fadeOut: false
        ).SetLoops(-1, LoopType.Restart);
    }

    void StopShake()
    {
        shakeTween?.Kill();
        rect.DOAnchorPos(originalPos, 0.15f).SetEase(Ease.OutQuad);
    }

    // Scale 轻微失真 --------------------------------------------
    void StartScaleDistort()
    {
        scaleDistortTween?.Kill();

        scaleDistortTween = rect.DOScale(
            originalScale * 1.02f, // 微弱拉伸
            0.25f
        )
        .SetEase(Ease.InOutSine)
        .SetLoops(-1, LoopType.Yoyo);
    }

    void StopDistort()
    {
        scaleDistortTween?.Kill();
        rect.DOScale(originalScale, 0.15f).SetEase(Ease.OutQuad);
    }

    // 模糊闪动（通过透明度 Flicker 实现“幽灵残影感”） ----------
    void StartFlicker()
    {
        flickerTween?.Kill();

        flickerTween = cg.DOFade(0.92f, 0.12f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    void StopFlicker()
    {
        flickerTween?.Kill();
        cg.DOFade(1f, 0.1f);
    }
}

