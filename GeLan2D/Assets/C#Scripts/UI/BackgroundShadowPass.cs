using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class BackgroundShadowPass : MonoBehaviour
{
    private RectTransform rect;
    private Image shadowImg;
    private Vector2 originalPos;

    void Start()
    {
        rect = GetComponent<RectTransform>();
        shadowImg = GetComponent<Image>();
        originalPos = rect.anchoredPosition;

        PlayShadowMovement();
        PlayShadowFlicker();
    }

    void PlayShadowMovement()
    {
        // 黑影缓慢飘动（左右 + 上下）
        rect.DOAnchorPos(new Vector2(originalPos.x + 40f, originalPos.y + 25f), 6f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    void PlayShadowFlicker()
    {
        // 透明度闪动：模拟“经过感”
        shadowImg.DOFade(0.06f, 2.2f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }
}

