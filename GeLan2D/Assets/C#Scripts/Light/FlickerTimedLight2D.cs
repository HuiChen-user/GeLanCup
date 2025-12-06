using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Light2D))]
public class FlickerTimedLight2D : MonoBehaviour
{
    [Header("亮度范围")]
    public float minIntensity = 0.2f;     
    public float maxIntensity = 1.0f;     

    [Header("闪烁控制")]
    public float flickerInterval = 0.1f;  
    public bool smooth = false;

    [Header("亮起持续时间")]
    public float activeDuration = 3f;     // 闪烁持续时间（秒）

    private Light2D light2D;
    private float timer;
    private float remainTime;
    private float targetValue;
    private bool isActive = false;

    private void Awake()
    {
        light2D = GetComponent<Light2D>();
        light2D.intensity = 0f; // 初始关闭
        targetValue = 0f;
    }

    private void Update()
    {
        if (!isActive) return;

        remainTime -= Time.deltaTime;
        if (remainTime <= 0)
        {
            // 自动熄灭
            isActive = false;
            light2D.intensity = 0f;
            return;
        }

        // 闪烁计时
        timer += Time.deltaTime;
        if (timer >= flickerInterval)
        {
            targetValue = Random.Range(0f, 1f); // 随机节奏
            timer = 0f;
        }

        float t = smooth
            ? Mathf.Lerp(light2D.intensity, Mathf.Lerp(minIntensity, maxIntensity, targetValue), Time.deltaTime * 6f)
            : Mathf.Lerp(minIntensity, maxIntensity, targetValue);

        light2D.intensity = t;
    }

    /// <summary>
    /// 外部调用此函数来控制灯：亮起闪烁几秒，之后熄灭
    /// </summary>
    public void Activate()
    {
        isActive = true;
        remainTime = activeDuration;
    }
}

