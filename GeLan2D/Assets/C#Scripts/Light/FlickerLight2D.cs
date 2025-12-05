using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Light2D))]
public class FlickerLight2D : MonoBehaviour
{
    [Header("该灯光专属亮度范围")]
    public float minIntensity = 0.2f;
    public float maxIntensity = 1.0f;

    private Light2D light2D;

    private void Awake()
    {
        light2D = GetComponent<Light2D>();
    }

    void Update()
    {
        if (FlickerLight2DManager.Instance == null)
        {
            Debug.LogError("场景缺少 FlickerLight2DManager！");
            return;
        }

        // 获取同步节奏（0~1）
        float t = FlickerLight2DManager.Instance.GetSyncValue();

        // 根据自己的 min/max 映射实际亮度
        light2D.intensity = Mathf.Lerp(minIntensity, maxIntensity, t);
    }
}