using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;

public class UIButtonShake : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private RectTransform rect;
    private Vector3 originalPos;
    private Tween shakeTween;

    void Start()
    {
        rect = GetComponent<RectTransform>();
        originalPos = rect.anchoredPosition;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // 防止重复播放 shake
        if (shakeTween != null && shakeTween.IsActive())
            shakeTween.Kill();

        // 开启轻微抖动
        shakeTween = rect.DOShakeAnchorPos(
            duration: 1f,        // 每次 shake 持续 1 秒
            strength: new Vector2(3f, 3f), // 抖动强度（建议小一点）
            vibrato: 20,         // 抖动频次
            randomness: 90,      // 随机性
            snapping: false,
            fadeOut: false       // 不要淡出，保持稳定抖动
        ).SetLoops(-1, LoopType.Restart);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // 停止抖动
        if (shakeTween != null && shakeTween.IsActive())
            shakeTween.Kill();

        // 位置恢复
        rect.DOAnchorPos(originalPos, 0.15f).SetEase(Ease.OutQuad);
    }
}

