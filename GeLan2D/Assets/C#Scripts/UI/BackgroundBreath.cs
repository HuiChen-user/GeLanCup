using UnityEngine;
using DG.Tweening;

public class BackgroundBreath : MonoBehaviour
{
    void Start()
    {
        // 背景呼吸：轻微缩放循环
        transform.DOScale(1.03f, 3f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }
}

