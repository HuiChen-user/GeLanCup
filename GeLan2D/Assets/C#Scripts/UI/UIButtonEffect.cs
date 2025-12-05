using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;

public class UIButtonEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private Vector3 originalScale;

    void Start()
    {
        originalScale = transform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.DOScale(originalScale * 1.08f, 0.2f).SetEase(Ease.OutQuad);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.DOScale(originalScale, 0.2f).SetEase(Ease.OutQuad);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOScale(originalScale * 0.9f, 0.08f))
            .Append(transform.DOScale(originalScale * 1.1f, 0.12f))
            .Append(transform.DOScale(originalScale, 0.1f))
            .SetEase(Ease.OutCubic);
    }
}
